module ProjectInfo

open System
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

let private releaseHeading =
    Regex(
        @"^###\s+(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:-(?<label>alpha|beta|rc)\.(?<number>\d+))?\s+\(",
        RegexOptions.Compiled
    )

let private readVersion () =
    let releaseNotes = Path.Combine(repoRoot, "RELEASE_NOTES.md")

    let versionMatch =
        File.ReadLines(releaseNotes)
        |> Seq.map releaseHeading.Match
        |> Seq.tryFind (fun candidate -> candidate.Success)
        |> Option.defaultWith (fun () -> failwith "RELEASE_NOTES.md has no supported release heading")

    let major = versionMatch.Groups.["major"].Value
    let minor = versionMatch.Groups.["minor"].Value
    let patch = versionMatch.Groups.["patch"].Value
    let baseVersion = $"{major}.{minor}.{patch}"

    let label = versionMatch.Groups.["label"].Value

    if String.IsNullOrEmpty label then
        { SemVer = baseVersion; Pep440 = baseVersion }
    else
        let number = versionMatch.Groups.["number"].Value
        let pepLabel = if label = "alpha" then "a" elif label = "beta" then "b" else "rc"

        { SemVer = $"{baseVersion}-{label}.{number}"
          Pep440 = $"{baseVersion}{pepLabel}{number}" }

let packageVersion = readVersion ()
