# Building and releasing

Repository: [Gh0stR1pp3r/control-resonant-save-patcher](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher).

The documented baseline is [v1.2.0](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/releases/tag/v1.2.0). Inspect the current branch, tags, release, and task instructions before publishing. Local scripts used by the original maintainer are not part of this repository and should not be assumed available to a future agent.

## Build

### Windows

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

`build.ps1` uses `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe`, compiles `Program.cs` with optimization and `System.Core.dll`, and outputs `CosmeticSavePatcher.exe`. Preserve compatibility with this compiler and .NET Framework when changing shared source. Windows 10/11 provides the runtime used by this build.

### Linux x64

With .NET 8 SDK or later:

```sh
dotnet publish CosmeticSavePatcher.Linux.csproj -c Release -o publish/linux-x64
```

Output: `publish/linux-x64/CosmeticSavePatcher-linux-x64`. Cross-building on Windows is supported. The project targets `net8.0` / `linux-x64`, pins runtime `8.0.31`, bundles the runtime in a compressed single file, includes native libraries for extraction, and uses invariant globalization. It targets glibc x64 Linux. Do not assume support for ARM or musl/Alpine.

The build may download official runtime packages. `DOTNET-NOTICES.txt` is embedded and distributed with the release. If changing the runtime, review the matching licenses/notices and describe the new compatibility requirements. A build alone is not a runtime test.

## Prepare a new version

1. Confirm the requested scope and examine the current remote branch. Preserve unrelated remote edits.
2. Update `Version`, `AssemblyVersion`, `AssemblyFileVersion`, and `AssemblyInformationalVersion` in `Program.cs`, plus `<Version>` in `CosmeticSavePatcher.Linux.csproj`.
3. Update `README.md`, `README.txt`, and relevant technical documents. Keep item counts, names, grant sources, and supported layouts consistent with source.
4. Build both artifacts. Keep the specific artifact hashes that were provided for user testing. Do not silently replace a tested build with different code.
5. If testing was requested, use disposable save copies and preserve backups. Useful cases include absent/false/already-true facts, an already-patched save, malformed checksums, timestamp ties, an incomplete newest set, and fallback selections. Never run a patcher on personal saves merely to validate a documentation change.
6. Record who confirmed which platform, which version, and what was observed. Do not carry old confirmation claims forward to untested behavior.

## Publish when authorized

An explicit user request to publish authorizes publication. Otherwise obtain authorization before changing GitHub. Posting issue comments is a separate external communication; do not infer that permission from reading or fixing an issue.

Use existing GitHub authentication or a token supplied through the environment, with only the required permissions. Never print the token, put it in a URL, commit it, or upload local environment files. The original workflow used `GH_TOKEN`; no token value belongs in this repository.

1. Commit the intended source and documentation using an explicit file list. Check for remote changes before updating `main`; do not force-push.
2. Create a new version tag on the intended source commit. Do not move a previously published tag to include later documentation edits.
3. Prepare release notes covering changes, full or linked item/source list, installation instructions, platform status, and material limitations.
4. Create a draft release and upload the assets below. Check uploaded sizes and SHA-256 digests against the prepared files before publishing it.
5. Publish as stable only with the intended confirmation status, and set it as latest when requested. Check that the tag resolves to the intended source and that download links point to the intended release.

### Release assets

| Asset | Purpose |
| --- | --- |
| `CosmeticSavePatcher.exe` | Windows executable |
| `CosmeticSavePatcher-linux-x64` | Linux executable |
| `README.txt` | Standalone player instructions |
| `DOTNET-NOTICES.txt` | Runtime notices |
| `SHA256SUMS.txt` | SHA-256 for both executables, README.txt, and notices |

Checksum-file format: lowercase hexadecimal digest, two spaces, asset basename, newline. To calculate a digest locally, use PowerShell `Get-FileHash -Algorithm SHA256 -LiteralPath <path>` or Linux `sha256sum <path>`.

The v1.2.0 binaries originally published with the owner's confirmation have these SHA-256 values:

```text
af4b97526e35d2e8074375f2cb8497a549a20189a6158cf0411fd501442bee27  CosmeticSavePatcher.exe
9061554985abb7ece66c6e7952048efb14d0605e0ebb006fa56de73711440fa9  CosmeticSavePatcher-linux-x64
```

These identify the published artifacts; they are not a guarantee that a fresh build will be byte-for-byte reproducible.

## Documentation-only publication

Commit the documentation changes without rebuilding executables or moving tags. When `README.txt` changes, update its downloadable release asset and its entry in `SHA256SUMS.txt`. Preserve the binary and notice assets and their digests. When correcting an installation path, check the README, downloadable instructions, and current release notes for the same error.

The released tag's automatic source archives remain a snapshot of that tag; `main` may contain later documentation corrections. The original v1.1.0 tag predates the Linux project. Use the appropriate source revision for the version/platform being built.

## Repository boundaries

Source, build files, project documentation, and runtime notices belong in the repository. Executables belong in release assets. Personal saves, game binaries, extracted archives, analysis dumps containing private data, access tokens, and publication journals do not belong in either public location.
