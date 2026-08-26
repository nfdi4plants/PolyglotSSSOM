namespace SSSOM

open Fable.Core

module internal ModelValue =

    let arrayOrEmpty value = defaultArg value [||]

    let nonNullArray (value: 'T array) =
        if isNull value then [||] else value

    let required argumentName value =
        if isNull (box value) then nullArg argumentName
        value

/// Represents one portable SSSOM mapping row.
[<AttachMembers>]
type Mapping(
    predicateId: EntityReference,
    mappingJustification: EntityReference,
    ?recordId: EntityReference,
    ?subjectId: EntityReference,
    ?subjectLabel: string,
    ?subjectCategory: string,
    ?predicateLabel: string,
    ?predicateModifier: PredicateModifier,
    ?objectId: EntityReference,
    ?objectLabel: string,
    ?objectCategory: string,
    ?authorId: EntityReference array,
    ?authorLabel: string array,
    ?reviewerId: EntityReference array,
    ?reviewerLabel: string array,
    ?creatorId: EntityReference array,
    ?creatorLabel: string array,
    ?license: UriReference,
    ?subjectType: EntityType,
    ?subjectSource: EntityReference,
    ?subjectSourceVersion: string,
    ?objectType: EntityType,
    ?objectSource: EntityReference,
    ?objectSourceVersion: string,
    ?predicateType: EntityType,
    ?mappingProvider: UriReference,
    ?mappingSource: EntityReference,
    ?mappingCardinality: MappingCardinality,
    ?cardinalityScope: string array,
    ?mappingTool: string,
    ?mappingToolId: EntityReference,
    ?mappingToolVersion: string,
    ?mappingDate: SssomDate,
    ?publicationDate: SssomDate,
    ?reviewDate: SssomDate,
    ?confidence: double,
    ?reviewerAgreement: double,
    ?curationRule: EntityReference array,
    ?curationRuleText: string array,
    ?subjectMatchField: EntityReference array,
    ?objectMatchField: EntityReference array,
    ?matchString: string array,
    ?subjectPreprocessing: EntityReference array,
    ?objectPreprocessing: EntityReference array,
    ?similarityScore: double,
    ?similarityMeasure: string,
    ?seeAlso: UriReference array,
    ?issueTrackerItem: EntityReference,
    ?derivedFrom: EntityReference array,
    ?other: string,
    ?comment: string,
    ?extensionValues: ExtensionValue array
) =
    let mutable predicateId = ModelValue.required (nameof predicateId) predicateId
    let mutable mappingJustification = ModelValue.required (nameof mappingJustification) mappingJustification
    let mutable recordId = recordId
    let mutable subjectId = subjectId
    let mutable subjectLabel = subjectLabel
    let mutable subjectCategory = subjectCategory
    let mutable predicateLabel = predicateLabel
    let mutable predicateModifier = predicateModifier
    let mutable objectId = objectId
    let mutable objectLabel = objectLabel
    let mutable objectCategory = objectCategory
    let mutable authorId = ModelValue.arrayOrEmpty authorId
    let mutable authorLabel = ModelValue.arrayOrEmpty authorLabel
    let mutable reviewerId = ModelValue.arrayOrEmpty reviewerId
    let mutable reviewerLabel = ModelValue.arrayOrEmpty reviewerLabel
    let mutable creatorId = ModelValue.arrayOrEmpty creatorId
    let mutable creatorLabel = ModelValue.arrayOrEmpty creatorLabel
    let mutable license = license
    let mutable subjectType = subjectType
    let mutable subjectSource = subjectSource
    let mutable subjectSourceVersion = subjectSourceVersion
    let mutable objectType = objectType
    let mutable objectSource = objectSource
    let mutable objectSourceVersion = objectSourceVersion
    let mutable predicateType = predicateType
    let mutable mappingProvider = mappingProvider
    let mutable mappingSource = mappingSource
    let mutable mappingCardinality = mappingCardinality
    let mutable cardinalityScope = ModelValue.arrayOrEmpty cardinalityScope
    let mutable mappingTool = mappingTool
    let mutable mappingToolId = mappingToolId
    let mutable mappingToolVersion = mappingToolVersion
    let mutable mappingDate = mappingDate
    let mutable publicationDate = publicationDate
    let mutable reviewDate = reviewDate
    let mutable confidence = confidence
    let mutable reviewerAgreement = reviewerAgreement
    let mutable curationRule = ModelValue.arrayOrEmpty curationRule
    let mutable curationRuleText = ModelValue.arrayOrEmpty curationRuleText
    let mutable subjectMatchField = ModelValue.arrayOrEmpty subjectMatchField
    let mutable objectMatchField = ModelValue.arrayOrEmpty objectMatchField
    let mutable matchString = ModelValue.arrayOrEmpty matchString
    let mutable subjectPreprocessing = ModelValue.arrayOrEmpty subjectPreprocessing
    let mutable objectPreprocessing = ModelValue.arrayOrEmpty objectPreprocessing
    let mutable similarityScore = similarityScore
    let mutable similarityMeasure = similarityMeasure
    let mutable seeAlso = ModelValue.arrayOrEmpty seeAlso
    let mutable issueTrackerItem = issueTrackerItem
    let mutable derivedFrom = ModelValue.arrayOrEmpty derivedFrom
    let mutable other = other
    let mutable comment = comment
    let mutable extensionValues = ModelValue.arrayOrEmpty extensionValues

    let multivalueNeedsV1_1 (values: string array) =
        values |> Array.exists (fun value -> value.Contains("|"))

    let entityValuesNeedV1_1 (values: EntityReference array) =
        values |> Array.map (fun value -> value.Value) |> multivalueNeedsV1_1

    let uriValuesNeedV1_1 (values: UriReference array) =
        values |> Array.map (fun value -> value.Value) |> multivalueNeedsV1_1

    static member private EntityReference(value: string) = EntityReference.Create value

    /// Creates an entity-to-entity mapping from lexical URI or CURIE values.
    static member CreateEntityMapping(subjectId: string, predicateId: string, objectId: string, mappingJustification: string) =
        Mapping(
            Mapping.EntityReference predicateId,
            Mapping.EntityReference mappingJustification,
            subjectId = Mapping.EntityReference subjectId,
            objectId = Mapping.EntityReference objectId
        )

    /// Creates a mapping asserting that no target term was found for the subject.
    static member CreateNoTermFoundMapping(subjectId: string, predicateId: string, mappingJustification: string) =
        Mapping(
            Mapping.EntityReference predicateId,
            Mapping.EntityReference mappingJustification,
            subjectId = Mapping.EntityReference subjectId,
            objectId = Mapping.EntityReference "sssom:NoTermFound",
            mappingCardinality = MappingCardinality.OneToNone
        )

    /// Gets or sets the required mapping predicate.
    member _.PredicateId
        with get () = predicateId
        and set value = predicateId <- ModelValue.required (nameof value) value

    /// Gets or sets the required mapping justification.
    member _.MappingJustification
        with get () = mappingJustification
        and set value = mappingJustification <- ModelValue.required (nameof value) value

    /// Gets or sets the optional v1.1 record identifier.
    member _.RecordId with get () = recordId and set value = recordId <- value
    /// Gets or sets the subject identifier.
    member _.SubjectId with get () = subjectId and set value = subjectId <- value
    /// Gets or sets the subject label.
    member _.SubjectLabel with get () = subjectLabel and set value = subjectLabel <- value
    /// Gets or sets the subject category.
    member _.SubjectCategory with get () = subjectCategory and set value = subjectCategory <- value
    /// Gets or sets the predicate label.
    member _.PredicateLabel with get () = predicateLabel and set value = predicateLabel <- value
    /// Gets or sets the predicate modifier.
    member _.PredicateModifier with get () = predicateModifier and set value = predicateModifier <- value
    /// Gets or sets the object identifier.
    member _.ObjectId with get () = objectId and set value = objectId <- value
    /// Gets or sets the object label.
    member _.ObjectLabel with get () = objectLabel and set value = objectLabel <- value
    /// Gets or sets the object category.
    member _.ObjectCategory with get () = objectCategory and set value = objectCategory <- value

    /// Gets or sets author identifiers.
    member _.AuthorId
        with get () = authorId
        and set value = authorId <- ModelValue.nonNullArray value

    /// Gets or sets author labels.
    member _.AuthorLabel
        with get () = authorLabel
        and set value = authorLabel <- ModelValue.nonNullArray value

    /// Gets or sets reviewer identifiers.
    member _.ReviewerId
        with get () = reviewerId
        and set value = reviewerId <- ModelValue.nonNullArray value

    /// Gets or sets reviewer labels.
    member _.ReviewerLabel
        with get () = reviewerLabel
        and set value = reviewerLabel <- ModelValue.nonNullArray value

    /// Gets or sets creator identifiers.
    member _.CreatorId
        with get () = creatorId
        and set value = creatorId <- ModelValue.nonNullArray value

    /// Gets or sets creator labels.
    member _.CreatorLabel
        with get () = creatorLabel
        and set value = creatorLabel <- ModelValue.nonNullArray value

    /// Gets or sets the mapping license.
    member _.License with get () = license and set value = license <- value
    /// Gets or sets the subject entity type.
    member _.SubjectType with get () = subjectType and set value = subjectType <- value
    /// Gets or sets the subject source.
    member _.SubjectSource with get () = subjectSource and set value = subjectSource <- value
    /// Gets or sets the subject source version.
    member _.SubjectSourceVersion with get () = subjectSourceVersion and set value = subjectSourceVersion <- value
    /// Gets or sets the object entity type.
    member _.ObjectType with get () = objectType and set value = objectType <- value
    /// Gets or sets the object source.
    member _.ObjectSource with get () = objectSource and set value = objectSource <- value
    /// Gets or sets the object source version.
    member _.ObjectSourceVersion with get () = objectSourceVersion and set value = objectSourceVersion <- value
    /// Gets or sets the predicate entity type.
    member _.PredicateType with get () = predicateType and set value = predicateType <- value
    /// Gets or sets the mapping provider.
    member _.MappingProvider with get () = mappingProvider and set value = mappingProvider <- value
    /// Gets or sets the mapping source.
    member _.MappingSource with get () = mappingSource and set value = mappingSource <- value
    /// Gets or sets the asserted mapping cardinality.
    member _.MappingCardinality with get () = mappingCardinality and set value = mappingCardinality <- value

    /// Gets or sets cardinality-scope values.
    member _.CardinalityScope
        with get () = cardinalityScope
        and set value = cardinalityScope <- ModelValue.nonNullArray value

    /// Gets or sets the mapping tool name.
    member _.MappingTool with get () = mappingTool and set value = mappingTool <- value
    /// Gets or sets the mapping tool identifier.
    member _.MappingToolId with get () = mappingToolId and set value = mappingToolId <- value
    /// Gets or sets the mapping tool version.
    member _.MappingToolVersion with get () = mappingToolVersion and set value = mappingToolVersion <- value
    /// Gets or sets the mapping date.
    member _.MappingDate with get () = mappingDate and set value = mappingDate <- value
    /// Gets or sets the publication date.
    member _.PublicationDate with get () = publicationDate and set value = publicationDate <- value
    /// Gets or sets the review date.
    member _.ReviewDate with get () = reviewDate and set value = reviewDate <- value
    /// Gets or sets confidence without materializing a default.
    member _.Confidence with get () = confidence and set value = confidence <- value
    /// Gets or sets reviewer agreement without materializing a default.
    member _.ReviewerAgreement with get () = reviewerAgreement and set value = reviewerAgreement <- value

    /// Gets or sets curation-rule identifiers.
    member _.CurationRule
        with get () = curationRule
        and set value = curationRule <- ModelValue.nonNullArray value

    /// Gets or sets curation-rule text values.
    member _.CurationRuleText
        with get () = curationRuleText
        and set value = curationRuleText <- ModelValue.nonNullArray value

    /// Gets or sets subject match fields.
    member _.SubjectMatchField
        with get () = subjectMatchField
        and set value = subjectMatchField <- ModelValue.nonNullArray value

    /// Gets or sets object match fields.
    member _.ObjectMatchField
        with get () = objectMatchField
        and set value = objectMatchField <- ModelValue.nonNullArray value

    /// Gets or sets match strings.
    member _.MatchString
        with get () = matchString
        and set value = matchString <- ModelValue.nonNullArray value

    /// Gets or sets subject preprocessing operations.
    member _.SubjectPreprocessing
        with get () = subjectPreprocessing
        and set value = subjectPreprocessing <- ModelValue.nonNullArray value

    /// Gets or sets object preprocessing operations.
    member _.ObjectPreprocessing
        with get () = objectPreprocessing
        and set value = objectPreprocessing <- ModelValue.nonNullArray value

    /// Gets or sets the similarity score.
    member _.SimilarityScore with get () = similarityScore and set value = similarityScore <- value
    /// Gets or sets the similarity measure.
    member _.SimilarityMeasure with get () = similarityMeasure and set value = similarityMeasure <- value

    /// Gets or sets related URI values.
    member _.SeeAlso
        with get () = seeAlso
        and set value = seeAlso <- ModelValue.nonNullArray value

    /// Gets or sets the issue tracker item.
    member _.IssueTrackerItem with get () = issueTrackerItem and set value = issueTrackerItem <- value

    /// Gets or sets identifiers of mappings from which this mapping was derived.
    member _.DerivedFrom
        with get () = derivedFrom
        and set value = derivedFrom <- ModelValue.nonNullArray value

    /// Gets or sets the free-form other value.
    member _.Other with get () = other and set value = other <- value
    /// Gets or sets the comment.
    member _.Comment with get () = comment and set value = comment <- value

    /// Gets or sets declared extension values retained on this mapping.
    member _.ExtensionValues
        with get () = extensionValues
        and set value = extensionValues <- ModelValue.nonNullArray value

    /// Creates an independent copy of this mapping and all of its mutable collections and extension values.
    member _.Clone() =
        Mapping(
            predicateId,
            mappingJustification,
            ?recordId = recordId,
            ?subjectId = subjectId,
            ?subjectLabel = subjectLabel,
            ?subjectCategory = subjectCategory,
            ?predicateLabel = predicateLabel,
            ?predicateModifier = predicateModifier,
            ?objectId = objectId,
            ?objectLabel = objectLabel,
            ?objectCategory = objectCategory,
            authorId = Array.copy authorId,
            authorLabel = Array.copy authorLabel,
            reviewerId = Array.copy reviewerId,
            reviewerLabel = Array.copy reviewerLabel,
            creatorId = Array.copy creatorId,
            creatorLabel = Array.copy creatorLabel,
            ?license = license,
            ?subjectType = subjectType,
            ?subjectSource = subjectSource,
            ?subjectSourceVersion = subjectSourceVersion,
            ?objectType = objectType,
            ?objectSource = objectSource,
            ?objectSourceVersion = objectSourceVersion,
            ?predicateType = predicateType,
            ?mappingProvider = mappingProvider,
            ?mappingSource = mappingSource,
            ?mappingCardinality = mappingCardinality,
            cardinalityScope = Array.copy cardinalityScope,
            ?mappingTool = mappingTool,
            ?mappingToolId = mappingToolId,
            ?mappingToolVersion = mappingToolVersion,
            ?mappingDate = mappingDate,
            ?publicationDate = publicationDate,
            ?reviewDate = reviewDate,
            ?confidence = confidence,
            ?reviewerAgreement = reviewerAgreement,
            curationRule = Array.copy curationRule,
            curationRuleText = Array.copy curationRuleText,
            subjectMatchField = Array.copy subjectMatchField,
            objectMatchField = Array.copy objectMatchField,
            matchString = Array.copy matchString,
            subjectPreprocessing = Array.copy subjectPreprocessing,
            objectPreprocessing = Array.copy objectPreprocessing,
            ?similarityScore = similarityScore,
            ?similarityMeasure = similarityMeasure,
            seeAlso = Array.copy seeAlso,
            ?issueTrackerItem = issueTrackerItem,
            derivedFrom = Array.copy derivedFrom,
            ?other = other,
            ?comment = comment,
            extensionValues = (extensionValues |> Array.map (fun value -> ExtensionValue(value.SlotName, value.Value)))
        )

    member internal _.RequiresV1_1 =
        recordId.IsSome
        || predicateType.IsSome
        || cardinalityScope.Length > 0
        || mappingToolId.IsSome
        || reviewDate.IsSome
        || reviewerAgreement.IsSome
        || derivedFrom.Length > 0
        || subjectType = Some EntityType.ComposedEntityExpression
        || objectType = Some EntityType.ComposedEntityExpression
        || mappingCardinality = Some MappingCardinality.NoneToNone
        || entityValuesNeedV1_1 authorId
        || multivalueNeedsV1_1 authorLabel
        || entityValuesNeedV1_1 reviewerId
        || multivalueNeedsV1_1 reviewerLabel
        || entityValuesNeedV1_1 creatorId
        || multivalueNeedsV1_1 creatorLabel
        || entityValuesNeedV1_1 curationRule
        || multivalueNeedsV1_1 curationRuleText
        || entityValuesNeedV1_1 subjectMatchField
        || entityValuesNeedV1_1 objectMatchField
        || multivalueNeedsV1_1 matchString
        || entityValuesNeedV1_1 subjectPreprocessing
        || entityValuesNeedV1_1 objectPreprocessing
        || uriValuesNeedV1_1 seeAlso
