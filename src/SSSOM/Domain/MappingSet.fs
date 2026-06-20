namespace SSSOM

open Fable.Core

[<AttachMembers>]
type MappingSet(
    ?Sssom_version: string,
    ?Curie_map: array<CurieMap>,
    ?Mappings: string,
    ?Mapping_set_id: string,
    ?Mapping_set_version: string,
    ?Mapping_set_source: string,
    ?Mapping_set_title: string,
    ?Mapping_set_description: string,
    ?Mapping_set_confidence: string,
    ?Creator_id: string,
    ?Creator_label: string,
    ?License: string,
    ?Subject_type: string,
    ?Subject_source: string,
    ?Subject_source_version: string,
    ?Object_type: string,
    ?Object_source: string,
    ?Object_source_version: string,
    ?Predicate_type: string,
    ?Mapping_provider: string,
    ?Cardinality_scope: string,
    ?Mapping_tool: string,
    ?Mapping_tool_id: string,
    ?Mapping_tool_version: string,
    ?Mapping_date: string,
    ?Publication_date: string,
    ?Subject_match_field: string,
    ?Object_match_field: string,
    ?Subject_preprocessing: string,
    ?Object_preprocessing: string,
    ?Similarity_measure: string,
    ?Curation_rule: string,
    ?Curation_rule_text: string,
    ?See_also: string,
    ?Issue_tracker: string,
    ?Other: string,
    ?Comment: string,
    ?Extension_definitions: string
) =
    let mutable _sssom_version = Sssom_version
    let mutable _curie_map = Curie_map
    let mutable _mappings = Mappings
    let mutable _mapping_set_id = Mapping_set_id
    let mutable _mapping_set_version = Mapping_set_version
    let mutable _mapping_set_source = Mapping_set_source
    let mutable _mapping_set_title = Mapping_set_title
    let mutable _mapping_set_description = Mapping_set_description
    let mutable _mapping_set_confidence = Mapping_set_confidence
    let mutable _creator_id = Creator_id
    let mutable _creator_label = Creator_label
    let mutable _license = License
    let mutable _subject_type = Subject_type
    let mutable _subject_source = Subject_source
    let mutable _subject_source_version = Subject_source_version
    let mutable _object_type = Object_type
    let mutable _object_source = Object_source
    let mutable _object_source_version = Object_source_version
    let mutable _predicate_type = Predicate_type
    let mutable _mapping_provider = Mapping_provider
    let mutable _cardinality_scope = Cardinality_scope
    let mutable _mapping_tool = Mapping_tool
    let mutable _mapping_tool_id = Mapping_tool_id
    let mutable _mapping_tool_version = Mapping_tool_version
    let mutable _mapping_date = Mapping_date
    let mutable _publication_date = Publication_date
    let mutable _subject_match_field = Subject_match_field
    let mutable _object_match_field = Object_match_field
    let mutable _subject_preprocessing = Subject_preprocessing
    let mutable _object_preprocessing = Object_preprocessing
    let mutable _similarity_measure = Similarity_measure
    let mutable _curation_rule = Curation_rule
    let mutable _curation_rule_text = Curation_rule_text
    let mutable _see_also = See_also
    let mutable _issue_tracker = Issue_tracker
    let mutable _other = Other
    let mutable _comment = Comment
    let mutable _extension_definitions = Extension_definitions

    member this.Sssom_version
        with get() = _sssom_version
        and set value = _sssom_version <- value

    member this.Curie_map
        with get() = _curie_map
        and set value = _curie_map <- value

    member this.Mappings
        with get() = _mappings
        and set value = _mappings <- value

    member this.Mapping_set_id
        with get() = _mapping_set_id
        and set value = _mapping_set_id <- value
    
    member this.Mapping_set_version
        with get() = _mapping_set_version
        and set value = _mapping_set_version <- value

    member this.Mapping_set_source
        with get() = _mapping_set_source
        and set value = _mapping_set_source <- value
    
    member this.Mapping_set_title
        with get() = _mapping_set_title
        and set value = _mapping_set_title <- value

    member this.Mapping_set_description
        with get() = _mapping_set_description
        and set value = _mapping_set_description <- value

    member this.Mapping_set_confidence
        with get() = _mapping_set_confidence
        and set value = _mapping_set_confidence <- value

    member this.Creator_id
        with get() = _creator_id
        and set value = _creator_id <- value
    
    member this.Creator_label
        with get() = _creator_label
        and set value = _creator_label <- value
    
    member this.License
        with get() = _license
        and set value = _license <- value

    member this.Subject_type
        with get() = _subject_type
        and set value = _subject_type <- value
    
    member this.Subject_source
        with get() = _subject_source
        and set value = _subject_source <- value

    member this.Subject_source_version
        with get() = _subject_source_version
        and set value = _subject_source_version <- value

    member this.Object_type
        with get() = _object_type
        and set value = _object_type <- value

    member this.Object_Source
        with get() = _object_source
        and set value = _object_source <- value

    member this.Object_source_version
        with get() = _object_source_version
        and set value = _object_source_version <- value

    member this.Predicate_type
        with get() = _predicate_type
        and set value = _predicate_type <- value

    member this.Mapping_provider
        with get() = _mapping_provider
        and set value = _mapping_provider <- value
    
    member this.Cardinality_scope
        with get() = _cardinality_scope
        and set value = _cardinality_scope <- value

    member this.Mapping_tool
        with get() = _mapping_tool
        and set value = _mapping_tool <- value

    member this.Mapping_tool_id
        with get() = _mapping_tool_id
        and set value = _mapping_tool_id <- value

    member this.Mapping_tool_version
        with get() = _mapping_tool_version
        and set value = _mapping_tool_version <- value

    member this.Mapping_date
        with get() = _mapping_date
        and set value = _mapping_date <- value

    member this.Publication_date
        with get() = _publication_date
        and set value = _publication_date <- value

    member this.Subject_match_field
        with get() = _subject_match_field
        and set value = _subject_match_field <- value

    member this.Object_match_field
        with get() = _object_match_field
        and set value = _object_match_field <- value

    member this.Subject_preprocessing
        with get() = _subject_preprocessing
        and set value = _subject_preprocessing <- value

    member this.Object_preprocessing
        with get() = _object_preprocessing
        and set value = _object_preprocessing <- value

    member this.Similarity_measure
        with get() = _similarity_measure
        and set value = _similarity_measure <- value

    member this.Curation_rule
        with get() = _curation_rule
        and set value = _curation_rule <- value

    member this.Curation_rule_text
        with get() = _curation_rule_text
        and set value = _curation_rule_text <- value

    member this.See_also
        with get() = _see_also
        and set value = _see_also <- value

    member this.Issue_tracker
        with get() = _issue_tracker
        and set value = _issue_tracker <- value

    member this.Other
        with get() = _other
        and set value = _other <- value

    member this.Comment
        with get() = _comment
        and set value = _comment <- value

    member this.Extension_definitions
        with get() = _extension_definitions
        and set value = _extension_definitions <- value