# Working on Control Resonant Save Patcher

This repository contains a small save editor for Control Resonant. Read this file before changing the implementation. The release baseline is v1.4.1 (October 1, 2026), adding Deathadder Jacket (Razer) for 15 item flags and 11 applied-entitlement IDs. The repository owner tested the exact Windows release build and confirmed it works as expected with game file version `0.564.208.5`. This is a user-reported result, without independent post-test save inspection. Linux v1.4.1 compiled; its execution, Epic/WGS in-game behavior and cloud persistence remain unconfirmed. Users in issue #3 confirmed a map/autosave stutter workaround: move backup folders and the patcher EXE outside the save directory after patching. This is a user-reported workaround, not a code fix or a proven scanning mechanism. Reported launch failures remain unresolved. Inspect the current source and release before assuming this baseline is still current.

Whole-save backups and Steam/Epic/WGS storage handling are retained from v1.4.0. That earlier Windows/Steam result confirmed header cleanup and an exact 70-file backup on an already-unlocked save. The Razer reward mapping is confirmed in installed game data; see the technical documents for its IDs and evidence.

## Start here

- [README.md](README.md): player instructions and supported rewards.
- [docs/save-format.md](docs/save-format.md): parsing, flag IDs, entitlement IDs, and patch boundaries.
- [docs/findings.md](docs/findings.md): evidence, confirmed behavior, and unresolved questions.
- [docs/releasing.md](docs/releasing.md): builds and publication workflow.

## Repository layout

- `Program.cs`: shared Windows/Linux implementation, version attributes, flag lists, parsing, backups, and writes.
- `build.ps1`: Windows .NET Framework compiler invocation; produces `CosmeticSavePatcher.exe`.
- `CosmeticSavePatcher.Linux.csproj`: self-contained Linux x64 build; produces `publish/linux-x64/CosmeticSavePatcher-linux-x64`.
- `README.md` and `README.txt`: keep overlapping instructions and item descriptions consistent.
- Epic Windows saves were reported under `%LOCALAPPDATA%\Remedy\CONTROLResonant\<account-id>\<slot-folder>\` (example slot: `slot-1`). Account and slot folder names vary; place the patcher beside actual `.chunk` files. A reported location alone does not establish in-game/cloud compatibility.
- `DOTNET-NOTICES.txt`: bundled runtime notices; also embedded in the Linux executable.

## Working rules

- Preserve unrelated facts, outfit choices, save sets, and preferences. Changes to the supported reward list need evidence and must fit the user's requested scope.
- Keep checksum and layout validation, backups of all recognized flat saves/preferences and every file in all indexed WGS containers, including metadata, full metadata-snapshot comparisons before writing (including zero-size-delta patches), full backup-membership and hash checks before replacement, and rollback handling. Mark failed backups INCOMPLETE and never install a patch after an incomplete backup. Stream copies to avoid retaining the whole store in memory. Stop on unsupported layouts instead of guessing offsets.
- Select the newest save by its embedded timestamp, never by its filename number or filesystem modification time. Scan flat saves beside the executable. For Epic, strip `.chunk` only from logical lookup keys and retain original physical paths. Reject duplicate normalized names and never combine extensionless and `.chunk` members in one set. Preserve `--containerDisplayName.chunk`. WGS discovery may follow only explicitly indexed container paths; do not recursively search arbitrary folders.
- Use **Control Resonant** in player-facing text. Keep `CONTROLResonant` where an actual process identifier or on-disk directory name is required, such as `Process.GetProcessesByName` or the Epic save path.
- Do not reintroduce references to experimental or modified game executables in the player README.
- Keep the issue #3 workaround in player instructions: after patching, move backup folders and the patcher EXE outside the save directory, preserve backups for recovery, and leave actual saves/preferences/WGS metadata in place. Results on other platforms remain unconfirmed.
- Do not commit personal saves, game executables, extracted game assets, credentials, or local publication journals. Publish only explicitly selected project files.
- Do not print or store authentication tokens. Use the user's existing authentication or an environment variable.
- Do not launch the game or patch a user's save unless the user requested that action. Use disposable copies for requested implementation tests. Add or run tests only when requested; clearly distinguish a successful build from an in-game confirmation.
- Publishing a release, changing GitHub content, and posting issue replies require authorization in the current task or existing session. An explicit request to publish is authorization; do not ask again solely because this file mentions authorization.
- Preserve remote changes: read the current branch and relevant files before publishing, use a normal commit, and never force-push over newer work.

## Build commands

From the repository root on Windows:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

For Linux x64, with a .NET 8 SDK or later (cross-building on Windows is supported):

```sh
dotnet publish CosmeticSavePatcher.Linux.csproj -c Release -o publish/linux-x64
```

See the release guide before changing versions, runtime packages, release assets, or compatibility claims. Keep these documents current when behavior or evidence changes.

Xbox WGS file support is credited to [hdfyeg35](https://github.com/hdfyeg35); preserve this credit and contributor history. See [WGS integration](docs/wgs-support.md).
