COSMETIC SAVE PATCHER - EXPERIMENTAL WGS BRANCH
Based on upstream v1.2.0

Restores 13 cosmetics and the Pickpocket's Tool charm in supported
CONTROL Resonant saves. This branch keeps the cosmetic and entitlement
patching logic from upstream v1.2.0 and adds automatic support for Xbox PC /
Microsoft Store WGS saves.

Supported save storage:

- Flat-file saves used by Steam and compatible installations.
- Xbox PC / Microsoft Store WGS saves stored through Xbox Connected Storage.

Save type detection is automatic. You do not need to select a platform or
storage type manually.

Upstream project:
https://github.com/Gh0stR1pp3r/control-resonant-save-patcher


EXPERIMENTAL BRANCH / AI-ASSISTED DEVELOPMENT

Xbox PC WGS support on this branch is experimental. It was developed with
assistance from OpenAI ChatGPT using the upstream v1.2.0 source and analysis
of real CONTROL Resonant Xbox PC WGS save data supplied for testing.

The upstream maintainer has not reviewed or endorsed these branch-specific
changes unless they explicitly state otherwise.

The original flat-file patching behavior is retained. The WGS path has been
validated against the supplied save structure, but broader testing across
additional Xbox accounts and save histories is still needed.

Back up your save before testing. For WGS testing, keep a full copy of the
account WGS directory in addition to the patcher's automatic backup. Run with
--check first.


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
same entitlement; it is included to preserve the full preorder grant rather
than being identified here as a separately advertised bonus. Pickpocket's
Tool is a charm.

The patcher also removes the matching applied-entitlement records used by the
supported preorder and promotional rewards, then recalculates the affected
save checksums.


WINDOWS - STEAM / FLAT-FILE SAVE

1. Close CONTROL Resonant completely.

2. Open the save directory containing a complete save set such as:

   auto-0-header
   auto-0-persi-global
   auto-0-player
   auto-0-bundle-container

3. Put CosmeticSavePatcher.exe in that same directory.

4. To inspect the save without changing anything, run:

   .\CosmeticSavePatcher.exe --check

5. Run CosmeticSavePatcher.exe normally to create a backup and patch the
   newest complete save set.

6. Start the game normally and load the patched save.

The Windows EXE is standalone. No Python, installer, internet connection or
administrator access is required. It uses the .NET Framework included with
Windows 10/11.


WINDOWS - XBOX PC / MICROSOFT STORE WGS SAVE

1. Close CONTROL Resonant completely.

2. Open the game's WGS directory. The currently observed package path is:

   %LOCALAPPDATA%\Packages\Remedy.CONTROLResonant_a0f1gph4eb81p\SystemAppData\wgs\

   If the package suffix differs on your system, look under:

   %LOCALAPPDATA%\Packages\Remedy.CONTROLResonant_*\SystemAppData\wgs\

3. Open the account directory inside wgs that contains containers.index.

4. Put CosmeticSavePatcher.exe directly beside containers.index.

5. Run the check mode first:

   .\CosmeticSavePatcher.exe --check

6. Confirm that it reports:

   Save format: Xbox / Microsoft Store WGS

   and that the reported newest save and WGS container are the ones you
   expect.

7. Run CosmeticSavePatcher.exe normally.

8. Start CONTROL Resonant and verify that the existing save loads correctly.

9. Make a normal in-game save/checkpoint and exit the game cleanly before
   relying on Xbox cloud synchronization.

Do not allow a known-bad test save to synchronize to the Xbox cloud if you
can avoid it.


STEAM SAVE FOLDER

Windows:

<Steam-folder>\userdata\<user-id>\3669870\remote\

<Steam-folder> means Steam's folder containing userdata, usually:

C:\Program Files (x86)\Steam

<user-id> means your Steam account's numbered folder inside userdata.

Put CosmeticSavePatcher.exe directly inside remote, beside the actual
CONTROL Resonant save files.

The names in angle brackets are placeholders; do not type them literally.


XBOX WGS STRUCTURE

Xbox PC saves do not expose the normal Remedy filenames directly as ordinary
files. WGS stores them through metadata that maps logical save names to
GUID-named blob files.

Conceptually:

containers.index
    -> logical container, for example slot-0
    -> GUID-named container directory
    -> container.N
    -> logical Remedy filename -> GUID blob
    -> actual save data

A typical account directory looks like:

<account-WGS-folder>\
|-- containers.index
|-- <GUID>\
|   |-- container.N
|   `-- <GUID blob files>
|-- <GUID>\
|   `-- ...
`-- <GUID>\
    `-- ...

The patcher reads containers.index to resolve the indexed save containers,
uses the sequence stored there to select each current container.N, and maps
the normal Remedy save names to their existing GUID blob files.

It does not extract and repack the save. It does not rebuild the WGS container
topology, create replacement GUID directories, or regenerate container.N.


WHAT THE WGS PATH CHANGES

For Xbox PC saves, the branch:

1. Detects WGS when containers.index exists beside the patcher.
2. Verifies that containers.index belongs to CONTROL Resonant.
3. Resolves indexed WGS containers instead of guessing from GUID folder names.
4. Reads each indexed container.N metadata file.
5. Maps logical Remedy save names to the existing GUID blob files.
6. Uses the same embedded-timestamp selection and RMDB validation used for
   flat-file saves.
7. Patches only the GUID blobs corresponding to the selected save set.
8. Updates that container's indexed byte total in containers.index only when
   the patched blob lengths change.

Existing WGS sync state and timestamps in containers.index are otherwise left
unchanged by this branch.


WHICH SAVE IS PATCHED?

A complete Remedy save set contains four matching logical names:

<prefix>-header
<prefix>-persi-global
<prefix>-player
<prefix>-bundle-container

The patcher chooses the newest complete set using the timestamp encoded in
the save header. It does not use the filename number or filesystem modified
time.

For flat saves, the files must be directly beside the patcher. Subfolders are
not searched.

For WGS, the logical names are resolved to their GUID blob paths before the
same newest-save selection and validation logic runs. The patcher can inspect
multiple indexed WGS containers and selects the single newest header. If two
save sets share the same newest embedded timestamp, it stops instead of
guessing.

For a particular flat-file save, put just its four files and the patcher in a
separate folder. After patching that copy, copy all four files back into the
original save folder before starting the game.


WHAT DOES IT CHANGE?

The patcher restores all 14 supported item flags listed above: 13 cosmetics
and the Pickpocket's Tool charm. Missing boolean facts are inserted and false
supported facts are enabled.

It removes matching applied-entitlement markers for:

- PC preorder
- PlayStation preorder
- NVIDIA promotion
- Beta tester reward
- Mailing promotion 1
- Mailing promotion 2
- Twitch drop 1
- Twitch drop 2
- Twitch drop 3
- China promotion

If preorder flags were lost and the two narrowly recognized fallback outfit
selections are found, those selections are restored too. Adding promotional
flags alone does not change the currently equipped outfit. Equip promotional
items yourself from the cosmetic menu.

Affected RMDB CRC32 checksums are recalculated. Other outfit choices,
preferences, other save sets, and the game executable are preserved.

Running the patcher again on an already patched save makes no further changes.


CHECKING WITHOUT CHANGING ANYTHING

Windows PowerShell:

.\CosmeticSavePatcher.exe --check

Linux:

./CosmeticSavePatcher-linux-x64 --check

Other options:

--check       Inspect and validate only. Do not patch or create backups.
--no-pause    Exit without waiting for Enter.
--help        Show command help.

Check mode reports the detected storage type and selected save set, validates
the files, calculates what would change, and exits without writing anything.


BACKUPS AND RESTORE

Before an actual patch, the program creates a timestamped directory named:

CosmeticSaveBackup-...

For flat saves, all four files from the selected save set are backed up.

For WGS saves, all four selected GUID blob files are backed up using paths
that mirror their original GUID directories. If a blob length change requires
containers.index to be updated, the original containers.index is backed up as
well.

RESTORE.txt records the logical save names, mirrored backup paths, original
SHA-256 hashes, and patched hashes.

To undo a flat-file patch:

1. Close CONTROL Resonant.
2. Copy the four backed-up save files back to their original save folder,
   replacing the patched copies.

To undo a WGS patch:

1. Close CONTROL Resonant.
2. Restore each file to the matching relative path recorded by the backup.
3. If containers.index is present in the backup, restore it too.

For early WGS testing, keeping a separate full copy of the account WGS folder
is strongly recommended.


LINUX / STEAM PROTON

The upstream Linux x64 build uses the flat-file path. Xbox WGS support is
Windows-only because Xbox Connected Storage is a Windows/Xbox app save format.

The normal upstream Steam save location is:

<Steam-folder>/userdata/<user-id>/3669870/remote/

If your installation instead exposes the save inside the Proton prefix, the
prefix begins under:

<SteamLibrary-folder>/steamapps/compatdata/3669870/pfx/

In either case, place CosmeticSavePatcher-linux-x64 directly beside the actual
files ending in -header, -persi-global, -player, and -bundle-container.

Then run:

chmod +x CosmeticSavePatcher-linux-x64
./CosmeticSavePatcher-linux-x64

The flat-file path is non-recursive. The patcher must be beside the actual save
files.

The upstream Linux x64 build includes its .NET runtime and does not require
Wine. The WGS branch does not add WGS support to Linux.


SUPPORTED SAVE FORMAT AND SAFETY CHECKS

The inherited v1.2.0 parser supports the analyzed CONTROL Resonant save layout:

- RMDB envelope version 2/2
- Header version 16
- Global version 23
- World facts version 4
- Known nine-slot outfit container

Before patching, it validates the relevant CRC32 checksums and expected save
structure. It stops instead of guessing when a save is incomplete, corrupt,
ambiguous, changed while being processed, or uses an unsupported layout.

The WGS implementation adds storage discovery and path resolution around the
existing parser; it does not bypass the existing save-format validation.

WGS-specific validation currently expects:

- containers.index version 14
- container metadata version 4
- The indexed container byte total to match the resolved blob files
- A CONTROL Resonant package name beginning with Remedy.CONTROLResonant_

Future CONTROL Resonant updates may change either the game save format or WGS
metadata behavior. Keep backups when testing after game updates.

The v1.2.0 patch logic was tested by the upstream repository owner against the
then-current game version on September 27, 2026. The analyzed executable
version documented by upstream is 0.563.737.9. That upstream testing does not
constitute broad validation of the experimental WGS branch.

No personal saves, account credentials, or game files are distributed by this
branch.


UPGRADING FROM THE UPSTREAM PATCHER

This branch retains the v1.2.0 reward set and patch logic. The main difference
is storage support: the Windows build can now detect and operate on either
flat saves or Xbox PC WGS saves.

If all 14 supported flags are already enabled and the associated entitlement
entries are absent, the program reports:

Already patched. No files changed.


SOURCE AND BUILDING

Use the normal upstream Windows build process from the full repository:

powershell -ExecutionPolicy Bypass -File .\build.ps1

The normal Windows output is:

CosmeticSavePatcher.exe

For the upstream Linux x64 build, install the .NET 8 SDK or later and run:

dotnet publish CosmeticSavePatcher.Linux.csproj -c Release -o publish/linux-x64

Output:

publish/linux-x64/CosmeticSavePatcher-linux-x64

The Linux build uses the same flat-file patching logic. Xbox WGS support is
Windows-only.

Development test packages for this branch may also include Build-Test.cmd and
Build-Test.ps1, which compile Program.cs with an installed Windows .NET
Framework C# compiler and produce:

CosmeticSavePatcher-WGS-Test.exe

Source files are not required to run a prebuilt patcher.


BRANCH STATUS

Base patch logic: upstream v1.2.0
Branch: wgs
Flat-file / Steam support: retained
Xbox PC WGS detection through containers.index: implemented
Indexed WGS container lookup: implemented
container.N logical-name to GUID-blob resolution: implemented
In-place WGS blob patching: implemented
WGS indexed byte-total update when blob lengths change: implemented
WGS rebuild/repack: intentionally not used
Broad multi-user WGS testing: pending

Treat this branch as experimental until the WGS implementation has received
wider testing and independent review.
