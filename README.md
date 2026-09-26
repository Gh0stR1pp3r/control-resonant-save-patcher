# Control Resonant Save Patcher

A Windows utility for restoring the six known preorder cosmetic flags in a supported CONTROLResonant save and removing the applied-entitlement entry associated with their revocation.

**[Download CosmeticSavePatcher.exe](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/releases/download/v1.0.0/CosmeticSavePatcher.exe)**

## How to use

1. Close the game.
2. Put `CosmeticSavePatcher.exe` in the folder containing your save files.
3. Double-click it. It checks the files, creates a backup, and patches the newest save set.
4. Read the result, press Enter to close, then load that save with the original game executable.

Only the EXE is needed. No Python, installer, internet connection, or administrator access is required. It uses the .NET Framework included with Windows 10/11.

## Choosing the save

The patcher uses the date inside the save, not its filename number or Windows modification date. Names can differ from the original examples, but each set must contain four files with the same prefix and these endings:

```text
-header
-persi-global
-player
-bundle-container
```

Keep all four together. Only the newest set beside the EXE is patched, even if several save slots are present. Subfolders are not searched. To patch a particular save, put just its four files and the EXE in a separate folder.

## Backups and undo

Before editing, the program copies all four files into a new `CosmeticSaveBackup-...` folder. To undo, close the game and copy those four backup files back into the save folder. Each backup includes `RESTORE.txt` with file hashes.

## What changes

- Removes entitlement ID `0x5C2B95A3` from the selected header.
- Restores six known cosmetic world-state flags.
- Restores the two observed fallback outfit selections when the flags were lost and those specific fallback values are present.
- Recalculates the affected CRC32 checksums.

Other outfit choices, preferences, other save sets, and the game executable are preserved. An already patched save requires no further changes.

## Check without editing

```powershell
.\CosmeticSavePatcher.exe --check
```

Use `--no-pause` for terminal automation or `--help` for a short summary.

## Compatibility and status

Supports the analyzed save layout: RMDB 2/2, header version 16, global version 23, world facts version 4, and the known nine-slot outfit container. The program stops on invalid checksums, unsupported layouts, incomplete newest sets, ambiguous newest dates, or unrecognized files ending in `-header`.

**Tested and confirmed working on the current game version**, as reported by the repository owner on September 26, 2026. Future game updates may change the format or ownership behavior.

Running the game with a modified executable that grants the entitlement again may record it as applied again. Rerun this tool before returning to the original executable in that case.

No game files, personal saves, or account credentials are distributed here.

## Build from source

On Windows with the .NET Framework C# compiler installed:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

This produces `CosmeticSavePatcher.exe` beside `Program.cs`. No external packages are used.
