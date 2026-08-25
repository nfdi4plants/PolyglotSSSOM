# Upstream SSSOM specification snapshots

The files below are vendored from the
[`mapping-commons/sssom`](https://github.com/mapping-commons/sssom) repository.
They are intentionally pinned to immutable Git commits. Do not replace their
sources with links to a branch such as `master` or `main`.

## Snapshots

| Local directory | Upstream revision | Status |
| --- | --- | --- |
| `spec/sssom-v1.0.0` | tag [`v1.0.0`](https://github.com/mapping-commons/sssom/tree/658de421c21a686f1213ff41879c9245ac0b4925), commit `658de421c21a686f1213ff41879c9245ac0b4925` | Stable SSSOM v1.0.0 |
| `spec/sssom-v1.1-draft` | commit [`c7042389ca49d8c3387e52125dc05720f6b6d856`](https://github.com/mapping-commons/sssom/tree/c7042389ca49d8c3387e52125dc05720f6b6d856) | Pinned SSSOM v1.1 draft |

Both snapshots are distributed upstream under the BSD 3-Clause License. Each
directory includes the upstream `LICENSE` verbatim. All other vendored files are
also byte-for-byte copies. `.gitattributes` disables line-ending conversion for
the snapshot directories so the checksums remain stable on every platform.

## Files and checksums

Checksums use SHA-256 over the exact vendored bytes.

| Local file | Immutable upstream source | SHA-256 |
| --- | --- | --- |
| `spec/sssom-v1.0.0/LICENSE` | [source](https://raw.githubusercontent.com/mapping-commons/sssom/658de421c21a686f1213ff41879c9245ac0b4925/LICENSE) | `f11b9b56cde6c1794c0cbc123fd9b2c17a1c59af150116dc9f5e1d30bd643a5b` |
| `spec/sssom-v1.0.0/src/docs/spec-intro.md` | [source](https://raw.githubusercontent.com/mapping-commons/sssom/658de421c21a686f1213ff41879c9245ac0b4925/src/docs/spec-intro.md) | `e57586d43378b37e46985e2b219ab3df30b008edede7e4ddbfe94d721d0eed5f` |
| `spec/sssom-v1.0.0/src/docs/spec-model.md` | [source](https://raw.githubusercontent.com/mapping-commons/sssom/658de421c21a686f1213ff41879c9245ac0b4925/src/docs/spec-model.md) | `5a56bc5cc342c879edd4263dbda89684a77e31a5266e990a92913d6e436570cb` |
| `spec/sssom-v1.0.0/src/docs/spec-formats-tsv.md` | [source](https://raw.githubusercontent.com/mapping-commons/sssom/658de421c21a686f1213ff41879c9245ac0b4925/src/docs/spec-formats-tsv.md) | `14e2e45ab9759479fc077961b81e5e58c8fcba0ec2e73cbeddfe0410adba0635` |
| `spec/sssom-v1.0.0/src/sssom_schema/schema/sssom_schema.yaml` | [source](https://raw.githubusercontent.com/mapping-commons/sssom/658de421c21a686f1213ff41879c9245ac0b4925/src/sssom_schema/schema/sssom_schema.yaml) | `dde9bd0aa706715859999685d58634cf26893f499523326f7e28fb3e11252ae1` |
| `spec/sssom-v1.1-draft/LICENSE` | [source](https://raw.githubusercontent.com/mapping-commons/sssom/c7042389ca49d8c3387e52125dc05720f6b6d856/LICENSE) | `f11b9b56cde6c1794c0cbc123fd9b2c17a1c59af150116dc9f5e1d30bd643a5b` |
| `spec/sssom-v1.1-draft/src/docs/spec-intro.md` | [source](https://raw.githubusercontent.com/mapping-commons/sssom/c7042389ca49d8c3387e52125dc05720f6b6d856/src/docs/spec-intro.md) | `08d1f089a6f3d81e9a0ccfd4c68bb2b0f049b7aeeaf716b3c76e34dab9e258b8` |
| `spec/sssom-v1.1-draft/src/docs/spec-model.md` | [source](https://raw.githubusercontent.com/mapping-commons/sssom/c7042389ca49d8c3387e52125dc05720f6b6d856/src/docs/spec-model.md) | `dd86d352429d9eb7c2ad16528e2654041bfb5f2f6fcb31a723066161a83ad0d5` |
| `spec/sssom-v1.1-draft/src/docs/spec-formats-tsv.md` | [source](https://raw.githubusercontent.com/mapping-commons/sssom/c7042389ca49d8c3387e52125dc05720f6b6d856/src/docs/spec-formats-tsv.md) | `86646c0e6616c12691bcc231eb6bdc56817ad48e55a73f5019bb566ce4a4ea91` |
| `spec/sssom-v1.1-draft/src/sssom_schema/schema/sssom_schema.yaml` | [source](https://raw.githubusercontent.com/mapping-commons/sssom/c7042389ca49d8c3387e52125dc05720f6b6d856/src/sssom_schema/schema/sssom_schema.yaml) | `ce1bcc9baea9bcb794cf5e99649d875c339be891dc23085ebfde125f5ae31945` |

## Manual update procedure

1. Select an immutable upstream tag or full commit hash. Updating the v1.1
   draft pin requires an explicit specification-delta review; never follow a
   moving branch silently.
2. Clone or fetch the upstream repository, disable checkout conversion with
   `git config core.autocrlf false`, and check out the selected commit in a
   detached state.
3. Copy only `LICENSE`, `src/docs/spec-intro.md`,
   `src/docs/spec-model.md`, `src/docs/spec-formats-tsv.md`, and
   `src/sssom_schema/schema/sssom_schema.yaml`. Do not edit the copied files.
4. Verify every local file against its upstream Git blob. For example, compare
   `git rev-parse <commit>:<upstream-path>` with
   `git hash-object --no-filters <local-path>`.
5. Recalculate each SHA-256 checksum with `sha256sum` or PowerShell
   `Get-FileHash -Algorithm SHA256`, then update this file and the pinned commit
   in `plans/tsv-refactor.md` together.
6. Review the schema and prose delta, update implementation fixtures and
   version-specific behavior as needed, and run the full cross-runtime suite.
