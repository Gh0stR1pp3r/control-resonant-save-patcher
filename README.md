# Control Resonant Save Patcher

![Control Resonant](https://exputer.com/wp-content/uploads/2026/08/control-resonant.jpg)

A Windows and Linux x64 utility for restoring unobtainable outfits/items in supported Control Resonant saves. Version **1.4.1** restores **14 cosmetics plus the Pickpocket's Tool charm** and removes their associated applied-entitlement entries.

**Windows v1.4.1 was tested by the repository owner, who confirmed it works as expected on October 1, 2026, using game file version `0.564.208.5`.** It adds Deathadder Jacket (Razer).

**Version 1.2.0 was tested and confirmed working on Windows with the then-current game version by the repository owner on September 27, 2026.**

**The Linux v1.2.0 version was also tested by the user who opened [issue #2](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/2), who reported that the patcher ran successfully.**

## Downloads

The downloads below are **v1.4.1**, available for Windows and Linux x64, with Steam, Epic `.chunk`, and Xbox app / Microsoft Store WGS save support. The Windows download is the exact build tested by the repository owner. Linux v1.4.1 compiled successfully; its execution, Epic/WGS in-game behavior, and cloud persistence remain unconfirmed.

- **[Windows: CosmeticSavePatcher.exe](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/releases/latest/download/CosmeticSavePatcher.exe)**
- **[Linux x64: CosmeticSavePatcher-linux-x64](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/releases/latest/download/CosmeticSavePatcher-linux-x64)** — includes .NET; no separate .NET installation or Wine needed. A Linux user reported successful execution in [issue #2](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/2).

## New in 1.4.1

- Adds Deathadder Jacket, world-fact key `0x3EE727291BFE7B2B`, and cleanup of its applied-entitlement item ID `0x48A1D491`.
- Retains Steam, Epic `.chunk`, Xbox WGS support and whole-save backups.
- Includes full product, description, company and copyright metadata. The Windows executable remains unsigned.
- This addition is based on current installed game databases; it is not a demonstrated fix for reported map delay or launch failures.

## New in 1.4.0

- Adds Epic Games saves ending in `.chunk`, preserving physical filenames and backups.
- Rejects duplicate normalized names and requires complete save sets within each format.
- Backs up every recognized save set and preference file in the selected folder; WGS backups include every indexed container and its metadata.
- Checks the complete backup file list and hashes before applying changes.
- Retains the same 14 reward flags and existing Steam/WGS support.
- Epic in-game compatibility and cloud persistence have not yet been confirmed.

## New in 1.3.0

- Adds Xbox app / Microsoft Store WGS file support, contributed by [hdfyeg35](https://github.com/hdfyeg35).
- Resolves logical save names through `containers.index` and `container.N`, retaining the existing 14-item patch logic.
- Rechecks complete metadata snapshots before writing, including patches that do not change file sizes.
- Rejects linked parent directories, duplicate blob mappings, and ambiguous GUID references.
- Backs up the four selected save blobs, `containers.index`, and the selected `container.N` with their original relative paths.

## New in 1.2.0

- Adds Optical Filtering Goggles, Communications Department Headset, Sierra Helmet, Sierra Vest, Sierra Suit, and MIO Specialist's Robe.
- Retains all eight previous item flags, including Exposed and the Pickpocket's Tool charm.
- Removes matching applied-entitlement markers for the added promotions and PC preorder, extending the existing revocation-prevention approach.
- Adding promotional flags alone leaves your equipped outfit unchanged; equip the items from the cosmetic menu.

To upgrade, replace the patcher with the new build for your operating system and run it beside your saves.

## Included items

| Item | Flag ID | Reward source |
| --- | --- | --- |
| Optical Filtering Goggles | `0x2E27C13F1B8BE2AD` | Mailing promotion 2 |
| Communications Department Headset | `0x46780E853CF1A90B` | Mailing promotion 1 |
| Sierra Helmet | `0xB961238CCE640F14` | Twitch drop 3 |
| Sierra Vest | `0xBF6BE35893F7D586` | Twitch drop 2 |
| Sierra Suit | `0xEF6FBC7BA02AB697` | Twitch drop 1 |
| MIO Specialist's Robe | `0xF37DC383C5C5E132` | China promotion |
| Deathadder Jacket | `0x3EE727291BFE7B2B` | Razer entitlement |
| Third Ice Baseball Cap | `0x5388457983503C2E` | NVIDIA promotion |
| Cracked Standard Issue Sunglasses | `0x747AFDEFDBABB85B` | Beta tester reward |
| Threshold Bureau Coat | `0x2859E4A7F3D0F7FB` | PS5 preorder |
| Threshold Bureau Gas Mask | `0x57C9EA469E42C97B` | PS5 preorder |
| Threshold Bureau Workwear | `0xA70D57F494331711` | PS5 preorder |
| Corrupted | `0x8F3741DBFF2CB8EB` | PC / PS5 preorder |
| Pickpocket's Tool (charm) | `0x09FC268427CD1CD6` | PC / PS5 preorder |
| Exposed (shirtless outfit) | `0x5F7C4166260F3CFD` | Also granted by the PC / PS5 preorder entitlement |

Reward sources are mapped from the installed game data. **Corrupted** is the preorder body appearance; **Exposed** is a separate shirtless-base flag granted by the same entitlement. Exposed is included to preserve the full preorder grant, rather than being identified here as a separately advertised bonus. Pickpocket's Tool is a charm.

## How to use

### Windows: Steam saves

1. Close the game.
2. Open your [Steam save folder](#steam-save-folder) and put `CosmeticSavePatcher.exe` inside it.
3. Double-click it. It checks the files, creates a backup, and patches the newest save set.
4. Read the result, press Enter to close, then start the game and load that save.

Only the EXE is needed. No Python, installer, internet connection, or administrator access is required. It uses the .NET Framework included with Windows 10/11.

### Windows: Xbox app / Microsoft Store WGS

Use the v1.4.1 Windows download above. Close the game and let any ongoing synchronization finish. Keep a full copy of the WGS account folder before first use.

1. Open `%LOCALAPPDATA%\Packages\` and find the game's package folder beginning with `Remedy.CONTROLResonant_`.
2. Open its `SystemAppData\wgs\` directory, then the account folder containing `containers.index`.
3. Put `CosmeticSavePatcher.exe` directly beside `containers.index`.
4. Run `.\CosmeticSavePatcher.exe --check` to inspect the selected save, then run it normally to apply the patch.
5. Start the game and check the items. Make a normal in-game save. Cloud persistence has not yet been confirmed for this integration.

WGS discovery follows only the folders and blobs referenced by the index. It is not a recursive search. It selects the newest header and stops if that save is incomplete; it does not fall back to an older save. If both different GUID files referenced by one entry exist, the patcher stops rather than guessing which is current.

### Epic Games: .chunk saves

A user reported the following Windows save location:

```text
%LOCALAPPDATA%\Remedy\CONTROLResonant\<account-id>\<slot-folder>\
```

Expanded form:

```text
C:\Users\<Windows-user>\AppData\Local\Remedy\CONTROLResonant\<account-id>\<slot-folder>\
```

`%LOCALAPPDATA%` opens the current Windows user's local AppData folder. `<Windows-user>` is your Windows profile folder name; `<account-id>` is the long generated folder name inside `CONTROLResonant`, which differs between accounts. `<slot-folder>` is the save-slot folder: the reported example was `slot-1`. If you have several account or slot folders, choose the one containing the save you want to patch. These placeholders are not literal folder names.

Press **Win+R**, enter `%LOCALAPPDATA%\Remedy\CONTROLResonant`, and open your account folder, then `slot-1` or the appropriate slot folder. Put the patcher **inside that slot folder, beside the actual `.chunk` files**. It does not search the parent account folder or subfolders.

1. Close the game and let any ongoing cloud synchronization finish.
2. Open the account and slot folder described above. Confirm that it contains matching `-header.chunk`, `-persi-global.chunk`, `-player.chunk`, and `-bundle-container.chunk` files.
3. Put the v1.4.1 patcher for your operating system beside those files. Keep the `.chunk` extensions and the existing `--containerDisplayName.chunk` file.
4. On Windows, double-click `CosmeticSavePatcher.exe`. On Linux, use the terminal commands in the Linux section below.
5. Read the result, then load the save in game. The newest set is chosen using the timestamp inside its header; all four matching files must be present.

The patcher backs up all `.chunk` files in that folder, including the container-name file, using their original filenames. To undo, close the game and follow [Backups and undo](#backups-and-undo). Duplicate filenames with and without `.chunk` are rejected. Files from the two formats are never combined into one save set.

Epic support has been implemented from the supplied saves; in-game behavior and Epic cloud persistence remain unconfirmed. Epic `.chunk` filename support requires v1.4.0 or later.

### Linux x64

1. Close the game completely.
2. Open the [Linux save location](#linux-steamproton) below and find the folder containing your save files. Download `CosmeticSavePatcher-linux-x64` and put it beside those files.
3. Open a terminal **in that folder** and run:

```sh
chmod +x CosmeticSavePatcher-linux-x64
./CosmeticSavePatcher-linux-x64
```

4. Read the result and press Enter to close. Start the game normally and load the patched save.

Only this one file is needed. It includes .NET 8.0.31 and uses the same automatic backups and save selection as the Windows build. It targets x64 Linux with glibc, not ARM or Alpine/musl. The bundled runtime extracts native libraries to your user's `.net` cache on first launch; the save files are read from beside the patcher.

## Steam save folder

### Windows

```text
<Steam-folder>\userdata\<user-id>\3669870\remote\
```

- [`<Steam-folder>`](https://www.pcgamingwiki.com/wiki/Glossary:Game_data#Steam_client) is the folder containing Steam's `userdata` directory, usually `C:\Program Files (x86)\Steam`.
- [`<user-id>`](https://www.pcgamingwiki.com/wiki/Glossary:Game_data#User_ID) is your Steam account's numbered folder inside `userdata`.

Open your Steam folder, then `userdata`, your numbered account folder, `3669870`, and finally `remote`. Put `CosmeticSavePatcher.exe` inside that `remote` folder, beside the save files.

### Linux (Steam/Proton)

```text
<Steam-folder>/userdata/<user-id>/3669870/remote/
```

`<Steam-folder>` is the Steam client folder containing `userdata`. It may be different from the Steam library where the game is installed. `<user-id>` is your Steam account's numbered folder inside `userdata`.

Open the Steam folder, then `userdata`, your numbered account folder, `3669870`, and `remote`. Put `CosmeticSavePatcher-linux-x64` **inside `remote`, beside the actual save files** ending in `-header`, `-persi-global`, `-player`, and `-bundle-container`. The patcher only checks its own folder and does not search subfolders.

A Linux user reported this location and successful execution of the patcher in [issue #2](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/2).

The angle-bracket names above are placeholders, not text to type literally.

## Choosing the save

The patcher uses the date inside the save, not its filename number or filesystem modification date. Names can differ from the original examples, but each set must contain four files with the same prefix and these endings:

```text
-header
-persi-global
-player
-bundle-container
```

Epic uses the same four endings followed by `.chunk`, for example `auto-2-header.chunk`. Keep all four files in the same format. The container display-name file is preserved.

For flat saves, keep all four together beside the patcher; subfolders are not searched. To patch a particular flat save, put just its four files and the patcher in a separate folder. Only files present beside the patcher are included in its backup; use the original save folder to back up all sets. After patching a separate copy, copy all four save files back into the original save folder before starting the game.

For WGS, these are logical filenames mapped by the index to GUID blobs. Keep the full account-folder structure intact, with the patcher beside `containers.index`. Both formats select the newest header and require all four matching files; they stop on ties or an incomplete newest save.

## Backups and undo

From v1.4.0 onward, before editing, the program creates a `CosmeticSaveBackup-...` folder containing:

- **Steam:** every save set in the current folder, all `preferences_*` files, and `steam_autocloud.vdf` / `remotecache.vdf` if present.
- **Epic:** every `.chunk` file in the current folder, including `--containerDisplayName.chunk`, plus any preferences and recognized Steam save files there.
- **WGS:** `containers.index` and every file in every indexed container directory, including preference containers and all `container.N` metadata. Original GUID paths are preserved.

Other account folders, unindexed WGS directories, patcher executables, and previous backup folders are excluded. Flat-save subfolders are not searched. An unexpected subfolder inside an indexed WGS container stops the patch. `RESTORE.txt` lists original paths and SHA-256 hashes. Failed backups are marked **INCOMPLETE** and must not be restored. The patch is applied only after the backup succeeds and the source file list and hashes have been rechecked. Only the newest save set is patched. `--check` and already-patched saves create no backup.

Earlier v1.3.0 backups contain only the selected save set and its WGS metadata.

To undo, close the game and let cloud synchronization finish. Restore **all files listed in a COMPLETE backup** to their original paths, including preferences and WGS metadata; do not copy `RESTORE.txt` into the save folder. This returns all included saves and preferences to the backup point. If you have saved again since patching, first move the current save data to a separate recovery folder so newer files are not mixed with the backup. Keep that copy until recovery is confirmed. Cloud persistence remains unconfirmed.

## What changes

- Restores the 15 item flags listed above (14 cosmetics and one charm), including flags missing from the save.
- Removes matching applied-entitlement entries for PC preorder, PS5 preorder, NVIDIA, beta testers, both mailing promotions, all three Twitch drops, the China promotion, and Razer.
- Restores the two observed fallback outfit selections only when preorder flags were lost and those specific fallback values are present. Adding promotional flags alone does not change your equipped outfit.
- Recalculates the affected CRC32 checksums.
- For WGS, updates the selected container byte total in `containers.index` only if the changed blobs have a different combined size. It preserves the GUID mapping and sync fields.

Other outfit choices, preferences, other save sets, and the game executable are preserved. WGS backups include metadata, and the index is updated only as described above. A save whose supported flags are enabled and whose matching applied markers are absent requires no further changes.

## Why entitlement entries can return

`Entitlement entries to remove` counts applied-grant markers, not new items to unlock. Removing a marker is intended to avoid the analyzed revocation path when the game does not recognize its entitlement. Version 1.4.1 removes matching markers without checking live account ownership.

If the game recognizes a grant, it may add its marker again on the next load/save. Running the patcher can then remove that marker again and create another backup even though all supported cosmetics are already enabled. The owner observed seven markers returning in this way; the supplied before/after files showed only a header edit and no cosmetic changes. There is no demonstrated benefit from repeatedly removing those seven markers while those grants remain recognized. You do not need to rerun the patcher after every session if the desired items are still available.

## Known issues

### Map-opening and autosave stuttering

Steam/Windows users reported stuttering when opening the map and during autosaves. Users in [issue #3](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/3) confirmed that moving the backup folder and patcher EXE out of the save directory resolved it for them.

[SonOfSaris](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/3#issuecomment-5975246550) identified the workaround. [Stonga2](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/3) reported the issue, and [krizz02](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/3#issuecomment-5982186761) confirmed the workaround resolved map and autosave stuttering.

**Workaround:** after patching finishes, close the patcher and move the `CosmeticSaveBackup-...` folder(s) and `CosmeticSavePatcher.exe` to another location **outside the game's save directory** before starting the game. Keep the backup folders intact so you can restore your saves if needed. Leave the actual save files, preferences and WGS metadata in place.

The original reporter also confirmed that moving the backup saves alone fixed their issue. These are user-confirmed workarounds; the exact scanning or synchronization mechanism has not been established, and results on other platforms are unconfirmed.

## Check without editing

Windows:

```powershell
.\CosmeticSavePatcher.exe --check
```

Linux (after making the download executable):

```sh
./CosmeticSavePatcher-linux-x64 --check
```

Use `--no-pause` for terminal automation or `--help` for a short summary.

## Compatibility and status

Supports the analyzed save layout: RMDB 2/2, header version 16, global version 23, world facts version 4, and the known nine-slot outfit container. The program stops on invalid checksums, unsupported layouts, incomplete newest sets, ambiguous newest dates, or unrecognized files ending in `-header` or `-header.chunk`. Some early checkpoints may lack the required nine-slot outfit container; those are rejected if selected.

**Windows v1.4.1 was tested by the repository owner and confirmed working on October 1, 2026**, following the Deathadder Jacket addition for game file version `0.564.208.5`. This is a user-reported result; no independent post-test save inspection was performed. Earlier Windows v1.2.0 testing covered the expanded promotional rewards on game version `0.563.737.9`. Future game updates may change the format or ownership behavior.

The Linux x64 build uses the same patching logic. A Linux user reported that v1.2.0 ran successfully in [issue #2](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/2). The report does not specify a distribution or confirm Steam Deck compatibility or in-game results.

The WGS storage reader accepts index version 14 and container metadata version 4. Unavailable referenced containers, ambiguous mappings, linked directories, and changing metadata cause it to stop. Shared source builds on Windows and Linux; the Xbox app WGS workflow is intended for Windows and is not a claim of Xbox cloud support on Linux.

Item names, flag IDs, and the corresponding reward sources were read from the installed game's databases. The full supported item list is shown above.

No game files, personal saves, or account credentials are distributed here.

## Build from source

On Windows with the .NET Framework C# compiler installed:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

This produces `CosmeticSavePatcher.exe` beside `Program.cs`. No external packages are used.

For Linux x64, install the .NET 8 SDK or later and run from the source folder (cross-building on Windows is supported):

```sh
dotnet publish CosmeticSavePatcher.Linux.csproj -c Release -o publish/linux-x64
```

The output is `publish/linux-x64/CosmeticSavePatcher-linux-x64`. The build downloads the official .NET runtime packages; the resulting executable works without a separate .NET installation. The project pins runtime 8.0.31. There are no third-party application dependencies.

[.NET runtime license and third-party notices](DOTNET-NOTICES.txt) are also embedded in the Linux executable.

Use source files from the version you want to build. The original `v1.1.0` tag predates Linux support; its automatic source archive does not contain the Linux project.

## Maintainer and agent documentation

Start with [AGENTS.md](AGENTS.md). Technical references cover the [save format and supported flags](docs/save-format.md), [findings and evidence](docs/findings.md), and [build and release workflow](docs/releasing.md).

## Credits

Xbox WGS file support was contributed by [hdfyeg35](https://github.com/hdfyeg35), with metadata and path-safety fixes added during integration.

Map/autosave stutter workaround: [SonOfSaris](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/3#issuecomment-5975246550) identified the workaround. [Stonga2](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/3) reported the issue, and [krizz02](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/3#issuecomment-5982186761) confirmed the workaround resolved map and autosave stuttering.
