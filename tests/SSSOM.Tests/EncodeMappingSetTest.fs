module SSSOM.Tests.EncodeMappingSetTests

open SSSOM
open Fable.Pyxpecto

let tests = 
    testList "EncodeMappingSet Tests" [
        
        testCase "should return a valid yaml output for valid mappingSet object" <| fun _ ->
            let result =
                EncodeMappingSet.EncodeMappingSet(
                    MappingSet(
                        Curie_map =
                            [|
                                CurieMap("FOODON", "http://purl.obolibrary.org/obo/FOODON_")
                                CurieMap("KF_FOOD", "https://kewl-foodie.inc/food/")
                                CurieMap("orcid", "https://orcid.org/")
                            |],
                        Mapping_set_id = NonRelativeURI.create "https://w3id.org/sssom/tutorial/example1.sssom.tsv",
                        Mapping_set_description = "Manually curated alignment of KEWL FOODIE INC internal food and nutrition database with Food Ontology (FOODON). Intended to be used for ontological analysis and grouping of KEWL FOODIE INC related data.",
                        License = NonRelativeURI.create "https://creativecommons.org/licenses/by/4.0/",
                        Mapping_date = Date.create "2022-05-02"
                    )
                )

            let expected = 
                "#curie_map:\n" +
                "#  FOODON: http://purl.obolibrary.org/obo/FOODON_\n" +
                "#  KF_FOOD: https://kewl-foodie.inc/food/\n" +
                "#  orcid: https://orcid.org/\n" +
                "#mapping_set_id: https://w3id.org/sssom/tutorial/example1.sssom.tsv\n" +
                "#mapping_set_description: Manually curated alignment of KEWL FOODIE INC internal food and nutrition database with Food Ontology (FOODON). Intended to be used for ontological analysis and grouping of KEWL FOODIE INC related data.\n" +
                "#license: https://creativecommons.org/licenses/by/4.0/\n" +
                "#mapping_date: 2022-05-02"

            Expect.equal result expected "Should correctly encode a MappingSet object into a commented YAML metadata string"
    ]