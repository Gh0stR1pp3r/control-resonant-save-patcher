CONTROL RESONANT SAVE PATCHER 1.4.1

Restores 14 cosmetics and the Pickpocket's Tool charm. Version 1.4.1 adds
Deathadder Jacket (Razer), flag 0x3EE727291BFE7B2B, and cleanup of its
applied-entitlement item ID 0x48A1D491. The repository owner tested the
Windows build and confirmed it works as expected on October 1, 2026,
using game file version 0.564.208.5. Linux v1.4.1 compiled successfully;
its execution, Epic/WGS in-game behavior and cloud persistence remain
unconfirmed. See Known issues below for the reported map/autosave
stutter workaround. Reported launch failures remain unresolved.

It retains whole-save backups and Steam, Epic and Xbox WGS support.
The Windows EXE has full product/company metadata and remains unsigned.

Previous release history: Version 1.4.0 adds
Epic Games .chunk saves, retaining Steam and Xbox WGS file support. Windows v1.2.0 was
tested and confirmed working on the then-current game version by the repository
owner on September 27, 2026. A Linux user reported successful execution (issue #2).

Version 1.4.0 supports Steam, Epic .chunk and Xbox WGS saves. Both Windows
and Linux builds compiled. A user-run Windows/Steam result was inspected:
header cleanup succeeded and all 70 original files were backed up exactly.
All 14 supported flags were already enabled; this was not a fresh unlock
test. Epic/WGS in-game behavior, cloud persistence and Linux v1.4.0 execution
remain unconfirmed. Earlier platform confirmations apply to v1.2.0.
Xbox WGS file support contributed by hdfyeg35: https://github.com/hdfyeg35

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
Deathadder Jacket - Razer entitlement
  0x3EE727291BFE7B2B
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
4. Start the game and load that save.

The EXE is standalone: copy only CosmeticSavePatcher.exe to the save folder.
No Python, installer, internet connection or administrator access is needed.
It uses the .NET Framework provided with Windows 10/11.

WINDOWS: XBOX APP / MICROSOFT STORE WGS

1. Close the game and wait for synchronization to finish. Keep a full copy
   of your WGS account folder before first use.
2. Under %LOCALAPPDATA%\Packages\ find the game package folder beginning
   with Remedy.CONTROLResonant_, then open SystemAppData\wgs\ and the
   account folder containing containers.index.
3. Put CosmeticSavePatcher.exe beside containers.index.
4. Run .\CosmeticSavePatcher.exe --check, review the chosen save, then run
   normally to patch it. The newest header must have all four files.
5. Check the items in game and make a normal save. In-game behavior and
   cloud persistence are not yet confirmed for this integration.

The patcher follows indexed GUID paths. It checks the complete index and
mapping snapshots before writing, and rejects linked parent directories,
duplicate mappings, and entries where both different GUID files exist.
It updates only the selected indexed byte total when blob sizes change.
WGS index version 14 and container metadata version 4 are supported.

EPIC GAMES: .chunk SAVES

Windows save location reported by a user:
%LOCALAPPDATA%\Remedy\CONTROLResonant\<account-id>\<slot-folder>\

Expanded form:
C:\Users\<Windows-user>\AppData\Local\Remedy\CONTROLResonant\<account-id>\<slot-folder>\

%LOCALAPPDATA% means your Windows user's local AppData folder.
<Windows-user> is your Windows profile folder name.
<account-id> is the long generated folder name inside CONTROLResonant;
it differs between accounts. <slot-folder> is the save-slot folder;
the reported example was slot-1. Use the slot containing your actual saves.
Do not type the angle-bracket placeholders literally.

Press Win+R, enter %LOCALAPPDATA%\Remedy\CONTROLResonant, then open
the account folder and slot-1 (or the appropriate slot folder). Put
the patcher inside that slot folder, beside the actual .chunk files.
The patcher does not search account folders or subfolders.

1. Close the game and let cloud synchronization finish.
2. Find the folder containing matching -header.chunk, -persi-global.chunk,
   -player.chunk and -bundle-container.chunk files.
3. Put this patcher beside those files and run it. On Linux use the terminal
   commands below. Keep the .chunk filenames and --containerDisplayName.chunk.
4. Read the result, then start the game and load the save.

The patcher backs up all .chunk files under their original names, including
--containerDisplayName.chunk. To undo, follow the Backups section below. Duplicate
names with and without .chunk are rejected. A save set must have all four
files in the same format; an incomplete newest set stops processing.
Epic in-game behavior and cloud persistence remain unconfirmed.

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
Epic appends .chunk to each ending. Keep original filenames and use the
same format for all four files. Keep all four together. The program uses the date inside each header,
not the filename number or filesystem modification date. It patches only
the newest set: flat files beside the patcher, or WGS blobs in indexed
folders. It stops if the newest set is incomplete or timestamps are tied.
Keep the full WGS account-folder structure intact. For a particular flat save,
put just its four files and the patcher into a separate folder. After
patching that copy, copy all four save files back into the original save
folder before starting the game. Only files in the selected folder can be
backed up; use the original save folder to include every save set.

What does it change?
It restores the 15 item flags listed above, including missing entries,
and removes matching applied-entitlement markers for PC preorder, PS5
preorder, NVIDIA, beta testers, both mailing promotions, all three Twitch
drops, the China promotion, and Razer. It updates CRC32 checksums as needed.
If preorder flags were lost and the two known fallback outfit selections
are found, it restores those selections too. Adding promotional flags
alone does not change your equipped outfit. Equip the items yourself
from the cosmetic menu. Other outfit choices are preserved.
Preferences, other save sets, and the game executable are not edited.
Running it again makes no changes if supported flags remain enabled and
matching applied-entitlement markers remain absent.

Backups
Before editing, this v1.4.1 build creates CosmeticSaveBackup-... beside
the patcher, containing ALL recognized save data in the selected folder:
- Steam: all save sets, preferences_* files, and steam_autocloud.vdf /
  remotecache.vdf when present.
- Epic: every .chunk file, including --containerDisplayName.chunk, plus
  any preferences and recognized Steam save files there.
- WGS: containers.index and every file in every indexed container folder,
  including preference containers and all container.N metadata.

Original names and relative GUID paths are preserved. Other account folders,
unindexed WGS directories, executables and previous backups are excluded.
Flat-save subfolders are not searched. Unexpected nested WGS folders stop
the patch. RESTORE.txt lists paths and SHA-256 hashes. An INCOMPLETE backup
must not be restored. Source file lists and hashes are rechecked before
patching. Only the newest save set is changed. --check and already-patched
saves create no backup. Earlier v1.3.0 used selected-set backups.

To undo, close the game and let synchronization finish. Restore ALL files
listed in a COMPLETE backup to their original paths, including preferences
and WGS metadata. Do not copy RESTORE.txt into the save folder. This returns
all included saves and preferences to the backup point. If newer saves
exist, move current save data to a separate recovery folder before restoring
so newer files are not mixed with the backup. Keep that copy until recovery
is confirmed. Cloud persistence remains unconfirmed.

Why entitlement entries can return
The entitlement-removal count describes applied-grant markers, not new
cosmetics. Removing markers is intended to avoid revocation of rewards
whose entitlement the game does not recognize. Version 1.4.1 removes
matching markers without checking live account ownership.
The game can add recognized grants back on a later load/save. The patcher
then removes them again and creates another backup, even when all supported
items are already enabled. The owner observed seven returning markers;
the compared files showed only header changes. Repeated cleanup has no
demonstrated benefit while those grants remain recognized. There is no need
to rerun after every session if the desired items are still available.

KNOWN ISSUES

Map-opening and autosave stuttering
Steam/Windows users reported stuttering when opening the map and during
autosaves. Users in issue #3 confirmed that moving the backup folder and
patcher EXE out of the save directory resolved it for them.

Workaround identified by SonOfSaris; issue reported by Stonga2 and
the map/autosave workaround confirmed by krizz02.
https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/3#issuecomment-5975246550

After patching finishes, close the patcher. Move CosmeticSaveBackup-...
folders and CosmeticSavePatcher.exe to another location OUTSIDE the game's
save directory before starting the game. Keep the backup folders intact
for recovery. Leave actual save files, preferences and WGS metadata in place.

The original reporter also confirmed that moving the backup saves alone
fixed their issue. The exact mechanism and results on other platforms
remain unconfirmed.
https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/3

Checking without changing anything
Windows PowerShell:
  .\CosmeticSavePatcher.exe --check
Linux (after chmod +x):
  ./CosmeticSavePatcher-linux-x64 --check
For terminal automation, add --no-pause. Use --help for a short summary.

Supported saves
Built for the supplied Control Resonant save format: RMDB 2/2, header 16,
global 23, world facts 4, and the known nine-slot outfit container.
WGS additionally validates metadata and indexed byte totals; shared source
builds for both platforms, with Xbox app WGS usage intended for Windows.
It stops on bad checksums, unsupported layouts, incomplete newest sets,
or tied newest timestamps. It does not guess new layouts after an update.
Unknown header files ending in -header or -header.chunk also cause it
to stop. Early checkpoints without the nine-slot outfit container are
not supported if selected.
The Windows 1.2.0 patcher was tested and confirmed working on the current
game version by the repository owner on September 27, 2026, including the
six added promotional cosmetics. The analyzed game executable version is
0.563.737.9; v1.4.1 was user-confirmed on 0.564.208.5. Item names and entitlement IDs come from the installed databases.
Future game updates may change compatibility.

Upgrading from an earlier version
Replace the old patcher with this version for your OS and run beside saves.
Previously supported unlocks are retained. If all 15 flags are already
enabled and the eleven associated entitlement entries are absent, the
program reports Already patched. Start the game, load the patched save,
and equip items from the cosmetic menu.

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

CREDITS
Workaround identified by SonOfSaris; issue reported by Stonga2 and
the map/autosave workaround confirmed by krizz02.
https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/3#issuecomment-5975246550

