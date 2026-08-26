module SSSOM.Tests.AuthoringTests

open Fable.Pyxpecto
open SSSOM

let private errorText (diagnostics: SssomDiagnostic array) =
    diagnostics
    |> Array.filter (fun diagnostic -> diagnostic.Severity = DiagnosticSeverity.Error)
    |> Array.map (fun diagnostic -> diagnostic.Code + ": " + diagnostic.Message)
    |> fun lines -> System.String.Join(" | ", lines)

let private expectValid document =
    let diagnostics = SssomCodec.Validate document
    Expect.isFalse (diagnostics |> Array.exists (fun diagnostic -> diagnostic.Severity = DiagnosticSeverity.Error)) (errorText diagnostics)

let private factoryTests =
    testList "Factories" [
        testCase "lexical factories create a minimal entity mapping document" <| fun _ ->
            let document = SssomDocument.Create("https://example.org/mappings", "https://example.org/license")
            let mapping =
                Mapping.CreateEntityMapping(
                    "skos:Concept",
                    "skos:exactMatch",
                    "skos:Collection",
                    "semapv:ManualMappingCuration"
                )

            document.AddMapping mapping

            Expect.equal document.Metadata.MappingSetId.Value "https://example.org/mappings" "mapping-set ID"
            Expect.equal document.Mappings.Length 1 "mapping count"
            Expect.equal document.Mappings.[0].SubjectId.Value.Value "skos:Concept" "subject"
            Expect.equal document.Mappings.[0].ObjectId.Value.Value "skos:Collection" "object"
            Expect.isNone document.Mappings.[0].RecordId "format-general add does not invent an ID"
            expectValid document

        testCase "NoTermFound factory supplies the required cardinality" <| fun _ ->
            let document = SssomDocument.Create("https://example.org/mappings", "https://example.org/license")
            let mapping =
                Mapping.CreateNoTermFoundMapping(
                    "skos:Concept",
                    "skos:exactMatch",
                    "semapv:ManualMappingCuration"
                )

            document.AddMapping mapping

            Expect.equal mapping.ObjectId.Value.Value "sssom:NoTermFound" "negative target"
            Expect.equal mapping.MappingCardinality (Some MappingCardinality.OneToNone) "negative cardinality"
            expectValid document
    ]

let private prefixTests =
    testCase "EnsurePrefix is idempotent and rejects conflicts" <| fun _ ->
        let metadata = MappingSet.Create("https://example.org/mappings", "https://example.org/license")
        metadata.EnsurePrefix("ex", "https://example.org/")
        metadata.EnsurePrefix("ex", "https://example.org/")
        metadata.EnsurePrefix("skos", "http://www.w3.org/2004/02/skos/core#")

        Expect.equal metadata.CurieMap.Length 1 "document prefix is added once and built-ins remain implicit"
        Expect.throws (fun () -> metadata.EnsurePrefix("ex", "https://other.example.org/")) "conflicting prefix"
        Expect.throws (fun () -> metadata.EnsurePrefix("skos", "https://wrong.example.org/")) "conflicting built-in"
        Expect.throws (fun () -> metadata.EnsurePrefix("relative", "prefix/")) "relative prefix expansion"

let private editingTests =
    testList "Document editing" [
        testCase "AddMappingWithRecordId assigns an ID, promotes v1.0, and rejects collisions atomically" <| fun _ ->
            let document = SssomDocument.Create("https://example.org/mappings", "https://example.org/license")
            document.Metadata.SssomVersion <- Some SssomVersion.V1_0
            let first = Mapping.CreateEntityMapping("skos:Concept", "skos:exactMatch", "skos:Collection", "semapv:ManualMappingCuration")
            document.AddMappingWithRecordId("urn:uuid:first", first)

            Expect.equal first.RecordId.Value.Value "urn:uuid:first" "assigned ID"
            Expect.equal document.Metadata.SssomVersion (Some SssomVersion.V1_1) "version promotion"
            Expect.equal (document.TryFindMappingByRecordId("urn:uuid:first")).Value first "record lookup"

            let duplicate = Mapping.CreateEntityMapping("skos:Collection", "skos:exactMatch", "skos:Concept", "semapv:ManualMappingCuration")
            Expect.throws (fun () -> document.AddMappingWithRecordId("urn:uuid:first", duplicate)) "duplicate record ID"
            Expect.isNone duplicate.RecordId "failed add does not mutate the candidate"
            Expect.equal document.Mappings.Length 1 "failed add is atomic"

        testCase "generic add preserves an existing record ID and promotes v1.1 data" <| fun _ ->
            let document = SssomDocument.Create("https://example.org/mappings", "https://example.org/license")
            document.Metadata.SssomVersion <- Some SssomVersion.V1_0
            let mapping = Mapping.CreateEntityMapping("skos:Concept", "skos:exactMatch", "skos:Collection", "semapv:ManualMappingCuration")
            mapping.RecordId <- Some(EntityReference.Create "urn:uuid:existing")

            document.AddMapping mapping

            Expect.equal document.Mappings.[0].RecordId.Value.Value "urn:uuid:existing" "existing ID"
            Expect.equal document.Metadata.SssomVersion (Some SssomVersion.V1_1) "version promotion"

        testCase "replace retains identity and remove returns the selected mapping" <| fun _ ->
            let document = SssomDocument.Create("https://example.org/mappings", "https://example.org/license")
            let original = Mapping.CreateEntityMapping("skos:Concept", "skos:exactMatch", "skos:Collection", "semapv:ManualMappingCuration")
            document.AddMappingWithRecordId("urn:uuid:replace", original)

            let replacement = original.Clone()
            replacement.ObjectId <- Some(EntityReference.Create "skos:OrderedCollection")
            let replaced = document.ReplaceMappingByRecordId("urn:uuid:replace", replacement)

            Expect.equal replaced original "previous mapping"
            Expect.equal (document.TryFindMappingByRecordId("urn:uuid:replace")).Value.ObjectId.Value.Value "skos:OrderedCollection" "replacement"

            let wrongIdentity = replacement.Clone()
            wrongIdentity.RecordId <- Some(EntityReference.Create "urn:uuid:other")
            Expect.throws (fun () -> document.ReplaceMappingByRecordId("urn:uuid:replace", wrongIdentity) |> ignore) "identity mismatch"

            let removed = document.RemoveMappingByRecordId("urn:uuid:replace")
            Expect.equal removed.Value replacement "removed mapping"
            Expect.isNone (document.RemoveMappingByRecordId("urn:uuid:replace")) "missing record"
    ]

let private cloneTest =
    testCase "deep clone isolates every mutable nested model object" <| fun _ ->
        let metadata = MappingSet.Create("https://example.org/mappings", "https://example.org/license")
        metadata.EnsurePrefix("ex", "https://example.org/")
        metadata.CreatorLabel <- [| "original creator" |]
        metadata.ExtensionDefinitions <- [| ExtensionDefinition("ext_note", property = EntityReference.Create "ex:note") |]
        metadata.ExtensionValues <- [| ExtensionValue("ext_note", "original metadata") |]

        let mapping = Mapping.CreateEntityMapping("ex:subject", "skos:exactMatch", "ex:object", "semapv:ManualMappingCuration")
        mapping.AuthorLabel <- [| "original author" |]
        mapping.ExtensionValues <- [| ExtensionValue("ext_note", "original mapping") |]
        let original = SssomDocument(metadata, [| mapping |])
        let clone = original.Clone()

        clone.Metadata.CurieMap.[0].PrefixUrl <- UriReference.Create "https://clone.example.org/"
        clone.Metadata.CreatorLabel.[0] <- "clone creator"
        clone.Metadata.ExtensionDefinitions.[0].SlotName <- "ext_clone"
        clone.Metadata.ExtensionValues.[0].Value <- "clone metadata"
        clone.Mappings.[0].AuthorLabel.[0] <- "clone author"
        clone.Mappings.[0].ExtensionValues.[0].Value <- "clone mapping"

        Expect.equal original.Metadata.CurieMap.[0].PrefixUrl.Value "https://example.org/" "prefix isolation"
        Expect.equal original.Metadata.CreatorLabel.[0] "original creator" "metadata array isolation"
        Expect.equal original.Metadata.ExtensionDefinitions.[0].SlotName "ext_note" "definition isolation"
        Expect.equal original.Metadata.ExtensionValues.[0].Value "original metadata" "metadata extension isolation"
        Expect.equal original.Mappings.[0].AuthorLabel.[0] "original author" "mapping array isolation"
        Expect.equal original.Mappings.[0].ExtensionValues.[0].Value "original mapping" "mapping extension isolation"

let private editedRoundTripTest =
    testCase "an edited canonical clone round trips without mutating the imported document" <| fun _ ->
        let imported = SssomCodec.DecodeEmbedded(CanonicalFixture.content ())
        let working = imported.Clone()
        working.Metadata.EnsurePrefix("uuid", "urn:uuid:")
        working.Mappings.[0].RecordId <- Some(EntityReference.Create "urn:uuid:imported")

        let added = Mapping.CreateEntityMapping("ex:a", "skos:exactMatch", "ex:d", "semapv:ManualMappingCuration")
        working.AddMappingWithRecordId("urn:uuid:added", added)

        let encodedResult = SssomCodec.TryEncodeCanonical working
        Expect.isTrue encodedResult.IsSuccess (errorText encodedResult.Diagnostics)
        let encoded = encodedResult.Content.Value
        let decodedResult = SssomCodec.TryDecodeEmbedded encoded
        Expect.isTrue decodedResult.IsSuccess (errorText decodedResult.Diagnostics + "\n" + encoded)
        let roundTripped = decodedResult.Document.Value

        Expect.isNone imported.Metadata.SssomVersion "imported version remains absent"
        Expect.isNone imported.Mappings.[0].RecordId "imported mapping remains unchanged"
        Expect.equal imported.Mappings.Length 1 "imported mapping array remains unchanged"
        Expect.isTrue (encoded.StartsWith("#sssom_version: 1.1\n")) "edited copy is v1.1"
        Expect.equal roundTripped.Mappings.Length 2 "round-trip mapping count"
        Expect.isTrue (roundTripped.Mappings |> Array.forall (fun mapping -> mapping.RecordId.IsSome)) "round-trip record IDs"

let tests =
    testList "Authoring ergonomics" [
        factoryTests
        prefixTests
        editingTests
        cloneTest
        editedRoundTripTest
    ]
