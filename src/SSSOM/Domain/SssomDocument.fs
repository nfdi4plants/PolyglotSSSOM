namespace SSSOM

open Fable.Core

/// Represents one SSSOM document as metadata plus mapping rows.
[<AttachMembers>]
type SssomDocument(metadata: MappingSet, mappings: Mapping array) =
    let mutable metadata = ModelValue.required (nameof metadata) metadata
    let mutable mappings = ModelValue.nonNullArray mappings

    let requireMapping argumentName (mapping: Mapping) =
        ModelValue.required argumentName mapping

    let matchingIndexes (recordId: EntityReference) =
        mappings
        |> Array.indexed
        |> Array.choose (fun (index, mapping) ->
            if isNull (box mapping) then
                None
            else
                mapping.RecordId
                |> Option.filter (fun value -> value.Value = recordId.Value)
                |> Option.map (fun _ -> index))

    let ensureRecordIdAvailable argumentName (recordId: EntityReference) =
        if matchingIndexes recordId |> Array.isEmpty |> not then
            invalidArg argumentName $"record_id '{recordId.Value}' already exists in this document."

    let promoteFor (mapping: Mapping) =
        if metadata.SssomVersion = Some SssomVersion.V1_0 && mapping.RequiresV1_1 then
            metadata.SssomVersion <- Some SssomVersion.V1_1

    /// Creates an empty document from lexical mapping-set and license URI values.
    static member Create(mappingSetId: string, license: string) =
        SssomDocument(MappingSet.Create(mappingSetId, license), [||])

    /// Gets or sets the required mapping-set metadata.
    member _.Metadata
        with get () = metadata
        and set value = metadata <- ModelValue.required (nameof value) value

    /// Gets or sets the mapping rows; null is normalized to an empty array.
    member _.Mappings
        with get () = mappings
        and set value = mappings <- ModelValue.nonNullArray value

    /// Appends a mapping while preserving its optional record identifier.
    member _.AddMapping(mapping: Mapping) =
        let mapping = requireMapping (nameof mapping) mapping
        mapping.RecordId |> Option.iter (ensureRecordIdAvailable (nameof mapping))
        promoteFor mapping
        mappings <- Array.append mappings [| mapping |]

    /// Assigns a caller-supplied v1.1 record identifier and appends the mapping atomically.
    member _.AddMappingWithRecordId(recordId: string, mapping: Mapping) =
        let mapping = requireMapping (nameof mapping) mapping
        let parsedRecordId = EntityReference.Create recordId

        match mapping.RecordId with
        | Some existing when existing.Value <> parsedRecordId.Value ->
            invalidArg
                (nameof mapping)
                $"The mapping already has record_id '{existing.Value}', which differs from '{parsedRecordId.Value}'."
        | _ -> ()

        ensureRecordIdAvailable (nameof recordId) parsedRecordId
        mapping.RecordId <- Some parsedRecordId
        promoteFor mapping
        mappings <- Array.append mappings [| mapping |]

    /// Tries to find the unique mapping carrying the supplied record identifier.
    member _.TryFindMappingByRecordId(recordId: string) =
        let parsedRecordId = EntityReference.Create recordId

        match matchingIndexes parsedRecordId with
        | [||] -> None
        | [| index |] -> Some mappings.[index]
        | _ -> invalidOp $"record_id '{recordId}' occurs more than once in this document."

    /// Replaces the unique mapping carrying the supplied record identifier and returns the previous mapping.
    member _.ReplaceMappingByRecordId(recordId: string, replacement: Mapping) =
        let replacement = requireMapping (nameof replacement) replacement
        let parsedRecordId = EntityReference.Create recordId

        match replacement.RecordId with
        | Some existing when existing.Value = parsedRecordId.Value -> ()
        | Some existing ->
            invalidArg
                (nameof replacement)
                $"Replacement record_id '{existing.Value}' differs from '{parsedRecordId.Value}'."
        | None -> invalidArg (nameof replacement) "A replacement mapping must retain the selected record_id."

        match matchingIndexes parsedRecordId with
        | [| index |] ->
            let previous = mappings.[index]
            let updated = Array.copy mappings
            updated.[index] <- replacement
            promoteFor replacement
            mappings <- updated
            previous
        | [||] -> invalidOp $"record_id '{recordId}' does not exist in this document."
        | _ -> invalidOp $"record_id '{recordId}' occurs more than once in this document."

    /// Removes and returns the unique mapping carrying the supplied record identifier.
    member _.RemoveMappingByRecordId(recordId: string) =
        let parsedRecordId = EntityReference.Create recordId

        match matchingIndexes parsedRecordId with
        | [||] -> None
        | [| index |] ->
            let removed = mappings.[index]

            mappings <-
                mappings
                |> Array.indexed
                |> Array.choose (fun (candidateIndex, mapping) ->
                    if candidateIndex = index then None else Some mapping)

            Some removed
        | _ -> invalidOp $"record_id '{recordId}' occurs more than once in this document."

    /// Creates an independent copy of the metadata, mapping array, and every mutable nested model object.
    member _.Clone() =
        SssomDocument(metadata.Clone(), mappings |> Array.map (fun mapping -> mapping.Clone()))
