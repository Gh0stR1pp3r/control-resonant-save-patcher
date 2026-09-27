# Control Resonant Save Patcher

![Control Resonant](https://exputer.com/wp-content/uploads/2026/08/control-resonant.jpg)

A Windows and Linux x64 utility for restoring preorder and promotional items in supported CONTROLResonant saves. Version **1.2.0** restores **13 cosmetics plus the Pickpocket's Tool charm** and removes their associated applied-entitlement entries.

**Version 1.2.0 was tested and confirmed working on Windows with the current game version by the repository owner on September 27, 2026.**

**The Linux version was also tested by the user who opened [issue #2](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/2), who reported that the patcher ran successfully.**

## Downloads

- **[Windows: CosmeticSavePatcher.exe](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/releases/latest/download/CosmeticSavePatcher.exe)**
- **[Linux x64: CosmeticSavePatcher-linux-x64](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/releases/latest/download/CosmeticSavePatcher-linux-x64)** — includes .NET; no separate .NET installation or Wine needed. A Linux user reported successful execution in [issue #2](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/2).

## New in 1.2.0

- Adds Optical Filtering Goggles, Communications Department Headset, Sierra Helmet, Sierra Vest, Sierra Suit, and MIO Specialist's Robe.
- Retains all eight previous item flags, including Exposed and the Pickpocket's Tool charm.
- Removes matching applied-entitlement markers for the added promotions and PC preorder, extending the existing revocation-prevention approach.
- Adding promotional flags alone leaves your equipped outfit unchanged; equip the items from the cosmetic menu.

To upgrade, replace the patcher with the new build for your operating system and run it beside your saves. No experimental game EXE is needed.

## Included items

| Item | Flag ID | Reward source |
| --- | --- | --- |
| Optical Filtering Goggles | `0x2E27C13F1B8BE2AD` | Mailing promotion 2 |
| Communications Department Headset | `0x46780E853CF1A90B` | Mailing promotion 1 |
| Sierra Helmet | `0xB961238CCE640F14` | Twitch drop 3 |
| Sierra Vest | `0xBF6BE35893F7D586` | Twitch drop 2 |
| Sierra Suit | `0xEF6FBC7BA02AB697` | Twitch drop 1 |
| MIO Specialist's Robe | `0xF37DC383C5C5E132` | China promotion |
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

### Windows

1. Close the game.
2. Open your [Steam save folder](#steam-save-folder) and put `CosmeticSavePatcher.exe` inside it.
3. Double-click it. It checks the files, creates a backup, and patches the newest save set.
4. Read the result, press Enter to close, then load that save with the original game executable.

Only the EXE is needed. No Python, installer, internet connection, or administrator access is required. It uses the .NET Framework included with Windows 10/11.

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

Keep all four together. Only the newest set beside the patcher is patched, even if several save slots are present. Subfolders are not searched. To patch a particular save, put just its four files and the patcher in a separate folder. After patching that copy, copy all four save files back into the original save folder before starting the game.

## Backups and undo

Before editing, the program copies all four files into a new `CosmeticSaveBackup-...` folder. To undo, close the game and copy those four backup files back into the save folder. Each backup includes `RESTORE.txt` with file hashes.

## What changes

- Restores the 14 item flags listed above (13 cosmetics and one charm), including flags missing from the save.
- Removes matching applied-entitlement entries for PC preorder, PS5 preorder, NVIDIA, beta testers, both mailing promotions, all three Twitch drops, and the China promotion.
- Restores the two observed fallback outfit selections only when preorder flags were lost and those specific fallback values are present. Adding promotional flags alone does not change your equipped outfit.
- Recalculates the affected CRC32 checksums.

Other outfit choices, preferences, other save sets, and the game executable are preserved. An already patched save requires no further changes.

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

Supports the analyzed save layout: RMDB 2/2, header version 16, global version 23, world facts version 4, and the known nine-slot outfit container. The program stops on invalid checksums, unsupported layouts, incomplete newest sets, ambiguous newest dates, or unrecognized files ending in `-header`.

**Windows v1.2.0 was tested and confirmed working on the current game version by the repository owner on September 27, 2026**, including the expanded promotional rewards. The analyzed executable version is `0.563.737.9`. Future game updates may change the format or ownership behavior.

The Linux x64 build uses the same patching logic. A Linux user reported that it ran successfully in [issue #2](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/2). The report does not specify a distribution or confirm Steam Deck compatibility or in-game results.

Item names, flag IDs, and the corresponding reward sources were read from the installed game's databases. The full supported item list is shown above.

Running the game with a modified executable that grants the entitlement again may record it as applied again. Rerun this tool before returning to the original executable in that case.

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
