# Findings and evidence

Recorded September 27, 2026, for patcher v1.2.0 and analyzed game executable version `0.563.737.9`. Treat these as findings for that build. Recheck compatibility after game updates.

## v1.4.1 release (October 1, 2026)

Current installed game data (EXE `0.564.208.5`, SHA-256 `a2e8e57c86ea60f12de1fa628f8db12014eb296fb259449ea688df9c7ba1497a`) maps category 14 / Remedy-service ID `0x8501EFDB` to `rmd_entitlement_razer`, item ID `0x48A1D491`. Its single granted fact, `wf_da208857baff7481`, hashes to `0x3EE727291BFE7B2B`; the outfit record `dylan_top_razerjacket` uses the same fact. English localization names the outfit **Deathadder Jacket** and its entitlement **Razer Jacket**. The serialized item ID and category are at decompressed entitlement-bundle offsets `0x2244` and `0x2248` in `0xea6067968c670742.bundle`.

Version 1.4.1 adds this fact to `ExtraUnlockFacts` and its item ID to `EntitlementsToRemove`, following the existing promotional-reward path. There are now 14 cosmetics and one charm. The service ID is not written into the save. Full backups, save selection, layout checks, CRC handling and outfit fallback conditions remain as in v1.4.0. Full product/company metadata is retained; the Windows EXE remains unsigned.

Static comparison found the inspected outfit status and entitlement grant/revoke/apply bodies unchanged apart from address relocation, including corresponding internal branches. This does not prove all save readers, dependencies or game data unchanged. The repository owner subsequently tested the Windows v1.4.1 build and explicitly reported that it "works as expected" on October 1, 2026. This is user-reported confirmation on game file version `0.564.208.5`; no independent post-test save inspection or extended persistence test was performed. Linux v1.4.1 compiled successfully but has not been run here. Epic/WGS in-game behavior and cloud persistence remain unconfirmed. The reported map-delay/launch problems are not established as fixed by this reward addition.

## What was confirmed

| Finding | Evidence and limits |
| --- | --- |
| Windows v1.4.1 works as expected | Repository owner explicitly confirmed the Windows test build after the Deathadder Jacket addition on October 1, 2026. User-reported confirmation; no independent post-test save inspection. |
| Windows v1.2.0 works as expected | Repository owner explicitly confirmed it after receiving test saves with the supported unlock facts absent. This is a user-reported in-game result, not an automated test suite or proof for every possible save. |
| Linux executable ran successfully | The author of [issue #2](https://github.com/Gh0stR1pp3r/control-resonant-save-patcher/issues/2) reported placing it beside the saves and running it successfully. Distribution, Steam Deck compatibility, and in-game results were not explicitly reported. |
| Linux save location | Issue #2 identifies Steam's `userdata/<user-id>/3669870/remote/` directory. The README uses `<Steam-folder>` to mean the Steam client folder containing `userdata`, which may differ from the game library folder. |
| Item names and grant sources | Mapped from the installed outfit and entitlement databases and English localization. The complete supported mapping is in [save-format.md](save-format.md). |
| Missing facts can be restored | The implementation inserts sorted boolean records, updates their count, and recalculates CRC. The owner confirmed v1.2.0 behavior after preparing a save with all 14 target facts absent. |

## Corrections to early assumptions

- The old list of eight unlock flags consisted of **seven cosmetics and the Pickpocket's Tool charm**. It was not eight cosmetics. v1.2.0 adds six cosmetic flags for a total of 13 cosmetics and one charm.
- **Third Ice Baseball Cap** is `0x5388457983503C2E`, granted through the NVIDIA entitlement. **Cracked Standard Issue Sunglasses** is `0x747AFDEFDBABB85B`, granted through the beta-tester entitlement. These mappings were initially unresolved; installed data resolved them.
- **Corrupted** is the `body_scarred` appearance. **Exposed** is the `base_shirtless` outfit. Both flags appear in PC and PS5 preorder grants. A dependency between the two has not been demonstrated. Do not describe Exposed as a separately advertised bonus merely because it has a separate grant flag.
- A fact absent from a save is not automatically false or evidence that the item is locked. Definitions can supply defaults, and runtime conditions may also affect availability.
- Applied-entitlement entries in the save header are different from cached ownership data in preferences and from platform/account ownership.
- A Proton compatibility prefix is not the reported save location for this Steam game. Keep the corrected userdata instructions from issue #2.

## How names were recovered

The research used installed game data locally; proprietary archives and personal saves are not distributed in this repository.

1. Read version-3 RMDTOC archive metadata and decompress relevant LZ4 blocks from base bundles.
2. Locate the main `/global/progression_and_economy_databases/outfit_item_database` and `/game_systems/entitlement_database` records.
3. Parse each WorldFact's internal `databaseID` (`wf_` followed by 16 hexadecimal characters), debug path, value fields, and entity metadata.
4. Calculate **CityHash64 of the UTF-8 internal database ID** to match the uint64 saved boolean key. Hashing the readable debug path does not produce the same identifier.
5. Resolve item-title tokens through the English string table, and associate facts with their containing entitlement records. Read the applied uint32 IDs directly from serialized entitlement records.

Example correspondences from the research:

| Internal WorldFact ID | Saved uint64 key | Item |
| --- | --- | --- |
| `wf_bc75711894856888` | `0x5F7C4166260F3CFD` | Exposed |
| `wf_3fd107fdbf49e7ba` | `0x8F3741DBFF2CB8EB` | Corrupted |
| `wf_86a0579866e1ce82` | `0x5388457983503C2E` | Third Ice Baseball Cap |
| `wf_3730e6cdd1166397` | `0x747AFDEFDBABB85B` | Cracked Standard Issue Sunglasses |
| `wf_8b6d05e5d0fb3a98` | `0x09FC268427CD1CD6` | Pickpocket's Tool |

Archive parsing was informed by the [RMDTOC tool's format implementation](https://github.com/amrshaheen61/Alan-Wake-2-RMDTOC-Tool/blob/master/Core/rmdtoc.cs); the analyzed v3 blob descriptors were 24 bytes, so older layouts could not be assumed unchanged. This repository contains the patcher, not the original archive-analysis tools. CityHash and LZ4 were research tools; they are not runtime dependencies of this application.

## Broader catalog and scope

The main outfit database yielded 52 distinct fact IDs across 53 item mappings; two rain-jacket entries shared a flag. Another 16 outfit-related IDs appeared outside that main database in template, debug/loadout, and New Game Plus records. Those extra records were not all established selectable menu unlocks.

The patcher's 15-item scope is intentional. Do not enable every discovered world fact: many facts track progression, and a named template record alone does not establish a valid cosmetic unlock. New support should have a defensible item mapping, an understood grant/revocation path, and user confirmation of the resulting behavior.

## Remaining uncertainty

- The exact conditions under which absent defaults become visible in the cosmetic menu have not all been checked.
- The full catalog does not establish every possible appearance variant, reward condition, or future DLC item.
- Runtime ownership checks and the save layout may change after updates.
- Linux process detection and save replacement behavior have not been exhaustively checked across distributions or Proton variants.
- Successful compilation, successful program execution, and confirmed in-game persistence are separate evidence levels. Preserve those distinctions in future documentation.

## WGS integration, October 1, 2026

Xbox WGS file support from [hdfyeg35](https://github.com/hdfyeg35), commit `2de2299b72fab471d8f67988c1dc7702422a03b4`, is integrated in v1.3.0 source. Integration adds full metadata snapshot checks, rejects linked parent directories and ambiguous GUID mappings, and always backs up the selected mapping plus index. See [implementation details and limits](wgs-support.md).

The fork reports analysis of supplied Xbox saves. No WGS in-game confirmation or cloud-persistence confirmation has been established for this integrated version. Earlier v1.2.0 confirmations concern flat saves. No implementation tests were requested or run during integration.

## Epic save analysis and local v1.4.0 support (October 1, 2026)

The user identified two supplied folders as Steam and Epic. The Steam snapshot contained 17 complete four-file sets; Epic contained 16. All 132 RMDB files had valid CRC32 checksums and envelope 2/2. Both used header version 16, global version 23, and world-fact version 4. Both newest saves had the expected nine-slot outfit layout. Their progress differed, so different flags and file sizes cannot be attributed to the storefront alone.

Epic filenames omit the example Steam `slot-0_` prefix and append `.chunk`. The extra `--containerDisplayName.chunk` contains ASCII `slot-1`; it is preserved. No WGS index was supplied. One old Epic checkpoint had readable facts but no recognized nine-slot outfit container. The existing patcher rejection for that layout remains.

Version 1.4.0 adds filename discovery using separate extensionless and `.chunk` containers with normalized-name collision rejection. Physical names are retained throughout backup and replacement. The RMDB edit logic and supported rewards remain unchanged. Runtime patching, in-game retention and cloud persistence have not been tested for Epic. File analysis and compilation alone do not establish those behaviors.

## Whole-save backup revision (v1.4.0)

The user requested backing up all save files rather than four selected files. Backup scope now includes all recognized flat saves, `.chunk` files, preferences and recognized Steam metadata in the selected directory; WGS includes the index and every file in all indexed containers, even preference-only containers. Backup file membership and SHA-256 values are checked before editing. The newest-set patch scope remains unchanged. Both builds compiled. Subsequent user-run Steam evidence is recorded below; broader platform confirmation remains outstanding.

## User-run full-backup v1.4.0 evidence (October 1, 2026)

The owner supplied before/after Steam saves and the generated backup. Of 70 save/preferences files, only the latest header changed, from 196 to 168 bytes. The exact difference was removal of seven uint32 applied-entitlement markers, an updated count (11 to 4) and CRC32. The entire global payload and equipped outfit records were unchanged. All 14 supported flags were already true. All 70 backed-up originals matched their recorded SHA-256 and the supplied original files. The executable matched the prepared full-backup Windows build.

The owner reported that loading and saving made the same seven markers return. This is consistent with the analyzed grant routine adding markers for recognized entitlements. The current patcher removes every matching marker without determining live ownership, causing redundant cleanup and backups in this case. Removal is intended to prevent the analyzed revocation path for unrecognized entitlements; repeated removal of recognized grants has no demonstrated benefit while ownership remains recognized. Do not claim that every session requires repatching, or skip all cleanup solely because the unlock flags are true. A refinement would require reliable ownership evidence. This behavior is documented but unchanged in the published full-backup build.

This Steam result does not demonstrate restoring missing flags with v1.4.0, Epic/WGS behavior, Linux v1.4.0 execution or cloud persistence. Earlier v1.2.0 unlock confirmations remain separate evidence.
