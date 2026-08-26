namespace SSSOM

open System
open YAMLicious.YAMLiciousTypes

module internal Encoder =

    let private isAbsoluteIri (value: string) =
        value.Contains("://") || value.StartsWith("urn:")

    let private normalizeEntity (metadata: MappingSet) (value: string) =
        if isAbsoluteIri value then CurieMap.contract metadata.CurieMap value else value

    let private normalizeValues (metadata: MappingSet) (versionDescriptor: SlotVersionDescriptor) (values: string array) =
        if versionDescriptor.Range = SlotRange.EntityReference then
            values |> Array.map (normalizeEntity metadata)
        else
            values

    let private initialMetadataValues (metadata: MappingSet) =
        MappingSetDescriptors.allDescriptors ()
        |> Array.map (fun descriptor -> descriptor.Name, MappingSetAccess.getValues metadata descriptor.Name)
        |> Map.ofArray

    let private initialMappingValues (mapping: Mapping) =
        MappingDescriptors.allDescriptors ()
        |> Array.map (fun descriptor -> descriptor.Name, MappingAccess.getValues mapping descriptor.Name)
        |> Map.ofArray

    let private valuesAt slot (values: Map<string, string array>) =
        values |> Map.tryFind slot |> Option.defaultValue [||]

    let private condense (version: SssomVersion) (metadata: MappingSet) (mappings: Mapping array) =
        let mutable metadataValues = initialMetadataValues metadata
        let mutable mappingValues = mappings |> Array.map initialMappingValues

        if mappingValues.Length > 0 then
            MappingDescriptors.allDescriptors ()
            |> Array.iter (fun descriptor ->
                match ModelVersion.descriptorFor version descriptor with
                | Some versionDescriptor when versionDescriptor.IsPropagated ->
                    let rowValues = mappingValues |> Array.map (valuesAt descriptor.Name)
                    let common = rowValues.[0]
                    let allSame = common.Length > 0 && rowValues |> Array.forall ((=) common)
                    let metadataCurrent = valuesAt descriptor.Name metadataValues

                    if allSame && (metadataCurrent.Length = 0 || metadataCurrent = common) then
                        metadataValues <- metadataValues |> Map.add descriptor.Name common
                        mappingValues <- mappingValues |> Array.map (Map.add descriptor.Name [||])
                | _ -> ())

        metadataValues, mappingValues

    let private mustQuoteYamlString (value: string) =
        let trimmed = value.Trim()
        let lower = value.ToLowerInvariant()
        let ambiguous =
            lower = "null"
            || lower = "~"
            || lower = "true"
            || lower = "false"
            || lower = ".nan"
            || lower = ".inf"
            || lower = "-.inf"
            || LexicalCodec.tryParseDouble value |> Option.isSome
            || SssomDate.TryCreate value |> Option.isSome

        let forbiddenStart =
            value.Length > 0
            && "-?:,[]{}#&*!|>'\"%@`".IndexOf(value.[0]) >= 0

        value = ""
        || trimmed <> value
        || ambiguous
        || forbiddenStart
        || value.Contains(": ")
        || value.Contains(" #")
        || value.Contains("\n")
        || value.Contains("\r")
        || value.Contains("\t")

    let private yamlString (value: string) =
        let style = if mustQuoteYamlString value then ScalarStyle.DoubleQuoted else ScalarStyle.Plain
        YAMLElement.Value(YAMLContent.create(value, style = style))

    let private yamlPlain (value: string) =
        let style =
            if value.Contains("\n") || value.Contains("\r") || value.Contains("\t") || value.Contains(": ") || value.Contains(" #") then
                ScalarStyle.DoubleQuoted
            else
                ScalarStyle.Plain

        YAMLElement.Value(YAMLContent.create(value, style = style))

    let private yamlValue (range: SlotRange) (value: string) =
        match range with
        | SlotRange.Text -> yamlString value
        | _ -> yamlPlain value

    let private yamlBlockSequence (range: SlotRange) (values: string array) =
        values
        |> Array.map (fun value -> YAMLElement.Object [ yamlValue range value ])
        |> Array.toList
        |> YAMLElement.Sequence

    let private yamlMapping (fields: (string * YAMLElement) list) =
        fields
        |> List.map (fun (key, value) -> YAMLElement.Mapping(YAMLContent.create key, value))
        |> YAMLElement.Object

    let private extensionProperty (definition: ExtensionDefinition) =
        definition.Property
        |> Option.map (fun value -> value.Value)
        |> Option.defaultValue ("http://sssom.invalid/" + definition.SlotName)

    let private usedDefinitions (document: SssomDocument) =
        let usedNames =
            Array.append
                document.Metadata.ExtensionValues
                (document.Mappings |> Array.collect (fun mapping -> mapping.ExtensionValues))
            |> Array.map (fun value -> value.SlotName)
            |> Set.ofArray

        document.Metadata.ExtensionDefinitions
        |> Array.filter (fun definition -> Set.contains definition.SlotName usedNames)
        |> Array.sortBy extensionProperty

    let private definitionElement (metadata: MappingSet) (definition: ExtensionDefinition) =
        let fields = ResizeArray<string * YAMLElement>()
        fields.Add("slot_name", yamlString definition.SlotName)

        definition.Property
        |> Option.iter (fun property -> fields.Add("property", yamlPlain (normalizeEntity metadata property.Value)))

        definition.TypeHint
        |> Option.iter (fun typeHint -> fields.Add("type_hint", yamlPlain (normalizeEntity metadata typeHint.Value)))

        yamlMapping (fields |> Seq.toList)

    let private extensionType (metadata: MappingSet) (definition: ExtensionDefinition) =
        definition.TypeHint
        |> Option.map (fun value -> value.Value)
        |> Option.defaultValue "xsd:string"
        |> CurieMap.tryExpand metadata.CurieMap
        |> Option.defaultValue "http://www.w3.org/2001/XMLSchema#string"

    let private extensionYamlValue (metadata: MappingSet) (definitions: ExtensionDefinition array) (extension: ExtensionValue) =
        let isString =
            definitions
            |> Array.tryFind (fun definition -> definition.SlotName = extension.SlotName)
            |> Option.map (extensionType metadata)
            |> Option.defaultValue "http://www.w3.org/2001/XMLSchema#string"
            |> (=) "http://www.w3.org/2001/XMLSchema#string"

        if isString then yamlString extension.Value else yamlPlain extension.Value

    let private allEntityValues (version: SssomVersion) (document: SssomDocument) (definitions: ExtensionDefinition array) =
        let descriptorValues (descriptors: SlotDescriptor array) (getter: 'a -> string -> string array) (source: 'a) =
            descriptors
            |> Array.collect (fun descriptor ->
                match ModelVersion.descriptorFor version descriptor with
                | Some versionDescriptor when versionDescriptor.Range = SlotRange.EntityReference -> getter source descriptor.Name
                | _ -> [||])

        [|
            yield! descriptorValues (MappingSetDescriptors.allDescriptors ()) MappingSetAccess.getValues document.Metadata

            for mapping in document.Mappings do
                yield! descriptorValues (MappingDescriptors.allDescriptors ()) MappingAccess.getValues mapping

            for definition in definitions do
                match definition.Property with
                | Some property -> yield property.Value
                | None -> ()

                match definition.TypeHint with
                | Some typeHint -> yield typeHint.Value
                | None -> ()

            for extension in document.Metadata.ExtensionValues do
                match definitions |> Array.tryFind (fun definition -> definition.SlotName = extension.SlotName) with
                | Some definition when extensionType document.Metadata definition = "https://w3id.org/linkml/Uriorcurie" ->
                    yield extension.Value
                | _ -> ()

            for mapping in document.Mappings do
                for extension in mapping.ExtensionValues do
                    match definitions |> Array.tryFind (fun definition -> definition.SlotName = extension.SlotName) with
                    | Some definition when extensionType document.Metadata definition = "https://w3id.org/linkml/Uriorcurie" ->
                        yield extension.Value
                    | _ -> ()
        |]

    let private prefixOfReference (metadata: MappingSet) (value: string) =
        let lexical = if isAbsoluteIri value then CurieMap.contract metadata.CurieMap value else value
        let separator = lexical.IndexOf(':')
        if separator > 0 then Some(lexical.Substring(0, separator)) else None

    let private usedCurieMap (version: SssomVersion) (document: SssomDocument) (definitions: ExtensionDefinition array) =
        let usedPrefixes =
            allEntityValues version document definitions
            |> Array.choose (prefixOfReference document.Metadata)
            |> Set.ofArray

        document.Metadata.CurieMap
        |> Array.filter (fun entry -> not (CurieMap.isBuiltIn entry.PrefixName) && Set.contains entry.PrefixName usedPrefixes)
        |> Array.sortBy (fun entry -> entry.PrefixName)

    let private metadataYaml
        (version: SssomVersion)
        (document: SssomDocument)
        (metadataValues: Map<string, string array>)
        (definitions: ExtensionDefinition array)
        =
        let fields = ResizeArray<string * YAMLElement>()
        let metadata = document.Metadata

        MappingSetDescriptors.allDescriptors ()
        |> Array.iter (fun descriptor ->
            match descriptor.Name with
            | "sssom_version" when version = SssomVersion.V1_1 ->
                fields.Add("sssom_version", yamlPlain "1.1")
            | "sssom_version" -> ()
            | "curie_map" ->
                let entries = usedCurieMap version document definitions

                if entries.Length > 0 then
                    entries
                    |> Array.map (fun entry -> entry.PrefixName, yamlPlain entry.PrefixUrl.Value)
                    |> Array.toList
                    |> yamlMapping
                    |> fun value -> fields.Add("curie_map", value)
            | "extension_definitions" ->
                if definitions.Length > 0 then
                    definitions
                    |> Array.map (definitionElement metadata)
                    |> Array.toList
                    |> YAMLElement.Sequence
                    |> fun value -> fields.Add("extension_definitions", value)
            | name ->
                match ModelVersion.descriptorFor version descriptor with
                | Some versionDescriptor ->
                    let values = valuesAt name metadataValues |> normalizeValues metadata versionDescriptor

                    if values.Length > 0 then
                        let value =
                            match versionDescriptor.Cardinality with
                            | SlotCardinality.Scalar -> yamlValue versionDescriptor.Range values.[0]
                            | SlotCardinality.Multivalued -> yamlBlockSequence versionDescriptor.Range values

                        fields.Add(name, value)
                | None -> ())

        let declaredNames = definitions |> Array.map (fun definition -> definition.SlotName) |> Set.ofArray

        metadata.ExtensionValues
        |> Array.filter (fun extension -> Set.contains extension.SlotName declaredNames)
        |> Array.sortBy (fun extension ->
            definitions
            |> Array.find (fun definition -> definition.SlotName = extension.SlotName)
            |> extensionProperty)
        |> Array.iter (fun extension ->
            fields.Add(extension.SlotName, extensionYamlValue metadata definitions extension))

        fields |> Seq.toList |> yamlMapping

    let private mappingCell (metadata: MappingSet) (version: SssomVersion) (descriptor: SlotDescriptor) (values: string array) =
        match ModelVersion.descriptorFor version descriptor with
        | None -> ""
        | Some versionDescriptor ->
            let normalized = normalizeValues metadata versionDescriptor values

            match versionDescriptor.Cardinality with
            | SlotCardinality.Scalar -> normalized |> Array.tryHead |> Option.defaultValue ""
            | SlotCardinality.Multivalued -> TextCodec.joinMultivalue version normalized

    let private compareRows (left: string array) (right: string array) =
        let compareText (leftText: string) (rightText: string) =
            let nextCodePoint (text: string) index =
                let first = int text.[index]

                if first >= 0xD800
                   && first <= 0xDBFF
                   && index + 1 < text.Length then
                    let second = int text.[index + 1]

                    if second >= 0xDC00 && second <= 0xDFFF then
                        0x10000 + ((first - 0xD800) * 0x400) + (second - 0xDC00), index + 2
                    else
                        first, index + 1
                else
                    first, index + 1

            let mutable leftIndex = 0
            let mutable rightIndex = 0
            let mutable comparison = 0

            while comparison = 0 && leftIndex < leftText.Length && rightIndex < rightText.Length do
                let leftCodePoint, nextLeft = nextCodePoint leftText leftIndex
                let rightCodePoint, nextRight = nextCodePoint rightText rightIndex
                comparison <- compare leftCodePoint rightCodePoint
                leftIndex <- nextLeft
                rightIndex <- nextRight

            if comparison <> 0 then comparison
            elif leftIndex < leftText.Length then 1
            elif rightIndex < rightText.Length then -1
            else 0

        let mutable index = 0
        let mutable result = 0

        while result = 0 && index < left.Length && index < right.Length do
            result <- compareText left.[index] right.[index]
            index <- index + 1

        if result <> 0 then result else compare left.Length right.Length

    let private mappingsTsv
        (version: SssomVersion)
        (document: SssomDocument)
        (mappingValues: Map<string, string array> array)
        (definitions: ExtensionDefinition array)
        =
        let metadata = document.Metadata
        let descriptors =
            MappingDescriptors.allDescriptors ()
            |> Array.filter (fun descriptor ->
                ModelVersion.descriptorFor version descriptor |> Option.isSome
                && (descriptor.Name = "predicate_id"
                    || descriptor.Name = "mapping_justification"
                    || mappingValues |> Array.exists (fun values -> valuesAt descriptor.Name values |> Array.isEmpty |> not)))

        let declaredNames = definitions |> Array.map (fun definition -> definition.SlotName) |> Set.ofArray
        let extensionDefinitions =
            definitions
            |> Array.filter (fun definition ->
                document.Mappings
                |> Array.exists (fun mapping ->
                    mapping.ExtensionValues
                    |> Array.exists (fun extension -> extension.SlotName = definition.SlotName)))
            |> Array.sortBy extensionProperty

        let extensionNames = extensionDefinitions |> Array.map (fun definition -> definition.SlotName)
        let header = Array.append (descriptors |> Array.map (fun descriptor -> descriptor.Name)) extensionNames

        let rows =
            document.Mappings
            |> Array.mapi (fun index mapping ->
                let standardCells =
                    descriptors
                    |> Array.map (fun descriptor ->
                        valuesAt descriptor.Name mappingValues.[index]
                        |> mappingCell metadata version descriptor)

                let extensionCells =
                    extensionNames
                    |> Array.map (fun name ->
                        mapping.ExtensionValues
                        |> Array.tryFind (fun extension -> Set.contains extension.SlotName declaredNames && extension.SlotName = name)
                        |> Option.map (fun extension -> extension.Value)
                        |> Option.defaultValue "")

                Array.append standardCells extensionCells)
            |> Array.sortWith compareRows

        [|
            yield header |> Array.map TextCodec.quoteTsv |> fun cells -> String.Join("\t", cells)

            for row in rows do
                yield row |> Array.map TextCodec.quoteTsv |> fun cells -> String.Join("\t", cells)
        |]
        |> fun lines -> String.Join("\n", lines)

    let tryEncode (document: SssomDocument) =
        let diagnostics = Validation.validateForPublicApi document |> ResizeArray

        if Diagnostics.hasErrors diagnostics then
            EncodeResult(None, diagnostics.ToArray())
        else
            try
                let version = ModelVersion.minimumVersion document
                let metadataValues, mappingValues = condense version document.Metadata document.Mappings
                let definitions = usedDefinitions document
                let yaml = metadataYaml version document metadataValues definitions

                let yamlText =
                    YAMLicious.Encode.write 2 yaml
                    |> TextCodec.normalizeLineEndings
                    |> fun value -> value.TrimEnd('\n')
                    |> fun value -> value.Split('\n')
                    |> Array.map (fun line -> "#" + line)
                    |> fun lines -> String.Join("\n", lines)

                let tsv = mappingsTsv version document mappingValues definitions
                EncodeResult(Some(yamlText + "\n" + tsv + "\n"), diagnostics.ToArray())
            with ex ->
                diagnostics.Add(
                    Diagnostics.error
                        DiagnosticCodes.InvalidValue
                        $"Canonical encoding failed: {ex.Message}"
                        None
                        None
                        None
                        None
                )

                EncodeResult(None, diagnostics.ToArray())
