module SSSOM.Tests.DecodeMappingSetTests

open SSSOM
open Fable.Pyxpecto

let tests = 
    testList "DecodeMappingSet Tests" [

        // ==========================================
        // extractMappingSet
        // ==========================================
        testList "extractMappingSet" [
            testCase "should extract rows with #" <| fun _ ->
                let input =
                    "# license: CC0\n" +
                    "# sssom_version: 1.0.0\n" + 
                    "subject_id\tobject_id\tpredicate_id\n" +
                    "HP:0001\tDOID:0002\tskos:exactMatch"

                let result = DecodeMappingSet.extractMappingSet(input)
                let expected = "# license: CC0\n# sssom_version: 1.0.0"
                
                Expect.equal result expected "Metadata rows starting with # should be extracted correctly"
        ]

        // ==========================================
        // isValidYamlInput
        // ==========================================
        testList "isValidYamlInput" [
            testCase "should return true for correct input" <| fun _ ->
                let validYaml =
                    "# sssom_version: 1.0.0\n" +
                    "# curie_map:\n" +
                    "#   HP: http://purl.obolibrary.org/obo/HP_\n" +
                    "#   DOID: http://purl.obolibrary.org/obo/DOID_"

                let result = DecodeMappingSet.isValidYamlInput(validYaml)
                Expect.isTrue result "Should return true for a valid YAML header"

            testCase "should return false for invalid input" <| fun _ ->
                let invalidYaml =
                    "# sssom_version: 1.0.0\n" +
                    "#   curie_map:\n" + 
                    "# HP: http://..."

                let result = DecodeMappingSet.isValidYamlInput(invalidYaml)
                Expect.isFalse result "Should return false for an improperly formatted YAML header"
        ]

        // ==========================================
        // DecodeMappingSet
        // ==========================================
        testList "DecodeMappingSet" [
            testCase "should create a valid mappingSet-object for valid input" <| fun _ ->
                let fullInput =
                    "# sssom_version: sssom:version1.0\n" +
                    "# mapping_set_id: http://example.org\n" +
                    "# curie_map:\n" +
                    "#   HP: http://purl.obolibrary.org/obo/HP_\n" +
                    "subject_id\tobject_id"

                let result = DecodeMappingSet.DecodeMappingSet(fullInput)

                Expect.equal result.Sssom_version (Some SssomVersion.V1_0) "Sssom_version should be parsed correctly"
                
                Expect.isSome result.Mapping_set_id "Mapping_set_id should not be None"
                Expect.equal result.Mapping_set_id.Value.Value "http://example.org" "Mapping_set_id value should match the input"
                
                Expect.isSome result.Curie_map "Curie_map should not be None"

            testCase "should throw exception if input is invalid" <| fun _ ->
                let badInput =
                    "#   sssom_version: 1.0.0\n" + 
                    "# bad_indent\n"

                let exceptionThrown = 
                    try 
                        DecodeMappingSet.DecodeMappingSet(badInput) |> ignore
                        None
                    with ex -> 
                        Some ex

                Expect.isSome exceptionThrown "An exception should be thrown for invalid YAML input"
                Expect.stringContains exceptionThrown.Value.Message "Yaml-input is not valid!" "The exception message should indicate invalid YAML input"
        ]
    ]