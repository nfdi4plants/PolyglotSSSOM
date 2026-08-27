// From the repository root:
// dotnet build src/SSSOM/SSSOM.fsproj
// dotnet fsi playground/write-insdcer-arc-base.fsx ../ER_ontologies/mappings/INSDCER-ARC.sssom.tsv
#r "nuget: Fable.Core, 5.2.0"
#r "nuget: YAMLicious, 1.0.0"
#r "../src/SSSOM/bin/SSSOM/Debug/netstandard2.0/PolyglotSSSOM.dll"

open System
open System.IO
open System.Text
open SSSOM

type MappingRow = {
    SubjectId: string
    SubjectLabel: string
    SubjectFile: string
    ObjectId: string
    ObjectLabel: string
    ObjectFile: string
}

let outputPath =
    match fsi.CommandLineArgs |> Array.skip 1 with
    | [| path |] -> Path.GetFullPath path
    | _ ->
        invalidArg
            "outputPath"
            "Pass exactly one output path, for example ../ER_ontologies/mappings/INSDCER-ARC.sssom.tsv."

let repository = "https://github.com/nfdi4plants/ER_ontologies"
let mappingSetId = repository + "/mappings/INSDCER-ARC-base"
let recordBase = mappingSetId + "/record/"
let ontologyFile folder name = repository + "/blob/main/ontologies/" + folder + "/" + name

let rows = [|
    { SubjectId = "INSDCER:1000001"
      SubjectLabel = "BioProject archive accession"
      SubjectFile = "INSDC.BioProject.obo"
      ObjectId = "INVMSO:00000008"
      ObjectLabel = "Investigation Identifier"
      ObjectFile = "INVMSO.obo" }
    { SubjectId = "INSDCER:1000014"
      SubjectLabel = "BioProject title"
      SubjectFile = "INSDC.BioProject.obo"
      ObjectId = "INVMSO:00000009"
      ObjectLabel = "Investigation Title"
      ObjectFile = "INVMSO.obo" }
    { SubjectId = "INSDCER:1000015"
      SubjectLabel = "BioProject description"
      SubjectFile = "INSDC.BioProject.obo"
      ObjectId = "INVMSO:00000010"
      ObjectLabel = "Investigation Description"
      ObjectFile = "INVMSO.obo" }
    { SubjectId = "INSDCER:1000017"
      SubjectLabel = "BioProject first public date"
      SubjectFile = "INSDC.BioProject.obo"
      ObjectId = "INVMSO:00000012"
      ObjectLabel = "Investigation Public Release Date"
      ObjectFile = "INVMSO.obo" }
    { SubjectId = "INSDCER:2000001"
      SubjectLabel = "Study archive accession"
      SubjectFile = "INSDC.Study.obo"
      ObjectId = "STDMSO:00000003"
      ObjectLabel = "Study Identifier"
      ObjectFile = "STDMSO.obo" }
    { SubjectId = "INSDCER:2000014"
      SubjectLabel = "Study title"
      SubjectFile = "INSDC.Study.obo"
      ObjectId = "STDMSO:00000004"
      ObjectLabel = "Study Title"
      ObjectFile = "STDMSO.obo" }
    { SubjectId = "INSDCER:2000019"
      SubjectLabel = "Study description"
      SubjectFile = "INSDC.Study.obo"
      ObjectId = "STDMSO:00000005"
      ObjectLabel = "Study Description"
      ObjectFile = "STDMSO.obo" }
|]

let metadata = MappingSet.Create(mappingSetId, "https://opensource.org/license/mit")
metadata.SssomVersion <- Some SssomVersion.V1_1
metadata.MappingSetVersion <- Some "2026-08-27"
metadata.MappingSetTitle <- Some "INSDCER to ARC base structural mappings"
metadata.MappingSetDescription <-
    Some
        "Conservative exact field mappings for the profile that represents the root INSDC BioProject as an ARC Investigation. Record placement, selectors, transformations, and process construction remain application policy."
metadata.CreatorId <- [| EntityReference.Create "https://github.com/nfdi4plants" |]
metadata.CreatorLabel <- [| "DataPLANT" |]
metadata.MappingProvider <- Some(UriReference.Create repository)
metadata.MappingTool <- Some "PolyglotSSSOM"
metadata.MappingToolVersion <- Some "0.1.0-alpha.1"
metadata.MappingDate <- Some(SssomDate.Create "2026-08-27")
metadata.IssueTracker <- Some(UriReference.Create(repository + "/issues"))
metadata.Comment <-
    Some "This base mapping is intentionally incomplete; see mappings/INSDCER-ARC-review.md."

// These repository-owned expansions are deliberate placeholders until real
// persistent namespaces are registered. Do not claim unprovisioned OBO PURLs.
metadata.EnsurePrefix(
    "INSDCER",
    repository + "/tree/main/ontologies/INSDC?term=INSDCER_"
)
metadata.EnsurePrefix(
    "INVMSO",
    repository + "/blob/main/ontologies/ARCSO/INVMSO.obo?term=INVMSO_"
)
metadata.EnsurePrefix(
    "STDMSO",
    repository + "/blob/main/ontologies/ARCSO/STDMSO.obo?term=STDMSO_"
)
metadata.EnsurePrefix("github", "https://github.com/")
metadata.EnsurePrefix("er", repository + "/")
metadata.EnsurePrefix("insdcarc", recordBase)

let document = SssomDocument(metadata, [||])

for row in rows do
    let mapping =
        Mapping.CreateEntityMapping(
            row.SubjectId,
            "skos:exactMatch",
            row.ObjectId,
            "semapv:ManualMappingCuration"
        )

    mapping.SubjectLabel <- Some row.SubjectLabel
    mapping.ObjectLabel <- Some row.ObjectLabel
    mapping.SubjectSource <-
        Some(EntityReference.Create(ontologyFile "INSDC" row.SubjectFile))
    mapping.ObjectSource <-
        Some(EntityReference.Create(ontologyFile "ARCSO" row.ObjectFile))
    mapping.Confidence <- Some 1.0

    let recordName =
        row.SubjectId.Replace(":", "_") + "-" + row.ObjectId.Replace(":", "_")

    document.AddMappingWithRecordId(recordBase + recordName, mapping)

let errors =
    SssomCodec.Validate document
    |> Array.filter (fun diagnostic -> diagnostic.Severity = DiagnosticSeverity.Error)

if errors.Length > 0 then
    errors
    |> Array.map (fun diagnostic -> diagnostic.Code + ": " + diagnostic.Message)
    |> String.concat Environment.NewLine
    |> failwith

let content = SssomCodec.EncodeCanonical document
let outputDirectory = Path.GetDirectoryName outputPath

if not (String.IsNullOrWhiteSpace outputDirectory) then
    Directory.CreateDirectory outputDirectory |> ignore

File.WriteAllText(outputPath, content, UTF8Encoding(false))

let roundTrip = File.ReadAllText outputPath |> SssomCodec.DecodeEmbedded
if SssomCodec.EncodeCanonical roundTrip <> content then
    failwith "The written mapping did not round-trip through the canonical codec."

printfn "Wrote %d canonical SSSOM mappings to %s" document.Mappings.Length outputPath
