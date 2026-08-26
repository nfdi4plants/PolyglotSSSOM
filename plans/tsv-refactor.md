# PolyglotSSSOM TSV v1.0/1.1 Refactor Plan

> **Status: Specification bundle, build/package foundation, portable domain model, TSV/YAML codec, and IR-ready authoring ergonomics implemented and package-smoke tested.**
>
> Captured on 2026-08-24. The project owner authorized the build-and-package
> foundation, specification bundle, and subsequent portable-domain-model work
> on 2026-08-25, then authorized the TSV/YAML codec implementation on
> 2026-08-26, followed by the authoring-ergonomics phase on the same date.
> Package publication and downstream BioFSharp work remain outside that
> authorization.

## Summary

- Support stable SSSOM v1.0 plus the v1.1 draft pinned at commit `c7042389ca49d8c3387e52125dc05720f6b6d856`.
- Focus exclusively on YAML-metadata-plus-TSV. RDF serialization and SSSOM hashing are deferred.
- Treat .NET, JavaScript, and Python as equal deliverables from one F# source tree. Breaking changes are allowed.

## Specification and Version Policy

- Vendor the relevant specification files directly rather than using a submodule:
  - v1.0.0 at `658de421c21a686f1213ff41879c9245ac0b4925`.
  - v1.1 draft at the pinned commit above.
  - Include `LICENSE`, `spec-intro.md`, `spec-model.md`, `spec-formats-tsv.md`, and `sssom_schema.yaml`.
  - Record source URLs, commits, SHA-256 checksums, licensing, and the manual update procedure in `spec/UPSTREAM.md`.
  - The complete byte-preserved snapshots live in `spec/sssom-v1.0.0` and `spec/sssom-v1.1-draft`; `.gitattributes` disables line-ending conversion for both.
- Decode versionless documents as v1.0. Reject unsupported future versions and contradictions such as declared v1.0 metadata using v1.1-only features.
- Canonical encoding selects the lowest required version:
  - Omit `sssom_version` for pure v1.0 documents.
  - Emit `sssom_version: 1.1` when a v1.1 slot, enum value, or TSV escape is required.
- Use native v1.1 `record_id`; do not introduce `ext_arc_record_id`.
- Include current-draft `derived_from` as a multivalued mapping slot.
- Preserve missing optional values as `option`/`None`/`undefined`. Specification-level algorithmic interpretations such as absent confidence being treated as `1.0` must not materialize values in the model.

## Refactor Phases and Public API

### 1. Build and package foundation

- Move the Fable tool manifest to `.config/dotnet-tools.json`, pin Fable `5.13.0`, and disable roll-forward.
- Pin the build SDK to the .NET 10.0.300 feature band with latest-patch roll-forward. The shipped library still targets `netstandard2.0`; tests target `net8.0`.
- Use the public stable YAMLicious `1.0.0`, Fable.Core `5.2.0`, Fable.Python `5.4.0`, Fable.Pyxpecto `2.0.0`, Fable.Package.SDK `1.4.0`, and Python `fable-library` `5.13.0`.
- Target `netstandard2.0`; add synchronized JavaScript and Python input projects and a FAKE build modeled after DataHubClient.
- Produce `PolyglotSSSOM`, `@nfdi4plants/polyglot-sssom`, and `polyglot-sssom`. Keep package versions prerelease until SSSOM 1.1 is final.
- Start all three packages at logical version `0.1.0-alpha.1` (`0.1.0a1` on Python), with `RELEASE_NOTES.md` as the single version source.
- Require Node 22+ and Python 3.12+. Native npm/Python package roots expose only their version during Phase 1; the curated model/codec exports belong to later phases.
- Add a thin, read-only CI job that invokes the FAKE package-smoke target and uploads artifacts. It must not publish them.
- Remove checked-in generated Fable output and ignore generated JavaScript/Python directories.

### 2. Portable domain model

Implemented on 2026-08-25. The public npm and Python package roots now expose
the curated model alongside the NuGet API; the POC codec was removed rather
than retained as a conflicting, unsupported implementation.

- Replace the POC model with documented `[<AttachMembers>]` mutable classes using explicit backing fields and PascalCase public properties.
- Keep `SssomDocument` as metadata plus a mapping array. Required constructor values remain required; optional scalar values use `option`, while multivalued slots use arrays with empty arrays representing no values.
- Correct every slot's v1.0/v1.1 placement, range, cardinality, propagation status, and conditional requirements through handwritten central descriptor catalogs.
- Add validated lexical value types for entity references, URI references, dates, and extension values. Preserve v1.0-compatible relative or string URI forms while enforcing non-relative URIs for v1.1 documents.
- Model CURIE maps as prefix entries. Provide deterministic expansion and contraction helpers using the normative built-in prefixes plus document entries; perform no ontology loading, lookup, reasoning, or network access.
- Support declared extension definitions and lexical extension values on both mapping sets and mappings. Validate type hints; warn and discard undeclared extensions.

### 3. TSV/YAML codecs and validation

Implemented on 2026-08-26. The same behavioral suite runs on .NET, Node, and
Python, including byte-identical canonical golden output. Packed NuGet, npm,
and Python artifacts also pass native codec consumer smokes. Publication is
still a separate human-authorized operation.

- Implement YAML codecs with YAMLicious using its syntax-tree to strict structural/type layer to typed decoder/canonical encoder pattern.
- Expose one static facade:
  - `DecodeEmbedded` / `TryDecodeEmbedded`
  - `DecodeExternal` / `TryDecodeExternal`
  - `EncodeCanonical` / `TryEncodeCanonical`
  - `Validate`
- Throw `SssomCodecException` from normal methods. Try methods return portable result classes containing an optional document/content and diagnostics.
- Diagnostics carry severity, stable code, message, and optional line, column, row, and slot.
- Enforce required fields, conditional literal/reviewer rules, ranges, enum values, record-ID consistency, CURIE prefixes, duplicate headers/metadata, row widths, extensions, and version constraints.
- Parse v1.1 multivalue escapes left-to-right (`\|`, `\\`). Preserve v1.0 backslash semantics; automatically require v1.1 when canonical output contains a literal pipe or backslash needing escaping.
- Canonical encoding must not mutate the caller. Apply specification propagation/condensation, descriptor-defined metadata and column order, deterministic CURIE/extension ordering, ordinal full-row sorting, invariant number/date formatting, and LF line endings.
- Read embedded or separate metadata, but emit canonical embedded metadata only.

### 4. IR-ready authoring ergonomics

Implemented on 2026-08-26. The shared behavioral suite and native packed
consumers exercise the compact cross-runtime path for ordinary mapping-set
authoring and later ArcIR integration. The complete low-level constructors and
mutable properties remain available.

- Add lexical-string factories for an empty mapping set/document, ordinary entity-to-entity mappings, and `sssom:NoTermFound` mappings. Factories validate and construct the existing lexical wrapper types internally; they do not introduce defaults for predicates, justifications, confidence, or provenance.
- Add deep `Clone` operations for mappings, mapping sets, and documents. Clones isolate every mutable mapping, metadata object, array, prefix entry, extension definition, and extension value so an imported document can remain untouched while a canonical working copy is edited.
- Add an idempotent prefix helper: an identical prefix definition is a no-op, while reusing a prefix name with a different expansion is rejected.
- Add explicit document editing operations:
  - `AddMapping(mapping)` is format-general and preserves the mapping's optional `RecordId`; it never invents an identifier.
  - `AddMappingWithRecordId(recordId, mapping)` validates and assigns the caller-supplied native v1.1 record ID, rejects collisions, promotes an explicitly v1.0 working document to v1.1, and appends the mapping atomically.
  - Find, replace, and remove mappings by `record_id`. Replacement retains the selected record ID; changing identity requires an explicit remove followed by an add.
- Editing operations enforce local invariants such as non-null arguments, valid lexical values, prefix conflicts, and duplicate record IDs. Whole-document SSSOM conformance remains explicit through `Validate` and canonical encoding so callers can construct documents incrementally.
- Document the construction, clone/edit, validation, and encoding workflows in F#, JavaScript, and Python. Exercise the same surface in the playground and native package-consumer smokes.
- Do not add fluent builders or TypeScript declarations in this phase. Keep mapping selection, ambiguity policy, provenance binding, and record-ID allocation outside PolyglotSSSOM.
- The future ArcIR integration may standardize its canonical mapping artifacts on v1.1 and must supply the record IDs it chooses when calling `AddMappingWithRecordId`; PolyglotSSSOM only validates, retains, and serializes those IDs.

## Test and Acceptance Plan

- Run the same Fable.Pyxpecto behavioral suite on .NET, Node, and Python.
- Add offline v1.0 and pinned-v1.1 fixtures covering embedded/external metadata, version inference, version conflicts, propagation, extensions, CURIE maps, literal mappings, `NoTermFound`, `0:0`, `record_id`, `derived_from`, and pipe/backslash escaping.
- Add golden tests proving byte-identical canonical output across all runtimes and semantic decode-encode-decode round trips.
- Add exhaustive descriptor-coverage and property-isolation tests so every slot is represented once and every setter changes only its own backing field.
- Add shared factory/editing tests for clone isolation, prefix conflicts, append order, record-ID collision handling, v1.1 promotion, record lookup/replacement/removal, and an edited canonical round trip that leaves the imported document unchanged.
- Lock regressions for the existing object-source backing-field bug, entity-type decoding bug, curation header typo, extension-definition encoder condition, scalar/multivalue mistakes, and broken generated JavaScript imports.
- Pack artifacts and run native consumer smoke tests from F#/.NET, Node ESM, and Python, using only the packed packages.
- The refactor is complete when all three runtime suites, canonical golden comparisons, package-content checks, and native smoke tests pass through the FAKE build.

## Assumptions

- No backward compatibility with the current POC API or generated files is required.
- No package is published as part of the refactor without separate authorization.
- SSSOM hashing, RDF, ontology integration, BioFSharp/ArcIR integration, and end-to-end DataHUB validation remain separate future work.
- When SSSOM 1.1 becomes final, perform a deliberate schema/specification delta audit before replacing the pinned draft.
