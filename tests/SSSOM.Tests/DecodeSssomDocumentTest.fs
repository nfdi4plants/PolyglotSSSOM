module SSSOM.Tests.DecodeSssomDocumentTests

open System
open Xunit
open SSSOM


[<Fact>]
let ``DecodeSssomDocument should return valid SssomDocument object for valid sssom.tsv file`` () =
    let inputString =
        "#curie_map:\n" +
        "#  FOODON: http://purl.obolibrary.org/obo/FOODON_\n" +
        "#  KF_FOOD: https://kewl-foodie.inc/food/\n" +
        "#  orcid: https://orcid.org/\n" +
        "#mapping_set_id: https://w3id.org/sssom/tutorial/example1.sssom.tsv\n" +
        "#mapping_set_description: Manually curated alignment of KEWL FOODIE INC internal food and nutrition database with Food Ontology (FOODON). Intended to be used for ontological analysis and grouping of KEWL FOODIE INC related data.\n" +
        "#license: https://creativecommons.org/licenses/by/4.0/\n" +
        "#mapping_date: 2022-05-02\n" +
        "predicate_id\tmapping_justification\tsubject_id\tsubject_label\tobject_id\tobject_label\tauthor_id\tconfidence\tcomment\n" +
        "skos:exactMatch\tsemapv:ManualMappingCuration\tKF_FOOD:F001\tapple\tFOODON:00002473\tapple (whole)\torcid:0000-0002-7356-1779\t0.95\tWe could map to FOODON:03310788 instead to cover sliced apples, but only 'whole' apple types exist.\n" +
        "skos:exactMatch\tsemapv:ManualMappingCuration\tKF_FOOD:F002\tGala apple (whole)\tFOODON:00003348\tapple (whole)\torcid:0000-0002-7356-1779\t1\t\n" +
        "skos:exactMatch\tsemapv:ManualMappingCuration\tKF_FOOD:F003\tpink\tFOODON:00004186\tPink apple (whole)\torcid:0000-0002-7356-1779\t0.9\tWe could map to FOODON:00004187 instead which more specifically refers to 'raw' Pink apples. Decided against to be consistent with other mapping choices.\n" +
        "skos:exactMatch\tsemapv:ManualMappingCuration\tKF_FOOD:F004\tbraeburn\tFOODON:00002473\tapple (whole)\torcid:0000-0002-7356-1779\t1\t\n"

    let decodeDocument = DecodeSssomDocument.DecodeSssomDocument(inputString)

    let reEncodeString = EncodeSssomDocument.EncodeSssomDocument(decodeDocument)

    Assert.Equal(inputString, reEncodeString)