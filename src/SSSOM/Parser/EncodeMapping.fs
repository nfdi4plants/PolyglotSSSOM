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

        let getValue opt = 
            match opt with
            | Some (value: string) -> value.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ")
            | _ -> ""

        let getFloatValue opt =
            match opt with
            | Some (v: double) -> v.ToString(System.Globalization.CultureInfo.InvariantCulture)
            | _ -> ""

        let getRowValues (i: Mapping) = 
            [|
                i.Predicate_id.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ")
                i.Mapping_justification.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ")
                getValue i.Record_id
                getValue i.Subject_id
                getValue i.Subject_label
                getValue i.Subject_category
                getValue i.Predicate_label
                getValue i.Predicate_modifier
                getValue i.Object_id
                getValue i.Object_label
                getValue i.Object_category
                getValue i.author_id
                getValue i.Author_label
                getValue i.Reviewer_id
                getValue i.Reviewer_label
                getValue i.Creator_id
                getValue i.Creator_label
                getValue i.License
                getValue i.Subject_type
                getValue i.Subject_source
                getValue i.Subject_source_version
                getValue i.Object_type
                getValue i.Object_source
                getValue i.Object_source_version
                getValue i.Predicate_type
                getValue i.Mapping_provider
                getValue i.Mapping_source
                getValue i.Mapping_cardinality
                getValue i.Cardinality_scope
                getValue i.Mapping_tool
                getValue i.Mapping_tool_id
                getValue i.Mapping_tool_version
                getValue i.Mapping_date
                getValue i.Publication_date
                getValue i.Review_date
                getFloatValue i.Confidence
                getFloatValue i.Reviewer_agreement
                getValue i.Curation_rule
                getValue i.Curation_rule_text
                getValue i.Subject_match_field
                getValue i.Object_match_field
                getValue i.Match_string
                getValue i.Subject_preprocessing
                getValue i.Object_preprocessing
                getFloatValue i.Similarity_score
                getValue i.Similarity_measure
                getValue i.See_also
                getValue i.Issue_tracker_item
                getValue i.Derived_from
                getValue i.Other
                getValue i.Comment
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


