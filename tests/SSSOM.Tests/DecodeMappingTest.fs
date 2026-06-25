module SSSOM.Tests.DecodeMappingTests

open SSSOM
open Fable.Pyxpecto

let tests =
    testList "DecodeMapping Tests" [

        // ==========================================
        // extractMapping
        // ==========================================
        testList "extractMapping" [
            testCase "should remove rows with #" <| fun _ -> 
                let input = "# This is a comment\npredicate_id\tmapping_justification\n# Another comment\nKF:01\tsemapv:ManualMappingCuration"
                let expected = "predicate_id\tmapping_justification\nKF:01\tsemapv:ManualMappingCuration"
                let result = DecodeMapping.extractMapping(input)
                Expect.equal result expected "should remove rows with #"

            testCase "should ignore comments with slashes or whitespaces" <| fun _ ->
                let input = "//   # Curie Map info\npredicate_id\tmapping_justification"
                let expected = "predicate_id\tmapping_justification"
                let result = DecodeMapping.extractMapping(input)
                Expect.equal result expected "should ignore comments with slashes or whitespaces"

            testCase "should process empty string correctly" <| fun _ ->
                let result = DecodeMapping.extractMapping("")
                Expect.equal result "" "should process empty string correctly"
        ]

        // ==========================================
        // isValidTsvInput
        // ==========================================
        testList "isValidTsvInput" [
            testCase "should return true for correct TSV header" <| fun _ ->
                let validTSV = "predicate_id\tmapping_justification\nKF:01\tsemapv:ManualMappingCuration"
                let result = DecodeMapping.isValidTsvInput(validTSV)
                Expect.isTrue result "should return true for correct TSV header"

            testCase "should return false, when 'predicate_id' is missing" <| fun _ ->
                let invalidTsv = "mapping_justification\nKF:01\tsemapv:ManualMappingCuration"
                let result = DecodeMapping.isValidTsvInput(invalidTsv)
                Expect.isFalse result "should return false, when 'predicate_id' is missing"

            testCase "should return false, when 'mapping_justification' is missing" <| fun _ ->
                let invalidTsv = "predicate_id\tconfidence\nKF:01\t0.95"
                let result = DecodeMapping.isValidTsvInput(invalidTsv)
                Expect.isFalse result "should return false, when 'mapping_justification' is missing"

            testCase "should return false, when 'predicate_id' & 'mapping_justification' are missing" <| fun _ ->
                let invalidTsv = "confidence\tobject_label\n0.99\tapple"
                let result = DecodeMapping.isValidTsvInput(invalidTsv)
                Expect.isFalse result "should return false, when 'predicate_id' & 'mapping_justification' are missing"

            testCase "TSV with inconsistent column counts should return false" <| fun _ ->
                let badColumnTsv = "predicate_id\tmapping_justification\nKF:01"
                let result = DecodeMapping.isValidTsvInput(badColumnTsv)
                Expect.isFalse result "TSV with inconsistent column counts should return false"
        ]

        // ==========================================
        // ParseTsvtoMappings
        // ==========================================
        testList "ParseTsvtoMappings" [
            testCase "DecodeMapping should parse valid TSV in mapping Object" <| fun _ ->
                let tsv = "predicate_id\tmapping_justification\tconfidence\nKF:01\tsemapv:ManualMappingCuration\t0.95"
                let result = DecodeMapping.DecodeMapping(tsv)
                let mapping = result.[0]

                Expect.equal mapping.Predicate_id.Value "KF:01" "Predicate_id should be parsed correctly"
                Expect.equal mapping.Mapping_justification.Value "semapv:ManualMappingCuration" "Mapping_justification should be parsed correctly"
                Expect.equal mapping.Confidence (Some 0.95) "Confidence should be parsed as floating number"
                Expect.equal mapping.Subject_id None "None existing Fields should be left empty"

            testCase "DecodeMapping should throw exception if tsv is invalid" <| fun _ ->
                let invalidTsv = "subject_id\tconfidence\nKF:01\t0.95"
                
                let exceptionThrown = 
                    try 
                        DecodeMapping.DecodeMapping(invalidTsv) |> ignore
                        None
                    with ex -> 
                        Some ex

                Expect.isSome exceptionThrown "Exception should be thrown, when TSV-input is invalid"
                Expect.stringContains exceptionThrown.Value.Message "Invalid TSV Input" "Exception should contain 'Invalid TSV Input'"

            testCase "DecodeMapping should return None for empty double-cells" <| fun _ ->
                let tsv = "predicate_id\tmapping_justification\tconfidence\nKF:01\tsemapv:ManualMappingCuration\t"
                let result = DecodeMapping.DecodeMapping(tsv)
                Expect.equal result.[0].Confidence None "DecodeMapping should return None for empty double-cells"

            testCase "DecodeMapping should return None for empty cells" <| fun _ ->
                let tsv = "predicate_id\tmapping_justification\tconfidence\tRecord_id\nKF:01\tsemapv:ManualMappingCuration\t\tINSDC:000001"
                let result = DecodeMapping.DecodeMapping(tsv)
                Expect.equal result.[0].Confidence None "DecodeMapping should return None for empty cells"
        ]
    ]