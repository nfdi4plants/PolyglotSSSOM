module SSSOM.Tests.DecodeMappingTests

open System
open Xunit
open SSSOM

// ==========================================
// extractMapping
// ==========================================
[<Fact>]
let ``extractMapping should remove rows with #`` () =
    let input = "# This is a comment\npredicate_id\tmapping_justification\n# Another comment\nKF:01\tManual"
    let expected = "predicate_id\tmapping_justification\nKF:01\tManual"

    let result = DecodeMapping.extractMapping(input)

    Assert.Equal(expected, result)

[<Fact>]
let ``extractMapping should ignore comments with slashes or whitespaces`` () =
    let input = "//   # Curie Map info\npredicate_id\tmapping_justification"
    let expected = "predicate_id\tmapping_justification"

    let result = DecodeMapping.extractMapping(input)

    Assert.Equal(expected, result)

[<Fact>]
let ``extractMapping should process empty string correctly`` () =
    let result = DecodeMapping.extractMapping("")

    Assert.Equal("", result)


// ==========================================
// isValidTsvInput
// ==========================================
[<Fact>]
let ``isValidTsvInput should return true for correct TSV header`` () =
    let validTSV = "predicate_id\tmapping_justification\nKF:01\tManualCuration"

    let result = DecodeMapping.isValidTsvInput(validTSV)

    Assert.True(result)

[<Fact>]
let ``isValidTsvInput should return false, when 'predicate_id' is missing`` () =
    let invalidTsv = "mapping_justification\nKF:01\tManualCuration"

    let result = DecodeMapping.isValidTsvInput(invalidTsv)

    Assert.False(result)

[<Fact>]
let ``isValidTsvInput should return false, when 'mapping_justification' is missing`` () =
    let invalidTsv = "predicate_id\tconfidence\nKF:01\t0.95"

    let result = DecodeMapping.isValidTsvInput(invalidTsv)

    Assert.False(result)

[<Fact>]
let ``isValidTsvInput should return false, when 'predicate_id' & 'mapping_justification' are missing`` () =
    let invalidTsv = "confidence\tobject_label\n0.99\tapple"

    let result = DecodeMapping.isValidTsvInput(invalidTsv)

    Assert.False(result)

[<Fact>]
let ``TSV with inconsistent column counts should return false`` () =
    let badColumnTsv = "predicate_id\tmapping_justification\nKF:01"

    let result = DecodeMapping.isValidTsvInput(badColumnTsv)

    Assert.False(result)

// ==========================================
// ParseTsvtoMappings
// ==========================================
[<Fact>]
let ``DecodeMapping should parse valid TSV in mapping Object`` () =
    let tsv = "predicate_id\tmapping_justification\tconfidence\nKF:01\tManual\t0.95"

    let result = DecodeMapping.DecodeMapping(tsv)

    let mapping = result.[0]
    Assert.Equal("KF:01", mapping.Predicate_id)
    Assert.Equal("Manual", mapping.Mapping_justification)
    Assert.Equal(Some 0.95, mapping.Confidence)
    Assert.Equal(None, mapping.Subject_id)

[<Fact>]
let ``DecodeMapping should throw exception if tsv is invalid`` () =
    let invalidTsv = "subject_id\tconfidence\nKF:01\t0.95"

    let ex = Assert.Throws<Exception>(fun () ->
        DecodeMapping.DecodeMapping(invalidTsv) |> ignore
    )

    Assert.Contains("Invalid TSV Input", ex.Message)


[<Fact>]
let ``DecodeMapping should return None for empty double-cells`` () =
    let tsv = "predicate_id\tmapping_justification\tconfidence\nKF:01\tManual\t"

    let result = DecodeMapping.DecodeMapping(tsv)

    Assert.Equal(None, result.[0].Confidence)

[<Fact>]
let ``DecodeMapping should return None for empty cells`` () =
    let tsv = "predicate_id\tmapping_justification\tconfidence\tRecord_id\nKF:01\tManual\t\tINSDC:000001"

    let result = DecodeMapping.DecodeMapping(tsv)

    Assert.Equal(None, result.[0].Confidence)