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

let private javaScriptIndex =
    $$"""export const version = "{{packageVersion.SemVer}}";
export { EntityReference, SssomDate, UriReference } from "./internal/Domain/LexicalValues.js";
export { ExtensionDefinition, ExtensionValue } from "./internal/Domain/Extensions.js";
export {
  EntityType,
  MappingCardinality,
  PredicateModifier,
  SssomVersion,
  EntityTypeModule_minimumVersion as minimumEntityTypeVersion,
  EntityTypeModule_parse as parseEntityType,
  EntityTypeModule_toLexical as entityTypeToLexical,
  EntityTypeModule_tryParse as tryParseEntityType,
  MappingCardinalityModule_minimumVersion as minimumMappingCardinalityVersion,
  MappingCardinalityModule_parse as parseMappingCardinality,
  MappingCardinalityModule_toLexical as mappingCardinalityToLexical,
  MappingCardinalityModule_tryParse as tryParseMappingCardinality,
  PredicateModifierModule_parse as parsePredicateModifier,
  PredicateModifierModule_toLexical as predicateModifierToLexical,
  PredicateModifierModule_tryParse as tryParsePredicateModifier,
  SssomVersionModule_parse as parseSssomVersion,
  SssomVersionModule_toLexical as sssomVersionToLexical,
  SssomVersionModule_tryParse as tryParseSssomVersion
} from "./internal/Domain/Enums.js";
export {
  PrefixEntry,
  CurieMap_builtInEntries as builtInPrefixes,
  CurieMap_contract as contractCurie,
  CurieMap_expand as expandCurie,
  CurieMap_isBuiltIn as isBuiltInPrefix,
  CurieMap_tryContract as tryContractCurie,
  CurieMap_tryExpand as tryExpandCurie
} from "./internal/Domain/CurieMap.js";
export {
  ConditionalRequirement,
  SlotCardinality,
  SlotDescriptor,
  SlotPlacement,
  SlotRange,
  SlotVersionDescriptor,
  MappingDescriptors_allDescriptors as mappingDescriptors,
  MappingDescriptors_tryFind as tryFindMappingDescriptor,
  MappingSetDescriptors_allDescriptors as mappingSetDescriptors,
  MappingSetDescriptors_tryFind as tryFindMappingSetDescriptor
} from "./internal/Domain/Descriptors.js";
export { Mapping } from "./internal/Domain/Mapping.js";
export { MappingSet } from "./internal/Domain/MappingSet.js";
export { SssomDocument } from "./internal/Domain/SssomDocument.js";
export {
  DecodeResult,
  DiagnosticSeverity,
  EncodeResult,
  SssomCodecException,
  SssomDiagnostic
} from "./internal/Codec/Diagnostics.js";
export { SssomCodec } from "./internal/Codec/SssomCodec.js";
"""

let private packNpm () =
    let packageRoot = Path.Combine(stagingDir, "npm")
    let generatedRoot = Path.Combine(packageRoot, "internal")
    recreateUnder artifactsDir packageRoot
    transpile javaScriptSourceProject "javascript" generatedRoot
    writeText (Path.Combine(packageRoot, "index.js")) javaScriptIndex
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

let private pythonInit =
    $$"""__version__ = "{{packageVersion.Pep440}}"

from ._generated.Domain.lexical_values import EntityReference, SssomDate, UriReference
from ._generated.Domain.extensions import ExtensionDefinition, ExtensionValue
from ._generated.Domain.curie_map import (
    PrefixEntry,
    CurieMap_builtInEntries as built_in_prefixes,
    CurieMap_contract as contract_curie,
    CurieMap_expand as expand_curie,
    CurieMap_isBuiltIn as is_built_in_prefix,
    CurieMap_tryContract as try_contract_curie,
    CurieMap_tryExpand as try_expand_curie,
)
from ._generated.Domain.enums import (
    SssomVersion_V1_0,
    SssomVersion_V1_1,
    SssomVersionModule_parse as parse_sssom_version,
    SssomVersionModule_toLexical as sssom_version_to_lexical,
    SssomVersionModule_tryParse as try_parse_sssom_version,
    EntityTypeModule_minimumVersion as minimum_entity_type_version,
    EntityTypeModule_parse as parse_entity_type,
    EntityTypeModule_toLexical as entity_type_to_lexical,
    EntityTypeModule_tryParse as try_parse_entity_type,
    MappingCardinalityModule_minimumVersion as minimum_mapping_cardinality_version,
    MappingCardinalityModule_parse as parse_mapping_cardinality,
    MappingCardinalityModule_toLexical as mapping_cardinality_to_lexical,
    MappingCardinalityModule_tryParse as try_parse_mapping_cardinality,
    PredicateModifierModule_parse as parse_predicate_modifier,
    PredicateModifierModule_toLexical as predicate_modifier_to_lexical,
    PredicateModifierModule_tryParse as try_parse_predicate_modifier,
)
from ._generated.Domain.descriptors import (
    SlotDescriptor,
    SlotVersionDescriptor,
    MappingDescriptors_allDescriptors as mapping_descriptors,
    MappingDescriptors_tryFind as try_find_mapping_descriptor,
    MappingSetDescriptors_allDescriptors as mapping_set_descriptors,
    MappingSetDescriptors_tryFind as try_find_mapping_set_descriptor,
)
from ._generated.Domain.mapping import Mapping
from ._generated.Domain.mapping_set import MappingSet
from ._generated.Domain.sssom_document import SssomDocument
from ._generated.Codec.diagnostics import (
    DecodeResult,
    DiagnosticSeverity_Error,
    DiagnosticSeverity_Warning,
    EncodeResult,
    SssomCodecException,
    SssomDiagnostic,
)
from ._generated.Codec.sssom_codec import SssomCodec

SSSOM_VERSION_1_0 = SssomVersion_V1_0.singleton
SSSOM_VERSION_1_1 = SssomVersion_V1_1.singleton
DIAGNOSTIC_ERROR = DiagnosticSeverity_Error.singleton
DIAGNOSTIC_WARNING = DiagnosticSeverity_Warning.singleton

__all__ = [
    "__version__",
    "EntityReference", "SssomDate", "UriReference",
    "ExtensionDefinition", "ExtensionValue", "PrefixEntry",
    "Mapping", "MappingSet", "SssomDocument",
    "DecodeResult", "EncodeResult", "SssomCodec", "SssomCodecException", "SssomDiagnostic",
    "DIAGNOSTIC_ERROR", "DIAGNOSTIC_WARNING",
    "SlotDescriptor", "SlotVersionDescriptor",
    "SSSOM_VERSION_1_0", "SSSOM_VERSION_1_1",
    "built_in_prefixes", "contract_curie", "expand_curie", "is_built_in_prefix",
    "try_contract_curie", "try_expand_curie",
    "parse_sssom_version", "sssom_version_to_lexical", "try_parse_sssom_version",
    "minimum_entity_type_version", "parse_entity_type", "entity_type_to_lexical", "try_parse_entity_type",
    "minimum_mapping_cardinality_version", "parse_mapping_cardinality",
    "mapping_cardinality_to_lexical", "try_parse_mapping_cardinality",
    "parse_predicate_modifier", "predicate_modifier_to_lexical", "try_parse_predicate_modifier",
    "mapping_descriptors", "try_find_mapping_descriptor",
    "mapping_set_descriptors", "try_find_mapping_set_descriptor",
]
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

    writeText (Path.Combine(moduleRoot, "__init__.py")) pythonInit
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
    let authored = SssomDocument.Create("https://example.org/authored", "https://example.org/license")
    authored.Metadata.EnsurePrefix("uuid", "urn:uuid:")
    let mapping = Mapping.CreateEntityMapping("skos:Concept", "skos:exactMatch", "skos:Collection", "semapv:ManualMappingCuration")
    authored.AddMappingWithRecordId("urn:uuid:smoke", mapping)
    let clone = authored.Clone()
    clone.Mappings.[0].Comment <- Some "clone only"
    if authored.Mappings.[0].Comment.IsSome then failwith "Expected an isolated clone"
    if authored.TryFindMappingByRecordId("urn:uuid:smoke") |> Option.isNone then failwith "Expected record lookup"
    let authoredContent = SssomCodec.EncodeCanonical authored
    if not (SssomCodec.DecodeEmbedded(authoredContent).Mappings.[0].RecordId.IsSome) then failwith "Expected authored record ID"

    let source = "#mapping_set_id: https://example.org/mappings\n#license: https://example.org/license\nsubject_id\tpredicate_id\tobject_id\tmapping_justification\nskos:Concept\tskos:exactMatch\tskos:Collection\tsemapv:ManualMappingCuration\n"
    let document = SssomCodec.DecodeEmbedded source
    if document.Mappings.Length <> 1 then failwith "Expected one mapping"
    if SssomCodec.EncodeCanonical(document) <> source then failwith "Expected a stable canonical round trip"
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
        $"""import {{ EntityReference, Mapping, SssomCodec, SssomDocument, expandCurie, mappingDescriptors, version }} from '@nfdi4plants/polyglot-sssom';
if (version !== '{packageVersion.SemVer}') throw new Error(`Unexpected version ${{version}}`);
const document = SssomDocument.Create('https://example.org/mappings', 'https://example.org/license');
document.Metadata.EnsurePrefix('uuid', 'urn:uuid:');
const mapping = Mapping.CreateEntityMapping('skos:Concept', 'skos:exactMatch', 'skos:Collection', 'semapv:ManualMappingCuration');
document.AddMappingWithRecordId('urn:uuid:smoke', mapping);
const clone = document.Clone();
clone.Mappings[0].Comment = 'clone only';
if (document.Mappings.length !== 1 || document.Mappings[0].Comment !== undefined) throw new Error('JavaScript authoring clone smoke failed');
if (document.TryFindMappingByRecordId('urn:uuid:smoke') === undefined) throw new Error('JavaScript record lookup smoke failed');
const authored = SssomCodec.EncodeCanonical(document);
if (!SssomCodec.DecodeEmbedded(authored).Mappings[0].RecordId) throw new Error('JavaScript authoring codec smoke failed');
if (mappingDescriptors().length !== 51) throw new Error('JavaScript descriptors smoke failed');
if (expandCurie([], 'skos:exactMatch') !== 'http://www.w3.org/2004/02/skos/core#exactMatch') throw new Error('JavaScript CURIE smoke failed');
const source = '#mapping_set_id: https://example.org/mappings\n#license: https://example.org/license\nsubject_id\tpredicate_id\tobject_id\tmapping_justification\nskos:Concept\tskos:exactMatch\tskos:Collection\tsemapv:ManualMappingCuration\n';
const decoded = SssomCodec.TryDecodeEmbedded(source);
if (!decoded.IsSuccess || SssomCodec.EncodeCanonical(decoded.Document) !== source) throw new Error('JavaScript codec smoke failed');
let rejectedInvalidReference = false;
try {{ new EntityReference('not-an-identifier'); }} catch {{ rejectedInvalidReference = true; }}
if (!rejectedInvalidReference) throw new Error('JavaScript lexical constructor bypassed validation');
console.log('npm smoke OK');
"""

    run npmCommand (npmArguments [ "install"; npmPackage; "--ignore-scripts"; "--no-audit"; "--no-fund" ]) directory []
    run "node" [ "smoke.mjs" ] directory []

let private smokePython pythonPackage =
    let directory = Path.Combine(smokeDir, "python")
    recreateUnder artifactsDir directory

    writeText
        (Path.Combine(directory, "smoke.py"))
        $"""import polyglot_sssom as sssom

assert sssom.__version__ == '{packageVersion.Pep440}'
document = sssom.SssomDocument.Create('https://example.org/mappings', 'https://example.org/license')
document.Metadata.EnsurePrefix('uuid', 'urn:uuid:')
mapping = sssom.Mapping.CreateEntityMapping('skos:Concept', 'skos:exactMatch', 'skos:Collection', 'semapv:ManualMappingCuration')
document.AddMappingWithRecordId('urn:uuid:smoke', mapping)
clone = document.Clone()
clone.Mappings[0].Comment = 'clone only'
assert len(document.Mappings) == 1 and document.Mappings[0].Comment is None
assert document.TryFindMappingByRecordId('urn:uuid:smoke') is not None
authored = sssom.SssomCodec.EncodeCanonical(document)
assert sssom.SssomCodec.DecodeEmbedded(authored).Mappings[0].RecordId is not None
assert len(sssom.mapping_descriptors()) == 51
assert sssom.expand_curie([], 'skos:exactMatch') == 'http://www.w3.org/2004/02/skos/core#exactMatch'
source = '#mapping_set_id: https://example.org/mappings\n#license: https://example.org/license\nsubject_id\tpredicate_id\tobject_id\tmapping_justification\nskos:Concept\tskos:exactMatch\tskos:Collection\tsemapv:ManualMappingCuration\n'
decoded = sssom.SssomCodec.TryDecodeEmbedded(source)
assert decoded.IsSuccess and sssom.SssomCodec.EncodeCanonical(decoded.Document) == source
try:
    sssom.EntityReference('not-an-identifier')
    raise AssertionError('Python lexical constructor bypassed validation')
except ValueError:
    pass
except Exception as error:
    if 'not a valid URI or CURIE' not in str(error):
        raise
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
