namespace SSSOM

open Fable.Core

[<AttachMembers>]
type Mapping(
    predicate_id: EntityReference,
    mapping_justification: EntityReference,
    ?Record_id: EntityReference,
    ?Subject_id: EntityReference,
    ?Subject_label: string,
    ?Subject_category: string,
    ?Predicate_label: string,
    ?Predicate_modifier: PredicateModifierEnum,
    ?Object_id: EntityReference,
    ?Object_label: string,
    ?Object_category: string,
    ?Author_id: EntityReference,
    ?Author_label: string,
    ?Reviewer_id: EntityReference,
    ?Reviewer_label: string,
    ?Creator_id: EntityReference,
    ?Creator_label: string,
    ?License: NonRelativeURI,
    ?Subject_type: EntityTypeEnum,
    ?Subject_source: EntityReference,
    ?Subject_source_version: string,
    ?Object_type: EntityTypeEnum,
    ?Object_source: EntityReference,
    ?Object_source_version: string,
    ?Predicate_type: EntityTypeEnum,
    ?Mapping_provider: NonRelativeURI,
    ?Mapping_source: EntityReference,
    ?Mapping_cardinality: MappingCardinalityEnum,
    ?Cardinality_scope: string,
    ?Mapping_tool: string,
    ?Mapping_tool_id: EntityReference,
    ?Mapping_tool_version: string,
    ?Mapping_date: Date,
    ?Publication_date: Date,
    ?Review_date: Date,
    ?Confidence: double,
    ?Reviewer_agreement: double,
    ?Curation_rule: EntityReference,
    ?Curation_rule_text: string,
    ?Subject_match_field: EntityReference,
    ?Object_match_field: EntityReference,
    ?Match_string: string,
    ?Subject_preprocessing: EntityReference,
    ?Object_preprocessing: EntityReference,
    ?Similarity_score: double,
    ?Similarity_measure: string,
    ?See_also: NonRelativeURI,
    ?Issue_tracker_item: EntityReference,
    ?Derived_from: EntityReference,
    ?Other: string,
    ?Comment: string
) =
    let mutable _predicate_id = predicate_id
    let mutable _mapping_justification = mapping_justification
    let mutable _record_id = Record_id
    let mutable _subject_id = Subject_id
    let mutable _subject_label = Subject_label
    let mutable _subject_category = Subject_category
    let mutable _predicate_label = Predicate_label
    let mutable _predicate_modifier = Predicate_modifier
    let mutable _object_id = Object_id
    let mutable _object_label = Object_label
    let mutable _object_category = Object_category
    let mutable _author_id = Author_id
    let mutable _author_label = Author_label
    let mutable _reviewer_id = Reviewer_id
    let mutable _reviewer_label = Reviewer_label
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
    let mutable _mapping_source = Mapping_source
    let mutable _mapping_cardinality = Mapping_cardinality
    let mutable _cardinality_scope = Cardinality_scope
    let mutable _mapping_tool = Mapping_tool
    let mutable _mapping_tool_id = Mapping_tool_id
    let mutable _mapping_tool_version = Mapping_tool_version
    let mutable _mapping_date = Mapping_date
    let mutable _publication_date = Publication_date
    let mutable _review_date = Review_date
    let mutable _confidence = Confidence
    let mutable _reviewer_agreement = Reviewer_agreement
    let mutable _curation_rule = Curation_rule
    let mutable _curation_rule_text = Curation_rule_text
    let mutable _subject_match_field = Subject_match_field
    let mutable _object_match_field = Object_match_field
    let mutable _match_string = Match_string
    let mutable _subject_preprocessing = Subject_preprocessing
    let mutable _object_preprocessing = Object_preprocessing
    let mutable _similarity_score = Similarity_score
    let mutable _similarity_measure = Similarity_measure
    let mutable _see_also = See_also
    let mutable _issue_tracker_item = Issue_tracker_item
    let mutable _derived_from = Derived_from
    let mutable _other = Other
    let mutable _comment = Comment


    member this.Predicate_id
        with get() = _predicate_id
        and set value = _predicate_id <- value

    member this.Mapping_justification
        with get() = _mapping_justification
        and set value = _mapping_justification <- value

    member this.Record_id
        with get() = _record_id
        and set value = _record_id <- value

    member this.Subject_id
        with get() = _subject_id
        and set value = _subject_id <- value

    member this.Subject_label
        with get() = _subject_label
        and set value = _subject_label <- value

    member this.Subject_category
        with get() = _subject_category
        and set value = _subject_category <- value

    member this.Predicate_label
        with get() = _predicate_label
        and set value = _predicate_label <- value

    member this.Predicate_modifier
        with get() = _predicate_modifier
        and set value = _predicate_modifier <- value

    member this.Object_id
        with get() = _object_id
        and set value = _object_id <- value

    member this.Object_label
        with get() = _object_label
        and set value = _object_label <- value

    member this.Object_category
        with get() = _object_category
        and set value = _object_category <- value

    member this.author_id
        with get() = _author_id
        and set value = _author_id <- value

    member this.Author_label
        with get() = _author_label
        and set value = _author_label <- value

    member this.Reviewer_id
        with get() = _reviewer_id
        and set value = _reviewer_id <- value

    member this.Reviewer_label
        with get() = _reviewer_label
        and set value = _reviewer_label <- value

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

    member this.Object_source
        with get() = _object_type
        and set value = _object_type <- value

    member this.Object_source_version
        with get() = _object_source_version
        and set value = _object_source_version <- value

    member this.Predicate_type
        with get() = _predicate_type
        and set value = _predicate_type <- value

    member this.Mapping_provider
        with get() = _mapping_provider
        and set value = _mapping_provider <- value

    member this.Mapping_source
        with get() = _mapping_source
        and set value = _mapping_source <- value

    member this.Mapping_cardinality
        with get() = _mapping_cardinality
        and set value = _mapping_cardinality <- value

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

    member this.Review_date
        with get() = _review_date
        and set value = _review_date <- value

    member this.Confidence
        with get() = _confidence
        and set value = _confidence <- value

    member this.Reviewer_agreement
        with get() = _reviewer_agreement
        and set value = _reviewer_agreement <- value

    member this.Curation_rule
        with get() = _curation_rule
        and set value = _curation_rule <- value

    member this.Curation_rule_text
        with get() = _curation_rule_text
        and set value = _curation_rule_text <- value

    member this.Subject_match_field
        with get() = _subject_match_field
        and set value = _subject_match_field <- value

    member this.Object_match_field
        with get() = _object_match_field
        and set value = _object_match_field <- value

    member this.Match_string
        with get() = _match_string
        and set value = _match_string <- value

    member this.Subject_preprocessing
        with get() = _subject_preprocessing
        and set value = _subject_preprocessing <- value

    member this.Object_preprocessing
        with get() = _object_preprocessing
        and set value = _object_preprocessing <- value

    member this.Similarity_score
        with get() = _similarity_score
        and set value = _similarity_score <- value

    member this.Similarity_measure
        with get() = _similarity_measure
        and set value = _similarity_measure <- value

    member this.See_also
        with get() = _see_also
        and set value = _see_also <- value

    member this.Issue_tracker_item
        with get() = _issue_tracker_item
        and set value = _issue_tracker_item <- value

    member this.Derived_from
        with get() = _derived_from
        and set value = _derived_from <- value

    member this.Other
        with get() = _other
        and set value = _other <- value

    member this.Comment
        with get() = _comment
        and set value = _comment <- value