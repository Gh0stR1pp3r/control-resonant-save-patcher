# Findings and evidence

Recorded September 27, 2026, for patcher v1.2.0 and analyzed game executable version `0.563.737.9`. Treat these as findings for that build. Recheck compatibility after game updates.

## What was confirmed

| Finding | Evidence and limits |
| --- | --- |
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

The patcher's 14-item scope is intentional. Do not enable every discovered world fact: many facts track progression, and a named template record alone does not establish a valid cosmetic unlock. New support should have a defensible item mapping, an understood grant/revocation path, and user confirmation of the resulting behavior.

## Remaining uncertainty

- The exact conditions under which absent defaults become visible in the cosmetic menu have not all been checked.
- The full catalog does not establish every possible appearance variant, reward condition, or future DLC item.
- Runtime ownership checks and the save layout may change after updates.
- Linux process detection and save replacement behavior have not been exhaustively checked across distributions or Proton variants.
- Successful compilation, successful program execution, and confirmed in-game persistence are separate evidence levels. Preserve those distinctions in future documentation.
