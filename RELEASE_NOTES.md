# Release Notes

### 1.0.1 (Unreleased)

### 1.0.0 (2026-10-29)

First stable release of PolyglotSSSOM: a shared API for reading, validating,
creating, and editing ontology mappings in .NET, JavaScript, and Python.

- Model SSSOM documents, mapping sets, individual mappings, and declared
  extensions with typed values and version-aware field descriptors.
- Read SSSOM/TSV with embedded YAML metadata or separate YAML and TSV inputs,
  including quoted fields and version-specific multivalue escaping.
- Validate mappings against SSSOM v1.0 and the repository's pinned v1.1 draft.
  Report structured errors and warnings through both throwing and nonthrowing
  codec entry points.
- Write deterministic canonical embedded SSSOM/TSV without mutating the input
  document.
- Create entity mappings and `NoTermFound` mappings with convenience factories;
  add, find, replace, and remove mappings by record ID. Deep-clone documents
  before editing to keep the original mapping set intact.
- Manage namespace prefixes and expand or contract CURIEs using built-in and
  document-specific prefix definitions.
- Package the same implementation as `PolyglotSSSOM` on NuGet,
  `@nfdi4plants/polyglot-sssom` on npm, and `polyglot-sssom` on PyPI. The NuGet
  package targets .NET Standard 2.0 and includes Fable sources for JavaScript
  and Python consumers.

SSSOM v1.1 support follows the draft snapshot documented in `spec/UPSTREAM.md`;
it does not imply compatibility with later revisions of that draft.

### 0.1.0-alpha.1 (2026-08-26)

- Published the initial NuGet preview containing the portable model, strict codec, and mapping-authoring API.
