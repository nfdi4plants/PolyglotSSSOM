namespace SSSOM

open System
open System.Text
open Fable.Core

[<AttachMembers>]
type EncodeMapping() = 


    static member EncodeMapping(mapping: list<Mapping>) =
        let tsvString = StringBuilder()

        let allHeaders = [|
            "predicate_id";
            "mapping_justification";
            "record_id";
            "subject_id";
            "subject_label";
            "subject_category";
            "predicate_label";
            "predicate_modifier";
            "object_id";
            "object_label";
            "object_category";
            "author_id";
            "author_label";
            "reviewer_id";
            "reviewer_label";
            "creator_id";
            "creator_label";
            "license";
            "subject_type";
            "subject_source";
            "subject_source_version";
            "object_type";
            "object_source";
            "object_source_version";
            "predicate_type";
            "mapping_provider";
            "mapping_source";
            "mapping_cardinality";
            "cardinality_scope";
            "mapping_tool";
            "mapping_tool_id";
            "mapping_tool_version";
            "mapping_date";
            "publication_date";
            "review_date";
            "confidence";
            "reviewer_agreement";
            "curation_role";
            "curation_role_text";
            "subject_match_field";
            "object_match_field";
            "match_string";
            "subject_preprocessing";
            "object_preprocessing";
            "similarity_score";
            "similarity_measure";
            "see_also";
            "issue_tracker_item";
            "derived_from";
            "other";
            "comment"
        |]

        let numCols = allHeaders.Length

        let processString (s: string) = 
            s.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ")

        let getOptionalString opt = 
            match opt with
            | Some (value: string) -> processString value
            | _ -> ""

        let getOptionalFloat opt =
            match opt with
            | Some (v: double) -> v.ToString(System.Globalization.CultureInfo.InvariantCulture)
            | _ -> ""

        let getEntityReference (opt: EntityReference) =
            processString opt.Value
 

        let getOptionalEntityReference (opt: option<EntityReference>) =
            match opt with
            | Some ref -> processString ref.Value
            | None -> ""

        let getOptionalPredicateModifierEnum (opt: option<PredicateModifierEnum>) =
            match opt with
            | Some enum -> PredicateModifierEnum.toString enum
            | None -> ""

        let getOptionalNonRelativeURI (opt: option<NonRelativeURI>) =
            match opt with
            | Some uri -> processString uri.Value
            | None -> ""

        let getOptionalEntityTypeEnum (opt: option<EntityTypeEnum>) =
            match opt with
            | Some enum -> EntityTypeEnum.toString enum
            | None -> ""

        let getOptionalMappingCardinalityEnum (opt: option<MappingCardinalityEnum>) =
            match opt with
            | Some enum -> MappingCardinalityEnum.toString enum
            | None -> ""

        let getOptionalDate (opt: option<Date>) =
            match opt with
            | Some date -> date.Value
            | None -> ""

        let getRowValues (i: Mapping) = 
            [|
                getEntityReference i.Predicate_id
                getEntityReference i.Mapping_justification
                getOptionalEntityReference i.Record_id
                getOptionalEntityReference i.Subject_id
                getOptionalString i.Subject_label
                getOptionalString i.Subject_category
                getOptionalString i.Predicate_label
                getOptionalPredicateModifierEnum i.Predicate_modifier
                getOptionalEntityReference i.Object_id
                getOptionalString i.Object_label
                getOptionalString i.Object_category
                getOptionalEntityReference i.author_id
                getOptionalString i.Author_label
                getOptionalEntityReference i.Reviewer_id
                getOptionalString i.Reviewer_label
                getOptionalEntityReference i.Creator_id
                getOptionalString i.Creator_label
                getOptionalNonRelativeURI i.License
                getOptionalEntityTypeEnum i.Subject_type
                getOptionalEntityReference i.Subject_source
                getOptionalString i.Subject_source_version
                getOptionalEntityTypeEnum i.Object_type
                getOptionalEntityTypeEnum i.Object_source
                getOptionalString i.Object_source_version
                getOptionalEntityTypeEnum i.Predicate_type
                getOptionalNonRelativeURI i.Mapping_provider
                getOptionalEntityReference i.Mapping_source
                getOptionalMappingCardinalityEnum i.Mapping_cardinality
                getOptionalString i.Cardinality_scope
                getOptionalString i.Mapping_tool
                getOptionalEntityReference i.Mapping_tool_id
                getOptionalString i.Mapping_tool_version
                getOptionalDate i.Mapping_date
                getOptionalDate i.Publication_date
                getOptionalDate i.Review_date
                getOptionalFloat i.Confidence
                getOptionalFloat i.Reviewer_agreement
                getOptionalEntityReference i.Curation_rule
                getOptionalString i.Curation_rule_text
                getOptionalEntityReference i.Subject_match_field
                getOptionalEntityReference i.Object_match_field
                getOptionalString i.Match_string
                getOptionalEntityReference i.Subject_preprocessing
                getOptionalEntityReference i.Object_preprocessing
                getOptionalFloat i.Similarity_score
                getOptionalString i.Similarity_measure
                getOptionalNonRelativeURI i.See_also
                getOptionalEntityReference i.Issue_tracker_item
                getOptionalEntityReference i.Derived_from
                getOptionalString i.Other
                getOptionalString i.Comment
            |]

        let columnHasData = Array.create numCols false

        for item in mapping do
            let rowVals = getRowValues item
            for colIdx in 0 .. numCols - 1 do
                if rowVals.[colIdx] <> "" then
                    columnHasData.[colIdx] <- true

        let activeHeaders = ResizeArray<string>()
        for colIdx in 0..numCols-1 do
            if columnHasData.[colIdx] then
                activeHeaders.Add(allHeaders.[colIdx])

        tsvString.Append(String.Join("\t", activeHeaders)).Append("\n") |> ignore

        for item in mapping do
            let rowVals = getRowValues item
            let activeValues = ResizeArray<string>()

            for colIdx in 0..numCols - 1 do
                if columnHasData.[colIdx] then
                    activeValues.Add(rowVals.[colIdx])

            tsvString.Append(String.Join("\t", activeValues)).Append("\n") |> ignore
        
        tsvString.ToString()


