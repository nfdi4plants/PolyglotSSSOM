# PolyglotSSSOM

PolyglotSSSOM is a cross-runtime YAML-metadata-plus-TSV implementation of the
[Simple Standard for Sharing Ontological Mappings (SSSOM)](https://mapping-commons.github.io/sssom/).
It is written once in F# and built for .NET, JavaScript, and Python.

Version `1.0.0` is the first stable release, providing a portable domain model,
a strict, version-aware TSV/YAML codec, and mapping-authoring APIs across all
three target runtimes.
The exact stable-v1.0 and pinned-v1.1 specification sources are documented in
[`spec/UPSTREAM.md`](spec/UPSTREAM.md).

## Requirements

- .NET SDK 10.0.300 feature band
- .NET 8 runtime for tests and package-consumer checks
- Node.js 22 or newer
- Python 3.12 or newer
- [uv](https://docs.astral.sh/uv/)

The shipped NuGet library targets `netstandard2.0`; .NET 10 is required only by
the Fable build toolchain.

## Build and test

On Windows use `build.cmd`; on macOS/Linux use `bash build.sh`.

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
extension, descriptor, CURIE, diagnostic, and `SssomCodec` APIs. Optional
scalars remain absent and multivalued slots use empty arrays for zero values.
The codec reads embedded or external metadata, validates v1.0 and the pinned
v1.1 draft, and writes deterministic canonical embedded SSSOM/TSV.

## Authoring mappings

The lexical factories cover the common entity-to-entity path without exposing
the complete positional constructors. `AddMapping` preserves an optional
record ID for general v1.0/v1.1 authoring. `AddMappingWithRecordId` assigns a
caller-owned native v1.1 ID atomically; PolyglotSSSOM never invents one.

F#:

```fsharp
let document =
    SssomDocument.Create(
        "https://example.org/mappings",
        "https://example.org/license"
    )

document.Metadata.EnsurePrefix("ex", "https://example.org/")
document.Metadata.EnsurePrefix("uuid", "urn:uuid:")

let mapping =
    Mapping.CreateEntityMapping(
        "ex:source",
        "skos:exactMatch",
        "ex:target",
        "semapv:ManualMappingCuration"
    )

document.AddMappingWithRecordId("urn:uuid:example", mapping)
let content = SssomCodec.EncodeCanonical document
```

JavaScript:

```javascript
const document = SssomDocument.Create(
  "https://example.org/mappings",
  "https://example.org/license"
);
document.Metadata.EnsurePrefix("ex", "https://example.org/");
document.Metadata.EnsurePrefix("uuid", "urn:uuid:");

const mapping = Mapping.CreateEntityMapping(
  "ex:source",
  "skos:exactMatch",
  "ex:target",
  "semapv:ManualMappingCuration"
);
document.AddMappingWithRecordId("urn:uuid:example", mapping);
const content = SssomCodec.EncodeCanonical(document);
```

Python:

```python
document = sssom.SssomDocument.Create(
    "https://example.org/mappings",
    "https://example.org/license",
)
document.Metadata.EnsurePrefix("ex", "https://example.org/")
document.Metadata.EnsurePrefix("uuid", "urn:uuid:")

mapping = sssom.Mapping.CreateEntityMapping(
    "ex:source",
    "skos:exactMatch",
    "ex:target",
    "semapv:ManualMappingCuration",
)
document.AddMappingWithRecordId("urn:uuid:example", mapping)
content = sssom.SssomCodec.EncodeCanonical(document)
```

Call `Clone()` before editing when the imported document must remain available
unchanged for its original round trip. Find, replace, and remove operations use
the stable `record_id`; full document correctness remains explicit through
`SssomCodec.Validate` or canonical encoding.

## CI and trusted publishing

[CI](.github/workflows/ci.yml) tests .NET, JavaScript and Python, builds all
three packages, and checks isolated consumers of those packages. Pull requests
and manual runs only validate. A push to `main` publishes when the first
`RELEASE_NOTES.md` heading has a release date rather than `(Unreleased)`.

The verification job uploads one `packages` artifact. A single publishing
job downloads those tested packages and reads the release notes from the
checked-out commit, following the
[DataHubClient workflow structure](https://github.com/nfdi4plants/DataHubClient/blob/main/.github/workflows/ci.yml).

The release flow is:

1. If `v<version>` does not exist, `gh release create` creates that tag at the
   tested commit and a GitHub release using the latest release-notes section.
2. If the tag already exists, skip GitHub release creation.
3. In either case, run trusted publishing to NuGet, npm and PyPI using the
   tested packages. Existing package versions cause publishing to fail.

The publishing job runs NuGet, npm and PyPI in that order and stops on the
first failure. A GitHub release can exist before all packages are available,
or remain after a publishing failure.
An existing tag without a GitHub release also skips GitHub release creation.
There is no custom tag/release reconciliation or automatic duplicate recovery.

### One-time publisher configuration

Create the GitHub environment **`release`** and allow deployments from the
**`main` branch**. Required reviewers are optional. Tag rules must allow GitHub
Actions to create the `v*` tags. The publishing job uses `contents: write`
for the GitHub release and `id-token: write` for registry OIDC authentication.
No registry API token is stored in the workflow.

Configure each registry's trusted publisher with this identity:

| Setting | Value |
| --- | --- |
| GitHub owner | `nfdi4plants` |
| Repository | `PolyglotSSSOM` |
| Workflow filename | `ci.yml` |
| Environment | `release` |

- **NuGet — `PolyglotSSSOM`:** create a
  [trusted publishing policy](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
  with this package scope. Set repository or environment secret `NUGET_USER`
  to the nuget.org profile username that owns the policy, not an email or API
  key. `NuGet/login@v1` obtains a temporary API key through OIDC.
- **npm — `@nfdi4plants/polyglot-sssom`:** configure a
  [trusted publisher](https://docs.npmjs.com/trusted-publishers/) and enable
  direct `npm publish`. The package must already exist. For a new package,
  [stage a separate bootstrap version](https://github.blog/changelog/2026-10-02-npm-staged-publishing-now-supports-creating-new-packages/)
  such as `0.0.0-bootstrap.1` with `--tag bootstrap`, and leave it pending while
  configuring trusted publishing. CI can publish `1.0.0` while that different
  version is staged. Reject the bootstrap entry only after the first real
  version is published; do not rely on the placeholder surviving rejection.
  Use a new publisher within npm's activation window (currently two days).
  CI uses Node 24 and npm 11; provenance is generated automatically.
- **PyPI — `polyglot-sssom`:** configure a
  [trusted publisher](https://docs.pypi.org/trusted-publishers/adding-a-publisher/)
  or a [pending publisher](https://docs.pypi.org/trusted-publishers/creating-a-project-through-oidc/)
  for a new project. A pending publisher creates the project on its first
  CI upload; no initial manual release is required.

### Releasing

1. Set the first heading to a new version and date, for example
   `### 1.0.0 (2026-10-29)`, followed by its release notes. Use `####` for
   subsections; the next `###` heading starts the previous release.
2. Run `dotnet fsi --exec tests/ReleaseMetadata.Tests.fsx` and
   `build.cmd TestPackages` (Windows) or `bash build.sh TestPackages` (Linux/macOS).
3. Commit and merge to `main`. CI creates the GitHub release/tag if needed,
   then publishes to NuGet, npm and PyPI in sequence.
4. Start the next development version with a new `(Unreleased)` heading.
   Otherwise, later main pushes attempt to publish the same version again.

Versions support `X.Y.Z` and `X.Y.Z-alpha.N`, `-beta.N`, or `-rc.N`, without
leading zeros. Python uses the equivalent `X.Y.ZaN`, `X.Y.ZbN`, or `X.Y.ZrcN`.
Prereleases are marked on GitHub and published to npm's `next` tag; stable
releases use `latest`. The root npm/Python manifests describe the build
environment; published versions come from `RELEASE_NOTES.md`.

Re-running the publishing job repeats its publication steps using the tested
package artifact. Registries reject versions that were already published,
and a failure stops later steps. Existing tags are left untouched; use the
original release commit when retrying.
