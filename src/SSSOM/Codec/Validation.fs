namespace SSSOM

open System
open System.Text.RegularExpressions

module internal Validation =

    let private addError
        (diagnostics: ResizeArray<SssomDiagnostic>)
        code
        message
        row
        slot
        =
        diagnostics.Add(Diagnostics.error code message None None row slot)

    let private addWarning
        (diagnostics: ResizeArray<SssomDiagnostic>)
        code
        message
        row
        slot
        =
        diagnostics.Add(Diagnostics.warning code message None None row slot)

    let validateLexical
        (diagnostics: ResizeArray<SssomDiagnostic>)
        version
        range
        value
        row
        slot
        =
        let invalid expected =
            addError diagnostics DiagnosticCodes.InvalidValue $"'{value}' is not a valid {expected} value." row slot

        match range with
        | SlotRange.Text -> true
        | SlotRange.Number ->
            match LexicalCodec.tryParseDouble value with
            | Some _ -> true
            | None -> invalid "finite number"; false
        | SlotRange.Date ->
            match SssomDate.TryCreate value with
            | Some _ -> true
            | None -> invalid "YYYY-MM-DD date"; false
        | SlotRange.EntityReference ->
            match EntityReference.TryCreate value with
            | Some _ -> true
            | None -> invalid "URI or CURIE"; false
        | SlotRange.Uri ->
            match UriReference.TryCreate value with
            | Some _ -> true
            | None -> invalid "URI lexical"; false
        | SlotRange.NonRelativeUri ->
            match UriReference.TryCreate value with
            | Some reference when reference.IsNonRelative -> true
            | _ -> invalid "non-relative URI"; false
        | SlotRange.SssomVersion ->
            match SssomVersion.tryParse value with
            | Some _ -> true
            | None ->
                addError diagnostics DiagnosticCodes.UnsupportedVersion $"SSSOM version '{value}' is not supported." row slot
                false
        | SlotRange.EntityType ->
            match EntityType.tryParse value with
            | Some parsed when version = SssomVersion.V1_1 || EntityType.minimumVersion parsed = SssomVersion.V1_0 -> true
            | Some _ ->
                addError diagnostics DiagnosticCodes.VersionConflict $"Entity type '{value}' requires SSSOM 1.1." row slot
                false
            | None -> invalid "entity-type enumeration"; false
        | SlotRange.PredicateModifier ->
            match PredicateModifier.tryParse value with
            | Some _ -> true
            | None -> invalid "predicate-modifier enumeration"; false
        | SlotRange.MappingCardinality ->
            match MappingCardinality.tryParse value with
            | Some parsed when version = SssomVersion.V1_1 || MappingCardinality.minimumVersion parsed = SssomVersion.V1_0 -> true
            | Some _ ->
                addError diagnostics DiagnosticCodes.VersionConflict $"Mapping cardinality '{value}' requires SSSOM 1.1." row slot
                false
            | None -> invalid "mapping-cardinality enumeration"; false
        | SlotRange.Prefix
        | SlotRange.ExtensionDefinition -> true

    let private isAbsoluteIri (value: string) =
        value.Contains("://") || value.StartsWith("urn:")

    let private tryCuriePrefix (value: string) =
        if String.IsNullOrWhiteSpace value || isAbsoluteIri value then
            None
        else
            let separator = value.IndexOf(':')
            if separator > 0 && separator < value.Length - 1 then Some(value.Substring(0, separator)) else None

    let private prefixExists (metadata: MappingSet) prefix =
        CurieMap.isBuiltIn prefix
        || metadata.CurieMap
           |> Array.exists (fun entry -> not (isNull (box entry)) && entry.PrefixName = prefix)

    let private validateEntityReference
        (diagnostics: ResizeArray<SssomDiagnostic>)
        (metadata: MappingSet)
        requireCurieSyntax
        row
        slot
        value
        =
        match tryCuriePrefix value with
        | Some prefix when prefixExists metadata prefix -> ()
        | Some prefix ->
            addError
                diagnostics
                DiagnosticCodes.CuriePrefix
                $"CURIE '{value}' uses undeclared prefix '{prefix}'."
                row
                slot
        | None when isAbsoluteIri value && not requireCurieSyntax ->
            if CurieMap.tryContract metadata.CurieMap value |> Option.isNone then
                addError
                    diagnostics
                    DiagnosticCodes.CuriePrefix
                    $"IRI '{value}' cannot be contracted because no CURIE prefix matches it."
                    row
                    slot
        | None when isAbsoluteIri value ->
            addError
                diagnostics
                DiagnosticCodes.CuriePrefix
                $"SSSOM/TSV identifier '{value}' must be serialized as a CURIE."
                row
                slot
        | None ->
            addError diagnostics DiagnosticCodes.InvalidValue $"'{value}' is not a CURIE." row slot

    let private descriptorEntityValues version descriptors getter source =
        descriptors
        |> Array.collect (fun descriptor ->
            match ModelVersion.descriptorFor version descriptor with
            | Some versionDescriptor when versionDescriptor.Range = SlotRange.EntityReference ->
                getter source descriptor.Name |> Array.map (fun value -> descriptor.Name, value)
            | _ -> [||])

    let private extensionProperty (definition: ExtensionDefinition) =
        definition.Property
        |> Option.map (fun value -> value.Value)
        |> Option.defaultValue ("http://sssom.invalid/" + definition.SlotName)

    let private extensionType (metadata: MappingSet) (definition: ExtensionDefinition) =
        definition.TypeHint
        |> Option.map (fun value -> value.Value)
        |> Option.defaultValue "xsd:string"
        |> CurieMap.tryExpand metadata.CurieMap
        |> Option.defaultValue "http://www.w3.org/2001/XMLSchema#string"

    let private extensionDefinitions
        (diagnostics: ResizeArray<SssomDiagnostic>)
        (metadata: MappingSet)
        requireCurieSyntax
        =
        let definitions = if isNull metadata.ExtensionDefinitions then [||] else metadata.ExtensionDefinitions
        let standardNames =
            Array.append (MappingSetDescriptors.allDescriptors ()) (MappingDescriptors.allDescriptors ())
            |> Array.map (fun descriptor -> descriptor.Name)
            |> Set.ofArray

        let validDefinitions = definitions |> Array.filter (fun definition -> not (isNull (box definition)))

        if validDefinitions.Length <> definitions.Length then
            addError
                diagnostics
                DiagnosticCodes.ExtensionDefinition
                "Extension definitions must not contain null values."
                None
                (Some "extension_definitions")

        validDefinitions
        |> Array.iter (fun definition ->
            if not (LexicalValidation.isNcName definition.SlotName) || Set.contains definition.SlotName standardNames then
                addError
                    diagnostics
                    DiagnosticCodes.ExtensionDefinition
                    $"Extension slot name '{definition.SlotName}' is invalid or conflicts with a standard slot."
                    None
                    (Some "extension_definitions")

            definition.Property
            |> Option.iter (fun value ->
                validateEntityReference diagnostics metadata requireCurieSyntax None (Some "extension_definitions") value.Value)

            definition.TypeHint
            |> Option.iter (fun value ->
                validateEntityReference diagnostics metadata requireCurieSyntax None (Some "extension_definitions") value.Value))

        validDefinitions
        |> Array.filter (fun definition -> not (isNull (box definition)))
        |> Array.countBy (fun definition -> definition.SlotName)
        |> Array.filter (fun (_, count) -> count > 1)
        |> Array.iter (fun (slotName, _) ->
            addError
                diagnostics
                DiagnosticCodes.ExtensionDefinition
                $"Extension slot name '{slotName}' is defined more than once."
                None
                (Some "extension_definitions"))

        validDefinitions
        |> Array.filter (fun definition -> not (isNull (box definition)))
        |> Array.countBy extensionProperty
        |> Array.filter (fun (_, count) -> count > 1)
        |> Array.iter (fun (property, _) ->
            addError
                diagnostics
                DiagnosticCodes.ExtensionDefinition
                $"Extension property '{property}' is used by more than one definition."
                None
                (Some "extension_definitions"))

        validDefinitions

    let private validateTypedExtension
        (diagnostics: ResizeArray<SssomDiagnostic>)
        (metadata: MappingSet)
        (definition: ExtensionDefinition)
        row
        (extension: ExtensionValue)
        =
        let invalid expected =
            addError
                diagnostics
                DiagnosticCodes.ExtensionValue
                $"Extension '{extension.SlotName}' value '{extension.Value}' is not a valid {expected}."
                row
                (Some extension.SlotName)

        match extensionType metadata definition with
        | "http://www.w3.org/2001/XMLSchema#integer" ->
            let mutable parsed = 0
            if not (Int32.TryParse(extension.Value, &parsed)) then invalid "integer"
        | "http://www.w3.org/2001/XMLSchema#double" ->
            if LexicalCodec.tryParseDouble extension.Value |> Option.isNone then invalid "number"
        | "http://www.w3.org/2001/XMLSchema#boolean" ->
            if extension.Value <> "true" && extension.Value <> "false" then invalid "boolean"
        | "http://www.w3.org/2001/XMLSchema#date" ->
            if SssomDate.TryCreate extension.Value |> Option.isNone then invalid "date"
        | "http://www.w3.org/2001/XMLSchema#dateTime" ->
            if not (Regex.IsMatch(extension.Value, "^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(Z|[+-][0-9]{2}:[0-9]{2})$")) then
                invalid "dateTime"
        | "https://w3id.org/linkml/Uriorcurie" ->
            validateEntityReference diagnostics metadata false row (Some extension.SlotName) extension.Value
        | _ -> ()

    let private validateExtensions
        (diagnostics: ResizeArray<SssomDiagnostic>)
        (metadata: MappingSet)
        (definitions: ExtensionDefinition array)
        (row: int option)
        (values: ExtensionValue array)
        =
        let source = if isNull values then [||] else values

        source
        |> Array.filter (fun value -> not (isNull (box value)))
        |> Array.countBy (fun value -> value.SlotName)
        |> Array.filter (fun (_, count) -> count > 1)
        |> Array.iter (fun (slotName, _) ->
            addError
                diagnostics
                DiagnosticCodes.ExtensionValue
                $"Extension slot '{slotName}' occurs more than once at the same document level."
                row
                (Some slotName))

        source
        |> Array.iter (fun extension ->
            if isNull (box extension) then
                addError diagnostics DiagnosticCodes.ExtensionValue "Extension values must not contain null values." row None
            else
                match definitions |> Array.tryFind (fun definition -> definition.SlotName = extension.SlotName) with
                | Some definition -> validateTypedExtension diagnostics metadata definition row extension
                | None ->
                    addWarning
                        diagnostics
                        DiagnosticCodes.UnknownSlot
                        $"Undeclared extension slot '{extension.SlotName}' will be discarded."
                        row
                        (Some extension.SlotName))

    let private effectiveEntityType version (metadata: MappingSet) (mappings: Mapping array) slot (mapping: Mapping) =
        let local =
            match slot with
            | "subject_type" -> mapping.SubjectType
            | "object_type" -> mapping.ObjectType
            | _ -> None

        if local.IsSome then
            local
        else
            let mayPropagate =
                MappingDescriptors.tryFind slot
                |> Option.bind (ModelVersion.descriptorFor version)
                |> Option.exists (fun descriptor -> descriptor.IsPropagated)

            let allLocalAbsent =
                mappings
                |> Array.forall (fun item ->
                    match slot with
                    | "subject_type" -> item.SubjectType.IsNone
                    | "object_type" -> item.ObjectType.IsNone
                    | _ -> true)

            if mayPropagate && allLocalAbsent then
                match slot with
                | "subject_type" -> metadata.SubjectType
                | "object_type" -> metadata.ObjectType
                | _ -> None
            else
                None

    let private validateConditionalRules
        (diagnostics: ResizeArray<SssomDiagnostic>)
        version
        (metadata: MappingSet)
        (mappings: Mapping array)
        =
        mappings
        |> Array.iteri (fun index mapping ->
            let row = Some(index + 1)
            let subjectType = effectiveEntityType version metadata mappings "subject_type" mapping
            let objectType = effectiveEntityType version metadata mappings "object_type" mapping

            if subjectType = Some EntityType.RdfsLiteral then
                if mapping.SubjectLabel.IsNone then
                    addError diagnostics DiagnosticCodes.ConditionalRequirement "subject_label is required when subject_type is 'rdfs literal'." row (Some "subject_label")
            elif mapping.SubjectId.IsNone then
                addError diagnostics DiagnosticCodes.ConditionalRequirement "subject_id is required unless subject_type is 'rdfs literal'." row (Some "subject_id")

            if objectType = Some EntityType.RdfsLiteral then
                if mapping.ObjectLabel.IsNone then
                    addError diagnostics DiagnosticCodes.ConditionalRequirement "object_label is required when object_type is 'rdfs literal'." row (Some "object_label")
            elif mapping.ObjectId.IsNone then
                addError diagnostics DiagnosticCodes.ConditionalRequirement "object_id is required unless object_type is 'rdfs literal'." row (Some "object_id")

            if version = SssomVersion.V1_1
               && (mapping.ReviewDate.IsSome || mapping.ReviewerAgreement.IsSome)
               && mapping.ReviewerId.Length = 0
               && mapping.ReviewerLabel.Length = 0 then
                addError
                    diagnostics
                    DiagnosticCodes.ConditionalRequirement
                    "reviewer_id or reviewer_label is required when review_date or reviewer_agreement is present."
                    row
                    (Some "reviewer_id"))

    let private validateNumbers (diagnostics: ResizeArray<SssomDiagnostic>) (metadata: MappingSet) (mappings: Mapping array) =
        let bounded (row: int option) (slot: string) (minimum: double) (maximum: double) (value: double option) =
            value
            |> Option.iter (fun number ->
                if Double.IsNaN number || Double.IsInfinity number || number < minimum || number > maximum then
                    addError
                        diagnostics
                        DiagnosticCodes.InvalidValue
                        $"{slot} must be between {minimum} and {maximum}."
                        row
                        (Some slot))

        bounded None "mapping_set_confidence" 0.0 1.0 metadata.MappingSetConfidence

        mappings
        |> Array.iteri (fun index mapping ->
            let row = Some(index + 1)
            bounded row "confidence" 0.0 1.0 mapping.Confidence
            bounded row "similarity_score" 0.0 1.0 mapping.SimilarityScore
            bounded row "reviewer_agreement" -1.0 1.0 mapping.ReviewerAgreement)

    let private validateRecordIds (diagnostics: ResizeArray<SssomDiagnostic>) (mappings: Mapping array) =
        let withIds = mappings |> Array.filter (fun mapping -> mapping.RecordId.IsSome)

        if withIds.Length > 0 && withIds.Length <> mappings.Length then
            addError
                diagnostics
                DiagnosticCodes.RecordId
                "record_id must be present on either every mapping row or no mapping rows."
                None
                (Some "record_id")

        withIds
        |> Array.map (fun mapping -> mapping.RecordId.Value.Value)
        |> Array.countBy id
        |> Array.filter (fun (_, count) -> count > 1)
        |> Array.iter (fun (recordId, _) ->
            addError diagnostics DiagnosticCodes.RecordId $"record_id '{recordId}' is not unique." None (Some "record_id"))

    let private validateCardinality (diagnostics: ResizeArray<SssomDiagnostic>) (mappings: Mapping array) =
        let noTerm = "sssom:NoTermFound"

        mappings
        |> Array.iteri (fun index mapping ->
            let row = Some(index + 1)
            let subjectNoTerm = mapping.SubjectId |> Option.exists (fun value -> value.Value = noTerm)
            let objectNoTerm = mapping.ObjectId |> Option.exists (fun value -> value.Value = noTerm)
            let expected =
                match subjectNoTerm, objectNoTerm with
                | true, true -> Some MappingCardinality.NoneToNone
                | true, false -> Some MappingCardinality.NoneToOne
                | false, true -> Some MappingCardinality.OneToNone
                | false, false -> None

            match expected, mapping.MappingCardinality with
            | Some required, Some actual when required = actual -> ()
            | Some required, _ ->
                addError
                    diagnostics
                    DiagnosticCodes.Cardinality
                    $"A NoTermFound mapping requires mapping_cardinality '{MappingCardinality.toLexical required}'."
                    row
                    (Some "mapping_cardinality")
            | None, Some MappingCardinality.OneToNone
            | None, Some MappingCardinality.NoneToOne
            | None, Some MappingCardinality.NoneToNone ->
                addError
                    diagnostics
                    DiagnosticCodes.Cardinality
                    "A zero-sided mapping cardinality requires sssom:NoTermFound in the corresponding identifier slot."
                    row
                    (Some "mapping_cardinality")
            | _ -> ()

            MappingDescriptors.allDescriptors ()
            |> Array.filter (fun descriptor -> descriptor.Name <> "subject_id" && descriptor.Name <> "object_id")
            |> Array.collect (fun descriptor -> MappingAccess.getValues mapping descriptor.Name |> Array.map (fun value -> descriptor.Name, value))
            |> Array.filter (fun (_, value) -> value = noTerm)
            |> Array.iter (fun (slot, _) ->
                addError
                    diagnostics
                    DiagnosticCodes.Cardinality
                    "sssom:NoTermFound may only occur in subject_id or object_id."
                    row
                    (Some slot)))

    let private validateVersionedSlots
        (diagnostics: ResizeArray<SssomDiagnostic>)
        (version: SssomVersion)
        (descriptors: SlotDescriptor array)
        (getter: 'a -> string -> string array)
        (sources: 'a array)
        =
        descriptors
        |> Array.iter (fun descriptor ->
            sources
            |> Array.iteri (fun index source ->
                let values = getter source descriptor.Name
                let row = if descriptor.Placement = SlotPlacement.Mapping then Some(index + 1) else None

                match ModelVersion.descriptorFor version descriptor with
                | None when descriptor.Name <> "sssom_version" && values.Length > 0 ->
                    addError
                        diagnostics
                        DiagnosticCodes.VersionConflict
                        $"Slot '{descriptor.Name}' is not available in SSSOM {SssomVersion.toLexical version}."
                        row
                        (Some descriptor.Name)
                | Some versionDescriptor ->
                    values
                    |> Array.iter (fun value ->
                        validateLexical diagnostics version versionDescriptor.Range value row (Some descriptor.Name) |> ignore)
                | None -> ()))

    let validateDocument version requireCurieSyntax (document: SssomDocument) =
        let diagnostics = ResizeArray<SssomDiagnostic>()

        if isNull (box document) then
            addError diagnostics DiagnosticCodes.MissingRequired "The SSSOM document is required." None None
            diagnostics.ToArray()
        elif isNull (box document.Metadata) then
            addError diagnostics DiagnosticCodes.MissingRequired "Mapping-set metadata is required." None None
            diagnostics.ToArray()
        else
            let metadata = document.Metadata
            let mappings = if isNull document.Mappings then [||] else document.Mappings
            let validMappings = mappings |> Array.filter (fun mapping -> not (isNull (box mapping)))

            if isNull (box metadata.MappingSetId) then
                addError diagnostics DiagnosticCodes.MissingRequired "mapping_set_id is required." None (Some "mapping_set_id")

            if isNull (box metadata.License) then
                addError diagnostics DiagnosticCodes.MissingRequired "license is required." None (Some "license")

            metadata.CurieMap
            |> Array.filter (fun entry -> not (isNull (box entry)))
            |> Array.countBy (fun entry -> entry.PrefixName)
            |> Array.filter (fun (_, count) -> count > 1)
            |> Array.iter (fun (prefix, _) ->
                addError diagnostics DiagnosticCodes.CuriePrefix $"CURIE prefix '{prefix}' is declared more than once." None (Some "curie_map"))

            metadata.CurieMap
            |> Array.iter (fun entry ->
                if isNull (box entry) then
                    addError diagnostics DiagnosticCodes.CuriePrefix "CURIE maps must not contain null entries." None (Some "curie_map")
                elif not entry.PrefixUrl.IsNonRelative then
                    addError diagnostics DiagnosticCodes.CuriePrefix $"CURIE prefix '{entry.PrefixName}' does not expand to a non-relative URI." None (Some "curie_map")
                elif CurieMap.isBuiltIn entry.PrefixName then
                    let expected =
                        CurieMap.builtInEntries ()
                        |> Array.find (fun builtIn -> builtIn.PrefixName = entry.PrefixName)
                        |> fun builtIn -> builtIn.PrefixUrl.Value

                    if entry.PrefixUrl.Value <> expected then
                        addError
                            diagnostics
                            DiagnosticCodes.CuriePrefix
                            $"Built-in prefix '{entry.PrefixName}' must expand to '{expected}'."
                            None
                            (Some "curie_map"))

            validateVersionedSlots
                diagnostics
                version
                (MappingSetDescriptors.allDescriptors ())
                MappingSetAccess.getValues
                [| metadata |]

            validateVersionedSlots
                diagnostics
                version
                (MappingDescriptors.allDescriptors ())
                MappingAccess.getValues
                validMappings

            descriptorEntityValues version (MappingSetDescriptors.allDescriptors ()) MappingSetAccess.getValues metadata
            |> Array.iter (fun (slot, value) -> validateEntityReference diagnostics metadata requireCurieSyntax None (Some slot) value)

            mappings
            |> Array.iteri (fun index mapping ->
                if isNull (box mapping) then
                    addError diagnostics DiagnosticCodes.MissingRequired "Mapping rows must not contain null values." (Some(index + 1)) None
                else
                    descriptorEntityValues version (MappingDescriptors.allDescriptors ()) MappingAccess.getValues mapping
                    |> Array.iter (fun (slot, value) ->
                        validateEntityReference diagnostics metadata requireCurieSyntax (Some(index + 1)) (Some slot) value))

            let definitions = extensionDefinitions diagnostics metadata requireCurieSyntax
            validateExtensions diagnostics metadata definitions None metadata.ExtensionValues

            mappings
            |> Array.iteri (fun index mapping ->
                if not (isNull (box mapping)) then
                    validateExtensions diagnostics metadata definitions (Some(index + 1)) mapping.ExtensionValues)

            validateConditionalRules diagnostics version metadata validMappings
            validateNumbers diagnostics metadata validMappings
            validateRecordIds diagnostics validMappings
            validateCardinality diagnostics validMappings

            diagnostics.ToArray()

    let validateForPublicApi (document: SssomDocument) =
        if isNull (box document) || isNull (box document.Metadata) then
            validateDocument SssomVersion.V1_0 false document
        elif document.Mappings |> Array.exists (fun mapping -> isNull (box mapping)) then
            validateDocument SssomVersion.V1_0 false document
        else
            let required = ModelVersion.minimumVersion document

            match document.Metadata.SssomVersion with
            | Some SssomVersion.V1_0 when required = SssomVersion.V1_1 ->
                Array.append
                    [| Diagnostics.error
                           DiagnosticCodes.VersionConflict
                           "The document declares SSSOM 1.0 but contains data that requires SSSOM 1.1."
                           None
                           None
                           None
                           (Some "sssom_version") |]
                    (validateDocument SssomVersion.V1_0 false document)
            | _ -> validateDocument required false document
