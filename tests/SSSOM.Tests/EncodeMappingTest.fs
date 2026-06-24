module SSSOM.Tests.EncodeMappingTests

open System
open Xunit
open SSSOM

[<Fact>]
let ``EncodeMapping should parse mapping object without optional fields in valid TSV `` () =
    let input =
        EncodeMapping.EncodeMapping(
            [
                Mapping(predicate_id = EntityReference.create "skos:exactMatch", mapping_justification = EntityReference.create "semapv:ManualMappingCuration")
                Mapping(predicate_id = EntityReference.create "skos:broadMatch", mapping_justification = EntityReference.create "semapv:LogicalReasoning")
                Mapping(predicate_id = EntityReference.create "skos:closeMatch", mapping_justification = EntityReference.create "semapv:ManualMappingCuration")
            ]
        )

    let expected = 
        "predicate_id\tmapping_justification\n" +
        "skos:exactMatch\tsemapv:ManualMappingCuration\n" +
        "skos:broadMatch\tsemapv:LogicalReasoning\n" +
        "skos:closeMatch\tsemapv:ManualMappingCuration\n"

    Assert.Equal(input, expected)

[<Fact>]
let ``EncodeMapping should parse mapping object with optional fields in valid TSV `` () =
    let input =
        EncodeMapping.EncodeMapping(
            [
                Mapping(
                    predicate_id = EntityReference.create "skos:exactMatch",
                    mapping_justification = EntityReference.create "semapv:ManualMappingCuration",
                    Subject_id = EntityReference.create "KF_FOOD:F001",
                    Subject_label = "apple",
                    Object_id = EntityReference.create "FOODON:00002473",
                    Object_label = "apple (whole)",
                    Author_id = EntityReference.create "orcid:0000-0002-7356-1779",
                    Confidence = 0.95,
                    Comment = "We could map to FOODON:03310788 instead to cover sliced apples, but only 'whole' apple types exist."
                )
                Mapping(
                    predicate_id = EntityReference.create "skos:exactMatch",
                    mapping_justification = EntityReference.create "semapv:ManualMappingCuration",
                    Subject_id = EntityReference.create "KF_FOOD:F002",
                    Subject_label = "Gala apple (whole)",
                    Object_id = EntityReference.create "FOODON:00003348",
                    Object_label = "apple (whole)",
                    Author_id = EntityReference.create "orcid:0000-0002-7356-1779",
                    Confidence = 1.0,
                    Comment = ""
                )
                Mapping(
                    predicate_id = EntityReference.create "skos:exactMatch",
                    mapping_justification = EntityReference.create "semapv:ManualMappingCuration",
                    Subject_id = EntityReference.create "KF_FOOD:F003",
                    Subject_label = "pink",
                    Object_id = EntityReference.create "FOODON:00004186",
                    Object_label = "Pink apple (whole)",
                    Author_id = EntityReference.create "orcid:0000-0002-7356-1779",
                    Confidence = 0.9,
                    Comment = "We could map to FOODON:00004187 instead which more specifically refers to 'raw' Pink apples. Decided against to be consistent with other mapping choices."
                )
                Mapping(
                    predicate_id = EntityReference.create "skos:exactMatch",
                    mapping_justification = EntityReference.create "semapv:ManualMappingCuration",
                    Subject_id = EntityReference.create "KF_FOOD:F004",
                    Subject_label = "braeburn",
                    Object_id = EntityReference.create "FOODON:00002473",
                    Object_label = "apple (whole)",
                    Author_id = EntityReference.create "orcid:0000-0002-7356-1779",
                    Confidence = 1.0,
                    Comment = ""
                )
            ]
        )

    let expected = 
        "predicate_id\tmapping_justification\tsubject_id\tsubject_label\tobject_id\tobject_label\tauthor_id\tconfidence\tcomment\n" +
        "skos:exactMatch\tsemapv:ManualMappingCuration\tKF_FOOD:F001\tapple\tFOODON:00002473\tapple (whole)\torcid:0000-0002-7356-1779\t0.95\tWe could map to FOODON:03310788 instead to cover sliced apples, but only 'whole' apple types exist.\n" +
        "skos:exactMatch\tsemapv:ManualMappingCuration\tKF_FOOD:F002\tGala apple (whole)\tFOODON:00003348\tapple (whole)\torcid:0000-0002-7356-1779\t1\t\n" +
        "skos:exactMatch\tsemapv:ManualMappingCuration\tKF_FOOD:F003\tpink\tFOODON:00004186\tPink apple (whole)\torcid:0000-0002-7356-1779\t0.9\tWe could map to FOODON:00004187 instead which more specifically refers to 'raw' Pink apples. Decided against to be consistent with other mapping choices.\n" +
        "skos:exactMatch\tsemapv:ManualMappingCuration\tKF_FOOD:F004\tbraeburn\tFOODON:00002473\tapple (whole)\torcid:0000-0002-7356-1779\t1\t\n"

    Assert.Equal(input, expected)