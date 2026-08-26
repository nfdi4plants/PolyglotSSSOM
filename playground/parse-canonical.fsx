// From the repository root:
// dotnet build src/SSSOM/SSSOM.fsproj
// dotnet fsi playground/parse-canonical.fsx
#r "nuget: Fable.Core, 5.2.0"
#r "nuget: YAMLicious, 1.0.0"
#r "../src/SSSOM/bin/SSSOM/Debug/netstandard2.0/PolyglotSSSOM.dll"

open System
open System.IO
open SSSOM

let fixturePath =
    Path.GetFullPath(
        Path.Combine(__SOURCE_DIRECTORY__, "..", "tests", "fixtures", "canonical-example.sssom.tsv")
    )

let input = File.ReadAllText fixturePath
let document = SssomCodec.DecodeEmbedded input

printfn "Parsed %d mapping(s) from %s" document.Mappings.Length fixturePath
printfn "Mapping set: %s" document.Metadata.MappingSetId.Value

for mapping in document.Mappings do
    printfn
        "  %s %s %s"
        (mapping.SubjectId |> Option.map (fun value -> value.Value) |> Option.defaultValue "<literal>")
        mapping.PredicateId.Value
        (mapping.ObjectId |> Option.map (fun value -> value.Value) |> Option.defaultValue "<literal>")

let canonical = SssomCodec.EncodeCanonical document

if canonical <> input then
    failwith "The canonical fixture did not round-trip byte-for-byte."

printfn "Canonical round trip: OK"
