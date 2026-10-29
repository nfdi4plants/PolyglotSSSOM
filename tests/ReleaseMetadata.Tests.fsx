#load "../build/ProjectInfo.fs"

open System
open ProjectInfo

let mutable passed = 0

let check name assertion =
    assertion ()
    passed <- passed + 1
    printfn "PASS %s" name

let equal expected actual =
    if expected <> actual then
        failwithf "Expected %A, got %A." expected actual

let rejects (expectedMessage: string) action =
    let error =
        try
            action ()
            None
        with error ->
            Some error

    match error with
    | Some error when error.Message.Contains(expectedMessage, StringComparison.Ordinal) -> ()
    | Some error -> failwithf "Expected an error containing '%s', got: %s" expectedMessage error.Message
    | None -> failwithf "Expected an error containing '%s'." expectedMessage

let parse heading = parseReleaseMetadata ("# Release Notes\n\n" + heading + "\n\n- Changes for this release.\n")

check "stable version and valid ISO date" (fun () ->
    let release = parse "### 12.34.56 (2026-10-29)"
    equal { SemVer = "12.34.56"; Pep440 = "12.34.56" } release.Version
    equal (Some(DateOnly(2026, 10, 29))) release.ReleaseDate)

for label, pepLabel in [ "alpha", "a"; "beta", "b"; "rc", "rc" ] do
    check $"{label} prerelease maps to PEP 440" (fun () ->
        let release = parse $"### 1.2.3-{label}.4 (2026-10-29)"
        equal $"1.2.3-{label}.4" release.Version.SemVer
        equal $"1.2.3{pepLabel}4" release.Version.Pep440
        validateReleaseTag release $"v1.2.3-{label}.4")

check "zero version components and zero prerelease number" (fun () ->
    equal "0.0.0rc0" (parse "### 0.0.0-rc.0 (2026-10-29)").Version.Pep440)

check "Unreleased is valid for normal packaging" (fun () ->
    let release = parse "### 1.2.3 (Unreleased)"
    equal None release.ReleaseDate
    equal "1.2.3" release.Version.SemVer)

check "Unreleased cannot publish" (fun () ->
    let release = parse "### 1.2.3 (Unreleased)"
    rejects "Cannot publish an Unreleased heading" (fun () -> validateReleaseTag release "v1.2.3"))

check "both supported tag forms" (fun () ->
    let release = parse "### 1.2.3 (2026-10-29)"
    validateReleaseTag release "1.2.3"
    validateReleaseTag release "v1.2.3")

for tag in [ "v1.2.4"; "v1.2.3-rc.1"; "V1.2.3"; "vv1.2.3"; "refs/tags/v1.2.3"; " v1.2.3"; "" ] do
    check $"reject mismatching tag '{tag}'" (fun () ->
        let release = parse "### 1.2.3 (2026-10-29)"
        rejects "does not match" (fun () -> validateReleaseTag release tag))

for version in [ "01.2.3"; "1.02.3"; "1.2.03"; "1.2.3-alpha.01"; "1.2.3-beta.01"; "1.2.3-rc.01"; "1.2"; "v1.2.3"; "1.2.3-preview.1"; "1.2.3-rc"; "1.2.3+build.1" ] do
    check $"reject unsupported version {version}" (fun () ->
        rejects "first release heading is unsupported" (fun () -> parse $"### {version} (2026-10-29)" |> ignore))

for date in [ "2026-02-29"; "2026-13-01"; "2026-10-32"; "0000-01-01" ] do
    check $"reject invalid calendar date {date}" (fun () ->
        rejects "Invalid release date" (fun () -> parse $"### 1.2.3 ({date})" |> ignore))

for heading in [ "### 1.2.3 (2026-1-01)"; "### 1.2.3 (29-10-2026)"; "### 1.2.3 (unreleased)"; "### 1.2.3 (2026-10-29) extra"; "### Next"; "###" ] do
    check $"reject malformed heading {heading}" (fun () ->
        rejects "first release heading is unsupported" (fun () -> parse heading |> ignore))

check "valid leap date" (fun () ->
    equal (Some(DateOnly(2024, 2, 29))) (parse "### 1.2.3 (2024-02-29)").ReleaseDate)

check "first heading wins" (fun () ->
    let release = parseReleaseMetadata "# Release Notes\r\n\r\n### 2.0.0 (Unreleased)\r\n\r\n### 1.0.0 (2026-10-29)\r\n"
    equal "2.0.0" release.Version.SemVer
    equal None release.ReleaseDate)

check "unsupported first heading cannot fall back to older release" (fun () ->
    rejects "first release heading is unsupported" (fun () ->
        parseReleaseMetadata "# Release Notes\n### 2.0.0-preview.1 (Unreleased)\n### 1.0.0 (2026-10-29)\n" |> ignore))

check "invalid first date cannot fall back to older release" (fun () ->
    rejects "Invalid release date" (fun () ->
        parseReleaseMetadata "# Release Notes\n### 2.0.0 (2026-02-30)\n### 1.0.0 (2026-10-29)\n" |> ignore))

check "missing release heading" (fun () ->
    rejects "has no ### release heading" (fun () -> parseReleaseMetadata "# Release Notes\nNo releases yet.\n" |> ignore))

check "latest section excludes heading and older releases" (fun () ->
    let release = parseReleaseMetadata "# Release Notes\nPreamble.\n\n### 2.0.0 (2026-10-29)\n\n- First change.\n\n- Second change.\n\n### 1.0.0 (2026-01-01)\n\n- Old change.\n"
    equal "- First change.\n\n- Second change." release.Notes)

check "notes normalize CRLF while preserving Markdown whitespace" (fun () ->
    let release = parseReleaseMetadata "### 1.2.3 (2026-10-29)\r\n \r\n    indented code\r\nline with hard break  \r\n\r\n"
    equal "    indented code\nline with hard break  " release.Notes)

check "nested headings remain in release notes" (fun () ->
    let release = parseReleaseMetadata "### 1.2.3 (2026-10-29)\n\n#### Added\n\n- New feature.\n\n#### Fixed\n\n- A fix.\n"
    equal "#### Added\n\n- New feature.\n\n#### Fixed\n\n- A fix." release.Notes)

check "backtick fenced headings do not split release notes" (fun () ->
    let notes = "- Example:\n\n````markdown\n### Not a release\n```\n### Still fenced\n````\n\n- After code."
    let release = parseReleaseMetadata ("### 1.2.3 (2026-10-29)\n\n" + notes + "\n\n### 1.2.2 (2026-10-28)\n- Older.\n")
    equal notes release.Notes)

check "tilde fences and longer closing delimiters preserve headings" (fun () ->
    let notes = "  ~~~markdown\n### Fenced\n```\n### Still fenced\n  ~~~~\n- After code."
    let release = parseReleaseMetadata ("### 1.2.3 (2026-10-29)\n" + notes + "\n### 1.2.2 (2026-10-28)\n- Older.\n")
    equal notes release.Notes)

check "fenced preamble does not masquerade as first release" (fun () ->
    let release = parseReleaseMetadata "# Notes\n\n```markdown\n### 9.0.0 (Unreleased)\n```\n\n### 1.2.3 (2026-10-29)\n- Real release.\n"
    equal "1.2.3" release.Version.SemVer
    equal "- Real release." release.Notes)

check "unsupported first actual heading still rejects after fenced preamble" (fun () ->
    rejects "first release heading is unsupported" (fun () ->
        parseReleaseMetadata "```\n### 9.0.0 (Unreleased)\n```\n### Next\n### 1.0.0 (2026-10-29)\n" |> ignore))

check "empty Unreleased body parses for development builds" (fun () ->
    let release = parseReleaseMetadata "### 1.2.3 (Unreleased)\n\n### 1.2.2 (2026-10-28)\n- Older.\n"
    equal None release.ReleaseDate
    equal "" release.Notes)

check "empty dated release cannot publish" (fun () ->
    let release = parseReleaseMetadata "### 1.2.3 (2026-10-29)\n \n\t\n### 1.2.2 (2026-10-28)\n- Older.\n"
    rejects "empty release notes" (fun () -> validateReleaseTag release "v1.2.3"))

check "nonempty latest section can publish" (fun () ->
    let release = parseReleaseMetadata "### 1.2.3 (2026-10-29)\n- A useful change.\n"
    validateReleaseTag release "v1.2.3")

printfn "%d release metadata checks passed." passed
