# PolyglotSSSOM

PolyglotSSSOM is a cross-runtime YAML-metadata-plus-TSV implementation of the
[Simple Standard for Sharing Ontological Mappings (SSSOM)](https://mapping-commons.github.io/sssom/).
It is written once in F# and built for .NET, JavaScript, and Python.

The current `0.1.0-alpha.1` line is an implementation prerelease. Its portable
domain model is available, while the replacement TSV/YAML codec is still under
development and is not exposed yet.
The exact stable-v1.0 and pinned-v1.1 specification sources are documented in
[`spec/UPSTREAM.md`](spec/UPSTREAM.md).

## Requirements

- .NET SDK 10.0.300 feature band
- Node.js 22 or newer
- Python 3.12 or newer
- [uv](https://docs.astral.sh/uv/)

The shipped NuGet library targets `netstandard2.0`; .NET 10 is required only by
the Fable build toolchain.

## Build and test

On Windows use `build.cmd`; on macOS/Linux use `./build.sh`.

```text
build.cmd                  run the shared suite on .NET, Node, and Python
build.cmd BuildAll         build/transpile all three libraries
build.cmd Pack             create NuGet, npm, and wheel artifacts
build.cmd TestPackages     pack and run isolated package-consumer smokes
```

Generated output is written below `artifacts/` and is never committed.

## Package identities

- NuGet: `PolyglotSSSOM`
- npm: `@nfdi4plants/polyglot-sssom`
- PyPI: `polyglot-sssom` (`import polyglot_sssom`)

All three packages expose the portable mapping-set, mapping, lexical-value,
extension, descriptor, and CURIE APIs. Optional scalars remain absent and
multivalued slots use empty arrays for zero values. The old proof-of-concept
decoder and encoder have been removed pending the version-aware codec phase.
