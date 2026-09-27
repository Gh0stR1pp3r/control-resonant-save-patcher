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
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]
[assembly: AssemblyInformationalVersion("1.1.0")]

internal static class Program
{
    const string Version = "1.1.0";
    static readonly uint[] EntitlementsToRemove = { 0x5C2B95A3, 0x897D368C, 0x7BD90E22 };
    const int MaxFileBytes = 64 * 1024 * 1024;
    static readonly ulong[] PreorderUnlockFacts = {
        0x09FC268427CD1CD6UL, 0x2859E4A7F3D0F7FBUL, 0x57C9EA469E42C97BUL,
        0x5F7C4166260F3CFDUL, 0x8F3741DBFF2CB8EBUL, 0xA70D57F494331711UL
    };
    // Both facts appeared with the cap and sunglasses. Their individual
    // item/category mappings remain unknown; restore the confirmed pair.
    static readonly ulong[] ExtraUnlockFacts = { 0x5388457983503C2EUL, 0x747AFDEFDBABB85BUL };
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

    static int Main(string[] args)
    {
        bool noPause = args.Contains("--no-pause");
        int status = 0;
        try
        {
            Console.WriteLine("COSMETIC SAVE PATCHER " + Version + "\n");
            Console.WriteLine("Preorder cosmetics + the additional cap and sunglasses.\n");
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
                    "Names may vary; matching files must end in -header, -persi-global,\n" +
                    "-player and -bundle-container. Subfolders are not scanned.\n\n" +
                    "--check     Inspect only: do not patch or create backups.\n" +
                    "--no-pause  Exit without waiting for Enter.\n\n" +
                    "Restores eight known cosmetic flags and removes up to three\n" +
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
        // selections. Adding the two extra facts must not change outfits.
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

    static void Run(bool checkOnly)
    {
#if NET8_0_OR_GREATER
        string directory = AppContext.BaseDirectory;
#else
        string directory = AppDomain.CurrentDomain.BaseDirectory;
#endif
        Console.WriteLine("Save folder: " + directory);
        string[] paths = Directory.GetFiles(directory, "*-header", SearchOption.TopDirectoryOnly);
        Require(paths.Length > 0, "No save files found beside this EXE. Place it with the four matching save files.");
        var headers = paths.Select(p => ParseHeader(p, Read(p))).OrderByDescending(h => h.Timestamp).ToList();
        SaveHeader latest = headers[0];
        Require(headers.Count(h => h.Timestamp == latest.Timestamp) == 1,
            "Multiple save sets share the newest date. Place just the desired set with this EXE in a separate folder.");
        string stem = latest.Path.Substring(0, latest.Path.Length - "header".Length);
        var originals = new Dictionary<string, byte[]>();
        foreach (string suffix in Suffixes)
        {
            string path = stem + suffix;
            byte[] data = Read(path);
            Validate(data, System.IO.Path.GetFileName(path));
            originals.Add(path, data);
        }
        Require(originals[latest.Path].SequenceEqual(latest.Bytes), "The save changed during inspection. Close the game and try again.");
        Console.WriteLine("Newest save: " + System.IO.Path.GetFileName(stem).TrimEnd('-'));
        Console.WriteLine("Saved (UTC): " + new DateTime(1970, 1, 1).AddSeconds(latest.Timestamp).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        int preorderFacts, extraFacts, slots;
        byte[] global = PatchGlobal(originals[stem + "persi-global"], out preorderFacts, out extraFacts, out slots);
        byte[] header = PatchHeader(latest);
        var changes = new Dictionary<string, byte[]>();
        // Global first: a process interruption cannot leave a half-written file.
        if (!global.SequenceEqual(originals[stem + "persi-global"])) changes.Add(stem + "persi-global", global);
        if (!header.SequenceEqual(latest.Bytes)) changes.Add(latest.Path, header);
        foreach (var change in changes) Validate(change.Value, System.IO.Path.GetFileName(change.Key));
        Console.WriteLine("Preorder unlock flags to restore: " + preorderFacts);
        Console.WriteLine("Cap/sunglasses unlock flags to restore: " + extraFacts);
        Console.WriteLine("Reverted outfit selections to restore: " + slots);
        Console.WriteLine("Entitlement entries to remove: " + latest.Entitlements.Count(id => EntitlementsToRemove.Contains(id)));
        if (changes.Count == 0) { Console.WriteLine("\nAlready patched. No files changed."); return; }
        if (checkOnly) { Console.WriteLine("\nCheck complete. No files changed. Run without --check to patch."); return; }
        CheckGameClosed();
        string lockPath = System.IO.Path.Combine(directory, "CosmeticSavePatcher.lock");
        try
        {
            using (var exclusive = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                Commit(directory, originals, changes);
        }
        finally
        {
            // A concurrent instance may own this lock, so cleanup failure is harmless.
            try { File.Delete(lockPath); } catch (IOException) { }
        }
        Console.WriteLine("\nDONE. Load this save using the original game executable.\n" +
            "Other save sets were not changed. You can run this patcher again after saving.");
    }

    static void Commit(string directory, Dictionary<string, byte[]> originals, Dictionary<string, byte[]> changes)
    {
        string backup = System.IO.Path.Combine(directory, "CosmeticSaveBackup-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8));
        Directory.CreateDirectory(backup);
        Console.WriteLine("Backup: " + backup);
        var log = new StringBuilder("Cosmetic Save Patcher " + Version + "\r\n\r\nClose the game, then copy the four save files from this backup folder\r\nback into the parent folder to undo this patch.\r\n\r\n");
        foreach (var file in originals)
        {
            WriteNew(System.IO.Path.Combine(backup, System.IO.Path.GetFileName(file.Key)), file.Value);
            log.AppendLine(System.IO.Path.GetFileName(file.Key) + " original SHA256 " + Hash(file.Value));
        }
        foreach (var file in changes) log.AppendLine(System.IO.Path.GetFileName(file.Key) + " patched SHA256  " + Hash(file.Value));
        string logPath = System.IO.Path.Combine(backup, "RESTORE.txt");
        File.WriteAllText(logPath, log.ToString(), Encoding.UTF8);
        var staged = new Dictionary<string, string>();
        var committed = new List<string>();
        try
        {
            foreach (var file in changes)
            {
                string temp = System.IO.Path.Combine(directory, ".cosmetic-" + Guid.NewGuid().ToString("N") + ".tmp");
                staged.Add(file.Key, temp);
                WriteNew(temp, file.Value);
            }
            CheckGameClosed();
            foreach (var file in originals)
                Require(Read(file.Key).SequenceEqual(file.Value), "A save changed during patching. No patch was installed; close the game and retry.");
            foreach (var file in changes)
            {
                File.Replace(staged[file.Key], file.Key, null);
                committed.Add(file.Key);
            }
            File.AppendAllText(logPath, "\r\nStatus: completed.\r\n");
        }
        catch (Exception failure)
        {
            bool restored = true;
            foreach (string path in committed.AsEnumerable().Reverse())
            {
                string temp = System.IO.Path.Combine(directory, ".cosmetic-rollback-" + Guid.NewGuid().ToString("N") + ".tmp");
                try { WriteNew(temp, originals[path]); File.Replace(temp, path, null); }
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
