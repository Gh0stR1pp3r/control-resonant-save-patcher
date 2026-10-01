using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

[assembly: AssemblyTitle("Cosmetic Save Patcher")]
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyFileVersion("1.2.0.0")]
[assembly: AssemblyInformationalVersion("1.2.0")]

internal static class Program
{
    const string Version = "1.2.0";
    // Serialized entitlement IDs from the installed entitlement database.
    // Remove applied markers so the existing revoke path cannot clear these rewards.
    static readonly uint[] EntitlementsToRemove = {
        0xB4F0E7DD, // PC preorder
        0x5C2B95A3, // PlayStation preorder
        0x7BD90E22, // NVIDIA
        0x897D368C, // Beta testers
        0xD8A2F11E, // Mailing promotion 1
        0xD7A2EF8B, // Mailing promotion 2
        0x50FAD256, // Twitch drop 1
        0x4FFAD0C3, // Twitch drop 2
        0x4EFACF30, // Twitch drop 3
        0xECA4A890  // China promotion
    };
    const int MaxFileBytes = 64 * 1024 * 1024;
    static readonly ulong[] PreorderUnlockFacts = {
        0x09FC268427CD1CD6UL, // Pickpocket's Tool (charm)
        0x2859E4A7F3D0F7FBUL, // Threshold Bureau Coat
        0x57C9EA469E42C97BUL, // Threshold Bureau Gas Mask
        0x5F7C4166260F3CFDUL, // Exposed
        0x8F3741DBFF2CB8EBUL, // Corrupted
        0xA70D57F494331711UL  // Threshold Bureau Workwear
    };
    static readonly ulong[] ExtraUnlockFacts = {
        0x5388457983503C2EUL, // Third Ice Baseball Cap
        0x747AFDEFDBABB85BUL, // Cracked Standard Issue Sunglasses
        0x2E27C13F1B8BE2ADUL, // Optical Filtering Goggles
        0x46780E853CF1A90BUL, // Communications Department Headset
        0xB961238CCE640F14UL, // Sierra Helmet
        0xBF6BE35893F7D586UL, // Sierra Vest
        0xEF6FBC7BA02AB697UL, // Sierra Suit
        0xF37DC383C5C5E132UL  // MIO Specialist's Robe
    };
    static readonly uint[] SlotIds = {
        0x22331ABB, 0x26332107, 0x2733229A, 0x25331F74, 0x1B330FB6,
        0x1C331149, 0x2833242D, 0x24331DE1, 0x21331928
    };
    static readonly string[] Suffixes = { "header", "persi-global", "player", "bundle-container" };
    static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

    sealed class SaveHeader
    {
        public string Path;
        public byte[] Bytes;
        public ulong Timestamp;
        public int CountPosition;
        public List<uint> Entitlements;
    }

    sealed class SaveContainer
    {
        public string Name;
        public Dictionary<string, string> Files;
        public string IndexPath;
        public long IndexSizeOffset;
        public ulong IndexedSize;
    }

    sealed class SaveCandidate
    {
        public SaveContainer Container;
        public SaveHeader Header;
    }

    static int Main(string[] args)
    {
        bool noPause = args.Contains("--no-pause");
        int status = 0;
        try
        {
            Console.WriteLine("COSMETIC SAVE PATCHER " + Version + "\n");
            Console.WriteLine("13 cosmetics + Pickpocket's Tool charm: preorder and promotional rewards.\n");
            if (args.Any(a => a != "--check" && a != "--no-pause" && a != "--help"))
                throw new InvalidOperationException("Unknown option. Use --help for instructions.");
            if (args.Contains("--help"))
            {
#if NET8_0_OR_GREATER
                Console.WriteLine("Place this program beside the save files and run it from a terminal.\n" +
#else
                Console.WriteLine("Place this EXE beside the save files and double-click it.\n" +
#endif
                    "Close the game first. The newest save set in this folder is patched.\n" +
                    "Steam: place the patcher beside the flat save files.\n" +
                    "Xbox PC: place it beside containers.index in the per-user WGS folder.\n" +
                    "WGS containers are not rebuilt; only mapped blobs and required size metadata change.\n\n" +
                    "--check     Inspect only: do not patch or create backups.\n" +
                    "--no-pause  Exit without waiting for Enter.\n\n" +
                    "Restores 14 item unlock flags and removes up to ten\n" +
                    "associated applied-entitlement entries.\n" +
                    "Supports the supplied game's header version 16 and global version 23.\n" +
                    "Backups are created automatically before patching.");
            }
            else Run(args.Contains("--check"));
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nSTOPPED: " + ex.Message);
            status = 1;
        }
        if (!noPause && !Console.IsInputRedirected)
        {
            Console.WriteLine("\nPress Enter to close.");
            Console.ReadLine();
        }
        return status;
    }

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }

    static void Bounds(byte[] b, int at, long length)
    {
        Require(at >= 0 && length >= 0 && at <= b.Length && length <= b.Length - at,
            "The save is truncated or uses an unsupported layout.");
    }

    static uint U32(byte[] b, int at) { Bounds(b, at, 4); return BitConverter.ToUInt32(b, at); }
    static ulong U64(byte[] b, int at) { Bounds(b, at, 8); return BitConverter.ToUInt64(b, at); }
    static void Put32(byte[] b, int at, uint value) { Bounds(b, at, 4); Buffer.BlockCopy(BitConverter.GetBytes(value), 0, b, at, 4); }

    static void RegularFile(string path)
    {
        Require(File.Exists(path), "Missing matching save file: " + System.IO.Path.GetFileName(path));
        Require((File.GetAttributes(path) & FileAttributes.ReparsePoint) == 0,
            "Linked save files are not supported: " + System.IO.Path.GetFileName(path));
    }

    static byte[] Read(string path)
    {
        RegularFile(path);
        long size = new FileInfo(path).Length;
        Require(size >= 20 && size <= MaxFileBytes, "Unexpected save size: " + System.IO.Path.GetFileName(path));
        byte[] bytes = File.ReadAllBytes(path);
        Require(bytes.Length <= MaxFileBytes, "Save size changed while reading.");
        return bytes;
    }

    static uint Crc32(byte[] bytes)
    {
        uint crc = 0xFFFFFFFF;
        for (int i = 16; i < bytes.Length; i++)
        {
            crc ^= bytes[i];
            for (int bit = 0; bit < 8; bit++)
                crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xEDB88320U : 0U);
        }
        return ~crc;
    }

    static void Validate(byte[] b, string name)
    {
        Require(b.Length >= 20 && b[0] == 'R' && b[1] == 'M' && b[2] == 'D' && b[3] == 'B',
            "Not an RMDB save: " + name);
        Require(U32(b, 4) == 2 && U32(b, 8) == 2, "Unsupported RMDB version: " + name);
        Require(U32(b, 12) == Crc32(b), "Checksum does not match: " + name + ". Nothing will be patched.");
    }

    static SaveHeader ParseHeader(string path, byte[] bytes)
    {
        Validate(bytes, System.IO.Path.GetFileName(path));
        Require(U32(bytes, 16) == 16, "Unsupported header version: " + System.IO.Path.GetFileName(path));
        uint length = U32(bytes, 45);
        Require(length <= 1024 * 1024, "Unexpected location string length.");
        Bounds(bytes, 49, (long)length + 25);
        StrictUtf8.GetString(bytes, 49, (int)length);
        int pos = 49 + (int)length + 21;
        uint count = U32(bytes, pos);
        Require(count <= 10000 && pos + 4L + count * 4L == bytes.Length, "Unsupported entitlement list layout.");
        var ids = new List<uint>();
        for (int i = 0; i < count; i++) ids.Add(U32(bytes, pos + 4 + i * 4));
        Require(ids.Distinct().Count() == ids.Count, "Duplicate applied entitlements; refusing to guess.");
        ulong timestamp = U64(bytes, 20);
        Require(timestamp > 0 && timestamp <= 253402300799UL, "Invalid save date.");
        return new SaveHeader { Path = path, Bytes = bytes, Timestamp = timestamp,
            CountPosition = pos, Entitlements = ids };
    }

    static byte[] PatchHeader(SaveHeader header)
    {
        var remaining = header.Entitlements.Where(id => !EntitlementsToRemove.Contains(id)).ToList();
        int removed = header.Entitlements.Count - remaining.Count;
        if (removed == 0) return (byte[])header.Bytes.Clone();
        byte[] result = new byte[header.Bytes.Length - removed * 4];
        Buffer.BlockCopy(header.Bytes, 0, result, 0, header.CountPosition);
        Put32(result, header.CountPosition, (uint)remaining.Count);
        int pos = header.CountPosition + 4;
        foreach (uint id in remaining)
        { Put32(result, pos, id); pos += 4; }
        Put32(result, 12, Crc32(result));
        return result;
    }

    static int RestoreFacts(SortedDictionary<ulong, byte> facts, IEnumerable<ulong> keys)
    {
        int restored = 0;
        foreach (ulong key in keys)
        {
            byte value;
            if (!facts.TryGetValue(key, out value) || value == 0) restored++;
            facts[key] = 1;
        }
        return restored;
    }

    static byte[] PatchGlobal(byte[] original, out int restoredPreorderFacts, out int restoredExtraFacts, out int restoredSlots)
    {
        Require(U32(original, 16) == 23 && U32(original, 20) == 4,
            "Unsupported global save or world-state version. No files were changed.");
        uint numericCount = U32(original, 24);
        Bounds(original, 28, numericCount * 16L + 4);
        int countPos = 28 + (int)numericCount * 16;
        uint boolCount = U32(original, countPos);
        Bounds(original, countPos + 4, boolCount * 9L);
        int oldEnd = countPos + 4 + (int)boolCount * 9;
        var facts = new SortedDictionary<ulong, byte>();
        ulong previous = 0;
        for (int i = 0; i < boolCount; i++)
        {
            int at = countPos + 4 + i * 9;
            ulong key = U64(original, at);
            byte value = original[at + 8];
            Require(value <= 1 && (i == 0 || key > previous), "Unsupported boolean-fact ordering or value.");
            facts.Add(key, value);
            previous = key;
        }
        restoredPreorderFacts = RestoreFacts(facts, PreorderUnlockFacts);
        restoredExtraFacts = RestoreFacts(facts, ExtraUnlockFacts);

        // Recognize a full container, not just a short byte pattern.
        var containers = new List<int>();
        for (int at = oldEnd; at <= original.Length - 188; at++)
        {
            if (U32(original, at) != 1 || U32(original, at + 4) != 9) continue;
            var ids = new HashSet<uint>();
            bool valid = true;
            for (int j = 0; j < 9; j++)
            {
                int record = at + 8 + j * 20;
                uint id = U32(original, record + 4);
                if (U32(original, record) != 3 || !SlotIds.Contains(id) || !ids.Add(id)) { valid = false; break; }
            }
            if (valid) containers.Add(at);
        }
        Require(containers.Count == 1, "Could not identify one supported outfit container. No files were changed.");
        byte[] edited = (byte[])original.Clone();
        restoredSlots = 0;
        // Only missing PREORDER facts justify restoring the old fallback
        // selections. Adding promotional facts must not change outfits.
        // The new items become available for the user to equip themselves.
        if (restoredPreorderFacts > 0)
        {
            for (int j = 0; j < 9; j++)
            {
                int record = containers[0] + 8 + j * 20;
                uint slot = U32(edited, record + 4), item = U32(edited, record + 8);
                if (U32(edited, record + 16) != 0xFFFFFFFF) continue;
                if (slot == 0x22331ABB && item == 0x947B4812 && U32(edited, record + 12) == 0x933B5BDE)
                { Put32(edited, record + 8, 0xCC5AC851); restoredSlots++; }
                if (slot == 0x24331DE1 && item == 0 && U32(edited, record + 12) == 0xFFFFFFFF)
                { Put32(edited, record + 8, 0x71128EA3); Put32(edited, record + 12, 0x933B5BDE); restoredSlots++; }
            }
        }
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(edited, 0, countPos);
            writer.Write((uint)facts.Count);
            foreach (var fact in facts) { writer.Write(fact.Key); writer.Write(fact.Value); }
            writer.Write(edited, oldEnd, edited.Length - oldEnd);
            writer.Flush();
            byte[] result = stream.ToArray();
            Put32(result, 12, Crc32(result));
            return result;
        }
    }

    static string Hash(byte[] bytes)
    {
        using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
    }

    static string RelativeSavePath(string directory, string path)
    {
        string root = System.IO.Path.GetFullPath(directory).TrimEnd(
            System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar;
        string target = System.IO.Path.GetFullPath(path);
        Require(target.StartsWith(root, StringComparison.OrdinalIgnoreCase), "Save file resolved outside the selected save folder.");
        return target.Substring(root.Length);
    }

    static void WriteNew(string path, byte[] data)
    {
        using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        { stream.Write(data, 0, data.Length); stream.Flush(true); }
    }
    static void CheckGameClosed()
    {
        var processes = Process.GetProcessesByName("CONTROLResonant");
        bool running = processes.Length != 0;
        foreach (var p in processes) p.Dispose();
        Require(!running, "Close CONTROLResonant before patching, then run this program again.");
    }

    static byte[] ReadMetadata(string path, int minimumBytes, int maximumBytes)
    {
        RegularFile(path);
        long size = new FileInfo(path).Length;
        Require(size >= minimumBytes && size <= maximumBytes,
            "Unexpected WGS metadata size: " + System.IO.Path.GetFileName(path));
        byte[] bytes = File.ReadAllBytes(path);
        Require(bytes.Length == size, "WGS metadata changed while reading.");
        return bytes;
    }

    static string ReadWgsString(BinaryReader reader, string field)
    {
        Require(reader.BaseStream.Length - reader.BaseStream.Position >= 4,
            "Truncated WGS " + field + ".");
        uint length = reader.ReadUInt32();
        Require(length <= 4096, "Unexpected WGS " + field + " length.");
        long byteLength = (long)length * 2;
        Require(reader.BaseStream.Length - reader.BaseStream.Position >= byteLength,
            "Truncated WGS " + field + ".");
        byte[] bytes = reader.ReadBytes((int)byteLength);
        return new UnicodeEncoding(false, false, true).GetString(bytes);
    }

    static string ReadWgsGuidFolder(BinaryReader reader, string field)
    {
        Require(reader.BaseStream.Length - reader.BaseStream.Position >= 16,
            "Truncated WGS " + field + ".");
        byte[] bytes = reader.ReadBytes(16);
        return new Guid(bytes).ToString("N").ToUpperInvariant();
    }

    static string WgsEntryName(byte[] metadata, int at)
    {
        Bounds(metadata, at, 128);
        string name = new UnicodeEncoding(false, false, true).GetString(metadata, at, 128).TrimEnd('\0');
        Require(name.Length != 0 && name.IndexOf('\0') < 0, "Invalid WGS blob name.");
        return name;
    }

    static string WgsEntryGuid(byte[] metadata, int at)
    {
        Bounds(metadata, at, 16);
        byte[] bytes = new byte[16];
        Buffer.BlockCopy(metadata, at, bytes, 0, 16);
        return new Guid(bytes).ToString("N").ToUpperInvariant();
    }

    static Dictionary<string, string> ReadWgsContainerFiles(string containerDirectory, string metadataPath)
    {
        byte[] metadata = ReadMetadata(metadataPath, 8, 16 * 1024 * 1024);
        Require(U32(metadata, 0) == 4, "Unsupported WGS container metadata version.");
        uint count = U32(metadata, 4);
        Require(count <= 10000 && 8L + count * 160L == metadata.Length,
            "Unsupported WGS container metadata layout.");
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int i = 0; i < count; i++)
        {
            int at = 8 + i * 160;
            string name = WgsEntryName(metadata, at);
            string firstGuid = WgsEntryGuid(metadata, at + 128);
            string secondGuid = WgsEntryGuid(metadata, at + 144);
            string firstPath = System.IO.Path.Combine(containerDirectory, firstGuid);
            string secondPath = System.IO.Path.Combine(containerDirectory, secondGuid);
            string path = File.Exists(secondPath) ? secondPath : firstPath;
            Require(File.Exists(path), "Missing WGS blob for " + name + ".");
            Require(!files.ContainsKey(name), "Duplicate WGS blob name: " + name);
            files.Add(name, path);
        }
        return files;
    }

    static List<SaveContainer> ReadWgsSaveContainers(string directory)
    {
        string indexPath = System.IO.Path.Combine(directory, "containers.index");
        byte[] index = ReadMetadata(indexPath, 32, 16 * 1024 * 1024);
        var result = new List<SaveContainer>();
        try
        {
            using (var stream = new MemoryStream(index, false))
            using (var reader = new BinaryReader(stream, Encoding.Unicode))
            {
                Require(reader.ReadUInt32() == 14, "Unsupported containers.index version.");
                uint count = reader.ReadUInt32();
                Require(count > 0 && count <= 1024, "Unexpected WGS container count.");
                reader.ReadUInt32();
                string package = ReadWgsString(reader, "package name");
                Require(package.StartsWith("Remedy.CONTROLResonant_", StringComparison.OrdinalIgnoreCase),
                    "containers.index is not a Control Resonant save.");
                reader.ReadUInt64();
                reader.ReadUInt32();
                ReadWgsString(reader, "account identifier");
                reader.ReadUInt64();
                for (int i = 0; i < count; i++)
                {
                    string name = ReadWgsString(reader, "container name");
                    ReadWgsString(reader, "container display name");
                    ReadWgsString(reader, "container cloud id");
                    byte sequence = reader.ReadByte();
                    reader.ReadUInt32();
                    string folder = ReadWgsGuidFolder(reader, "container id");
                    reader.ReadUInt64();
                    reader.ReadUInt64();
                    long sizeOffset = stream.Position;
                    ulong indexedSize = reader.ReadUInt64();
                    string containerDirectory = System.IO.Path.Combine(directory, folder);
                    string metadataPath = System.IO.Path.Combine(containerDirectory,
                        "container." + sequence.ToString(CultureInfo.InvariantCulture));
                    Require(Directory.Exists(containerDirectory), "Missing WGS container directory for " + name + ".");
                    var files = ReadWgsContainerFiles(containerDirectory, metadataPath);
                    long actualSize = files.Values.Sum(path => new FileInfo(path).Length);
                    Require(indexedSize <= long.MaxValue && actualSize == (long)indexedSize,
                        "WGS container size does not match containers.index for " + name + ".");
                    if (files.Keys.Any(file => file.EndsWith("-header", StringComparison.Ordinal)))
                    {
                        result.Add(new SaveContainer { Name = name, Files = files, IndexPath = indexPath,
                            IndexSizeOffset = sizeOffset, IndexedSize = indexedSize });
                    }
                }
                Require(stream.Position == stream.Length, "Unsupported trailing data in containers.index.");
            }
        }
        catch (EndOfStreamException)
        {
            throw new InvalidDataException("containers.index is truncated.");
        }
        Require(result.Count > 0, "No Control Resonant save containers were found in containers.index.");
        return result;
    }

    static List<SaveContainer> DiscoverSaveContainers(string directory, out bool wgs)
    {
        string indexPath = System.IO.Path.Combine(directory, "containers.index");
        if (File.Exists(indexPath))
        {
            wgs = true;
            Console.WriteLine("Save format: Xbox / Microsoft Store WGS");
            return ReadWgsSaveContainers(directory);
        }
        wgs = false;
        Console.WriteLine("Save format: flat files (Steam/compatible)");
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string path in Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly))
        {
            string name = System.IO.Path.GetFileName(path);
            if (!Suffixes.Any(suffix => name.EndsWith("-" + suffix, StringComparison.Ordinal))) continue;
            Require(!files.ContainsKey(name), "Duplicate save filename: " + name);
            files.Add(name, path);
        }
        return new List<SaveContainer> { new SaveContainer { Name = "flat", Files = files } };
    }

    static void Run(bool checkOnly)
    {
#if NET8_0_OR_GREATER
        string directory = AppContext.BaseDirectory;
#else
        string directory = AppDomain.CurrentDomain.BaseDirectory;
#endif
        Console.WriteLine("Save folder: " + directory);
        bool wgs;
        List<SaveContainer> containers = DiscoverSaveContainers(directory, out wgs);
        var candidates = new List<SaveCandidate>();
        foreach (SaveContainer container in containers)
        {
            foreach (var file in container.Files)
            {
                if (!file.Key.EndsWith("-header", StringComparison.Ordinal)) continue;
                candidates.Add(new SaveCandidate { Container = container, Header = ParseHeader(file.Key, Read(file.Value)) });
            }
        }
        Require(candidates.Count > 0, wgs
            ? "No Control Resonant save headers were found in the active WGS containers."
            : "No save files found beside this EXE. Place it with the four matching save files.");
        candidates = candidates.OrderByDescending(candidate => candidate.Header.Timestamp).ToList();
        SaveCandidate latestCandidate = candidates[0];
        SaveHeader latest = latestCandidate.Header;
        Require(candidates.Count(candidate => candidate.Header.Timestamp == latest.Timestamp) == 1,
            "Multiple save sets share the newest date. Keep only the desired flat save set, or resolve the WGS ambiguity before patching.");
        string stem = latest.Path.Substring(0, latest.Path.Length - "header".Length);
        var originals = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var filePaths = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string suffix in Suffixes)
        {
            string name = stem + suffix;
            string path;
            Require(latestCandidate.Container.Files.TryGetValue(name, out path), "Missing matching save file: " + name);
            byte[] data = Read(path);
            Validate(data, name);
            originals.Add(name, data);
            filePaths.Add(name, path);
        }
        Require(originals[latest.Path].SequenceEqual(latest.Bytes), "The save changed during inspection. Close the game and try again.");
        Console.WriteLine("Newest save: " + System.IO.Path.GetFileName(stem).TrimEnd('-'));
        if (wgs) Console.WriteLine("WGS container: " + latestCandidate.Container.Name);
        Console.WriteLine("Saved (UTC): " + new DateTime(1970, 1, 1).AddSeconds(latest.Timestamp).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        int preorderFacts, extraFacts, slots;
        byte[] global = PatchGlobal(originals[stem + "persi-global"], out preorderFacts, out extraFacts, out slots);
        byte[] header = PatchHeader(latest);
        var changes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        // Global first: a process interruption cannot leave a half-written file.
        if (!global.SequenceEqual(originals[stem + "persi-global"])) changes.Add(stem + "persi-global", global);
        if (!header.SequenceEqual(latest.Bytes)) changes.Add(latest.Path, header);
        foreach (var change in changes) Validate(change.Value, change.Key);
        if (wgs && changes.Count != 0)
        {
            long sizeDelta = changes.Sum(change => (long)change.Value.Length - originals[change.Key].Length);
            if (sizeDelta != 0)
            {
                SaveContainer wgsContainer = latestCandidate.Container;
                Require(wgsContainer.IndexSizeOffset >= 0 && wgsContainer.IndexSizeOffset <= int.MaxValue - 8,
                    "Unsupported WGS index size field location.");
                byte[] indexOriginal = ReadMetadata(wgsContainer.IndexPath, 32, 16 * 1024 * 1024);
                int sizeOffset = (int)wgsContainer.IndexSizeOffset;
                Require(U64(indexOriginal, sizeOffset) == wgsContainer.IndexedSize,
                    "containers.index changed during inspection. Close the game and try again.");
                long newSize = checked((long)wgsContainer.IndexedSize + sizeDelta);
                Require(newSize >= 0, "Invalid WGS container size after patching.");
                byte[] indexPatched = (byte[])indexOriginal.Clone();
                // Keep the existing WGS sync state/timestamps and only keep the indexed byte total consistent.
                Buffer.BlockCopy(BitConverter.GetBytes((ulong)newSize), 0, indexPatched, sizeOffset, 8);
                const string indexName = "containers.index";
                originals.Add(indexName, indexOriginal);
                changes.Add(indexName, indexPatched);
                filePaths.Add(indexName, wgsContainer.IndexPath);
            }
        }
        Console.WriteLine("Preorder item flags to restore (of " + PreorderUnlockFacts.Length + "): " + preorderFacts);
        Console.WriteLine("Promotional cosmetic flags to restore (of " + ExtraUnlockFacts.Length + "): " + extraFacts);
        Console.WriteLine("Reverted outfit selections to restore: " + slots);
        Console.WriteLine("Entitlement entries to remove: " + latest.Entitlements.Count(id => EntitlementsToRemove.Contains(id)));
        if (changes.Count == 0) { Console.WriteLine("\nAlready patched. No files changed."); return; }
        if (checkOnly) { Console.WriteLine("\nCheck complete. No files changed. Run without --check to patch."); return; }
        CheckGameClosed();
        string lockPath = System.IO.Path.Combine(directory, "CosmeticSavePatcher.lock");
        try
        {
            using (var exclusive = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                Commit(directory, filePaths, originals, changes);
        }
        finally
        {
            // A concurrent instance may own this lock, so cleanup failure is harmless.
            try { File.Delete(lockPath); } catch (IOException) { }
        }
        Console.WriteLine("\nDONE. Load this save using the original game executable.\n" +
            (wgs ? "For Xbox cloud sync, load the patched save and make a normal in-game save before closing the game.\n" : "") +
            "Other save sets were not changed. You can run this patcher again after saving.");
    }
    static void Commit(string directory, Dictionary<string, string> filePaths,
        Dictionary<string, byte[]> originals, Dictionary<string, byte[]> changes)
    {
        string backup = System.IO.Path.Combine(directory, "CosmeticSaveBackup-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8));
        Directory.CreateDirectory(backup);
        Console.WriteLine("Backup: " + backup);
        var log = new StringBuilder("Cosmetic Save Patcher " + Version + "\r\n\r\nClose the game before restoring files from this backup.\r\nBackup paths mirror the original save-folder layout, including WGS GUID folders.\r\n\r\n");
        foreach (var file in originals)
        {
            string relativePath = RelativeSavePath(directory, filePaths[file.Key]);
            string backupPath = System.IO.Path.Combine(backup, relativePath);
            string backupDirectory = System.IO.Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(backupDirectory)) Directory.CreateDirectory(backupDirectory);
            WriteNew(backupPath, file.Value);
            log.AppendLine(file.Key + " -> " + relativePath + " original SHA256 " + Hash(file.Value));
        }
        foreach (var file in changes) log.AppendLine(file.Key + " patched SHA256  " + Hash(file.Value));
        string logPath = System.IO.Path.Combine(backup, "RESTORE.txt");
        File.WriteAllText(logPath, log.ToString(), Encoding.UTF8);
        var staged = new Dictionary<string, string>(StringComparer.Ordinal);
        var committed = new List<string>();
        try
        {
            foreach (var file in changes)
            {
                string target = filePaths[file.Key];
                string targetDirectory = System.IO.Path.GetDirectoryName(target);
                Require(!string.IsNullOrEmpty(targetDirectory), "Could not resolve save directory for " + file.Key + ".");
                string temp = System.IO.Path.Combine(targetDirectory, ".cosmetic-" + Guid.NewGuid().ToString("N") + ".tmp");
                staged.Add(file.Key, temp);
                WriteNew(temp, file.Value);
            }
            CheckGameClosed();
            foreach (var file in originals)
                Require(Read(filePaths[file.Key]).SequenceEqual(file.Value), "A save changed during patching. No patch was installed; close the game and retry.");
            foreach (var file in changes.OrderBy(file => file.Key == "containers.index" ? 1 : 0))
            {
                File.Replace(staged[file.Key], filePaths[file.Key], null);
                committed.Add(file.Key);
            }
            File.AppendAllText(logPath, "\r\nStatus: completed.\r\n");
        }
        catch (Exception failure)
        {
            bool restored = true;
            foreach (string name in committed.AsEnumerable().Reverse())
            {
                string target = filePaths[name];
                string targetDirectory = System.IO.Path.GetDirectoryName(target);
                string temp = System.IO.Path.Combine(targetDirectory, ".cosmetic-rollback-" + Guid.NewGuid().ToString("N") + ".tmp");
                try { WriteNew(temp, originals[name]); File.Replace(temp, target, null); }
                catch { restored = false; }
                finally { try { if (File.Exists(temp)) File.Delete(temp); } catch (IOException) { } }
            }
            throw new IOException("Could not finish patching: " + failure.Message + "\n" +
                (restored ? "Original save files were retained or restored." : "Restore all four save files from the backup before playing.") +
                "\nBackup: " + backup, failure);
        }
        finally
        {
            foreach (string temp in staged.Values)
                try { if (File.Exists(temp)) File.Delete(temp); } catch (IOException) { }
        }
    }
}
