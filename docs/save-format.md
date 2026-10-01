# Save format and patch boundaries

Baseline: v1.3.0 source; the RMDB payload logic is retained from v1.2.0. See [WGS support](wgs-support.md) for storage metadata and backups. This is a partial decoder for the observed layout, not a general specification of every Control Resonant save version. All offsets below are decimal byte offsets from the start of a file unless stated otherwise. Serialized numbers use little-endian byte order.

## Save selection and files

A set consists of four files with the same prefix:

```text
<prefix>-header
<prefix>-persi-global
<prefix>-player
<prefix>-bundle-container
```

Enumerate `*-header` beside the patcher. Parse every candidate, select the greatest embedded timestamp, reject ties, and require its complete four-file set. An invalid candidate header stops processing. The application uses its executable directory, not the terminal's working directory. Flat discovery does not search subdirectories; WGS follows explicit index references. Neither path falls back to an older complete set.

Files must be regular files without a reparse-point attribute, between 20 bytes and 64 MiB. These are implementation limits, not claims about every valid game save.

## RMDB envelope

| Offset | Type | Expected value / meaning |
| --- | --- | --- |
| 0 | 4 bytes | ASCII `RMDB` |
| 4 | uint32 | 2 |
| 8 | uint32 | 2 |
| 12 | uint32 | CRC32 of all bytes starting at offset 16 |
| 16 | section payload | Layout depends on file type |

CRC uses reflected polynomial `0xEDB88320`, initial value `0xFFFFFFFF`, and final bitwise complement. Recalculate after editing either file. Validate the original CRC before making changes. Player and bundle files receive envelope validation but their payloads are not decoded by the patcher.

## Header version 16

| Offset / expression | Type | Meaning |
| --- | --- | --- |
| 16 | uint32 | Header version 16 |
| 20 | uint64 | Unix timestamp, seconds UTC |
| 45 | uint32 | Location-string byte length `L` |
| 49 | `L` bytes | Strict UTF-8 location string |
| `49 + L` | 21 bytes | Preserved opaque fields |
| `49 + L + 21` | uint32 | Applied-entitlement count `E` |
| Count offset + 4 | `E` uint32 values | Applied-entitlement IDs, ending exactly at EOF |

The timestamp must be positive and representable through year 9999. The parser limits `L` to 1 MiB and `E` to 10,000, rejects duplicate entitlement IDs, and checks bounds. Preserve the order of retained entitlement entries.

The applied list records which rewards the game has applied. It is not a platform ownership cache. The patcher removes only the ten IDs below when present, updates the count, and recalculates CRC. It does not edit preferences or platform/account ownership.

## Global version 23 / world facts version 4

| Offset / expression | Type | Meaning |
| --- | --- | --- |
| 16 | uint32 | Global version 23 |
| 20 | uint32 | World-facts version 4 |
| 24 | uint32 | Numeric-fact count `N` |
| 28 | `N` records, 16 bytes each | uint64 key + float64 value; preserve bytes |
| `28 + 16*N` | uint32 | Boolean-fact count `B` |
| Boolean count offset + 4 | `B` records, 9 bytes each | uint64 key + uint8 boolean |
| After boolean records | remaining payload | Preserve except the two narrowly recognized fallback selections below |

Boolean keys must be unique and strictly increasing; values must be 0 or 1. Set each supported key to 1, inserting missing records, serialize in ascending unsigned-key order, and update the count. Insertion shifts the remaining payload by nine bytes per added fact. No fixed offset inside that payload should be assumed.

### Supported unlock facts

`PreorderUnlockFacts` contains six entries; `ExtraUnlockFacts` contains eight. The total is **13 cosmetic flags plus one charm flag**.

| Item | uint64 flag ID | Grant source in game data |
| --- | --- | --- |
| Pickpocket's Tool (charm) | `0x09FC268427CD1CD6` | PC / PS5 preorder |
| Threshold Bureau Coat | `0x2859E4A7F3D0F7FB` | PS5 preorder |
| Threshold Bureau Gas Mask | `0x57C9EA469E42C97B` | PS5 preorder |
| Exposed (shirtless base) | `0x5F7C4166260F3CFD` | Also granted by PC / PS5 preorder |
| Corrupted (body appearance) | `0x8F3741DBFF2CB8EB` | PC / PS5 preorder |
| Threshold Bureau Workwear | `0xA70D57F494331711` | PS5 preorder |
| Third Ice Baseball Cap | `0x5388457983503C2E` | NVIDIA promotion |
| Cracked Standard Issue Sunglasses | `0x747AFDEFDBABB85B` | Beta testers |
| Optical Filtering Goggles | `0x2E27C13F1B8BE2AD` | Mailing promotion 2 |
| Communications Department Headset | `0x46780E853CF1A90B` | Mailing promotion 1 |
| Sierra Helmet | `0xB961238CCE640F14` | Twitch drop 3 |
| Sierra Vest | `0xBF6BE35893F7D586` | Twitch drop 2 |
| Sierra Suit | `0xEF6FBC7BA02AB697` | Twitch drop 1 |
| MIO Specialist's Robe | `0xF37DC383C5C5E132` | China promotion |

### Applied-entitlement IDs to remove

These are serialized uint32 item IDs read from the entitlement database. Do not confuse them with the uint64 world-fact keys or integer entitlement categories.

| Grant | uint32 applied ID | Database record |
| --- | --- | --- |
| PC preorder | `0xB4F0E7DD` | `pre_order` |
| PS5 preorder | `0x5C2B95A3` | `pre_order_ps` |
| NVIDIA | `0x7BD90E22` | `rmd_entitlement_Nvidia` (serialized name uses lowercase `nvidia`) |
| Beta testers | `0x897D368C` | `rmd_entitlement_beta_testers` |
| Mailing promotion 1 | `0xD8A2F11E` | `rmd_entitlement_mailing_promo_1` |
| Mailing promotion 2 | `0xD7A2EF8B` | `rmd_entitlement_mailing_promo_2` |
| Twitch drop 1 | `0x50FAD256` | `rmd_entitlement_twitch_1` |
| Twitch drop 2 | `0x4FFAD0C3` | `rmd_entitlement_twitch_2` |
| Twitch drop 3 | `0x4EFACF30` | `rmd_entitlement_twitch_3` |
| China promotion | `0xECA4A890` | `china` |

Observed revocation behavior motivated removing the applied markers while restoring the unlock facts. This approach was confirmed for the supplied saves; future game logic can change it.

## Outfit container and fallback selections

Scan the payload after boolean facts for exactly one full 188-byte container: uint32 version 1, uint32 count 9, followed by nine 20-byte records. Each record is five uint32 values: record version 3, slot ID, item ID, material-set ID, override item ID.

The records must contain each of these slot IDs exactly once:

```text
22331ABB 26332107 2733229A 25331F74 1B330FB6
1C331149 2833242D 24331DE1 21331928
```

Only when at least one of the six preorder facts was absent or false, and the override is `0xFFFFFFFF`, apply these narrowly matched repairs:

| Slot | Required current item / material | Replacement item / material |
| --- | --- | --- |
| `0x22331ABB` | `0x947B4812` / `0x933B5BDE` | `0xCC5AC851` / unchanged |
| `0x24331DE1` | `0x00000000` / `0xFFFFFFFF` | `0x71128EA3` / `0x933B5BDE` |

These were observed fallback values. Do not generalize them into arbitrary outfit replacement. Adding only promotional flags does not trigger selection repairs. The current implementation still requires a uniquely recognized outfit container even when no repair is needed.

## Writing and failure handling

`--check` calculates and reports proposed changes without writing or creating backups. A no-op patch also creates no backup.

For an actual change, check that the game is closed, take the patcher lock, back up all four files to a unique `CosmeticSaveBackup-...` directory, and record original/patched SHA-256 hashes in `RESTORE.txt`. Stage changed files, recheck the game process and all original bytes, then replace changed files. Only header/global payloads are edited; for WGS the selected indexed byte total is adjusted if needed. WGS backups always include the selected container mapping and index. Attempt rollback from preserved originals on failure.

Replacement is per file, not a transaction over the entire set. A crash between replacements can still require restoring all backed-up paths, including WGS metadata. Process detection currently uses the name `CONTROLResonant`; its reliability across Proton configurations is not independently established.
