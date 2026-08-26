module SSSOM.Tests.CodecTests

open Fable.Pyxpecto
open SSSOM

let private diagnosticsText (diagnostics: SssomDiagnostic array) =
    diagnostics
    |> Array.map (fun diagnostic -> diagnostic.Code + ": " + diagnostic.Message)
    |> fun lines -> System.String.Join(" | ", lines)

let private decoded (result: DecodeResult) =
    Expect.isTrue result.IsSuccess (diagnosticsText result.Diagnostics)
    result.Document |> Option.get

let private entity value = EntityReference.Create value
let private uri value = UriReference.Create value

let private v1_0Embedded =
    "#curie_map:\n"
    + "#  ex: https://example.org/\n"
    + "#mapping_set_id: https://example.org/set\n"
    + "#license: https://example.org/license\n"
    + "#mapping_tool: mapper\n"
    + "subject_id\tpredicate_id\tobject_id\tmapping_justification\tconfidence\n"
    + "ex:b\tskos:exactMatch\tex:c\tsemapv:ManualMappingCuration\t0.9555\n"

let private v1_0Canonical =
    "#curie_map:\n"
    + "#  ex: https://example.org/\n"
    + "#mapping_set_id: https://example.org/set\n"
    + "#license: https://example.org/license\n"
    + "#mapping_tool: mapper\n"
    + "subject_id\tpredicate_id\tobject_id\tmapping_justification\tconfidence\n"
    + "ex:b\tskos:exactMatch\tex:c\tsemapv:ManualMappingCuration\t0.956\n"

let private decodeTests =
    testList "Decoding" [
        testCase "embedded v1.0 metadata and mappings decode with propagation" <| fun _ ->
            let document = SssomCodec.TryDecodeEmbedded v1_0Embedded |> decoded
            Expect.isNone document.Metadata.SssomVersion "versionless input remains lexically versionless"
            Expect.isNone document.Metadata.MappingTool "unambiguous metadata is propagated into mappings"
            Expect.equal document.Mappings.Length 1 "mapping count"
            Expect.equal document.Mappings.[0].MappingTool (Some "mapper") "mapping tool propagation"
            Expect.equal document.Mappings.[0].Confidence (Some 0.9555) "input precision is retained in memory"

        testCase "external metadata accepts scalar shorthand for a multivalued slot" <| fun _ ->
            let metadata =
                "curie_map:\n"
                + "  ex: https://example.org/\n"
                + "mapping_set_id: https://example.org/set\n"
                + "license: https://example.org/license\n"
                + "creator_id: ex:creator\n"

            let tsv =
                "subject_id\tpredicate_id\tobject_id\tmapping_justification\tauthor_id\n"
                + "ex:s\tskos:exactMatch\tex:o\tsemapv:ManualMappingCuration\tex:a|ex:b\n"

            let document = SssomCodec.TryDecodeExternal(metadata, tsv) |> decoded
            Expect.equal document.Metadata.CreatorId.Length 1 "scalar metadata shorthand"
            Expect.equal document.Metadata.CreatorId.[0].Value "ex:creator" "creator value"
            Expect.equal (document.Mappings.[0].AuthorId |> Array.map (fun value -> value.Value)) [| "ex:a"; "ex:b" |] "v1.0 multivalue"

        testCase "v1.1 multivalue escaping is parsed left to right" <| fun _ ->
            let input =
                "#sssom_version: 1.1\n"
                + "#curie_map:\n"
                + "#  ex: https://example.org/\n"
                + "#mapping_set_id: https://example.org/set\n"
                + "#license: https://example.org/license\n"
                + "record_id\tsubject_id\tpredicate_id\tobject_id\tmapping_justification\tderived_from\n"
                + "ex:r1\tex:s\tskos:exactMatch\tex:o\tsemapv:ManualMappingCuration\tex:a\\|b|ex:c\\\\d|ex:e\\\\|ex:f\n"

            let document = SssomCodec.TryDecodeEmbedded input |> decoded
            let values = document.Mappings.[0].DerivedFrom |> Array.map (fun value -> value.Value)
            Expect.equal values [| "ex:a|b"; "ex:c\\d"; "ex:e\\"; "ex:f" |] "escape order"

        testCase "quoted TSV values retain tabs, quotes, and line breaks" <| fun _ ->
            let input =
                "#curie_map:\n"
                + "#  ex: https://example.org/\n"
                + "#mapping_set_id: https://example.org/set\n"
                + "#license: https://example.org/license\n"
                + "subject_id\tpredicate_id\tobject_id\tmapping_justification\tcomment\n"
                + "ex:s\tskos:exactMatch\tex:o\tsemapv:ManualMappingCuration\t\"first\tline\nsecond \"\"line\"\"\"\n"

            let document = SssomCodec.TryDecodeEmbedded input |> decoded
            Expect.equal document.Mappings.[0].Comment (Some "first\tline\nsecond \"line\"") "quoted value"
    ]

let private canonicalTests =
    testList "Canonical encoding" [
        testCase "v1.0 output is deterministic and rounds numbers to three decimals" <| fun _ ->
            let document = SssomCodec.DecodeEmbedded v1_0Embedded
            Expect.equal (SssomCodec.EncodeCanonical document) v1_0Canonical "canonical golden output"

        testCase "encoding condenses common values without mutating mappings" <| fun _ ->
            let metadata =
                MappingSet(
                    uri "https://example.org/set",
                    uri "https://example.org/license",
                    curieMap = [| PrefixEntry("ex", uri "https://example.org/") |]
                )

            let first = Mapping(entity "skos:exactMatch", entity "semapv:ManualMappingCuration", subjectId = entity "ex:b", objectId = entity "ex:c", mappingTool = "mapper")
            let second = Mapping(entity "skos:exactMatch", entity "semapv:ManualMappingCuration", subjectId = entity "ex:a", objectId = entity "ex:d", mappingTool = "mapper")
            let document = SssomDocument(metadata, [| first; second |])
            let encoded = SssomCodec.EncodeCanonical document

            Expect.stringContains encoded "#mapping_tool: mapper\n" "common propagated value is condensed"
            Expect.equal first.MappingTool (Some "mapper") "first caller row is not mutated"
            Expect.equal second.MappingTool (Some "mapper") "second caller row is not mutated"
            Expect.isNone metadata.MappingTool "caller metadata is not mutated"
            let rows = encoded.Split('\n') |> Array.filter (fun line -> line.StartsWith("ex:"))
            Expect.isTrue (rows.[0].StartsWith("ex:a")) "rows use ordinal descriptor-order sorting"

        testCase "v1.1 is inferred when a multivalue contains a literal pipe" <| fun _ ->
            let metadata =
                MappingSet(
                    uri "https://example.org/set",
                    uri "https://example.org/license",
                    curieMap = [| PrefixEntry("ex", uri "https://example.org/") |]
                )

            let mapping =
                Mapping(
                    entity "skos:exactMatch",
                    entity "semapv:ManualMappingCuration",
                    subjectId = entity "ex:s",
                    objectId = entity "ex:o",
                    authorLabel = [| "left|right" |]
                )

            let encoded = SssomCodec.EncodeCanonical(SssomDocument(metadata, [| mapping |]))
            Expect.isTrue (encoded.StartsWith("#sssom_version: 1.1\n")) "version marker"
            Expect.stringContains encoded "left\\|right" "literal pipe escaping"

        testCase "canonical output is stable over decode encode decode" <| fun _ ->
            let first = SssomCodec.DecodeEmbedded v1_0Embedded |> SssomCodec.EncodeCanonical
            let second = SssomCodec.DecodeEmbedded first |> SssomCodec.EncodeCanonical
            Expect.equal second first "semantic round trip"
    ]

let private extensionTests =
    testCase "declared extensions are retained and undeclared slots are warned and discarded" <| fun _ ->
        let input =
            "#sssom_version: 1.1\n"
            + "#curie_map:\n"
            + "#  ex: https://example.org/\n"
            + "#mapping_set_id: https://example.org/set\n"
            + "#license: https://example.org/license\n"
            + "#extension_definitions:\n"
            + "#  - slot_name: ext_note\n"
            + "#    property: ex:note\n"
            + "#ext_note: set-note\n"
            + "#unknown: discarded\n"
            + "subject_id\tpredicate_id\tobject_id\tmapping_justification\text_note\tbad\n"
            + "ex:s\tskos:exactMatch\tex:o\tsemapv:ManualMappingCuration\trow-note\tdiscarded\n"

        let result = SssomCodec.TryDecodeEmbedded input
        let document = decoded result
        Expect.equal document.Metadata.ExtensionDefinitions.Length 1 "definition"
        Expect.equal document.Metadata.ExtensionValues.[0].Value "set-note" "metadata extension"
        Expect.equal document.Mappings.[0].ExtensionValues.[0].Value "row-note" "mapping extension"
        Expect.equal (result.Diagnostics |> Array.filter (fun diagnostic -> diagnostic.Code = "SSSOM_UNKNOWN_SLOT") |> Array.length) 2 "discard warnings"
        let encoded = SssomCodec.EncodeCanonical document
        Expect.stringContains encoded "#ext_note: set-note" "metadata extension encoding"
        Expect.stringContains encoded "\text_note\n" "mapping extension column"
        Expect.isFalse (encoded.Contains("unknown")) "unknown metadata is absent"
        Expect.isFalse (encoded.Contains("\tbad")) "unknown column is absent"

let private validationTests =
    testList "Validation and diagnostics" [
        testCase "literal mappings and NoTermFound cardinality satisfy conditional rules" <| fun _ ->
            let metadata = MappingSet(uri "https://example.org/set", uri "https://example.org/license")
            let literal =
                Mapping(
                    entity "skos:exactMatch",
                    entity "semapv:ManualMappingCuration",
                    subjectLabel = "literal",
                    subjectType = EntityType.RdfsLiteral,
                    objectId = entity "ex:o"
                )

            metadata.CurieMap <- [| PrefixEntry("ex", uri "https://example.org/") |]
            let noMatch =
                Mapping(
                    entity "skos:exactMatch",
                    entity "semapv:ManualMappingCuration",
                    subjectId = entity "ex:s",
                    objectId = entity "sssom:NoTermFound",
                    mappingCardinality = MappingCardinality.OneToNone
                )

            let diagnostics = SssomCodec.Validate(SssomDocument(metadata, [| literal; noMatch |]))
            Expect.isFalse (diagnostics |> Array.exists (fun diagnostic -> diagnostic.Severity = DiagnosticSeverity.Error)) (diagnosticsText diagnostics)

        testCase "missing literal labels and reviewer identities are errors" <| fun _ ->
            let metadata = MappingSet(uri "https://example.org/set", uri "https://example.org/license", sssomVersion = SssomVersion.V1_1)
            let mapping =
                Mapping(
                    entity "skos:exactMatch",
                    entity "semapv:ManualMappingCuration",
                    subjectType = EntityType.RdfsLiteral,
                    objectId = entity "skos:Concept",
                    reviewerAgreement = 0.5
                )

            let diagnostics = SssomCodec.Validate(SssomDocument(metadata, [| mapping |]))
            let codes = diagnostics |> Array.map (fun diagnostic -> diagnostic.Code)
            Expect.containsAll codes [| "SSSOM_CONDITIONAL_REQUIREMENT" |] "conditional diagnostics"
            Expect.isTrue ((codes |> Array.filter ((=) "SSSOM_CONDITIONAL_REQUIREMENT") |> Array.length) >= 2) "both rules"

        testCase "record identifiers must be complete and unique" <| fun _ ->
            let metadata = MappingSet(uri "https://example.org/set", uri "https://example.org/license", sssomVersion = SssomVersion.V1_1)
            let mapping value =
                Mapping(
                    entity "skos:exactMatch",
                    entity "semapv:ManualMappingCuration",
                    recordId = entity value,
                    subjectId = entity "skos:Concept",
                    objectId = entity "skos:Collection"
                )

            let diagnostics = SssomCodec.Validate(SssomDocument(metadata, [| mapping "sssom:r"; mapping "sssom:r" |]))
            Expect.isTrue (diagnostics |> Array.exists (fun diagnostic -> diagnostic.Code = "SSSOM_RECORD_ID")) "duplicate record ID"

        testCase "decode reports duplicate metadata, row width, unknown prefixes, and version conflicts" <| fun _ ->
            let input =
                "#sssom_version: 1.0\n"
                + "#mapping_set_id: https://example.org/set\n"
                + "#mapping_set_id: https://example.org/duplicate\n"
                + "#license: https://example.org/license\n"
                + "record_id\tsubject_id\tpredicate_id\tobject_id\tmapping_justification\n"
                + "ex:r\tex:s\tskos:exactMatch\tex:o\n"

            let result = SssomCodec.TryDecodeEmbedded input
            let codes = result.Diagnostics |> Array.map (fun diagnostic -> diagnostic.Code)
            Expect.isFalse result.IsSuccess "invalid document"
            Expect.containsAll codes [| "SSSOM_DUPLICATE_METADATA"; "SSSOM_ROW_WIDTH"; "SSSOM_VERSION_CONFLICT" |] "structural diagnostics"

        testCase "unsupported versions are rejected without throwing from TryDecode" <| fun _ ->
            let input =
                "#sssom_version: 2.0\n"
                + "#mapping_set_id: https://example.org/set\n"
                + "#license: https://example.org/license\n"
                + "subject_id\tpredicate_id\tobject_id\tmapping_justification\n"

            let result = SssomCodec.TryDecodeEmbedded input
            Expect.isFalse result.IsSuccess "unsupported version"
            Expect.isTrue (result.Diagnostics |> Array.exists (fun diagnostic -> diagnostic.Code = "SSSOM_UNSUPPORTED_VERSION")) "stable code"

        testCase "undeclared CURIE prefixes are rejected" <| fun _ ->
            let input =
                "#mapping_set_id: https://example.org/set\n"
                + "#license: https://example.org/license\n"
                + "subject_id\tpredicate_id\tobject_id\tmapping_justification\n"
                + "ex:s\tskos:exactMatch\tex:o\tsemapv:ManualMappingCuration\n"

            let result = SssomCodec.TryDecodeEmbedded input
            Expect.isFalse result.IsSuccess "unknown prefixes prevent a document result"
            Expect.isTrue (result.Diagnostics |> Array.exists (fun diagnostic -> diagnostic.Code = "SSSOM_CURIE_PREFIX")) "prefix diagnostic"

        testCase "v1.1 rejects legacy relative URI values while v1.0 accepts them" <| fun _ ->
            let mappings =
                "subject_id\tpredicate_id\tobject_id\tmapping_justification\n"
                + "skos:Concept\tskos:exactMatch\tskos:Collection\tsemapv:ManualMappingCuration\n"

            let v1_0 =
                "#mapping_set_id: set.tsv\n"
                + "#license: license.txt\n"
                + mappings

            let v1_1 =
                "#sssom_version: 1.1\n"
                + "#mapping_set_id: set.tsv\n"
                + "#license: license.txt\n"
                + mappings

            Expect.isTrue (SssomCodec.TryDecodeEmbedded(v1_0).IsSuccess) "legacy v1.0 URI forms"
            let result = SssomCodec.TryDecodeEmbedded v1_1
            Expect.isFalse result.IsSuccess "v1.1 URI constraint"
            Expect.isTrue (result.Diagnostics |> Array.exists (fun diagnostic -> diagnostic.Code = "SSSOM_INVALID_VALUE")) "range diagnostic"
    ]

let tests =
    testList "TSV/YAML codecs" [
        decodeTests
        canonicalTests
        extensionTests
        validationTests
    ]
