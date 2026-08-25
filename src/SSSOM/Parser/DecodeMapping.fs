namespace SSSOM

open System
open System.Text
open System.Collections.Generic
open Fable.Core

[<AttachMembers>]
type DecodeMapping() =
    static member extractMapping (source: string) =
        let lines = source.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
        let processYamlInput = StringBuilder()

        for line in lines do
            let checkLine = line.TrimStart([|' '; '\t'; '/'|])

            if not (checkLine.StartsWith("#")) then
                processYamlInput.Append(line + "\n") |> ignore

        processYamlInput.ToString().TrimEnd('\n')

    static member isValidTsvInput (source: string) =
        let prcessedString = DecodeMapping.extractMapping(source)
        let lines = prcessedString.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)

        if lines.Length = 0 then
            false
        else
            let mutable isValid = true

            let headers = lines.[0].Split([|'\t'|])
            let expectedColumns = headers.Length


            let mutable hasPredicate_id = false
            let mutable hasMapping_justification = false

            for header in headers do
                let lowercaseHeader = header.ToLower()
                if lowercaseHeader.Contains("predicate_id") then
                    hasPredicate_id <- true

                if lowercaseHeader.Contains("mapping_justification") then
                    hasMapping_justification <- true

            if hasPredicate_id && hasMapping_justification then
                for i = 1 to lines.Length - 1 do
                    if isValid then
                        let currentLine = lines.[i]
                        let currentColumns = currentLine.Split([|'\t'|]).Length

                        if currentColumns <> expectedColumns then
                            isValid <- false
            else
                isValid <- false
            isValid

    static member DecodeMapping (source: string) =
        let tsvString = DecodeMapping.extractMapping(source)

        if not (DecodeMapping.isValidTsvInput(tsvString)) then
            failwith "Invalid TSV Input: Missing required columns or inconsistent column count."

        let lines = tsvString.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
        let headers = lines.[0].Split([|'\t'|])

        let headerIndexMap = new Dictionary<string, int>()

        for i = 0 to headers.Length - 1 do
            headerIndexMap.Add(headers.[i], i)
        
        let mappings = ResizeArray<Mapping>()

        for i = 1 to lines.Length - 1 do
            let columns = lines.[i].Split([|'\t'|])

            let getOptionalString (colName: string) =
                match headerIndexMap.TryGetValue(colName.ToLower()) with
                | true, index ->
                    if index < columns.Length && not (String.IsNullOrWhiteSpace(columns.[index])) then
                        Some (columns.[index])
                    else
                        None
                | false, _ ->
                    None

            let getRequiredString (colName: string) =
                match getOptionalString colName with
                | Some value -> value
                | None -> failwith (sprintf "Row %d: Mandatory field '%s' is missing or empty!" i colName)

            let getOptionalDouble (colName: string) =
                match getOptionalString colName with
                | Some strValue ->
#if FABLE_COMPILER
                    match Double.TryParse(strValue) with
#else
                    match
                        Double.TryParse(
                            strValue,
                            Globalization.NumberStyles.Float ||| Globalization.NumberStyles.AllowThousands,
                            Globalization.CultureInfo.InvariantCulture
                        )
                    with
#endif
                    | true, value -> Some value
                    | _ -> None
                | None -> 
                    None

            let getEntityReference (colName: string) =
                match getRequiredString colName with
                | entityreference -> EntityReference.create entityreference

            let getOptionalEntityReference (colName: string) =
                match getOptionalString colName with
                | Some entityRef -> Some (EntityReference.create entityRef)
                | None -> None

            let getOptionalPredicateModifierEnum (colName: string) =
                match getOptionalString colName with
                | Some enum -> Some (PredicateModifierEnum.create enum)
                | None -> None

            let getOptionalNonRelativeURI (colName: string) =
                match getOptionalString colName with
                | Some uri -> Some (NonRelativeURI.create uri)
                | None -> None

            let getOptionalEntityTypeEnum (colName: string) =
                match getOptionalString colName with
                | Some enum -> Some (EntityTypeEnum.create colName)
                | None -> None

            let getOptionalMappingCardinalityEnum (colName: string) =
                match getOptionalString colName with
                | Some enum -> Some (MappingCardinalityEnum.create enum)
                | None -> None

            let getOptionalDate (colName: string) =
                match getOptionalString colName with
                | Some date -> Some (Date.create date)
                | None -> None

            let predicateId = getEntityReference "predicate_id"
            let mappingJustification = getEntityReference "mapping_justification"

            let newMapping = 
                Mapping(
                    predicate_id = predicateId,
                    mapping_justification = mappingJustification,
                    ?Record_id = getOptionalEntityReference "record_id",
                    ?Subject_id = getOptionalEntityReference "subject_id",
                    ?Subject_label = getOptionalString "subject_label",
                    ?Subject_category = getOptionalString "subject_category",
                    ?Predicate_label = getOptionalString "predicate_label",
                    ?Predicate_modifier = getOptionalPredicateModifierEnum "predicate_modifier",
                    ?Object_id = getOptionalEntityReference "object_id",
                    ?Object_label = getOptionalString "object_label",
                    ?Object_category = getOptionalString "object_category",
                    ?Author_id = getOptionalEntityReference "author_id",
                    ?Author_label = getOptionalString "author_label",
                    ?Reviewer_id = getOptionalEntityReference "reviewer_id",
                    ?Reviewer_label = getOptionalString "reviewer_label",
                    ?Creator_id = getOptionalEntityReference "creator_id",
                    ?Creator_label = getOptionalString "creator_label",
                    ?License = getOptionalNonRelativeURI "license",
                    ?Subject_type = getOptionalEntityTypeEnum "subject_type",
                    ?Subject_source = getOptionalEntityReference "subject_source",
                    ?Subject_source_version = getOptionalString "subject_source_version",
                    ?Object_type = getOptionalEntityTypeEnum "object_type",
                    ?Object_source = getOptionalEntityReference "object_source",
                    ?Object_source_version = getOptionalString "object_source_version",
                    ?Predicate_type = getOptionalEntityTypeEnum "predicate_type",
                    ?Mapping_provider = getOptionalNonRelativeURI "mapping_provider",
                    ?Mapping_source = getOptionalEntityReference "mapping_source",
                    ?Mapping_cardinality = getOptionalMappingCardinalityEnum "mapping_cardinality",
                    ?Cardinality_scope = getOptionalString "cardinality_scope",
                    ?Mapping_tool = getOptionalString "mapping_tool",
                    ?Mapping_tool_id = getOptionalEntityReference "mapping_tool_id",
                    ?Mapping_tool_version = getOptionalString "mapping_tool_version",
                    ?Mapping_date = getOptionalDate "mapping_date",
                    ?Publication_date = getOptionalDate "publication_date",
                    ?Review_date = getOptionalDate "review_date",
                    ?Confidence = getOptionalDouble "confidence",
                    ?Reviewer_agreement = getOptionalDouble "reviewer_agreement",
                    ?Curation_rule = getOptionalEntityReference "curation_rule",
                    ?Curation_rule_text = getOptionalString "curation_rule_text",
                    ?Subject_match_field = getOptionalEntityReference "subject_match_field",
                    ?Object_match_field = getOptionalEntityReference "object_match_field",
                    ?Match_string = getOptionalString "match_string",
                    ?Subject_preprocessing = getOptionalEntityReference "subject_preprocessing",
                    ?Object_preprocessing = getOptionalEntityReference "object_preprocessing",
                    ?Similarity_score = getOptionalDouble "similarity_score",
                    ?Similarity_measure = getOptionalString "similarity_measure",
                    ?See_also = getOptionalNonRelativeURI "see_also",
                    ?Issue_tracker_item = getOptionalEntityReference "issue_tracker_item",
                    ?Derived_from = getOptionalEntityReference "derived_from",
                    ?Other = getOptionalString "other",
                    ?Comment = getOptionalString "comment"
                )

            mappings.Add(newMapping)
        
        mappings |> Seq.toList
