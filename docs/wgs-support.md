# Xbox WGS support

Source version 1.3.0, integrated October 1, 2026. Xbox WGS file support was contributed by [hdfyeg35](https://github.com/hdfyeg35) in [commit 2de2299](https://github.com/hdfyeg35/control-resonant-save-patcher/commit/2de2299b72fab471d8f67988c1dc7702422a03b4). The integration keeps the existing 14 rewards and RMDB payload edits, with additional storage-safety checks.

## Discovery

Place the built patcher in the per-account WGS folder beside `containers.index`. The Xbox app package is under `%LOCALAPPDATA%\Packages\Remedy.CONTROLResonant_*\SystemAppData\wgs\`. Use the actual installed package name and account directory.

The presence of `containers.index` selects WGS discovery. Otherwise the patcher uses flat filenames beside its executable. WGS discovery follows only indexed paths. Shared source can parse copied WGS data on supported file systems, but the documented Xbox app workflow is for Windows; Linux cloud functionality is not established.

The index reader accepts version 14, checks the game package name and bounded UTF-16 strings, resolves each container GUID and its `container.<sequence>` file, and checks the indexed byte total against mapped blobs. The mapping reader accepts version 4 and fixed 160-byte entries: 128 bytes of UTF-16 logical filename followed by two 16-byte GUID references. GUIDs use the .NET byte layout and uppercase 32-character filenames.

If just one reference has a file, it is used; identical GUID references are allowed. If both distinct referenced GUID files exist, discovery stops. Their active/cloud semantics have not been established well enough to choose by existence or timestamp. Duplicate logical names, duplicate physical blob mappings, duplicate container references, unavailable containers, and inconsistent totals also stop discovery. Pending/deleted cloud states are not guessed or automatically repaired.

Header candidates retain their logical names and resolve to physical GUID files. Selection is by the embedded header timestamp; ties and an incomplete newest set stop processing. The existing RMDB checks and edits then operate on those files.

## Snapshot and path checks

- Retain complete original bytes for `containers.index` and every mapping file read during discovery.
- Compare every snapshot before reporting the proposed patch, again before backup creation, and immediately before replacement. These checks also run for a same-size patch; the index is never re-read and substituted as a new baseline for an old offset.
- Build an index edit only from the retained snapshot. The only changed index field is the selected container's total byte size, and only when the combined size delta is nonzero.
- Require lexical containment using platform-appropriate case comparison. Reject reparse points/symlinks on every parent directory from the selected save root to each target, as well as the target file. Recheck target paths before staging, replacement, and rollback.

These checks reject observed changes; they do not provide an OS transaction or a lock shared with the Xbox cloud service. Close the game and let synchronization finish first. A concurrent writer acting after the final comparison remains outside the patcher's lock.

## Backup, commit, and recovery

Any actual WGS patch backs up the four selected blobs, the index, and the selected mapping file, even when metadata itself is unchanged. Backups mirror physical GUID paths and record logical names and hashes. All discovered mapping snapshots are checked for concurrency; only the selected mapping needs to be included in the backup.

Stage replacements beside their targets. Replace changed blob files first, followed by the index if its total changes. Keep GUIDs, mapping contents, sync-state fields, and timestamps in the index unchanged. On failure, attempt rollback and direct the user to restore every backed-up path if rollback fails.

The backup is not a full account-store snapshot. Restore it before advancing the store with new saves or synchronization. If other containers have changed since the backup, restoring an old global index with only the selected blobs may be inconsistent; keep a full account-folder copy for such recovery.

## Validation status

This merge has not been confirmed in game or across Xbox cloud upload/download. Earlier Windows and Linux v1.2.0 reports apply to flat saves. Compilation is not runtime confirmation. When the user requests testing, cover absent and false facts, zero and nonzero size deltas, metadata changes, linked GUID directories, ambiguous GUID entries, incomplete saves, rollback, and an in-game save/reload/cloud cycle.

The public stable downloads remain v1.2.0 until a separate release is authorized and published. The source merge preserves the contributor's commit in history and credits their WGS work in both README formats.
