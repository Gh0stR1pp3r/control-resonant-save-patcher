COSMETIC SAVE PATCHER 1.2.0

Restores 13 cosmetics and the Pickpocket's Tool charm. Adds six promotional
cosmetics to the eight item flags supported by 1.1.0. Windows v1.2.0 was
tested and confirmed working on the current game version by the repository
owner on September 27, 2026. A Linux user reported successful execution (issue #2).

INCLUDED ITEMS

Optical Filtering Goggles - Mailing promotion 2
  0x2E27C13F1B8BE2AD
Communications Department Headset - Mailing promotion 1
  0x46780E853CF1A90B
Sierra Helmet - Twitch drop 3
  0xB961238CCE640F14
Sierra Vest - Twitch drop 2
  0xBF6BE35893F7D586
Sierra Suit - Twitch drop 1
  0xEF6FBC7BA02AB697
MIO Specialist's Robe - China promotion
  0xF37DC383C5C5E132
Third Ice Baseball Cap - NVIDIA promotion
  0x5388457983503C2E
Cracked Standard Issue Sunglasses - Beta tester reward
  0x747AFDEFDBABB85B
Threshold Bureau Coat - PS5 preorder
  0x2859E4A7F3D0F7FB
Threshold Bureau Gas Mask - PS5 preorder
  0x57C9EA469E42C97B
Threshold Bureau Workwear - PS5 preorder
  0xA70D57F494331711
Corrupted - PC / PS5 preorder
  0x8F3741DBFF2CB8EB
Pickpocket's Tool (charm) - PC / PS5 preorder
  0x09FC268427CD1CD6
Exposed (shirtless outfit) - Also granted by the PC / PS5 preorder entitlement
  0x5F7C4166260F3CFD

Reward sources come from the installed game data. Corrupted is the preorder
body appearance. Exposed is a separate shirtless-base flag granted by the
same entitlement; it is not identified here as a separately advertised
bonus. Pickpocket's Tool is a charm.

WINDOWS

1. Close the game.
2. Open the Steam save folder described below and put
   CosmeticSavePatcher.exe inside it.
3. Double-click it. It checks the saves, creates a backup, then patches
   the newest save set. Read the result and press Enter to close.
4. Start the game with the original executable and load that save.

The EXE is standalone: copy only CosmeticSavePatcher.exe to the save folder.
No Python, installer, internet connection or administrator access is needed.
It uses the .NET Framework provided with Windows 10/11.

LINUX X64
1. Close the game completely.
2. Open the Linux save location below and find the actual save files.
   Download CosmeticSavePatcher-linux-x64 and put it beside those files.
3. Open a terminal in that folder and run:
     chmod +x CosmeticSavePatcher-linux-x64
     ./CosmeticSavePatcher-linux-x64
4. Read the result and press Enter to close. Start the game normally and
   load the patched save.

Only that one file is needed. It includes .NET 8.0.31; no separate .NET
installation or Wine is needed. It targets x64 Linux with glibc, not ARM
or Alpine/musl. Its runtime extracts native libraries to the user's .net
cache. Saves are read from beside the patcher. A Linux user reported
successful execution in issue #2. The report does not specify a Linux
distribution or confirm Steam Deck compatibility or in-game results.

Steam save folder
Windows:
<Steam-folder>\userdata\<user-id>\3669870\remote\

<Steam-folder> means Steam's folder containing userdata, usually:
C:\Program Files (x86)\Steam
<user-id> means your Steam account's numbered folder inside userdata.

Open your Steam folder, then userdata, your numbered account folder,
3669870, and finally remote. Put CosmeticSavePatcher.exe inside remote,
beside the save files.

Linux (Steam/Proton):
<Steam-folder>/userdata/<user-id>/3669870/remote/

<Steam-folder> means the Steam client folder containing userdata. It may
be different from the Steam library where the game is installed.
<user-id> means your Steam account's numbered folder inside userdata.
Open your Steam folder, then userdata, your numbered account folder,
3669870, and remote. Put CosmeticSavePatcher-linux-x64 inside remote,
beside the files ending in -header, -persi-global, -player, and
-bundle-container. The patcher does not search subfolders.
A Linux user reported this location and successful execution in issue #2:
https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/2

The names in angle brackets are placeholders; do not type them literally.

Which files?
The save names do not need to match the uploaded examples. The four files
must have the same prefix and end in:
  -header
  -persi-global
  -player
  -bundle-container
Keep all four together. The program uses the date inside each header,
not the filename number or filesystem modification date. It patches only
the newest set in the same folder as the patcher, even when multiple save
slots are present. It does not search subfolders. For a particular save,
put just its four files and the patcher into a separate folder. After
patching that copy, copy all four save files back into the original save
folder before starting the game.

What does it change?
It restores the 14 item flags listed above, including missing entries,
and removes matching applied-entitlement markers for PC preorder, PS5
preorder, NVIDIA, beta testers, both mailing promotions, all three Twitch
drops, and the China promotion. It updates CRC32 checksums as needed.
If preorder flags were lost and the two known fallback outfit selections
are found, it restores those selections too. Adding promotional flags
alone does not change your equipped outfit. Equip the items yourself
from the cosmetic menu. Other outfit choices are preserved.
Preferences, other save sets, and the game executable are not edited.
Running it again on an already patched save makes no further changes.

Backups
Before editing, it copies all four files into a new CosmeticSaveBackup-...
folder beside the patcher. To undo: close the game and copy those four backup
files back into the save folder, replacing the patched copies.
RESTORE.txt in each backup lists the original and patched file hashes.

Checking without changing anything
Windows PowerShell:
  .\CosmeticSavePatcher.exe --check
Linux (after chmod +x):
  ./CosmeticSavePatcher-linux-x64 --check
For terminal automation, add --no-pause. Use --help for a short summary.

Supported saves
Built for the supplied CONTROLResonant save format: RMDB 2/2, header 16,
global 23, world facts 4, and the known nine-slot outfit container.
It stops on bad checksums, unsupported layouts, incomplete newest sets,
or tied newest timestamps. It does not guess new layouts after an update.
Unknown header files ending in -header also cause it to stop.
The Windows 1.2.0 patcher was tested and confirmed working on the current
game version by the repository owner on September 27, 2026, including the
six added promotional cosmetics. The analyzed game executable version is
0.563.737.9. Item names and entitlement IDs come from the installed databases.
Future game updates may change compatibility. A later run with a patched
game EXE may record entitlement entries again; rerun this tool before
returning to the original EXE.

Upgrading from 1.0.0 or 1.1.0
Replace the old patcher with this version for your OS and run beside saves.
Previously supported unlocks are retained. If all 14 flags are already
enabled and the ten associated entitlement entries are absent, the
program reports Already patched. Load with the original game EXE and
equip items from the cosmetic menu.

Source and building
Source files are available in the repository, and not needed to run it.
Windows build:
  powershell -ExecutionPolicy Bypass -File build.ps1
Linux x64 build (with .NET 8 SDK or later; may also build on Windows):
  dotnet publish CosmeticSavePatcher.Linux.csproj -c Release -o publish/linux-x64
Output: publish/linux-x64/CosmeticSavePatcher-linux-x64
The build downloads official .NET runtime packages; no third-party
application dependencies are used. Runtime version is pinned to 8.0.31.
Use source files from the version you want to build. The original
v1.1.0 automatic source archive predates the Linux project.
