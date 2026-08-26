namespace SSSOM

open System
open YAMLicious.YAMLiciousTypes

module internal YamlCodec =

    let private scalarContent element =
        match element with
        | YAMLElement.Value content -> Some content
        | YAMLElement.Object [ YAMLElement.Value content ] -> Some content
        | _ -> None

    let private isNullScalar (content: YAMLContent) =
        content.Style.IsNone && (content.Value = "null" || content.Value = "~")

    let tryScalar element =
        scalarContent element
        |> Option.bind (fun content -> if isNullScalar content then None else Some content.Value)

    let tryMultivalue element =
        match element with
        | YAMLElement.Object [ YAMLElement.Sequence items ]
        | YAMLElement.Sequence items ->
            items
            |> List.map scalarContent
            |> fun values ->
                if values |> List.exists Option.isNone then
                    None
                else
                    values
                    |> List.choose id
                    |> List.filter (isNullScalar >> not)
                    |> List.map (fun content -> content.Value)
                    |> List.toArray
                    |> Some
        | _ -> tryScalar element |> Option.map (fun value -> [| value |])

    let tryObject element =
        match element with
        | YAMLElement.Object items -> Some items
        | _ -> None

    let trySequence element =
        match element with
        | YAMLElement.Object [ YAMLElement.Sequence items ]
        | YAMLElement.Sequence items -> Some items
        | _ -> None

    let mappings items =
        items
        |> List.choose (function
            | YAMLElement.Mapping(key, value) -> Some(key.Value, value)
            | _ -> None)

    let rec private hasForbiddenFeature element =
        let contentForbidden (content: YAMLContent) = content.Anchor.IsSome || content.Tag.IsSome

        match element with
        | YAMLElement.Mapping(key, value) -> contentForbidden key || hasForbiddenFeature value
        | YAMLElement.Value content -> contentForbidden content
        | YAMLElement.Sequence items
        | YAMLElement.Object items -> items |> List.exists hasForbiddenFeature
        | YAMLElement.Alias _
        | YAMLElement.DocumentStart
        | YAMLElement.DocumentEnd -> true
        | YAMLElement.Comment _
        | YAMLElement.Nil -> false

    let parseRoot yaml (diagnostics: ResizeArray<SssomDiagnostic>) =
        try
            let root = YAMLicious.Decode.read yaml

            if hasForbiddenFeature root then
                diagnostics.Add(
                    Diagnostics.error
                        DiagnosticCodes.MetadataStructure
                        "The metadata block uses a forbidden YAML tag, anchor, alias, or document marker."
                        None
                        None
                        None
                        None
                )

            match root with
            | YAMLElement.Object items ->
                let unexpected =
                    items
                    |> List.exists (function
                        | YAMLElement.Mapping _
                        | YAMLElement.Comment _ -> false
                        | _ -> true)

                if unexpected then
                    diagnostics.Add(
                        Diagnostics.error
                            DiagnosticCodes.MetadataStructure
                            "The metadata root must contain only YAML key/value mappings."
                            None
                            None
                            None
                            None
                    )

                let pairs = mappings items |> List.toArray

                pairs
                |> Array.countBy fst
                |> Array.filter (fun (_, count) -> count > 1)
                |> Array.iter (fun (key, _) ->
                    diagnostics.Add(
                        Diagnostics.error
                            DiagnosticCodes.DuplicateMetadata
                            $"Metadata key '{key}' occurs more than once."
                            None
                            None
                            None
                            (Some key)
                    ))

                pairs
            | _ ->
                diagnostics.Add(
                    Diagnostics.error
                        DiagnosticCodes.MetadataStructure
                        "The metadata block must be a YAML object."
                        None
                        None
                        None
                        None
                )

                [||]
        with ex ->
            diagnostics.Add(
                Diagnostics.error DiagnosticCodes.YamlParse $"Cannot parse the YAML metadata block: {ex.Message}" None None None None
            )

            [||]

module internal Decoder =

    let private addStructureError (diagnostics: ResizeArray<SssomDiagnostic>) slot expected =
        diagnostics.Add(
            Diagnostics.error
                DiagnosticCodes.MetadataStructure
                $"Metadata slot '{slot}' must be {expected}."
                None
                None
                None
                (Some slot)
        )

    let private pairValue key (pairs: (string * YAMLElement) array) =
        pairs |> Array.tryFind (fun (name, _) -> name = key) |> Option.map snd

    let private declaredVersion pairs (diagnostics: ResizeArray<SssomDiagnostic>) =
        match pairValue "sssom_version" pairs with
        | None -> None, SssomVersion.V1_0
        | Some element ->
            match YamlCodec.tryScalar element with
            | Some value ->
                match SssomVersion.tryParse value with
                | Some version -> Some version, version
                | None ->
                    diagnostics.Add(
                        Diagnostics.error
                            DiagnosticCodes.UnsupportedVersion
                            $"SSSOM version '{value}' is not supported."
                            None
                            None
                            None
                            (Some "sssom_version")
                    )

                    None, SssomVersion.V1_0
            | None ->
                addStructureError diagnostics "sssom_version" "a scalar version value"
                None, SssomVersion.V1_0

    let private parseCurieMap pairs (diagnostics: ResizeArray<SssomDiagnostic>) =
        match pairValue "curie_map" pairs with
        | None -> [||]
        | Some element ->
            match YamlCodec.tryObject element with
            | None ->
                addStructureError diagnostics "curie_map" "a YAML object"
                [||]
            | Some items ->
                let entries = YamlCodec.mappings items |> List.toArray

                entries
                |> Array.countBy fst
                |> Array.filter (fun (_, count) -> count > 1)
                |> Array.iter (fun (prefix, _) ->
                    diagnostics.Add(
                        Diagnostics.error
                            DiagnosticCodes.CuriePrefix
                            $"CURIE prefix '{prefix}' is declared more than once."
                            None
                            None
                            None
                            (Some "curie_map")
                    ))

                entries
                |> Array.choose (fun (prefix, valueElement) ->
                    match YamlCodec.tryScalar valueElement with
                    | Some value when LexicalValidation.isNcName prefix ->
                        match UriReference.TryCreate value with
                        | Some uri when uri.IsNonRelative -> Some(PrefixEntry(prefix, uri))
                        | _ ->
                            diagnostics.Add(
                                Diagnostics.error
                                    DiagnosticCodes.CuriePrefix
                                    $"CURIE prefix '{prefix}' must expand to a non-relative URI."
                                    None
                                    None
                                    None
                                    (Some "curie_map")
                            )

                            None
                    | Some _ ->
                        diagnostics.Add(
                            Diagnostics.error
                                DiagnosticCodes.CuriePrefix
                                $"CURIE prefix name '{prefix}' is not an NCName."
                                None
                                None
                                None
                                (Some "curie_map")
                        )

                        None
                    | None ->
                        addStructureError diagnostics "curie_map" "a YAML object with scalar URI values"
                        None)

    let private isResolvableCurie curieMap (value: string) =
        not (value.Contains("://") || value.StartsWith("urn:"))
        && CurieMap.tryExpand curieMap value |> Option.isSome

    let private parseExtensionDefinitions pairs curieMap (diagnostics: ResizeArray<SssomDiagnostic>) =
        match pairValue "extension_definitions" pairs with
        | None -> [||]
        | Some element ->
            match YamlCodec.trySequence element with
            | None ->
                addStructureError diagnostics "extension_definitions" "a YAML sequence of objects"
                [||]
            | Some items ->
                let parsed = ResizeArray<ExtensionDefinition>()

                items
                |> List.iter (fun item ->
                    match YamlCodec.tryObject item with
                    | None ->
                        diagnostics.Add(
                            Diagnostics.warning
                                DiagnosticCodes.ExtensionDefinition
                                "An extension definition was ignored because it is not a YAML object."
                                None
                                None
                                None
                                (Some "extension_definitions")
                        )
                    | Some fields ->
                        let pairs = YamlCodec.mappings fields |> List.toArray
                        let names = pairs |> Array.map fst
                        let allowed = Set.ofList [ "slot_name"; "property"; "type_hint" ]
                        let unexpected = names |> Array.exists (fun name -> not (Set.contains name allowed))
                        let duplicates = names |> Array.distinct |> Array.length <> names.Length

                        let scalar name = pairValue name pairs |> Option.bind YamlCodec.tryScalar

                        let slotName = scalar "slot_name"
                        let property = scalar "property"
                        let typeHint = scalar "type_hint"
                        let standardSlot name =
                            MappingSetDescriptors.tryFind name |> Option.isSome
                            || MappingDescriptors.tryFind name |> Option.isSome

                        let valid =
                            not unexpected
                            && not duplicates
                            && slotName |> Option.exists (fun name -> LexicalValidation.isNcName name && not (standardSlot name))
                            && property |> Option.forall (isResolvableCurie curieMap)
                            && typeHint |> Option.forall (isResolvableCurie curieMap)

                        if valid then
                            let definition =
                                ExtensionDefinition(
                                    slotName.Value,
                                    ?property = (property |> Option.map EntityReference.Create),
                                    ?typeHint = (typeHint |> Option.map EntityReference.Create)
                                )

                            let duplicateProperty =
                                match definition.Property with
                                | Some property ->
                                    parsed
                                    |> Seq.exists (fun current ->
                                        current.Property |> Option.exists (fun candidate -> candidate.Value = property.Value))
                                | None -> false

                            if parsed |> Seq.exists (fun current -> current.SlotName = definition.SlotName) || duplicateProperty then
                                diagnostics.Add(
                                    Diagnostics.warning
                                        DiagnosticCodes.ExtensionDefinition
                                        $"Duplicate extension definition '{definition.SlotName}' was ignored."
                                        None
                                        None
                                        None
                                        (Some "extension_definitions")
                                )
                            else
                                parsed.Add definition
                        else
                            let name = slotName |> Option.defaultValue "<missing>"
                            diagnostics.Add(
                                Diagnostics.warning
                                    DiagnosticCodes.ExtensionDefinition
                                    $"Invalid extension definition '{name}' was ignored."
                                    None
                                    None
                                    None
                                    (Some "extension_definitions")
                            ))

                parsed.ToArray()

    let private metadataValues
        version
        (descriptor: SlotDescriptor)
        element
        (diagnostics: ResizeArray<SssomDiagnostic>)
        =
        match ModelVersion.descriptorFor version descriptor with
        | None ->
            diagnostics.Add(
                Diagnostics.error
                    DiagnosticCodes.VersionConflict
                    $"Metadata slot '{descriptor.Name}' is not available in SSSOM {SssomVersion.toLexical version}."
                    None
                    None
                    None
                    (Some descriptor.Name)
            )

            None
        | Some versionDescriptor ->
            let values =
                match versionDescriptor.Cardinality with
                | SlotCardinality.Scalar ->
                    match YamlCodec.tryScalar element with
                    | Some value -> Some [| value |]
                    | None ->
                        addStructureError diagnostics descriptor.Name "a scalar value"
                        None
                | SlotCardinality.Multivalued ->
                    match YamlCodec.tryMultivalue element with
                    | Some values -> Some values
                    | None ->
                        addStructureError diagnostics descriptor.Name "a scalar or sequence of scalar values"
                        None

            values
            |> Option.bind (fun parsed ->
                let valid =
                    parsed
                    |> Array.map (fun value ->
                        Validation.validateLexical
                            diagnostics
                            version
                            versionDescriptor.Range
                            value
                            None
                            (Some descriptor.Name))
                    |> Array.forall id

                if valid then Some parsed else None)

    let private requiredMetadataValue name pairs version diagnostics =
        match MappingSetDescriptors.tryFind name, pairValue name pairs with
        | Some descriptor, Some element -> metadataValues version descriptor element diagnostics |> Option.bind Array.tryHead
        | _ ->
            diagnostics.Add(
                Diagnostics.error DiagnosticCodes.MissingRequired $"Metadata slot '{name}' is required." None None None (Some name)
            )

            None

    let private decodeMetadata yaml diagnostics =
        let pairs = YamlCodec.parseRoot yaml diagnostics
        let declared, version = declaredVersion pairs diagnostics
        let curieMap = parseCurieMap pairs diagnostics
        let definitions = parseExtensionDefinitions pairs curieMap diagnostics
        let mappingSetId = requiredMetadataValue "mapping_set_id" pairs version diagnostics
        let license = requiredMetadataValue "license" pairs version diagnostics

        let metadata =
            MappingSet(
                mappingSetId |> Option.defaultValue "urn:sssom:invalid" |> UriReference.Create,
                license |> Option.defaultValue "urn:sssom:invalid" |> UriReference.Create,
                ?sssomVersion = declared,
                curieMap = curieMap,
                extensionDefinitions = definitions
            )

        MappingSetDescriptors.allDescriptors ()
        |> Array.filter (fun descriptor ->
            descriptor.Name <> "sssom_version"
            && descriptor.Name <> "curie_map"
            && descriptor.Name <> "mapping_set_id"
            && descriptor.Name <> "license"
            && descriptor.Name <> "extension_definitions")
        |> Array.iter (fun descriptor ->
            pairValue descriptor.Name pairs
            |> Option.iter (fun element ->
                metadataValues version descriptor element diagnostics
                |> Option.iter (MappingSetAccess.setValues metadata descriptor.Name)))

        let standardNames =
            MappingSetDescriptors.allDescriptors ()
            |> Array.map (fun descriptor -> descriptor.Name)
            |> Set.ofArray

        let extensionValues = ResizeArray<ExtensionValue>()

        pairs
        |> Array.filter (fun (name, _) -> not (Set.contains name standardNames))
        |> Array.iter (fun (name, element) ->
            match definitions |> Array.tryFind (fun definition -> definition.SlotName = name) with
            | None ->
                diagnostics.Add(
                    Diagnostics.warning
                        DiagnosticCodes.UnknownSlot
                        $"Undeclared metadata extension '{name}' was discarded."
                        None
                        None
                        None
                        (Some name)
                )
            | Some _ ->
                match YamlCodec.tryScalar element with
                | Some value -> extensionValues.Add(ExtensionValue(name, value))
                | None ->
                    diagnostics.Add(
                        Diagnostics.warning
                            DiagnosticCodes.ExtensionValue
                            $"Extension '{name}' was discarded because its value is not scalar."
                            None
                            None
                            None
                            (Some name)
                    ))

        metadata.ExtensionValues <- extensionValues.ToArray()
        metadata, version

    let private mappingCellValues version (descriptor: SlotDescriptor) value =
        match ModelVersion.descriptorFor version descriptor with
        | Some versionDescriptor when value = "" -> [||]
        | Some versionDescriptor when versionDescriptor.Cardinality = SlotCardinality.Multivalued ->
            TextCodec.splitMultivalue version value
        | _ -> [| value |]

    let private decodeMappings
        version
        (metadata: MappingSet)
        (parsed: ParsedTsv)
        (diagnostics: ResizeArray<SssomDiagnostic>)
        =
        let definitions = metadata.ExtensionDefinitions
        let headers = parsed.Header
        let standard = headers |> Array.map MappingDescriptors.tryFind

        [| "predicate_id"; "mapping_justification" |]
        |> Array.iter (fun required ->
            if headers |> Array.contains required |> not then
                diagnostics.Add(
                    Diagnostics.error
                        DiagnosticCodes.MissingRequired
                        $"The TSV header must contain required column '{required}'."
                        None
                        None
                        None
                        (Some required)
                ))

        headers
        |> Array.iteri (fun index name ->
            match standard.[index] with
            | Some descriptor when ModelVersion.descriptorFor version descriptor |> Option.isNone ->
                diagnostics.Add(
                    Diagnostics.error
                        DiagnosticCodes.VersionConflict
                        $"Column '{name}' is not available in SSSOM {SssomVersion.toLexical version}."
                        None
                        None
                        None
                        (Some name)
                )
            | Some _ -> ()
            | None when definitions |> Array.exists (fun definition -> definition.SlotName = name) -> ()
            | None ->
                diagnostics.Add(
                    Diagnostics.warning
                        DiagnosticCodes.UnknownSlot
                        $"Undeclared TSV column '{name}' was discarded."
                        None
                        None
                        None
                        (Some name)
                ))

        parsed.Rows
        |> Array.mapi (fun rowIndex row ->
            let cells =
                Array.init headers.Length (fun index -> if index < row.Cells.Length then row.Cells.[index] else "")

            let valueFor name =
                headers
                |> Array.tryFindIndex ((=) name)
                |> Option.map (fun index -> cells.[index])
                |> Option.defaultValue ""

            let requiredValue name =
                let value = valueFor name
                if value = "" then
                    diagnostics.Add(
                        Diagnostics.error
                            DiagnosticCodes.MissingRequired
                            $"Mapping row {rowIndex + 1} is missing required slot '{name}'."
                            (Some row.Line)
                            None
                            (Some(rowIndex + 1))
                            (Some name)
                    )
                    "sssom:invalid"
                else
                    value

            let requiredEntity name =
                let value = requiredValue name

                match EntityReference.TryCreate value with
                | Some reference -> reference
                | None ->
                    diagnostics.Add(
                        Diagnostics.error
                            DiagnosticCodes.InvalidValue
                            $"Mapping row {rowIndex + 1} has invalid required entity reference '{name}'."
                            (Some row.Line)
                            None
                            (Some(rowIndex + 1))
                            (Some name)
                    )

                    EntityReference.Create "sssom:invalid"

            let mapping =
                Mapping(
                    requiredEntity "predicate_id",
                    requiredEntity "mapping_justification"
                )

            let extensionValues = ResizeArray<ExtensionValue>()

            headers
            |> Array.iteri (fun columnIndex name ->
                let cell = cells.[columnIndex]

                match standard.[columnIndex] with
                | Some descriptor ->
                    match ModelVersion.descriptorFor version descriptor with
                    | None -> ()
                    | Some versionDescriptor ->
                        let values = mappingCellValues version descriptor cell
                        let valid =
                            values
                            |> Array.map (fun value ->
                                Validation.validateLexical
                                    diagnostics
                                    version
                                    versionDescriptor.Range
                                    value
                                    (Some(rowIndex + 1))
                                    (Some name))
                            |> Array.forall id

                        if valid then MappingAccess.setValues mapping name values
                | None when cell <> "" && definitions |> Array.exists (fun definition -> definition.SlotName = name) ->
                    extensionValues.Add(ExtensionValue(name, cell))
                | None -> ())

            mapping.ExtensionValues <- extensionValues.ToArray()
            mapping)

    let private propagate version (metadata: MappingSet) (mappings: Mapping array) =
        if mappings.Length > 0 then
            MappingDescriptors.allDescriptors ()
            |> Array.iter (fun descriptor ->
                match ModelVersion.descriptorFor version descriptor with
                | Some versionDescriptor when versionDescriptor.IsPropagated ->
                    let metadataValues = MappingSetAccess.getValues metadata descriptor.Name
                    let allMappingsAbsent = mappings |> Array.forall (fun mapping -> MappingAccess.getValues mapping descriptor.Name |> Array.isEmpty)

                    if metadataValues.Length > 0 && allMappingsAbsent then
                        mappings |> Array.iter (fun mapping -> MappingAccess.setValues mapping descriptor.Name metadataValues)
                        MappingSetAccess.setValues metadata descriptor.Name [||]
                | _ -> ())

    let decode yaml tsv firstTsvLine =
        let diagnostics = ResizeArray<SssomDiagnostic>()
        let metadata, version = decodeMetadata yaml diagnostics
        let parsed = TextCodec.parseTsv tsv firstTsvLine diagnostics
        let mappings = decodeMappings version metadata parsed diagnostics
        let document = SssomDocument(metadata, mappings)

        if not (Diagnostics.hasErrors diagnostics) then
            propagate version metadata mappings
            Validation.validateDocument version true document |> Array.iter diagnostics.Add

        if Diagnostics.hasErrors diagnostics then
            DecodeResult(None, diagnostics.ToArray())
        else
            DecodeResult(Some document, diagnostics.ToArray())

    let embedded content =
        let diagnostics = ResizeArray<SssomDiagnostic>()
        let yaml, tsv, firstTsvLine = TextCodec.splitEmbedded content diagnostics
        let decoded = decode yaml tsv firstTsvLine
        decoded.Diagnostics |> Array.iter diagnostics.Add

        if Diagnostics.hasErrors diagnostics then DecodeResult(None, diagnostics.ToArray())
        else DecodeResult(decoded.Document, diagnostics.ToArray())

    let externalMetadata yaml tsv =
        let diagnostics = ResizeArray<SssomDiagnostic>()
        let normalizedYaml = TextCodec.normalizeLineEndings yaml
        let normalizedTsv = TextCodec.normalizeLineEndings tsv
        let yamlHasBom = normalizedYaml.Length > 0 && normalizedYaml.[0] = '\uFEFF'
        let tsvHasBom = normalizedTsv.Length > 0 && normalizedTsv.[0] = '\uFEFF'

        if yamlHasBom || tsvHasBom then
            diagnostics.Add(
                Diagnostics.error
                    DiagnosticCodes.MetadataStructure
                    "External metadata and TSV content must not start with a UTF-8 byte-order mark."
                    (Some 1)
                    (Some 1)
                    None
                    None
            )

        let cleanYaml = if yamlHasBom then normalizedYaml.Substring(1) else normalizedYaml
        let cleanTsv = if tsvHasBom then normalizedTsv.Substring(1) else normalizedTsv
        let decoded = decode cleanYaml cleanTsv 1
        decoded.Diagnostics |> Array.iter diagnostics.Add

        if Diagnostics.hasErrors diagnostics then DecodeResult(None, diagnostics.ToArray())
        else DecodeResult(decoded.Document, diagnostics.ToArray())
