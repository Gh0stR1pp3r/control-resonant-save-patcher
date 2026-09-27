# Control Resonant Save Patcher

A Windows utility for restoring preorder cosmetics **plus the additional baseball cap and sunglasses** in supported CONTROLResonant saves. Version **1.1.0** restores eight known cosmetic flags and removes the three associated applied-entitlement entries to avoid the observed revocation with the original game executable.

**[Download the latest CosmeticSavePatcher.exe](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/releases/latest/download/CosmeticSavePatcher.exe)**

## New in 1.1.0

- Adds support for the additional baseball cap and sunglasses, including saves where their unlock flags were never present.
- Preserves existing preorder unlocks when upgrading from 1.0.0.
- Makes the new items available without changing your equipped outfit solely because their flags were added. Equip them from the cosmetic menu.

To upgrade, replace the old patcher EXE with this version and run it beside your saves. No experimental game EXE is needed.

## How to use

1. Close the game.
2. Open your [Steam save folder](#steam-save-folder) and put `CosmeticSavePatcher.exe` inside it.
3. Double-click it. It checks the files, creates a backup, and patches the newest save set.
4. Read the result, press Enter to close, then load that save with the original game executable.

Only the EXE is needed. No Python, installer, internet connection, or administrator access is required. It uses the .NET Framework included with Windows 10/11.

## Steam save folder

```text
<Steam-folder>\userdata\<user-id>\3669870\remote\
```

- [`<Steam-folder>`](https://www.pcgamingwiki.com/wiki/Glossary:Game_data#Steam_client) is the folder where Steam is installed, usually `C:\Program Files (x86)\Steam`.
- [`<user-id>`](https://www.pcgamingwiki.com/wiki/Glossary:Game_data#User_ID) is your Steam account's numbered folder inside `userdata`.

Open your Steam folder, then `userdata`, your numbered account folder, `3669870`, and finally `remote`. Put `CosmeticSavePatcher.exe` inside that `remote` folder, beside the save files. The angle-bracket names above are placeholders, not text to type literally.

## Choosing the save

The patcher uses the date inside the save, not its filename number or Windows modification date. Names can differ from the original examples, but each set must contain four files with the same prefix and these endings:

```text
-header
-persi-global
-player
-bundle-container
```

Keep all four together. Only the newest set beside the EXE is patched, even if several save slots are present. Subfolders are not searched. To patch a particular save, put just its four files and the EXE in a separate folder. After patching that copy, copy all four save files back into your Steam `remote` folder before starting the game.

## Backups and undo

Before editing, the program copies all four files into a new `CosmeticSaveBackup-...` folder. To undo, close the game and copy those four backup files back into the save folder. Each backup includes `RESTORE.txt` with file hashes.

## What changes

- Removes any matching applied-entitlement entries `0x5C2B95A3`, `0x897D368C`, and `0x7BD90E22` from the selected header.
- Restores eight known cosmetic world-state flags: six preorder flags and two additional flags associated with the cap and sunglasses.
- Restores the two observed fallback outfit selections only when preorder flags were lost and those specific fallback values are present. Adding just the cap and sunglasses flags does not change your equipped outfit.
- Recalculates the affected CRC32 checksums.

Other outfit choices, preferences, other save sets, and the game executable are preserved. An already patched save requires no further changes.

## Check without editing

```powershell
.\CosmeticSavePatcher.exe --check
```

Use `--no-pause` for terminal automation or `--help` for a short summary.

## Compatibility and status

Supports the analyzed save layout: RMDB 2/2, header version 16, global version 23, world facts version 4, and the known nine-slot outfit container. The program stops on invalid checksums, unsupported layouts, incomplete newest sets, ambiguous newest dates, or unrecognized files ending in `-header`.

**Version 1.1.0's combined support for preorder cosmetics, the cap, and sunglasses was tested and confirmed working on the current game version**, as reported by the repository owner on September 27, 2026. The analyzed executable version is `0.563.737.9`. Future game updates may change the format or ownership behavior.

The new flags were identified together in a controlled save comparison. Individual flag-to-item and entitlement-category mappings remain unidentified.

Running the game with a modified executable that grants the entitlement again may record it as applied again. Rerun this tool before returning to the original executable in that case.

No game files, personal saves, or account credentials are distributed here.

## Build from source

On Windows with the .NET Framework C# compiler installed:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

This produces `CosmeticSavePatcher.exe` beside `Program.cs`. No external packages are used.
