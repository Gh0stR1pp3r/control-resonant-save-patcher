COSMETIC SAVE PATCHER 1.1.0

Adds the two newly identified flags for the baseball cap and sunglasses.
Tested and confirmed working by the repository owner on September 27, 2026.

1. Close the game.
2. Open the Steam save folder described below and put
   CosmeticSavePatcher.exe inside it.
3. Double-click it. It checks the saves, creates a backup, then patches
   the newest save set. Read the result and press Enter to close.
4. Start the game with the original executable and load that save.

The EXE is standalone: copy only CosmeticSavePatcher.exe to the save folder.
No Python, installer, internet connection or administrator access is needed.
It uses the .NET Framework provided with Windows 10/11.

Steam save folder
<Steam-folder>\userdata\<user-id>\3669870\remote\

<Steam-folder> means the folder where Steam is installed, usually:
C:\Program Files (x86)\Steam
<user-id> means your Steam account's numbered folder inside userdata.

Open your Steam folder, then userdata, your numbered account folder,
3669870, and finally remote. Put CosmeticSavePatcher.exe inside remote,
beside the save files. The names in angle brackets are placeholders;
do not type them literally.

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
put just its four files and the EXE into a separate folder. After
patching that copy, copy all four save files back into your Steam remote
folder before starting the game.

What does it change?
It removes the matching entitlement entries 5C2B95A3, 897D368C and 7BD90E22
from the selected header, if present. It restores all eight known cosmetic
world flags (six preorder flags plus the two additional flags associated
with the cap and sunglasses), and updates CRC32 checksums as needed.
It also works on supported saves where the new flags were never present.
If preorder flags were lost and the two known fallback outfit selections
are found, it restores those selections too. Adding just the new cap and
sunglasses flags does not change your equipped outfit. Equip them yourself
from the cosmetic menu after patching. Other outfit choices are preserved.
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
The combined patcher for preorder cosmetics, the additional cap and
sunglasses was tested and confirmed working on the current game version
by the repository owner on September 27, 2026. Future game updates
may change compatibility. A later run with a patched game EXE may record
the entitlement entries again; rerun this tool before returning to the
original EXE. Exact flag-to-item and category mappings remain unidentified.

Upgrading from 1.0.0
Replace the old patcher EXE with this version and run it beside your saves.
If the two extra flags are missing, it reports two cap/sunglasses flags
to restore. Previously restored preorder flags are retained. If all eight
flags are already enabled and the three entitlement entries are absent,
"Already patched" is expected. Load the patched save with the original
game EXE and equip the new items from the cosmetic menu.

Source and building
Program.cs and build.ps1 are included beside this README for transparency.
They are not needed to run the EXE. Build with:
  powershell -ExecutionPolicy Bypass -File build.ps1
