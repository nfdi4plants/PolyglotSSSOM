# PolyglotSSSOM

PolyglotSSSOM is a cross-runtime YAML-metadata-plus-TSV implementation of the
[Simple Standard for Sharing Ontological Mappings (SSSOM)](https://mapping-commons.github.io/sssom/).
It is written once in F# and built for .NET, JavaScript, and Python.

The current `0.1.0-alpha.1` line is an implementation prerelease. Its domain and
codec API will change while the SSSOM v1.0/v1.1 refactor is completed.

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

During Phase 1 the native npm and Python package roots expose only `version` and
`__version__`, respectively. Curated domain and codec exports are part of later
refactor phases.
