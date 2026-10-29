module ProjectInfo

open System
open System.Globalization
open System.IO
open System.Text.RegularExpressions

let repoRoot = Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, ".."))
let artifactsDir = Path.Combine(repoRoot, "artifacts")
let buildDir = Path.Combine(artifactsDir, "build")
let testsDir = Path.Combine(artifactsDir, "tests")
let stagingDir = Path.Combine(artifactsDir, "staging")
let packagesDir = Path.Combine(artifactsDir, "packages")
let smokeDir = Path.Combine(artifactsDir, "smoke")
let npmCacheDir = Path.Combine(artifactsDir, "npm-cache")
let uvCacheDir = Path.Combine(artifactsDir, "uv-cache")
let nugetCacheDir = Path.Combine(artifactsDir, "nuget-cache")

let dotNetSourceProject = "src/SSSOM/SSSOM.fsproj"
let javaScriptSourceProject = "src/SSSOM/SSSOM.JavaScript.fsproj"
let pythonSourceProject = "src/SSSOM/SSSOM.Python.fsproj"
let dotNetTestProject = "tests/SSSOM.Tests/SSSOM.Tests.fsproj"
let javaScriptTestProject = "tests/SSSOM.Tests/SSSOM.JavaScript.Tests.fsproj"
let pythonTestProject = "tests/SSSOM.Tests/SSSOM.Python.Tests.fsproj"

type PackageVersion =
    { SemVer: string
      Pep440: string }

type ReleaseMetadata =
    { Version: PackageVersion
      ReleaseDate: DateOnly option
      Notes: string }

let private headingStart = Regex(@"^ {0,3}###(?:[ \t]|$)", RegexOptions.Compiled)

let private releaseHeading =
    Regex(
        @"^ {0,3}###[ \t]+(?<major>0|[1-9][0-9]*)\.(?<minor>0|[1-9][0-9]*)\.(?<patch>0|[1-9][0-9]*)(?:-(?<label>alpha|beta|rc)\.(?<number>0|[1-9][0-9]*))?[ \t]+\((?<date>Unreleased|[0-9]{4}-[0-9]{2}-[0-9]{2})\)[ \t]*$",
        RegexOptions.Compiled
    )

let private fenceStart = Regex(@"^ {0,3}(?<fence>`{3,}|~{3,})(?<info>.*)$", RegexOptions.Compiled)

/// Locate release boundaries without interpreting headings inside Markdown code fences.
let private firstReleaseSection (releaseNotes: string) =
    let lines = releaseNotes.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')
    let headings = ResizeArray<int>()
    let mutable fence: (char * int) option = None

    for index in 0 .. lines.Length - 1 do
        let line = lines.[index]
        let fenceMatch = fenceStart.Match line

        match fence with
        | Some(character, length) ->
            if fenceMatch.Success then
                let delimiter = fenceMatch.Groups.["fence"].Value
                if delimiter.[0] = character
                   && delimiter.Length >= length
                   && String.IsNullOrWhiteSpace fenceMatch.Groups.["info"].Value then
                    fence <- None
        | None ->
            if fenceMatch.Success then
                let delimiter = fenceMatch.Groups.["fence"].Value
                let info = fenceMatch.Groups.["info"].Value
                // Backtick fence info strings cannot themselves contain backticks.
                if delimiter.[0] <> '`' || not (info.Contains '`') then
                    fence <- Some(delimiter.[0], delimiter.Length)
            elif headingStart.IsMatch line then
                headings.Add index

    if headings.Count = 0 then
        invalidArg "releaseNotes" "RELEASE_NOTES.md has no ### release heading."

    let first = headings.[0]
    let next = if headings.Count > 1 then headings.[1] else lines.Length
    let notes =
        lines
        |> Array.skip (first + 1)
        |> Array.take (next - first - 1)
        |> Array.skipWhile String.IsNullOrWhiteSpace
        |> Array.rev
        |> Array.skipWhile String.IsNullOrWhiteSpace
        |> Array.rev
        |> String.concat "\n"

    lines.[first], notes

/// Parse the first release heading and its Markdown body; never select an older release on failure.
let parseReleaseMetadata (releaseNotes: string) =
    let heading, notes = firstReleaseSection releaseNotes

    let versionMatch = releaseHeading.Match heading

    if not versionMatch.Success then
        invalidArg
            "releaseNotes"
            $"The first release heading is unsupported: {heading}. Expected ### X.Y.Z[-alpha.N|-beta.N|-rc.N] (Unreleased or YYYY-MM-DD), without leading-zero version components."

    let major = versionMatch.Groups.["major"].Value
    let minor = versionMatch.Groups.["minor"].Value
    let patch = versionMatch.Groups.["patch"].Value
    let baseVersion = $"{major}.{minor}.{patch}"
    let label = versionMatch.Groups.["label"].Value

    let version =
        if String.IsNullOrEmpty label then
            { SemVer = baseVersion; Pep440 = baseVersion }
        else
            let number = versionMatch.Groups.["number"].Value
            let pepLabel = if label = "alpha" then "a" elif label = "beta" then "b" else "rc"

            { SemVer = $"{baseVersion}-{label}.{number}"
              Pep440 = $"{baseVersion}{pepLabel}{number}" }

    let dateText = versionMatch.Groups.["date"].Value

    let releaseDate =
        if dateText = "Unreleased" then
            None
        else
            match DateOnly.TryParseExact(dateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None) with
            | true, date -> Some date
            | false, _ -> invalidArg "releaseNotes" $"Invalid release date in the first release heading: {dateText}."

    { Version = version; ReleaseDate = releaseDate; Notes = notes }

/// Empty work-in-progress sections are allowed, but published releases must explain their changes.
let validateReleaseNotes (metadata: ReleaseMetadata) =
    if String.IsNullOrWhiteSpace metadata.Notes then
        invalidArg "metadata" "Cannot publish a release with empty release notes."

/// Publishing accepts the exact SemVer tag, optionally prefixed with v, and requires a dated release.
let validateReleaseTag (metadata: ReleaseMetadata) (tag: string) =
    if tag <> metadata.Version.SemVer && tag <> $"v{metadata.Version.SemVer}" then
        invalidArg "tag" $"Release tag '{tag}' does not match the first release heading version '{metadata.Version.SemVer}'."

    if metadata.ReleaseDate.IsNone then
        invalidArg "metadata" "Cannot publish an Unreleased heading. Set its release date to YYYY-MM-DD first."

    validateReleaseNotes metadata

let releaseMetadata =
    Path.Combine(repoRoot, "RELEASE_NOTES.md")
    |> File.ReadAllText
    |> parseReleaseMetadata

let packageVersion = releaseMetadata.Version
