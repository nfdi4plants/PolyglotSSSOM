module SSSOM.Tests.DecodeMappingSetTests

open System
open Xunit
open SSSOM


// ==========================================
// extractMapping
// ==========================================
[<Fact>]
let ``extractMappingSet should extract rows with #`` () =
    let input =
        "# license: CC0\n" +
        "# sssom_version: 1.0.0\n" + 
        "subject_id\tobject_id\tpredicate_id\n" +
        "HP:0001\tDOID:0002\tskos:exactMatch"

    let result = DecodeMappingSet.extractMappingSet(input)

    let expected = "# license: CC0\n# sssom_version: 1.0.0"
    Assert.Equal(expected, result)


// ==========================================
// isValidTsvInput
// ==========================================
[<Fact>]
let ``isValidYamlInput should return true for correct input`` () =
    let validYaml =
        "# sssom_version: 1.0.0\n" +
        "# curie_map:\n" +
        "#   HP: http://purl.obolibrary.org/obo/HP_\n" +
        "#   DOID: http://purl.obolibrary.org/obo/DOID_"

    let result = DecodeMappingSet.isValidYamlInput(validYaml)

    Assert.True(result)

[<Fact>]
let ``isValidYamlInput should return false for invalid input`` () =
    let invalidYaml =
        "# sssom_version: 1.0.0\n" +
        "#   curie_map:\n" + 
        "# HP: http://..."

    let result = DecodeMappingSet.isValidYamlInput(invalidYaml)

    Assert.False(result)

// ==========================================
// DecodeMappingSet
// ==========================================
[<Fact>]
let ``DecodeMappingSet should create a valid mappingSet-object for valid Input`` () =
    let fullInput =
        "# sssom_version: sssom:version1.0\n" +
        "# mapping_set_id: http://example.org\n" +
        "# curie_map:\n" +
        "#   HP: http://purl.obolibrary.org/obo/HP_\n" +
        "subject_id\tobject_id"

    let result = DecodeMappingSet.DecodeMappingSet(fullInput)

    Assert.Equal(Some SssomVersion.V1_0, result.Sssom_version)

    Assert.True(result.Mapping_set_id.IsSome)

    Assert.Equal("http://example.org", result.Mapping_set_id.Value.Value)

    Assert.True(result.Curie_map.IsSome)

[<Fact>]
let ``DecodeMappingSet should throw exception if input is invalid`` () =
    let badInput =
        "#   sssom_version: 1.0.0\n" + 
        "# bad_indent\n"

    let ex = Assert.Throws<Exception>(fun () ->
        DecodeMappingSet.DecodeMappingSet(badInput) |> ignore
    )

    Assert.Contains("Yaml-input is not valid!", ex.Message)