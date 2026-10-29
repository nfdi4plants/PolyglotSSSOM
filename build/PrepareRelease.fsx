#load "ProjectInfo.fs"

open System
open System.IO
open System.Text
open ProjectInfo

let writeOutputs outputs =
    let path = Environment.GetEnvironmentVariable "GITHUB_OUTPUT"
    if not (String.IsNullOrWhiteSpace path) then
        File.AppendAllText(path, String.concat "\n" outputs + "\n", UTF8Encoding(false))

try
    let arguments = fsi.CommandLineArgs |> Array.skip 1
    if arguments.Length <> 1 then
        invalidArg "arguments" "Usage: dotnet fsi --exec build/PrepareRelease.fsx <notes-output-path>"

    if releaseMetadata.ReleaseDate.IsNone then
        writeOutputs [ "release=false" ]
        printfn "Version %s is Unreleased; skipping publication." packageVersion.SemVer
    else
        let tag = $"v{packageVersion.SemVer}"
        validateReleaseTag releaseMetadata tag
        let prerelease = packageVersion.SemVer.Contains("-")
        let prereleaseText = if prerelease then "true" else "false"
        let npmTag = if prerelease then "next" else "latest"
        File.WriteAllText(arguments.[0], releaseMetadata.Notes + "\n", UTF8Encoding(false))
        writeOutputs
            [ "release=true"
              $"semver={packageVersion.SemVer}"
              $"tag={tag}"
              $"prerelease={prereleaseText}"
              $"npm_tag={npmTag}" ]
        printfn "Prepared %s and its latest release-notes section." tag
with error ->
    eprintfn "Release preparation failed: %s" error.Message
    exit 1
