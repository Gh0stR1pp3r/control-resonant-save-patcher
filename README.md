# Control Resonant Save Patcher

![Control Resonant](https://exputer.com/wp-content/uploads/2026/08/control-resonant.jpg)

A save patcher for restoring the supported preorder and promotional rewards in **CONTROL Resonant** saves.

This `wgs` branch supports both:

- **Flat-file saves** used by Steam and compatible installations.
- **Xbox PC / Microsoft Store WGS saves** stored through Xbox Connected Storage.

The cosmetic/entitlement patching logic is based on upstream **v1.2.0**. The `wgs` branch extends how saves are discovered, backed up, and written; it does not change the intended reward set.

Upstream project: https://github.com/Gh0stR1pp3r/control-resonant-save-patcher

## Experimental branch / AI-assisted development

> [!WARNING]
> The Xbox PC WGS support on this branch is **experimental** and was developed with assistance from **OpenAI ChatGPT**, using the upstream v1.2.0 source plus analysis of real CONTROL Resonant Xbox PC WGS save data supplied for testing.
>
> The upstream maintainer has not reviewed or endorsed these branch-specific changes unless they explicitly state otherwise. Back up your save before testing and use `--check` first.

The original flat-file patching behavior is retained. The new WGS path has been validated against the supplied save structure, but broader testing across additional Xbox accounts and save histories is still needed.

## Supported save types

| Save type | Platform / store | Detection | Where the patcher goes |
| --- | --- | --- | --- |
| **Flat save** | Steam Windows, Steam/Proton, compatible flat-file installs | Finds complete `*-header`, `*-persi-global`, `*-player`, `*-bundle-container` sets | Directly beside the save files |
| **Xbox WGS** | Xbox app / Microsoft Store on Windows | Finds `containers.index` | In the account WGS directory, directly beside `containers.index` |

Detection is automatic. You do not need to select a save type manually.

## Included rewards

The v1.2.0 patch logic restores the following supported rewards:

- Optical Filtering Goggles
- Communications Department Headset
- Sierra Helmet
- Sierra Vest
- Sierra Suit
- MIO Specialist's Robe
- Third Ice Baseball Cap
- Cracked Standard Issue Sunglasses
- Threshold Bureau Coat
- Threshold Bureau Gas Mask
- Threshold Bureau Workwear
- Corrupted
- Exposed
- Pickpocket's Tool charm

The patcher also removes the matching applied-entitlement records used by the supported preorder and promotional rewards, then recalculates the affected save checksums.

## Windows usage

### Steam / flat-file save

1. Close CONTROL Resonant completely.
2. Open the save directory containing files such as:

   ```text
   auto-0-header
   auto-0-persi-global
   auto-0-player
   auto-0-bundle-container
   ```

3. Put `CosmeticSavePatcher.exe` in that same directory.
4. Run `CosmeticSavePatcher.exe --check` first if you want to verify the selected save without editing it.
5. Run `CosmeticSavePatcher.exe` normally to create a backup and patch the newest complete save set.
6. Start the game normally and load the patched save.

### Xbox PC / Microsoft Store WGS save

1. Close CONTROL Resonant completely.
2. Open:

   ```text
   %LOCALAPPDATA%\Packages\Remedy.CONTROLResonant_a0f1gph4eb81p\SystemAppData\wgs\
   ```

3. Open the account directory inside `wgs` that contains `containers.index`.
4. Put `CosmeticSavePatcher.exe` directly beside `containers.index`.
5. Run:

   ```powershell
   .\CosmeticSavePatcher.exe --check
   ```

6. Confirm that the patcher reports Xbox / Microsoft Store WGS storage and identifies the expected newest save.
7. Run the patcher normally.
8. Start CONTROL Resonant and verify that your existing save loads correctly.
9. Trigger a normal in-game save/checkpoint and exit cleanly before relying on Xbox cloud synchronization.

For early testing, keep a full copy of the WGS account directory in addition to the patcher's automatic backup.

## Steam save folder

A normal Windows Steam save is under:

```text
<Steam-folder>\userdata\<user-id>\3669870\remote\
```

Put the Windows patcher directly inside `remote`, beside the actual CONTROL Resonant save files.

The patcher still uses the timestamp stored inside the save header to choose the newest complete save set; it does not rely on the filename number or filesystem modified time.

## Xbox WGS structure

Xbox PC saves do not expose the Remedy filenames directly as normal files. WGS stores them through two metadata layers:

```text
containers.index
    ↓
logical container (for example slot-0)
    ↓
GUID-named container directory
    ↓
container.N
    ↓
logical Remedy filename → GUID blob
    ↓
actual save data
```

A typical account directory looks like:

```text
<account-WGS-folder>\
├── containers.index
├── <GUID>\
│   ├── container.N
│   └── <GUID blob files>
├── <GUID>\
│   └── ...
└── <GUID>\
    └── ...
```

The `wgs` branch reads `containers.index` to locate the active logical container, reads its current `container.N`, and maps the normal Remedy save names to their existing GUID blob files.

It does **not** extract and repack the save and does **not** rebuild the WGS container topology.

## What the WGS patcher changes

For Xbox PC saves, the patcher:

1. Detects WGS from `containers.index`.
2. Resolves the indexed save container instead of guessing from GUID directory names.
3. Reads the active `container.N` metadata.
4. Maps logical save names to the existing GUID blobs.
5. Uses the same newest-save selection and RMDB validation as the flat-file path.
6. Patches only the GUID blobs corresponding to the selected save set.
7. Updates the affected indexed container byte-total only when a blob length changes.

It does **not** regenerate GUID directories, recreate `container.N`, or rebuild the WGS save using an import/repack operation.

## Check without editing

Windows:

```powershell
.\CosmeticSavePatcher.exe --check
```

The check path validates the save, reports the detected storage type and selected save set, and exits without applying the cosmetic patch.

Other useful switches:

```text
--check       Validate and report without editing
--no-pause    Do not wait for Enter before exiting
--help        Show command help
```

## Backups and restore

Before an actual patch, the program creates a timestamped backup directory.

For flat saves, the selected save files are copied into the backup.

For WGS saves, the backup preserves the affected GUID blob paths and also preserves `containers.index` if its indexed byte-total must be updated. A `RESTORE.txt` file records hashes and restore information.

If a test fails:

1. Close CONTROL Resonant.
2. Restore the files from the patcher's backup, or restore your full WGS copy.
3. Only then start the game again.

Do not allow a known-bad WGS test save to synchronize to the Xbox cloud if you can avoid it.

## Linux / Steam Proton

The upstream Linux x64 build uses the flat-file save path. WGS support is Windows-only because Xbox Connected Storage is a Windows/Xbox app save format.

For Steam/Proton, locate the game prefix under:

```text
<SteamLibrary-folder>/steamapps/compatdata/3669870/pfx/
```

Find the directory containing the CONTROL Resonant `*-header`, `*-persi-global`, `*-player`, and `*-bundle-container` files, then place the Linux patcher beside those files.

Example:

```sh
chmod +x CosmeticSavePatcher-linux-x64
./CosmeticSavePatcher-linux-x64
```

The flat-file path is non-recursive: the patcher must be beside the actual save files.

## Choosing the save

A complete Remedy save set consists of four matching logical names:

```text
<prefix>-header
<prefix>-persi-global
<prefix>-player
<prefix>-bundle-container
```

The patcher chooses the newest complete set using the date encoded in the save header. This behavior is shared by flat-file and WGS saves.

On WGS, those logical names are resolved to GUID blobs before the same selection and validation logic runs.

## Compatibility and safety checks

The inherited v1.2.0 save parser validates the expected RMDB/header/global/world-facts layouts and the relevant CRC32 checksums before patching. It stops instead of guessing when the save is incomplete, ambiguous, corrupt, or uses an unsupported layout.

The WGS implementation adds storage resolution around that parser; it does not bypass the existing format validation.

Future CONTROL Resonant updates may change either the game save format or WGS metadata behavior, so keep backups when testing new game versions.

No personal saves, account credentials, or game files are distributed here.

## Build from source

Use the normal upstream Windows build process when this branch is applied to the full repository:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

The development test package also includes `Build-Test.cmd` / `Build-Test.ps1`, which compile `Program.cs` with an installed Windows .NET Framework C# compiler and produce:

```text
CosmeticSavePatcher-WGS-Test.exe
```

## Branch status

- Base patch logic: upstream v1.2.0
- Branch: `wgs`
- Flat-file / Steam support: retained
- Xbox PC WGS detection through `containers.index`: implemented
- Indexed container lookup: implemented
- `container.N` logical-name → GUID-blob resolution: implemented
- In-place WGS blob patching: implemented
- WGS rebuild/repack: intentionally not used
- Broad multi-user WGS testing: pending

This branch should be treated as experimental until the WGS implementation has received wider testing and independent review.
