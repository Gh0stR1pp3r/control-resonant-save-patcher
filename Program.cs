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
[assembly: AssemblyVersion("1.4.0.0")]
[assembly: AssemblyFileVersion("1.4.0.0")]
[assembly: AssemblyInformationalVersion("1.4.0")]

internal static class Program
{
    const string Version = "1.4.0";
    // Xbox WGS storage support contributed by https://github.com/hdfyeg35.
    static readonly StringComparison PathComparison = System.IO.Path.DirectorySeparatorChar == '\\'
        ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    static readonly StringComparer PathComparer = System.IO.Path.DirectorySeparatorChar == '\\'
        ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
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
        public string FileExtension;
        public string IndexPath;
        public string MetadataPath;
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
                    "Close the game first. The newest save must have all four matching files.\n" +
                    "Steam: place the patcher beside the flat save files.\n" +
                    "Epic Games: place it beside the four matching .chunk save files; keep their names.\n" +
                    "Xbox PC: place it beside containers.index in the per-user WGS folder.\n" +
                    "WGS containers are not rebuilt; only mapped blobs and required size metadata change.\n\n" +
                    "--check     Inspect only: do not patch or create backups.\n" +
                    "--no-pause  Exit without waiting for Enter.\n\n" +
                    "Restores 14 item unlock flags and removes up to ten\n" +
                    "associated applied-entitlement entries.\n" +
                    "Supports the supplied game's header version 16 and global version 23.\n" +
                    "Backups include all save sets and preferences in this folder,\n" +
                    "or all indexed WGS containers and metadata, before patching.");
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
        RequireSafeSavePath(directory, path);
        string root = System.IO.Path.GetFullPath(directory).TrimEnd(
            System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar;
        string target = System.IO.Path.GetFullPath(path);
        Require(target.StartsWith(root, PathComparison), "Save file resolved outside the selected save folder.");
        return target.Substring(root.Length);
    }

    static void RequireSafeSavePath(string directory, string path)
    {
        string root = System.IO.Path.GetFullPath(directory).TrimEnd(
            System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar)
            + System.IO.Path.DirectorySeparatorChar;
        string target = System.IO.Path.GetFullPath(path);
        Require(target.StartsWith(root, PathComparison), "Save file resolved outside the selected save folder.");
        // GetFullPath is lexical: inspect every parent under the selected root
        // so a GUID-directory junction cannot redirect blob writes elsewhere.
        string parent = System.IO.Path.GetDirectoryName(target);
        while (true)
        {
            Require(!string.IsNullOrEmpty(parent) && Directory.Exists(parent), "Missing save parent directory.");
            Require((File.GetAttributes(parent) & FileAttributes.ReparsePoint) == 0,
                "Linked save directories are not supported: " + parent);
            string normalized = parent.TrimEnd(System.IO.Path.DirectorySeparatorChar,
                System.IO.Path.AltDirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar;
            if (string.Equals(normalized, root, PathComparison)) break;
            Require(normalized.StartsWith(root, PathComparison), "Save parent resolved outside the selected folder.");
            parent = System.IO.Path.GetDirectoryName(parent);
        }
        RegularFile(target);
    }

    static void RequireMetadataUnchanged(string directory, Dictionary<string, byte[]> snapshots)
    {
        foreach (var snapshot in snapshots)
        {
            RequireSafeSavePath(directory, snapshot.Key);
            Require(ReadMetadata(snapshot.Key, 8, 16 * 1024 * 1024).SequenceEqual(snapshot.Value),
                "WGS metadata changed during inspection. Wait for synchronization to finish and try again.");
        }
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
        Require(!running, "Close Control Resonant before patching, then run this program again.");
    }

    static bool IsFlatBackupFile(string name)
    {
        return Suffixes.Any(suffix => name.EndsWith("-" + suffix, StringComparison.Ordinal))
            || name.EndsWith(".chunk", StringComparison.Ordinal)
            || name.StartsWith("preferences_", StringComparison.Ordinal)
            || name == "steam_autocloud.vdf" || name == "remotecache.vdf";
    }

    static HashSet<string> CollectBackupPaths(string directory, bool wgs,
        Dictionary<string, byte[]> metadataSnapshots)
    {
        var paths = new HashSet<string>(PathComparer);
        foreach (string path in Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly))
            if (IsFlatBackupFile(System.IO.Path.GetFileName(path))) paths.Add(path);
        if (wgs)
        {
            string indexPath = System.IO.Path.Combine(directory, "containers.index");
            paths.Add(indexPath);
            // Discovery snapshots include mappings for every indexed container,
            // even containers without a game header (for example preferences).
            foreach (string metadataPath in metadataSnapshots.Keys)
            {
                RequireSafeSavePath(directory, metadataPath);
                if (string.Equals(metadataPath, indexPath, PathComparison)) continue;
                string containerDirectory = System.IO.Path.GetDirectoryName(metadataPath);
                // WGS blobs and container.N mappings are direct children. Refuse
                // unexpected nested layouts rather than silently omitting data.
                Require(Directory.GetDirectories(containerDirectory).Length == 0,
                    "Unexpected nested WGS directory; could not make a complete backup.");
                foreach (string path in Directory.GetFiles(containerDirectory)) paths.Add(path);
            }
        }
        foreach (string path in paths) RequireSafeSavePath(directory, path);
        return paths;
    }

    static string HashFile(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var sha = SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }

    static string CopyBackupFile(string directory, string source, string destination)
    {
        RequireSafeSavePath(directory, source);
        using (var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            input.CopyTo(output);
            output.Flush(true);
        }
        return HashFile(destination);
    }

    static void RequireBackupUnchanged(string directory, bool wgs,
        Dictionary<string, byte[]> metadataSnapshots, Dictionary<string, string> backupHashes,
        IEnumerable<string> stagedPaths = null)
    {
        RequireMetadataUnchanged(directory, metadataSnapshots);
        var currentPaths = CollectBackupPaths(directory, wgs, metadataSnapshots);
        // WGS staging files are created beside their blobs. Exclude only this
        // operation's exact temp paths from the original-store membership check.
        if (stagedPaths != null) currentPaths.ExceptWith(stagedPaths);
        Require(currentPaths.SetEquals(backupHashes.Keys),
            "The save-file list changed during backup. No patch was installed; close the game and wait for synchronization.");
        foreach (var file in backupHashes)
        {
            RequireSafeSavePath(directory, file.Key);
            Require(HashFile(file.Key) == file.Value,
                "A save or preference file changed during backup. No patch was installed; close the game and retry.");
        }
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

    static Dictionary<string, string> ReadWgsContainerFiles(string directory, string containerDirectory,
        string metadataPath, out byte[] metadata)
    {
        RequireSafeSavePath(directory, metadataPath);
        metadata = ReadMetadata(metadataPath, 8, 16 * 1024 * 1024);
        Require(U32(metadata, 0) == 4, "Unsupported WGS container metadata version.");
        uint count = U32(metadata, 4);
        Require(count <= 10000 && 8L + count * 160L == metadata.Length,
            "Unsupported WGS container metadata layout.");
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        var physicalPaths = new HashSet<string>(PathComparer);
        for (int i = 0; i < count; i++)
        {
            int at = 8 + i * 160;
            string name = WgsEntryName(metadata, at);
            Require(name.IndexOf('/') < 0 && name.IndexOf('\\') < 0 && name.IndexOf(':') < 0
                && name != "." && name != "..", "Unsupported WGS blob name.");
            string firstGuid = WgsEntryGuid(metadata, at + 128);
            string secondGuid = WgsEntryGuid(metadata, at + 144);
            string firstPath = System.IO.Path.Combine(containerDirectory, firstGuid);
            string secondPath = System.IO.Path.Combine(containerDirectory, secondGuid);
            bool firstExists = File.Exists(firstPath), secondExists = File.Exists(secondPath);
            Require(!(firstExists && secondExists && !string.Equals(firstPath, secondPath, PathComparison)),
                "Ambiguous WGS blob references for " + name + ". Both GUID files exist; no files were changed.");
            string path = secondExists ? secondPath : firstPath;
            Require(File.Exists(path), "Missing WGS blob for " + name + ".");
            RequireSafeSavePath(directory, path);
            Require(!files.ContainsKey(name), "Duplicate WGS blob name: " + name);
            Require(physicalPaths.Add(path), "Multiple WGS names reference the same physical blob.");
            files.Add(name, path);
        }
        return files;
    }

    static List<SaveContainer> ReadWgsSaveContainers(string directory, Dictionary<string, byte[]> metadataSnapshots)
    {
        string indexPath = System.IO.Path.Combine(directory, "containers.index");
        RequireSafeSavePath(directory, indexPath);
        byte[] index = ReadMetadata(indexPath, 32, 16 * 1024 * 1024);
        metadataSnapshots.Add(indexPath, index);
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
                    byte[] metadata;
                    var files = ReadWgsContainerFiles(directory, containerDirectory, metadataPath, out metadata);
                    Require(!metadataSnapshots.ContainsKey(metadataPath), "Duplicate WGS container reference.");
                    metadataSnapshots.Add(metadataPath, metadata);
                    long actualSize = files.Values.Sum(path => new FileInfo(path).Length);
                    Require(indexedSize <= long.MaxValue && actualSize == (long)indexedSize,
                        "WGS container size does not match containers.index for " + name + ".");
                    if (files.Keys.Any(file => file.EndsWith("-header", StringComparison.Ordinal)))
                    {
                        result.Add(new SaveContainer { Name = name, Files = files, IndexPath = indexPath, MetadataPath = metadataPath,
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

    static List<SaveContainer> DiscoverSaveContainers(string directory, out bool wgs,
        out Dictionary<string, byte[]> metadataSnapshots)
    {
        metadataSnapshots = new Dictionary<string, byte[]>(PathComparer);
        string indexPath = System.IO.Path.Combine(directory, "containers.index");
        if (File.Exists(indexPath))
        {
            wgs = true;
            Console.WriteLine("Save format: Xbox / Microsoft Store WGS");
            return ReadWgsSaveContainers(directory, metadataSnapshots);
        }
        wgs = false;
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        var chunks = new Dictionary<string, string>(StringComparer.Ordinal);
        var logicalNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (string path in Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly))
        {
            string name = System.IO.Path.GetFileName(path);
            bool chunk = name.EndsWith(".chunk", StringComparison.Ordinal);
            // Normalize only the logical key. Reads, replacements, backups and
            // rollback retain the original physical path, including .chunk.
            if (chunk) name = name.Substring(0, name.Length - ".chunk".Length);
            if (!Suffixes.Any(suffix => name.EndsWith("-" + suffix, StringComparison.Ordinal))) continue;
            Require(logicalNames.Add(name),
                "Ambiguous save filenames for " + name + ". Keep only one copy, with or without .chunk, in this folder.");
            (chunk ? chunks : files).Add(name, path);
        }
        // Keep formats separate so an incomplete .chunk set cannot borrow a
        // matching extensionless file (or vice versa).
        var containers = new List<SaveContainer>();
        if (files.Count > 0) containers.Add(new SaveContainer { Name = "Steam / compatible", Files = files, FileExtension = "" });
        if (chunks.Count > 0) containers.Add(new SaveContainer { Name = "Epic Games (.chunk)", Files = chunks, FileExtension = ".chunk" });
        Console.WriteLine("Save format: " + (chunks.Count == 0 ? "flat files (Steam/compatible)"
            : files.Count == 0 ? "Epic Games (.chunk)" : "flat files and Epic Games (.chunk)"));
        return containers;
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
        Dictionary<string, byte[]> metadataSnapshots;
        List<SaveContainer> containers = DiscoverSaveContainers(directory, out wgs, out metadataSnapshots);
        var candidates = new List<SaveCandidate>();
        foreach (SaveContainer container in containers)
        {
            foreach (var file in container.Files)
            {
                if (!file.Key.EndsWith("-header", StringComparison.Ordinal)) continue;
                RequireSafeSavePath(directory, file.Value);
                candidates.Add(new SaveCandidate { Container = container, Header = ParseHeader(file.Key, Read(file.Value)) });
            }
        }
        Require(candidates.Count > 0, wgs
            ? "No Control Resonant save headers were found in the active WGS containers."
            : "No save files found beside the patcher. Place it with four matching Steam or Epic .chunk save files.");
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
            Require(latestCandidate.Container.Files.TryGetValue(name, out path),
                "Missing matching save file: " + name + (latestCandidate.Container.FileExtension ?? ""));
            RequireSafeSavePath(directory, path);
            byte[] data = Read(path);
            Validate(data, name);
            originals.Add(name, data);
            filePaths.Add(name, path);
        }
        Require(originals[latest.Path].SequenceEqual(latest.Bytes), "The save changed during inspection. Close the game and try again.");
        Console.WriteLine("Newest save: " + System.IO.Path.GetFileName(stem).TrimEnd('-'));
        if (wgs) Console.WriteLine("WGS container: " + latestCandidate.Container.Name);
        else Console.WriteLine("Selected format: " + latestCandidate.Container.Name);
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
            SaveContainer wgsContainer = latestCandidate.Container;
            // Back up the original mapping even for a same-size patch. Never
            // combine offsets from discovery with a later index snapshot.
            const string indexName = "containers.index";
            byte[] indexOriginal = metadataSnapshots[wgsContainer.IndexPath];
            originals.Add(indexName, indexOriginal);
            filePaths.Add(indexName, wgsContainer.IndexPath);
            string metadataName = RelativeSavePath(directory, wgsContainer.MetadataPath);
            originals.Add(metadataName, metadataSnapshots[wgsContainer.MetadataPath]);
            filePaths.Add(metadataName, wgsContainer.MetadataPath);
            long sizeDelta = changes.Sum(change => (long)change.Value.Length - originals[change.Key].Length);
            if (sizeDelta != 0)
            {
                Require(wgsContainer.IndexSizeOffset >= 0 && wgsContainer.IndexSizeOffset <= int.MaxValue - 8,
                    "Unsupported WGS index size field location.");
                int sizeOffset = (int)wgsContainer.IndexSizeOffset;
                Require(U64(indexOriginal, sizeOffset) == wgsContainer.IndexedSize,
                    "containers.index changed during inspection. Close the game and try again.");
                long newSize = checked((long)wgsContainer.IndexedSize + sizeDelta);
                Require(newSize >= 0, "Invalid WGS container size after patching.");
                byte[] indexPatched = (byte[])indexOriginal.Clone();
                // Keep the existing WGS sync state/timestamps and only keep the indexed byte total consistent.
                Buffer.BlockCopy(BitConverter.GetBytes((ulong)newSize), 0, indexPatched, sizeOffset, 8);
                changes.Add(indexName, indexPatched);
            }
        }
        RequireMetadataUnchanged(directory, metadataSnapshots);
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
                Commit(directory, filePaths, originals, changes, metadataSnapshots, wgs);
        }
        finally
        {
            // A concurrent instance may own this lock, so cleanup failure is harmless.
            try { File.Delete(lockPath); } catch (IOException) { }
        }
        Console.WriteLine("\nDONE. Start the game and load this save.\n" +
            (wgs ? "Make a normal in-game save after checking the items. Xbox cloud persistence is not yet confirmed.\n" : "") +
            "Other save sets were not changed. You can run this patcher again after saving.");
    }
    static void Commit(string directory, Dictionary<string, string> filePaths,
        Dictionary<string, byte[]> originals, Dictionary<string, byte[]> changes,
        Dictionary<string, byte[]> metadataSnapshots, bool wgs)
    {
        RequireMetadataUnchanged(directory, metadataSnapshots);
        string backup = System.IO.Path.Combine(directory, "CosmeticSaveBackup-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8));
        Directory.CreateDirectory(backup);
        Console.WriteLine("Backup: " + backup);
        var log = new StringBuilder("Cosmetic Save Patcher " + Version + "\r\n\r\n" +
            "Close the game and let cloud synchronization finish before restoring.\r\n" +
            "Restore all listed files to their original relative paths, including preferences and WGS metadata.\r\n" +
            "This backup covers all recognized save files in this folder and all files in indexed WGS containers.\r\n" +
            "Other account folders, unindexed WGS directories, patcher files and older backups are not included.\r\n" +
            "For recovery after later saves, move the current save data aside first; do not mix it with this snapshot.\r\n" +
            "Do not copy RESTORE.txt into the save folder. Cloud persistence is not guaranteed.\r\n\r\n");
        string logPath = System.IO.Path.Combine(backup, "RESTORE.txt");
        File.WriteAllText(logPath, log.ToString() + "Backup status: INCOMPLETE. Do not restore this backup.\r\n", Encoding.UTF8);
        var staged = new Dictionary<string, string>(StringComparer.Ordinal);
        var committed = new List<string>();
        var backupHashes = new Dictionary<string, string>(PathComparer);
        bool backupComplete = false;
        try
        {
            HashSet<string> backupPaths = CollectBackupPaths(directory, wgs, metadataSnapshots);
            Require(filePaths.Values.All(path => backupPaths.Contains(path)),
                "A selected save file is missing from the backup plan.");
            var expectedHashes = new Dictionary<string, string>(PathComparer);
            foreach (var file in originals) expectedHashes[filePaths[file.Key]] = Hash(file.Value);
            foreach (var file in metadataSnapshots) expectedHashes[file.Key] = Hash(file.Value);
            foreach (string path in backupPaths.OrderBy(path => path, PathComparer))
            {
                string relativePath = RelativeSavePath(directory, path);
                string backupPath = System.IO.Path.Combine(backup, relativePath);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(backupPath));
                string hash = CopyBackupFile(directory, path, backupPath);
                string expected;
                Require(!expectedHashes.TryGetValue(path, out expected) || hash == expected,
                    "A selected save or WGS mapping changed before backup. No patch was installed.");
                backupHashes.Add(path, hash);
                log.AppendLine(relativePath + " original SHA256 " + hash);
            }
            RequireBackupUnchanged(directory, wgs, metadataSnapshots, backupHashes);
            foreach (var file in changes)
                log.AppendLine(RelativeSavePath(directory, filePaths[file.Key]) + " patched SHA256  " + Hash(file.Value));
            File.WriteAllText(logPath, log.ToString() + "\r\nBackup status: COMPLETE. Patch status: pending.\r\n", Encoding.UTF8);
            backupComplete = true;
            Console.WriteLine("Backed up " + backupHashes.Count + " save and metadata files.");
            foreach (var file in changes)
            {
                string target = filePaths[file.Key];
                RequireSafeSavePath(directory, target);
                string targetDirectory = System.IO.Path.GetDirectoryName(target);
                Require(!string.IsNullOrEmpty(targetDirectory), "Could not resolve save directory for " + file.Key + ".");
                string temp = System.IO.Path.Combine(targetDirectory, ".cosmetic-" + Guid.NewGuid().ToString("N") + ".tmp");
                staged.Add(file.Key, temp);
                WriteNew(temp, file.Value);
            }
            CheckGameClosed();
            RequireBackupUnchanged(directory, wgs, metadataSnapshots, backupHashes, staged.Values);
            foreach (var file in originals)
            {
                RequireSafeSavePath(directory, filePaths[file.Key]);
                Require(Read(filePaths[file.Key]).SequenceEqual(file.Value), "A save changed during patching. No patch was installed; close the game and retry.");
            }
            foreach (var file in changes.OrderBy(file => file.Key == "containers.index" ? 2
                : file.Key.EndsWith("-persi-global", StringComparison.Ordinal) ? 0 : 1))
            {
                RequireSafeSavePath(directory, filePaths[file.Key]);
                File.Replace(staged[file.Key], filePaths[file.Key], null);
                committed.Add(file.Key);
            }
            File.AppendAllText(logPath, "\r\nStatus: completed.\r\n");
        }
        catch (Exception failure)
        {
            try { File.AppendAllText(logPath, "\r\nPatch status: failed; " +
                (backupComplete ? "backup completed before patching." : "backup incomplete; do not restore it.") + "\r\n"); }
            catch { } // Logging failure must not prevent rollback.
            bool restored = true;
            foreach (string name in committed.AsEnumerable().Reverse())
            {
                string target = filePaths[name];
                string targetDirectory = System.IO.Path.GetDirectoryName(target);
                string temp = System.IO.Path.Combine(targetDirectory, ".cosmetic-rollback-" + Guid.NewGuid().ToString("N") + ".tmp");
                try { RequireSafeSavePath(directory, target); WriteNew(temp, originals[name]); File.Replace(temp, target, null); }
                catch { restored = false; }
                finally { try { if (File.Exists(temp)) File.Delete(temp); } catch (IOException) { } }
            }
            throw new IOException("Could not finish patching: " + failure.Message + "\n" +
                (restored ? "Original save files were retained or restored." : "Restore every backed-up path, including WGS metadata, before playing.") +
                "\n" + (backupComplete ? "Backup: " : "Incomplete backup (do not restore): ") + backup, failure);
        }
        finally
        {
            foreach (string temp in staged.Values)
                try { if (File.Exists(temp)) File.Delete(temp); } catch (IOException) { }
        }
    }
}
