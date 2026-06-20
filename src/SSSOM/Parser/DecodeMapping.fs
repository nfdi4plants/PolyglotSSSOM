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
                    match Double.TryParse(strValue, Globalization.CultureInfo.InvariantCulture) with
                    | true, value -> Some value
                    | _ -> None
                | None -> 
                    None

            let predicateId = getRequiredString "predicate_id"
            let mappingJustification = getRequiredString "mapping_justification"

            let newMapping = 
                Mapping(
                    predicate_id = predicateId,
                    mapping_justification = mappingJustification,
                    ?Record_id = getOptionalString "record_id",
                    ?Subject_id = getOptionalString "subject_id",
                    ?Subject_label = getOptionalString "subject_label",
                    ?Subject_category = getOptionalString "subject_category",
                    ?Predicate_label = getOptionalString "predicate_label",
                    ?Predicate_modifier = getOptionalString "predicate_modifier",
                    ?Object_id = getOptionalString "object_id",
                    ?Object_label = getOptionalString "object_label",
                    ?Object_category = getOptionalString "object_category",
                    ?Author_id = getOptionalString "author_id",
                    ?Author_label = getOptionalString "author_label",
                    ?Reviewer_id = getOptionalString "reviewer_id",
                    ?Reviewer_label = getOptionalString "reviewer_label",
                    ?Creator_id = getOptionalString "creator_id",
                    ?Creator_label = getOptionalString "creator_label",
                    ?License = getOptionalString "license",
                    ?Subject_type = getOptionalString "subject_type",
                    ?Subject_source = getOptionalString "subject_source",
                    ?Subject_source_version = getOptionalString "subject_source_version",
                    ?Object_type = getOptionalString "object_type",
                    ?Object_source = getOptionalString "object_source",
                    ?Object_source_version = getOptionalString "object_source_version",
                    ?Predicate_type = getOptionalString "predicate_type",
                    ?Mapping_provider = getOptionalString "mapping_provider",
                    ?Mapping_source = getOptionalString "mapping_source",
                    ?Mapping_cardinality = getOptionalString "mapping_cardinality",
                    ?Cardinality_scope = getOptionalString "cardinality_scope",
                    ?Mapping_tool = getOptionalString "mapping_tool",
                    ?Mapping_tool_id = getOptionalString "mapping_tool_id",
                    ?Mapping_tool_version = getOptionalString "mapping_tool_version",
                    ?Mapping_date = getOptionalString "mapping_date",
                    ?Publication_date = getOptionalString "publication_date",
                    ?Review_date = getOptionalString "review_date",
                    ?Confidence = getOptionalDouble "confidence",
                    ?Reviewer_agreement = getOptionalDouble "reviewer_agreement",
                    ?Curation_rule = getOptionalString "curation_rule",
                    ?Curation_rule_text = getOptionalString "curation_rule_text",
                    ?Subject_match_field = getOptionalString "subject_match_field",
                    ?Object_match_field = getOptionalString "object_match_field",
                    ?Match_string = getOptionalString "match_string",
                    ?Subject_preprocessing = getOptionalString "subject_preprocessing",
                    ?Object_preprocessing = getOptionalString "object_preprocessing",
                    ?Similarity_score = getOptionalDouble "similarity_score",
                    ?Similarity_measure = getOptionalString "similarity_measure",
                    ?See_also = getOptionalString "see_also",
                    ?Issue_tracker_item = getOptionalString "issue_tracker_item",
                    ?Derived_from = getOptionalString "derived_from",
                    ?Other = getOptionalString "other",
                    ?Comment = getOptionalString "comment"
                )

            mappings.Add(newMapping)
        
        mappings |> Seq.toList