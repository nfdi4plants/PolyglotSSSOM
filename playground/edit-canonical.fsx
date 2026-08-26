// From the repository root:
// dotnet build src/SSSOM/SSSOM.fsproj
// dotnet fsi playground/edit-canonical.fsx
#r "nuget: Fable.Core, 5.2.0"
#r "nuget: YAMLicious, 1.0.0"
#r "../src/SSSOM/bin/SSSOM/Debug/netstandard2.0/PolyglotSSSOM.dll"

open System.IO
open SSSOM

let fixturePath =
    Path.GetFullPath(
        Path.Combine(__SOURCE_DIRECTORY__, "..", "tests", "fixtures", "canonical-example.sssom.tsv")
    )

let imported = File.ReadAllText fixturePath |> SssomCodec.DecodeEmbedded
let working = imported.Clone()

working.Metadata.EnsurePrefix("uuid", "urn:uuid:")
working.Mappings.[0].RecordId <- Some(EntityReference.Create "urn:uuid:imported")

let added =
    Mapping.CreateEntityMapping(
        "ex:a",
        "skos:exactMatch",
        "ex:d",
        "semapv:ManualMappingCuration"
    )

working.AddMappingWithRecordId("urn:uuid:added", added)

let diagnostics = SssomCodec.Validate working

for diagnostic in diagnostics do
    printfn "%A %s: %s" diagnostic.Severity diagnostic.Code diagnostic.Message

let encoded = SssomCodec.EncodeCanonical working

printfn "Imported mappings: %d" imported.Mappings.Length
printfn "Edited mappings: %d" working.Mappings.Length
printfn "\n%s" encoded
