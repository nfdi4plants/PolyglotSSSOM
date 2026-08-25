open System
open System.IO
open System.IO.Compression
open System.Xml.Linq

open BlackFox.Fake
open Fake.Core

open Helpers
open ProjectInfo

initializeContext ()
Directory.SetCurrentDirectory(repoRoot)

let private initialGitStatus = capture "git" [ "status"; "--porcelain=v1" ] repoRoot []
let private uvEnvironment = [ "UV_CACHE_DIR", uvCacheDir ]
let private npmCommand =
    if OperatingSystem.IsWindows() then resolveExecutable "npm.cmd" else resolveExecutable "npm"
let private npmArguments arguments = [ "--cache"; npmCacheDir ] @ arguments

let private transpile project language outputDirectory =
    recreateUnder artifactsDir outputDirectory
    run "dotnet" [ "fable"; project; "--lang"; language; "-o"; outputDirectory; "--noCache" ] repoRoot []
    removeFableGitIgnores outputDirectory

let private compileItems project =
    XDocument.Load(Path.Combine(repoRoot, project)).Descendants(XName.Get "Compile")
    |> Seq.choose (fun element ->
        match element.Attribute(XName.Get "Include") with
        | null -> None
        | attribute -> Some attribute.Value)
    |> Seq.toList

let private requireMatchingCompileItems label projects =
    let expectedProject, expectedItems = projects |> List.head

    for project, actualItems in projects |> List.tail do
        if actualItems <> expectedItems then
            failwithf "%s compile order differs between %s and %s" label expectedProject project

let verifyProjectSources = BuildTask.create "VerifyProjectSources" [] {
    [ dotNetSourceProject; javaScriptSourceProject; pythonSourceProject ]
    |> List.map (fun project -> project, compileItems project)
    |> requireMatchingCompileItems "Source"

    [ dotNetTestProject; javaScriptTestProject; pythonTestProject ]
    |> List.map (fun project -> project, compileItems project)
    |> requireMatchingCompileItems "Test"
}

let clean = BuildTask.create "Clean" [ verifyProjectSources ] {
    recreateUnder repoRoot artifactsDir
}

let prepare = BuildTask.create "Prepare" [ clean ] {
    ensureDirectory artifactsDir
    run "dotnet" [ "tool"; "restore" ] repoRoot []
    run "uv" [ "sync"; "--locked" ] repoRoot uvEnvironment

    let nodeVersion = capture "node" [ "--version" ] repoRoot []
    let majorText = nodeVersion.TrimStart('v').Split('.').[0]

    match Int32.TryParse majorText with
    | true, major when major < 22 -> Trace.traceImportant $"Node {nodeVersion} is below the supported Node 22 floor; CI provides the acceptance runtime."
    | _ -> ()
}

let buildDotNet = BuildTask.create "BuildDotNet" [ prepare ] {
    run "dotnet" [ "build"; dotNetSourceProject; "--configuration"; "Release" ] repoRoot []
}

let buildJavaScript = BuildTask.create "BuildJavaScript" [ prepare ] {
    transpile javaScriptSourceProject "javascript" (Path.Combine(buildDir, "javascript"))
}

let buildPython = BuildTask.create "BuildPython" [ prepare ] {
    transpile pythonSourceProject "python" (Path.Combine(buildDir, "python"))
}

let buildAll = BuildTask.createEmpty "BuildAll" [ buildDotNet; buildJavaScript; buildPython ]

let testDotNet = BuildTask.create "TestDotNet" [ prepare ] {
    run "dotnet" [ "run"; "--project"; dotNetTestProject; "--configuration"; "Release" ] repoRoot []
}

let testJavaScript = BuildTask.create "TestJavaScript" [ prepare ] {
    let outputDirectory = Path.Combine(testsDir, "javascript")
    transpile javaScriptTestProject "javascript" outputDirectory
    writeText (Path.Combine(outputDirectory, "package.json")) "{ \"type\": \"module\" }"
    let entryPoint = findEntry outputDirectory [ "Main.js"; "Main.fs.js" ]
    run "node" [ entryPoint ] repoRoot []
}

let testPython = BuildTask.create "TestPython" [ prepare ] {
    let outputDirectory = Path.Combine(testsDir, "python")
    transpile pythonTestProject "python" outputDirectory
    let entryPoint = findEntry outputDirectory [ "main.py"; "Main.py" ]
    run "uv" [ "run"; "--locked"; "python"; entryPoint ] repoRoot uvEnvironment
}

let testAll = BuildTask.createEmpty "TestAll" [ testDotNet; testJavaScript; testPython ]

let private packNuGet () =
    ensureDirectory packagesDir

    run
        "dotnet"
        [ "pack"
          dotNetSourceProject
          "--configuration"
          "Release"
          "--output"
          packagesDir
          $"/p:Version={packageVersion.SemVer}" ]
        repoRoot
        []

let private npmManifest =
    $$"""{
  "name": "@nfdi4plants/polyglot-sssom",
  "version": "{{packageVersion.SemVer}}",
  "description": "A cross-runtime YAML metadata plus TSV implementation of SSSOM.",
  "type": "module",
  "main": "./index.js",
  "exports": {
    ".": "./index.js"
  },
  "files": [
    "index.js",
    "internal/**/*.js"
  ],
  "engines": {
    "node": ">=22"
  },
  "license": "MIT",
  "repository": {
    "type": "git",
    "url": "https://github.com/nfdi4plants/PolyglotSSSOM.git"
  }
}
"""

let private packNpm () =
    let packageRoot = Path.Combine(stagingDir, "npm")
    let generatedRoot = Path.Combine(packageRoot, "internal")
    recreateUnder artifactsDir packageRoot
    transpile javaScriptSourceProject "javascript" generatedRoot
    writeText (Path.Combine(packageRoot, "index.js")) $"export const version = \"{packageVersion.SemVer}\";\n"
    writeText (Path.Combine(packageRoot, "package.json")) npmManifest
    copyFile (Path.Combine(repoRoot, "README.md")) (Path.Combine(packageRoot, "README.md"))
    copyFile (Path.Combine(repoRoot, "LICENSE")) (Path.Combine(packageRoot, "LICENSE"))
    ensureDirectory packagesDir
    run npmCommand (npmArguments [ "pack"; packageRoot; "--pack-destination"; packagesDir; "--ignore-scripts" ]) repoRoot []

let private pythonManifest =
    $$"""[build-system]
requires = ["hatchling==1.32.0"]
build-backend = "hatchling.build"

[project]
name = "polyglot-sssom"
version = "{{packageVersion.Pep440}}"
description = "A cross-runtime YAML metadata plus TSV implementation of SSSOM."
readme = "README.md"
requires-python = ">=3.12"
license = "MIT"
dependencies = [
    "fable-library==5.13.0",
]

[project.urls]
Homepage = "https://github.com/nfdi4plants/PolyglotSSSOM"
Repository = "https://github.com/nfdi4plants/PolyglotSSSOM.git"

[tool.hatch.build.targets.wheel]
packages = ["polyglot_sssom"]
"""

let private packPython () =
    let packageRoot = Path.Combine(stagingDir, "python")
    let moduleRoot = Path.Combine(packageRoot, "polyglot_sssom")
    let generatedRoot = Path.Combine(moduleRoot, "_generated")
    recreateUnder artifactsDir packageRoot
    transpile pythonSourceProject "python" generatedRoot
    deleteUnder artifactsDir (Path.Combine(generatedRoot, "fable_modules", "fable_python"))

    for pattern in [ "*.fs"; "*.fsproj"; "*.toml" ] do
        Directory.EnumerateFiles(generatedRoot, pattern, SearchOption.AllDirectories)
        |> Seq.iter File.Delete

    writeText (Path.Combine(moduleRoot, "__init__.py")) $"__version__ = \"{packageVersion.Pep440}\"\n"
    writeText (Path.Combine(packageRoot, "pyproject.toml")) pythonManifest
    copyFile (Path.Combine(repoRoot, "README.md")) (Path.Combine(packageRoot, "README.md"))
    copyFile (Path.Combine(repoRoot, "LICENSE")) (Path.Combine(packageRoot, "LICENSE"))
    ensureDirectory packagesDir
    run "uv" [ "build"; "--wheel"; "--out-dir"; packagesDir; packageRoot ] repoRoot uvEnvironment
    deleteUnder artifactsDir (Path.Combine(packagesDir, ".gitignore"))

let pack = BuildTask.create "Pack" [ testAll ] {
    recreateUnder artifactsDir packagesDir
    packNuGet ()
    packNpm ()
    packPython ()
}

let private inspectPackages () =
    let nugetPackage = exactlyOne packagesDir $"PolyglotSSSOM.{packageVersion.SemVer}.nupkg"
    let npmPackage = exactlyOne packagesDir $"nfdi4plants-polyglot-sssom-{packageVersion.SemVer}.tgz"
    let pythonPackage = exactlyOne packagesDir $"polyglot_sssom-{packageVersion.Pep440}-py3-none-any.whl"

    use nugetArchive = ZipFile.OpenRead(nugetPackage)
    let nugetEntries = nugetArchive.Entries |> Seq.map _.FullName |> Seq.toArray

    for expected in [ "lib/netstandard2.0/PolyglotSSSOM.dll"; "lib/netstandard2.0/PolyglotSSSOM.xml" ] do
        if not (nugetEntries |> Array.contains expected) then failwithf "NuGet package lacks %s" expected

    if not (nugetEntries |> Array.exists (fun path -> path.StartsWith("fable/") && path.EndsWith(".fs"))) then
        failwith "NuGet package lacks its Fable source payload"

    use pythonArchive = ZipFile.OpenRead(pythonPackage)

    let pythonEntries = pythonArchive.Entries |> Seq.map _.FullName |> Seq.toArray

    if not (pythonEntries |> Array.contains "polyglot_sssom/__init__.py") then
        failwith "Python wheel lacks polyglot_sssom/__init__.py"

    if pythonEntries |> Array.exists (fun path -> path.EndsWith(".fs") || path.EndsWith(".fsproj")) then
        failwith "Python wheel contains F# build sources"

    if pythonEntries |> Array.exists (fun path -> path.Contains("/fable_python/")) then
        failwith "Python wheel contains unused Fable.Python bindings"

    nugetPackage, npmPackage, pythonPackage

let private smokeProject =
    $$"""<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Program.fs" />
    <PackageReference Include="PolyglotSSSOM" Version="{{packageVersion.SemVer}}" />
  </ItemGroup>
</Project>
"""

let private smokeProgram =
    """module Program

open SSSOM

[<EntryPoint>]
let main _ =
    let input =
        "#mapping_set_id: https://example.org/mappings\n" +
        "predicate_id\tmapping_justification\tsubject_id\tobject_id\n" +
        "skos:exactMatch\tsemapv:ManualMappingCuration\texample:subject\texample:object\n"

    let document = DecodeSssomDocument.DecodeSssomDocument(input)
    if List.length document.Mappings <> 1 then failwith "Expected one decoded mapping"
    printfn "PolyglotSSSOM package smoke OK"
    0
"""

let private nugetConfig =
    $"""<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="{packagesDir}" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"""

let private smokeNuGet () =
    let directory = Path.Combine(smokeDir, "nuget")
    let project = Path.Combine(directory, "Consumer.fsproj")
    let config = Path.Combine(directory, "NuGet.Config")
    recreateUnder artifactsDir directory
    writeText project smokeProject
    writeText (Path.Combine(directory, "Program.fs")) smokeProgram
    writeText config nugetConfig

    let environment = [ "NUGET_PACKAGES", nugetCacheDir ]
    run "dotnet" [ "restore"; project; "--configfile"; config; "--packages"; nugetCacheDir ] repoRoot environment
    run "dotnet" [ "run"; "--project"; project; "--configuration"; "Release"; "--no-restore" ] repoRoot environment

    let jsOutput = Path.Combine(directory, "javascript")
    run "dotnet" [ "fable"; project; "--lang"; "javascript"; "-o"; jsOutput; "--noCache"; "--noRestore" ] repoRoot environment
    writeText (Path.Combine(jsOutput, "package.json")) "{ \"type\": \"module\" }"
    run "node" [ findEntry jsOutput [ "Program.js"; "Program.fs.js" ] ] repoRoot []

    let pyOutput = Path.Combine(directory, "python")
    run "dotnet" [ "fable"; project; "--lang"; "python"; "-o"; pyOutput; "--noCache"; "--noRestore" ] repoRoot environment
    run "uv" [ "run"; "--locked"; "python"; findEntry pyOutput [ "program.py"; "Program.py" ] ] repoRoot uvEnvironment

let private smokeNpm npmPackage =
    let directory = Path.Combine(smokeDir, "npm")
    recreateUnder artifactsDir directory
    writeText (Path.Combine(directory, "package.json")) "{ \"name\": \"polyglot-sssom-smoke\", \"private\": true, \"type\": \"module\" }"

    writeText
        (Path.Combine(directory, "smoke.mjs"))
        $"""import {{ version }} from '@nfdi4plants/polyglot-sssom';
if (version !== '{packageVersion.SemVer}') throw new Error(`Unexpected version ${{version}}`);
const packageRoot = new URL('./node_modules/@nfdi4plants/polyglot-sssom/', import.meta.url);
const {{ DecodeSssomDocument }} = await import(new URL('internal/Parser/DecodeSssomDocument.js', packageRoot));
const {{ length }} = await import(new URL('internal/fable_modules/fable-library-js.5.13.0/List.js', packageRoot));
const input = '#mapping_set_id: https://example.org/mappings\npredicate_id\tmapping_justification\tsubject_id\tobject_id\nskos:exactMatch\tsemapv:ManualMappingCuration\texample:subject\texample:object\n';
if (length(DecodeSssomDocument.DecodeSssomDocument(input).Mappings) !== 1) throw new Error('Generated JavaScript smoke failed');
console.log('npm smoke OK');
"""

    run npmCommand (npmArguments [ "install"; npmPackage; "--ignore-scripts"; "--no-audit"; "--no-fund" ]) directory []
    run "node" [ "smoke.mjs" ] directory []

let private smokePython pythonPackage =
    let directory = Path.Combine(smokeDir, "python")
    recreateUnder artifactsDir directory

    writeText
        (Path.Combine(directory, "smoke.py"))
        $"""import polyglot_sssom
from fable_library.list import length
from polyglot_sssom._generated.Parser.decode_sssom_document import DecodeSssomDocument

assert polyglot_sssom.__version__ == '{packageVersion.Pep440}'
input = '#mapping_set_id: https://example.org/mappings\npredicate_id\tmapping_justification\tsubject_id\tobject_id\nskos:exactMatch\tsemapv:ManualMappingCuration\texample:subject\texample:object\n'
assert length(DecodeSssomDocument.DecodeSssomDocument(input).Mappings) == 1
print('Python smoke OK')
"""

    run
        "uv"
        [ "run"; "--isolated"; "--no-project"; "--with"; pythonPackage; "python"; Path.Combine(directory, "smoke.py") ]
        repoRoot
        uvEnvironment

let testPackages = BuildTask.create "TestPackages" [ pack ] {
    let _, npmPackage, pythonPackage = inspectPackages ()
    smokeNuGet ()
    smokeNpm npmPackage
    smokePython pythonPackage

    let finalGitStatus = capture "git" [ "status"; "--porcelain=v1" ] repoRoot []
    if finalGitStatus <> initialGitStatus then
        failwith $"Build changed repository state.\nBefore:\n{initialGitStatus}\nAfter:\n{finalGitStatus}"
}

[<EntryPoint>]
let main args = runOrDefault testAll args
