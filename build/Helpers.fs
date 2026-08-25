module Helpers

open BlackFox.Fake
open Fake.Core

open System
open System.Diagnostics
open System.IO

let initializeContext () =
    let executionContext = Context.FakeExecutionContext.Create false "build.fsx" []
    Context.setExecutionContext (Context.RuntimeContext.Fake executionContext)

let private startProcess
    (command: string)
    (args: string list)
    (workingDirectory: string)
    (environment: (string * string) list)
    (captureOutput: bool)
    =
    let info = ProcessStartInfo(command)
    info.UseShellExecute <- false
    info.WorkingDirectory <- workingDirectory
    info.RedirectStandardOutput <- captureOutput
    info.RedirectStandardError <- captureOutput

    args |> List.iter info.ArgumentList.Add
    environment |> List.iter (fun (key, value) -> info.Environment.[key] <- value)

    use childProcess = new Process()
    childProcess.StartInfo <- info

    if not (childProcess.Start()) then
        failwithf "Could not start %s" command

    let output = if captureOutput then childProcess.StandardOutput.ReadToEnd() else ""
    let error = if captureOutput then childProcess.StandardError.ReadToEnd() else ""
    childProcess.WaitForExit()

    if childProcess.ExitCode <> 0 then
        let detail = if captureOutput then $"\n{output}{error}" else ""
        failwithf "%s failed with exit code %d%s" command childProcess.ExitCode detail

    output.TrimEnd()

let run command args workingDirectory environment =
    startProcess command args workingDirectory environment false |> ignore

let capture command args workingDirectory environment =
    startProcess command args workingDirectory environment true

let resolveExecutable (fileName: string) =
    let pathValue = Environment.GetEnvironmentVariable("PATH")

    if String.IsNullOrWhiteSpace pathValue then
        fileName
    else
        pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
        |> Array.map (fun directory -> Path.Combine(directory.Trim().Trim('"'), fileName))
        |> Array.tryFind File.Exists
        |> Option.defaultValue fileName

let writeText (path: string) (content: string) =
    let parent = Path.GetDirectoryName(path)
    if not (String.IsNullOrEmpty parent) then Directory.CreateDirectory(parent) |> ignore
    File.WriteAllText(path, content)

let copyFile (source: string) (destination: string) =
    let parent = Path.GetDirectoryName(destination)
    if not (String.IsNullOrEmpty parent) then Directory.CreateDirectory(parent) |> ignore
    File.Copy(source, destination, true)

let private ensureDescendant (parent: string) (path: string) =
    let fullParent = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar) + string Path.DirectorySeparatorChar
    let fullPath = Path.GetFullPath(path)

    if not (fullPath.StartsWith(fullParent, StringComparison.OrdinalIgnoreCase)) then
        failwithf "Refusing to clean path outside %s: %s" parent fullPath

    fullPath

let recreateUnder (parent: string) (path: string) =
    let fullPath = ensureDescendant parent path
    if Directory.Exists(fullPath) then Directory.Delete(fullPath, true)
    Directory.CreateDirectory(fullPath) |> ignore

let deleteUnder (parent: string) (path: string) =
    let fullPath = ensureDescendant parent path
    if Directory.Exists(fullPath) then Directory.Delete(fullPath, true)
    elif File.Exists(fullPath) then File.Delete(fullPath)

let ensureDirectory (path: string) = Directory.CreateDirectory(path) |> ignore

let removeFableGitIgnores (root: string) =
    if Directory.Exists(root) then
        Directory.EnumerateFiles(root, ".gitignore", SearchOption.AllDirectories)
        |> Seq.filter (fun path -> path.Contains("fable_modules", StringComparison.OrdinalIgnoreCase))
        |> Seq.iter File.Delete

let findEntry (root: string) (candidates: string list) =
    let files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories) |> Seq.toArray

    candidates
    |> List.tryPick (fun candidate ->
        files
        |> Array.tryFind (fun path -> String.Equals(Path.GetFileName(path), candidate, StringComparison.OrdinalIgnoreCase)))
    |> Option.defaultWith (fun () -> failwithf "Could not find entry point beneath %s" root)

let exactlyOne (root: string) (pattern: string) =
    let matches = Directory.GetFiles(root, pattern, SearchOption.TopDirectoryOnly)
    if matches.Length <> 1 then failwithf "Expected one %s in %s, found %d" pattern root matches.Length
    matches.[0]

let runOrDefault defaultTarget args =
    Trace.trace (sprintf "%A" args)

    try
        match args with
        | [| target |] -> Target.runOrDefault target
        | values when values.Length > 1 -> Target.run 0 values.[0] (values |> Array.tail |> List.ofArray)
        | _ -> BuildTask.runOrDefault defaultTarget

        0
    with error ->
        Trace.traceError (string error)
        1
