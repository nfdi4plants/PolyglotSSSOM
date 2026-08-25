namespace SSSOM

open Fable.Core

/// Identifies the document level on which a standard slot is valid.
[<RequireQualifiedAccess>]
type SlotPlacement =
    | MappingSet
    | Mapping

/// Identifies the specification-level lexical range of a slot.
[<RequireQualifiedAccess>]
type SlotRange =
    | Text
    | Number
    | Date
    | EntityReference
    | Uri
    | NonRelativeUri
    | SssomVersion
    | Prefix
    | ExtensionDefinition
    | EntityType
    | PredicateModifier
    | MappingCardinality

/// Identifies whether a slot has one or multiple lexical values.
[<RequireQualifiedAccess>]
type SlotCardinality =
    | Scalar
    | Multivalued

/// Identifies conditional required-field groups from the SSSOM model.
[<RequireQualifiedAccess>]
type ConditionalRequirement =
    | NoCondition
    | SubjectIdentifier
    | SubjectLiteralLabel
    | ObjectIdentifier
    | ObjectLiteralLabel
    | ReviewerIdentity

/// Describes a slot's constraints in one SSSOM specification version.
[<AttachMembers>]
type SlotVersionDescriptor(
    range: SlotRange,
    cardinality: SlotCardinality,
    isRequired: bool,
    isPropagated: bool,
    conditionalRequirement: ConditionalRequirement
) =

    /// Gets the specification-level range.
    member _.Range = range

    /// Gets the slot cardinality.
    member _.Cardinality = cardinality

    /// Gets whether the slot is unconditionally required.
    member _.IsRequired = isRequired

    /// Gets whether mapping-set values may propagate to mappings.
    member _.IsPropagated = isPropagated

    /// Gets the conditional required-field group.
    member _.ConditionalRequirement = conditionalRequirement

/// Describes one standard slot across supported SSSOM versions.
[<AttachMembers>]
type SlotDescriptor(
    name: string,
    propertyName: string,
    placement: SlotPlacement,
    order: int,
    v1_0: SlotVersionDescriptor option,
    v1_1: SlotVersionDescriptor option
) =

    /// Gets the normative snake_case slot name.
    member _.Name = name

    /// Gets the portable PascalCase model property name.
    member _.PropertyName = propertyName

    /// Gets the document level on which the slot is valid.
    member _.Placement = placement

    /// Gets the normative relative ordering used by canonical encoding.
    member _.Order = order

    /// Gets the v1.0 constraints, or None when the slot did not exist.
    member _.V1_0 = v1_0

    /// Gets the v1.1 constraints, or None when the slot does not exist.
    member _.V1_1 = v1_1

module private DescriptorFactory =

    let version range cardinality required propagated condition =
        Some(SlotVersionDescriptor(range, cardinality, required, propagated, condition))

    let scalar range required propagated =
        version range SlotCardinality.Scalar required propagated ConditionalRequirement.NoCondition

    let multi range required propagated =
        version range SlotCardinality.Multivalued required propagated ConditionalRequirement.NoCondition

    let conditional range cardinality propagated condition =
        version range cardinality false propagated condition

    let slot placement order name propertyName v1_0 v1_1 =
        SlotDescriptor(name, propertyName, placement, order, v1_0, v1_1)

/// The complete standard mapping-set metadata slot catalog.
module MappingSetDescriptors =

    open DescriptorFactory

    let private at order name propertyName v1_0 v1_1 =
        slot SlotPlacement.MappingSet order name propertyName v1_0 v1_1

    let private all =
        [|
            at 0 "sssom_version" "SssomVersion" None (scalar SlotRange.SssomVersion false false)
            at 1 "curie_map" "CurieMap" (multi SlotRange.Prefix false false) (multi SlotRange.Prefix false false)
            at 2 "mapping_set_id" "MappingSetId" (scalar SlotRange.Uri true false) (scalar SlotRange.NonRelativeUri true false)
            at 3 "mapping_set_version" "MappingSetVersion" (scalar SlotRange.Text false false) (scalar SlotRange.Text false false)
            at 4 "mapping_set_source" "MappingSetSource" (multi SlotRange.Uri false false) (multi SlotRange.NonRelativeUri false false)
            at 5 "mapping_set_title" "MappingSetTitle" (scalar SlotRange.Text false false) (scalar SlotRange.Text false false)
            at 6 "mapping_set_description" "MappingSetDescription" (scalar SlotRange.Text false false) (scalar SlotRange.Text false false)
            at 7 "mapping_set_confidence" "MappingSetConfidence" None (scalar SlotRange.Number false false)
            at 8 "creator_id" "CreatorId" (multi SlotRange.EntityReference false false) (multi SlotRange.EntityReference false false)
            at 9 "creator_label" "CreatorLabel" (multi SlotRange.Text false false) (multi SlotRange.Text false false)
            at 10 "license" "License" (scalar SlotRange.Uri true false) (scalar SlotRange.NonRelativeUri true false)
            at 11 "subject_type" "SubjectType" (scalar SlotRange.EntityType false true) (scalar SlotRange.EntityType false true)
            at 12 "subject_source" "SubjectSource" (scalar SlotRange.EntityReference false true) (scalar SlotRange.EntityReference false true)
            at 13 "subject_source_version" "SubjectSourceVersion" (scalar SlotRange.Text false true) (scalar SlotRange.Text false true)
            at 14 "object_type" "ObjectType" (scalar SlotRange.EntityType false true) (scalar SlotRange.EntityType false true)
            at 15 "object_source" "ObjectSource" (scalar SlotRange.EntityReference false true) (scalar SlotRange.EntityReference false true)
            at 16 "object_source_version" "ObjectSourceVersion" (scalar SlotRange.Text false true) (scalar SlotRange.Text false true)
            at 17 "predicate_type" "PredicateType" None (scalar SlotRange.EntityType false true)
            at 18 "mapping_provider" "MappingProvider" (scalar SlotRange.Uri false true) (scalar SlotRange.NonRelativeUri false true)
            at 19 "cardinality_scope" "CardinalityScope" None (multi SlotRange.Text false true)
            at 20 "mapping_tool" "MappingTool" (scalar SlotRange.Text false true) (scalar SlotRange.Text false true)
            at 21 "mapping_tool_id" "MappingToolId" None (scalar SlotRange.EntityReference false true)
            at 22 "mapping_tool_version" "MappingToolVersion" (scalar SlotRange.Text false true) (scalar SlotRange.Text false true)
            at 23 "mapping_date" "MappingDate" (scalar SlotRange.Date false true) (scalar SlotRange.Date false true)
            at 24 "publication_date" "PublicationDate" (scalar SlotRange.Date false false) (scalar SlotRange.Date false false)
            at 25 "subject_match_field" "SubjectMatchField" (multi SlotRange.EntityReference false true) (multi SlotRange.EntityReference false true)
            at 26 "object_match_field" "ObjectMatchField" (multi SlotRange.EntityReference false true) (multi SlotRange.EntityReference false true)
            at 27 "subject_preprocessing" "SubjectPreprocessing" (multi SlotRange.EntityReference false true) (multi SlotRange.EntityReference false true)
            at 28 "object_preprocessing" "ObjectPreprocessing" (multi SlotRange.EntityReference false true) (multi SlotRange.EntityReference false true)
            at 29 "similarity_measure" "SimilarityMeasure" None (scalar SlotRange.Text false true)
            at 30 "curation_rule" "CurationRule" None (multi SlotRange.EntityReference false true)
            at 31 "curation_rule_text" "CurationRuleText" None (multi SlotRange.Text false true)
            at 32 "see_also" "SeeAlso" (multi SlotRange.Text false false) (multi SlotRange.NonRelativeUri false false)
            at 33 "issue_tracker" "IssueTracker" (scalar SlotRange.Uri false false) (scalar SlotRange.NonRelativeUri false false)
            at 34 "other" "Other" (scalar SlotRange.Text false false) (scalar SlotRange.Text false false)
            at 35 "comment" "Comment" (scalar SlotRange.Text false false) (scalar SlotRange.Text false false)
            at 36 "extension_definitions" "ExtensionDefinitions" (multi SlotRange.ExtensionDefinition false false) (multi SlotRange.ExtensionDefinition false false)
        |]

    /// Gets a fresh array containing every standard mapping-set descriptor.
    let allDescriptors () = Array.copy all

    /// Tries to find a mapping-set descriptor by its normative slot name.
    let tryFind name = all |> Array.tryFind (fun descriptor -> descriptor.Name = name)

/// The complete standard mapping row slot catalog.
module MappingDescriptors =

    open DescriptorFactory

    let private at order name propertyName v1_0 v1_1 =
        slot SlotPlacement.Mapping order name propertyName v1_0 v1_1

    let private bothScalar range required propagated =
        scalar range required propagated, scalar range required propagated

    let private bothMulti range propagated =
        multi range false propagated, multi range false propagated

    let private all =
        let scalarBoth range required propagated = bothScalar range required propagated
        let multiBoth range propagated = bothMulti range propagated
        let subjectId = conditional SlotRange.EntityReference SlotCardinality.Scalar false ConditionalRequirement.SubjectIdentifier
        let subjectLabel = conditional SlotRange.Text SlotCardinality.Scalar false ConditionalRequirement.SubjectLiteralLabel
        let objectId = conditional SlotRange.EntityReference SlotCardinality.Scalar false ConditionalRequirement.ObjectIdentifier
        let objectLabel = conditional SlotRange.Text SlotCardinality.Scalar false ConditionalRequirement.ObjectLiteralLabel
        let reviewerId = conditional SlotRange.EntityReference SlotCardinality.Multivalued false ConditionalRequirement.ReviewerIdentity
        let reviewerLabel = conditional SlotRange.Text SlotCardinality.Multivalued false ConditionalRequirement.ReviewerIdentity

        [|
            at 0 "record_id" "RecordId" None (scalar SlotRange.EntityReference false false)
            at 1 "subject_id" "SubjectId" subjectId subjectId
            at 2 "subject_label" "SubjectLabel" subjectLabel subjectLabel
            at 3 "subject_category" "SubjectCategory" (fst (scalarBoth SlotRange.Text false false)) (snd (scalarBoth SlotRange.Text false false))
            at 4 "predicate_id" "PredicateId" (fst (scalarBoth SlotRange.EntityReference true false)) (snd (scalarBoth SlotRange.EntityReference true false))
            at 5 "predicate_label" "PredicateLabel" (fst (scalarBoth SlotRange.Text false false)) (snd (scalarBoth SlotRange.Text false false))
            at 6 "predicate_modifier" "PredicateModifier" (fst (scalarBoth SlotRange.PredicateModifier false false)) (snd (scalarBoth SlotRange.PredicateModifier false false))
            at 7 "object_id" "ObjectId" objectId objectId
            at 8 "object_label" "ObjectLabel" objectLabel objectLabel
            at 9 "object_category" "ObjectCategory" (fst (scalarBoth SlotRange.Text false false)) (snd (scalarBoth SlotRange.Text false false))
            at 10 "mapping_justification" "MappingJustification" (fst (scalarBoth SlotRange.EntityReference true false)) (snd (scalarBoth SlotRange.EntityReference true false))
            at 11 "author_id" "AuthorId" (fst (multiBoth SlotRange.EntityReference false)) (snd (multiBoth SlotRange.EntityReference false))
            at 12 "author_label" "AuthorLabel" (fst (multiBoth SlotRange.Text false)) (snd (multiBoth SlotRange.Text false))
            at 13 "reviewer_id" "ReviewerId" (multi SlotRange.EntityReference false false) reviewerId
            at 14 "reviewer_label" "ReviewerLabel" (multi SlotRange.Text false false) reviewerLabel
            at 15 "creator_id" "CreatorId" (fst (multiBoth SlotRange.EntityReference false)) (snd (multiBoth SlotRange.EntityReference false))
            at 16 "creator_label" "CreatorLabel" (fst (multiBoth SlotRange.Text false)) (snd (multiBoth SlotRange.Text false))
            at 17 "license" "License" (scalar SlotRange.Uri false false) (scalar SlotRange.NonRelativeUri false false)
            at 18 "subject_type" "SubjectType" (fst (scalarBoth SlotRange.EntityType false true)) (snd (scalarBoth SlotRange.EntityType false true))
            at 19 "subject_source" "SubjectSource" (fst (scalarBoth SlotRange.EntityReference false true)) (snd (scalarBoth SlotRange.EntityReference false true))
            at 20 "subject_source_version" "SubjectSourceVersion" (fst (scalarBoth SlotRange.Text false true)) (snd (scalarBoth SlotRange.Text false true))
            at 21 "object_type" "ObjectType" (fst (scalarBoth SlotRange.EntityType false true)) (snd (scalarBoth SlotRange.EntityType false true))
            at 22 "object_source" "ObjectSource" (fst (scalarBoth SlotRange.EntityReference false true)) (snd (scalarBoth SlotRange.EntityReference false true))
            at 23 "object_source_version" "ObjectSourceVersion" (fst (scalarBoth SlotRange.Text false true)) (snd (scalarBoth SlotRange.Text false true))
            at 24 "predicate_type" "PredicateType" None (scalar SlotRange.EntityType false true)
            at 25 "mapping_provider" "MappingProvider" (scalar SlotRange.Uri false true) (scalar SlotRange.NonRelativeUri false true)
            at 26 "mapping_source" "MappingSource" (fst (scalarBoth SlotRange.EntityReference false false)) (snd (scalarBoth SlotRange.EntityReference false false))
            at 27 "mapping_cardinality" "MappingCardinality" (fst (scalarBoth SlotRange.MappingCardinality false false)) (snd (scalarBoth SlotRange.MappingCardinality false false))
            at 28 "cardinality_scope" "CardinalityScope" None (multi SlotRange.Text false true)
            at 29 "mapping_tool" "MappingTool" (fst (scalarBoth SlotRange.Text false true)) (snd (scalarBoth SlotRange.Text false true))
            at 30 "mapping_tool_id" "MappingToolId" None (scalar SlotRange.EntityReference false true)
            at 31 "mapping_tool_version" "MappingToolVersion" (fst (scalarBoth SlotRange.Text false true)) (snd (scalarBoth SlotRange.Text false true))
            at 32 "mapping_date" "MappingDate" (fst (scalarBoth SlotRange.Date false true)) (snd (scalarBoth SlotRange.Date false true))
            at 33 "publication_date" "PublicationDate" (fst (scalarBoth SlotRange.Date false false)) (snd (scalarBoth SlotRange.Date false false))
            at 34 "review_date" "ReviewDate" None (scalar SlotRange.Date false false)
            at 35 "confidence" "Confidence" (fst (scalarBoth SlotRange.Number false false)) (snd (scalarBoth SlotRange.Number false false))
            at 36 "reviewer_agreement" "ReviewerAgreement" None (scalar SlotRange.Number false false)
            at 37 "curation_rule" "CurationRule" (multi SlotRange.EntityReference false false) (multi SlotRange.EntityReference false true)
            at 38 "curation_rule_text" "CurationRuleText" (multi SlotRange.Text false false) (multi SlotRange.Text false true)
            at 39 "subject_match_field" "SubjectMatchField" (fst (multiBoth SlotRange.EntityReference true)) (snd (multiBoth SlotRange.EntityReference true))
            at 40 "object_match_field" "ObjectMatchField" (fst (multiBoth SlotRange.EntityReference true)) (snd (multiBoth SlotRange.EntityReference true))
            at 41 "match_string" "MatchString" (fst (multiBoth SlotRange.Text false)) (snd (multiBoth SlotRange.Text false))
            at 42 "subject_preprocessing" "SubjectPreprocessing" (fst (multiBoth SlotRange.EntityReference true)) (snd (multiBoth SlotRange.EntityReference true))
            at 43 "object_preprocessing" "ObjectPreprocessing" (fst (multiBoth SlotRange.EntityReference true)) (snd (multiBoth SlotRange.EntityReference true))
            at 44 "similarity_score" "SimilarityScore" (fst (scalarBoth SlotRange.Number false false)) (snd (scalarBoth SlotRange.Number false false))
            at 45 "similarity_measure" "SimilarityMeasure" (scalar SlotRange.Text false false) (scalar SlotRange.Text false true)
            at 46 "see_also" "SeeAlso" (multi SlotRange.Text false false) (multi SlotRange.NonRelativeUri false false)
            at 47 "issue_tracker_item" "IssueTrackerItem" (fst (scalarBoth SlotRange.EntityReference false false)) (snd (scalarBoth SlotRange.EntityReference false false))
            at 48 "derived_from" "DerivedFrom" None (multi SlotRange.EntityReference false false)
            at 49 "other" "Other" (fst (scalarBoth SlotRange.Text false false)) (snd (scalarBoth SlotRange.Text false false))
            at 50 "comment" "Comment" (fst (scalarBoth SlotRange.Text false false)) (snd (scalarBoth SlotRange.Text false false))
        |]

    /// Gets a fresh array containing every standard mapping descriptor.
    let allDescriptors () = Array.copy all

    /// Tries to find a mapping descriptor by its normative slot name.
    let tryFind name = all |> Array.tryFind (fun descriptor -> descriptor.Name = name)
