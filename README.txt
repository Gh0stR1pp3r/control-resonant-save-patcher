COSMETIC SAVE PATCHER

1. Close the game.
2. Put CosmeticSavePatcher.exe in the folder containing your save files.
3. Double-click it. It checks the saves, creates a backup, then patches
   the newest save set. Read the result and press Enter to close.
4. Start the game with the original executable and load that save.

The EXE is standalone: copy only CosmeticSavePatcher.exe to the save folder.
No Python, installer, internet connection or administrator access is needed.
It uses the .NET Framework provided with Windows 10/11.

Which files?
The save names do not need to match the uploaded examples. The four files
must have the same prefix and end in:
  -header
  -persi-global
  -player
  -bundle-container
Keep all four together. The program uses the date inside each header,
not the filename number or Windows modification date. It patches only
the newest set in the same folder as the EXE, even when multiple save
slots are present. It does not search subfolders. For a particular save,
put just its four files and the EXE into a separate folder.

What does it change?
It removes entitlement ID 5C2B95A3 from the selected header, restores the
six known cosmetic world flags, and updates both CRC32 checksums as needed.
If flags were lost and the two known fallback outfit selections are found,
it restores those selections too. Other outfit choices are preserved.
Preferences, other save sets, and the game executable are not edited.
Running it again on an already patched save makes no further changes.

Backups
Before editing, it copies all four files into a new CosmeticSaveBackup-...
folder beside the EXE. To undo: close the game and copy those four backup
files back into the save folder, replacing the patched copies.
RESTORE.txt in each backup lists the original and patched file hashes.

Checking without changing anything
From a terminal run:
  CosmeticSavePatcher.exe --check
For terminal automation, add --no-pause. Use --help for a short summary.

Supported saves
Built for the supplied CONTROLResonant save format: RMDB 2/2, header 16,
global 23, world facts 4, and the known nine-slot outfit container.
It stops on bad checksums, unsupported layouts, incomplete newest sets,
or tied newest timestamps. It does not guess new layouts after an update.
Unknown header files ending in -header also cause it to stop.
The underlying save recovery was confirmed working by the user; this
standalone implementation has been compiled but has not been run or
tested in-game. A later run with the patched game EXE may record the
entitlement again; rerun this tool before returning to the original EXE.

Source and building
Program.cs and build.ps1 are included beside this README for transparency.
They are not needed to run the EXE. Build with:
  powershell -ExecutionPolicy Bypass -File build.ps1
