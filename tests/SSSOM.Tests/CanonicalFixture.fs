module SSSOM.Tests.CanonicalFixture

[<Literal>]
let private ResourceName = "SSSOM.Tests.fixtures.canonical-example.sssom.tsv"

// Fable does not expose .NET manifest resources. Keeping the portable fallback
// here lets the identical behavioral test run in JavaScript and Python; the
// .NET path below verifies that it remains byte-identical to the fixture.
let private portableContent =
    "#curie_map:\n"
    + "#  ex: https://example.org/\n"
    + "#mapping_set_id: https://example.org/set\n"
    + "#license: https://example.org/license\n"
    + "#mapping_tool: mapper\n"
    + "subject_id\tpredicate_id\tobject_id\tmapping_justification\tconfidence\n"
    + "ex:b\tskos:exactMatch\tex:c\tsemapv:ManualMappingCuration\t0.956\n"

let content () =
#if FABLE_COMPILER
    portableContent
#else
    let assembly = System.Reflection.Assembly.GetExecutingAssembly()
    use stream = assembly.GetManifestResourceStream ResourceName

    if isNull stream then
        failwith $"Embedded canonical fixture '{ResourceName}' was not found."

    use reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8, true)
    let embeddedContent = reader.ReadToEnd()

    if embeddedContent <> portableContent then
        failwith $"Embedded canonical fixture '{ResourceName}' differs from the cross-runtime fallback."

    embeddedContent
#endif
