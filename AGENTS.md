# Working on Control Resonant Save Patcher

This repository contains a small save editor for Control Resonant. Read this file before changing the implementation. The documented baseline is v1.2.0, September 27, 2026; inspect the current source and release before assuming that baseline is still current.

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
- `DOTNET-NOTICES.txt`: bundled runtime notices; also embedded in the Linux executable.

## Working rules

- Preserve unrelated facts, outfit choices, save sets, and preferences. Changes to the supported reward list need evidence and must fit the user's requested scope.
- Keep checksum and layout validation, backups of all four save files, change detection before writing, and rollback handling. Stop on unsupported layouts instead of guessing offsets.
- Select the newest save by its embedded timestamp, never by its filename number or filesystem modification time. Scan beside the executable, without recursing.
- Use **Control Resonant** in player-facing text. Keep `CONTROLResonant` where an actual process identifier is required, such as `Process.GetProcessesByName`.
- Do not reintroduce references to experimental or modified game executables in the player README.
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
