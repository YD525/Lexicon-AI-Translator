# Phoenix Translator reference corpus

This document records the external reference candidates and the deterministic fixtures used by the
Phoenix Translator test suite. External archives are manual references unless an individual file has
an unambiguous author and redistribution license.

## External candidates

| Coverage | Source | Version | Author | Archive SHA-256 | Redistribution decision |
|---|---|---|---|---|---|
| ESP and PEX | [Simple Offence Suppression MCM](https://www.nexusmods.com/skyrimspecialedition/mods/41774) | 0.6 | wankingSkeever and po3 | `f6394af5d304765410a3a07aaf81f5325f570cef797583b05dd8bbefd3df0433` | Manual reference only until the ownership of every selected file is resolved. The Nexus page requires attribution and CC BY 4.0 for reused parts, while also identifying third-party assets. |
| MCM and mixed project | [MCM Helper](https://www.nexusmods.com/skyrimspecialedition/mods/53000) | 1.6.2 | Parapets | `24b07dadc471f58929255b5c189847e5a0b2c883c1834887ca9cb558e521be30` | Preferred manual end-to-end project. The linked source is MIT licensed, but the packaged archive also contains credited dependencies and generated binaries. Only files with a verified source path may be extracted. |
| XML | [Shuffler90s Fomod Creation Tool Tutorial and Samples](https://www.nexusmods.com/skyrimspecialedition/mods/15678) | 1.0 | Shuffler90 | `079a885e6f86b3f5127121bc8568aabc1085ff9797742442885844849c7bb80b` | A minimal XML sample may be redistributed with attribution. Commercial and Donation Points use remain restricted. |

The complete permission statements, credits, upload timestamps, and file manifests are recorded in
[YD525/Phoenix-Translator#26](https://github.com/YD525/Phoenix-Translator/issues/26).

## Committed fixtures

All files under `PhoenixTranslator.PresetTests/TestData` are original synthetic data created for this
repository. They contain no external mod content.

- `Mcm/valid-mcm-english.txt` covers tab-separated identifiers and escaped line breaks.
- `Mcm/malformed-mcm.txt` covers missing prefixes and missing values.
- `Xml/valid-translation.xml` covers two distinct translation records and record identifiers.
- `Xml/malformed-translation.xml` covers a truncated document.
- `manifest.tsv` records the SHA-256 digest, rights marker, and origin of every committed fixture.

## Repository ownership

- Phoenix Translator owns MCM, XML, project-workflow, and screenshot fixtures.
- EspReader owns generated ESP and ESM parser fixtures.
- PexReader owns generated binary PEX fixtures and malformed corpora.
- PexInterface owns semantic PEX and decompilation expectations derived from PexReader fixtures.
- Phoenix Engine owns provider, persistence, provenance, review, comparison, and validation fixtures.

Binary parser fixtures must be generated in their owning reader repository. They must not be copied
into Phoenix Translator merely to simplify test setup.

## Refresh procedure

1. Record the source page, exact file version, upload timestamp, author, uploader, credits, permission
   statement, and archive SHA-256.
2. Verify the locally obtained archive digest before extracting any file.
3. Map every proposed file to its source and license. Reject ambiguous ownership.
4. Keep only the smallest required file plus attribution and license metadata. Never commit installers,
   PDBs, native binaries, images, or unrelated assets.
5. Keep automated tests offline and deterministic.
6. Treat every upstream version or digest change as a new review.
