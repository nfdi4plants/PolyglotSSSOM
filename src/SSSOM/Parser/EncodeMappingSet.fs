namespace SSSOM

open System
open System.Text
open Fable.Core

[<AttachMembers>]
type EncodeMappingSet() =
    static member EncodeMappingSet(mappingSet: MappingSet) =
        let builder = StringBuilder()

        let addField(yamlKey: string) (optValue: string option) =
            match optValue with
            | Some value when not (String.IsNullOrWhiteSpace(value)) ->
                let cleanValue = value.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ")

                builder.Append("#").Append(yamlKey).Append(": ").Append(cleanValue).Append("\n") |> ignore
            |_ -> ()

        let addNonRelativeUri (yamlKey: string) (optValue: NonRelativeURI option) =
            match optValue with
            | Some v ->
                let value = v.Value
                let cleanValue = value.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ")

                builder.Append("#").Append(yamlKey).Append(": ").Append(cleanValue).Append("\n") |> ignore
            |_ -> ()

        let addSssom (yamlKey: string) (optValue: SssomVersion option) =
            match optValue with
            | Some v ->
                let value = SssomVersion.toString v
                let cleanValue = value.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ")

                builder.Append("#").Append(yamlKey).Append(": ").Append(cleanValue).Append("\n") |> ignore
            | _ -> ()

        let addEntityReference (yamlKey: string) (optValue: EntityReference option) =
            match optValue with
            | Some v ->
                let value = v.Value
                let cleanValue = value.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ")

                builder.Append("#").Append(yamlKey).Append(": ").Append(cleanValue).Append("\n") |> ignore
            | _ -> ()

        let addExtensionDefinition (optValue: option<array<ExtensionDefinition>>) =
            match optValue with
            | Some extList when not (extList.Length <> 0) ->
                builder.Append("#extension_definitions:\n") |> ignore

                for ext in extList do

                    match ext.Property with
                    | Some p -> builder.Append("#  property: ").Append(p.Value).Append("\n") |> ignore
                    | None -> ()

                    match ext.Slot_Name with
                    | Some s -> builder.Append("#  slot_name: ").Append(s).Append("\n") |> ignore
                    | None -> ()

                    match ext.Type_hint with
                    | Some t -> builder.Append("#  type_hint: ").Append(t).Append("\n") |> ignore
                    | None -> ()
            | _ -> ()

        match mappingSet.Curie_map with
        | Some curies when curies.Length > 0 ->
            builder.Append("#curie_map:\n") |> ignore
            for curie in curies do
                builder.Append("#  ").Append(curie.Prefix_name).Append(": ").Append(curie.Prefix_url).Append("\n") |> ignore
        | _ -> ()

        addSssom "sssom_version" mappingSet.Sssom_version
        addField "mappings" mappingSet.Mappings
        addNonRelativeUri "mapping_set_id" mappingSet.Mapping_set_id
        addField "mapping_set_version" mappingSet.Mapping_set_version
        addNonRelativeUri "mapping_set_source" mappingSet.Mapping_set_source
        addField "mapping_set_title" mappingSet.Mapping_set_title
        addField "mapping_set_description" mappingSet.Mapping_set_description
        addField "mapping_set_confidence" mappingSet.Mapping_set_confidence
        addEntityReference "creator_id" mappingSet.Creator_id
        addField "creator_label" mappingSet.Creator_label
        addNonRelativeUri "license" mappingSet.License
        addField "subject_type" mappingSet.Subject_type
        addEntityReference "subject_source" mappingSet.Subject_source
        addField "object_type" mappingSet.Object_type
        addEntityReference "object_source" mappingSet.Object_Source
        addField "object_source_version" mappingSet.Object_source_version
        addField "predicate_type" mappingSet.Predicate_type
        addField "cardinality_scope" mappingSet.Cardinality_scope
        addField "mapping_tool" mappingSet.Mapping_tool
        addEntityReference "mapping_tool_id" mappingSet.Mapping_tool_id
        addField "mapping_tool_version" mappingSet.Mapping_tool_version
        addField "mapping_date" mappingSet.Mapping_date
        addField "publication_date" mappingSet.Publication_date
        addEntityReference "subject_match_field" mappingSet.Subject_match_field
        addEntityReference "object_match_field" mappingSet.Object_match_field
        addEntityReference "subject_preprocessing" mappingSet.Subject_preprocessing
        addEntityReference "object_preprocessing" mappingSet.Object_preprocessing
        addField "similarity_measure" mappingSet.Similarity_measure
        addEntityReference "curation_rule" mappingSet.Curation_rule
        addField "curation_rule_text" mappingSet.Curation_rule_text
        addNonRelativeUri "see_also" mappingSet.See_also
        addNonRelativeUri "issue_tracker" mappingSet.Issue_tracker
        addField "other" mappingSet.Other
        addExtensionDefinition mappingSet.Extension_definitions
        addField "comment" mappingSet.Comment

        builder.ToString().TrimEnd('\n')




