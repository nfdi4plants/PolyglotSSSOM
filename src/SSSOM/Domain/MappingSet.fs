namespace SSSOM

open Fable.Core

/// Represents portable SSSOM mapping-set metadata.
[<AttachMembers>]
type MappingSet(
    mappingSetId: UriReference,
    license: UriReference,
    ?sssomVersion: SssomVersion,
    ?curieMap: PrefixEntry array,
    ?mappingSetVersion: string,
    ?mappingSetSource: UriReference array,
    ?mappingSetTitle: string,
    ?mappingSetDescription: string,
    ?mappingSetConfidence: double,
    ?creatorId: EntityReference array,
    ?creatorLabel: string array,
    ?subjectType: EntityType,
    ?subjectSource: EntityReference,
    ?subjectSourceVersion: string,
    ?objectType: EntityType,
    ?objectSource: EntityReference,
    ?objectSourceVersion: string,
    ?predicateType: EntityType,
    ?mappingProvider: UriReference,
    ?cardinalityScope: string array,
    ?mappingTool: string,
    ?mappingToolId: EntityReference,
    ?mappingToolVersion: string,
    ?mappingDate: SssomDate,
    ?publicationDate: SssomDate,
    ?subjectMatchField: EntityReference array,
    ?objectMatchField: EntityReference array,
    ?subjectPreprocessing: EntityReference array,
    ?objectPreprocessing: EntityReference array,
    ?similarityMeasure: string,
    ?curationRule: EntityReference array,
    ?curationRuleText: string array,
    ?seeAlso: UriReference array,
    ?issueTracker: UriReference,
    ?other: string,
    ?comment: string,
    ?extensionDefinitions: ExtensionDefinition array,
    ?extensionValues: ExtensionValue array
) =
    let mutable mappingSetId = ModelValue.required (nameof mappingSetId) mappingSetId
    let mutable license = ModelValue.required (nameof license) license
    let mutable sssomVersion = sssomVersion
    let mutable curieMap = ModelValue.arrayOrEmpty curieMap
    let mutable mappingSetVersion = mappingSetVersion
    let mutable mappingSetSource = ModelValue.arrayOrEmpty mappingSetSource
    let mutable mappingSetTitle = mappingSetTitle
    let mutable mappingSetDescription = mappingSetDescription
    let mutable mappingSetConfidence = mappingSetConfidence
    let mutable creatorId = ModelValue.arrayOrEmpty creatorId
    let mutable creatorLabel = ModelValue.arrayOrEmpty creatorLabel
    let mutable subjectType = subjectType
    let mutable subjectSource = subjectSource
    let mutable subjectSourceVersion = subjectSourceVersion
    let mutable objectType = objectType
    let mutable objectSource = objectSource
    let mutable objectSourceVersion = objectSourceVersion
    let mutable predicateType = predicateType
    let mutable mappingProvider = mappingProvider
    let mutable cardinalityScope = ModelValue.arrayOrEmpty cardinalityScope
    let mutable mappingTool = mappingTool
    let mutable mappingToolId = mappingToolId
    let mutable mappingToolVersion = mappingToolVersion
    let mutable mappingDate = mappingDate
    let mutable publicationDate = publicationDate
    let mutable subjectMatchField = ModelValue.arrayOrEmpty subjectMatchField
    let mutable objectMatchField = ModelValue.arrayOrEmpty objectMatchField
    let mutable subjectPreprocessing = ModelValue.arrayOrEmpty subjectPreprocessing
    let mutable objectPreprocessing = ModelValue.arrayOrEmpty objectPreprocessing
    let mutable similarityMeasure = similarityMeasure
    let mutable curationRule = ModelValue.arrayOrEmpty curationRule
    let mutable curationRuleText = ModelValue.arrayOrEmpty curationRuleText
    let mutable seeAlso = ModelValue.arrayOrEmpty seeAlso
    let mutable issueTracker = issueTracker
    let mutable other = other
    let mutable comment = comment
    let mutable extensionDefinitions = ModelValue.arrayOrEmpty extensionDefinitions
    let mutable extensionValues = ModelValue.arrayOrEmpty extensionValues

    /// Creates minimal mapping-set metadata from lexical URI values.
    static member Create(mappingSetId: string, license: string) =
        MappingSet(UriReference.Create mappingSetId, UriReference.Create license)

    /// Gets or sets the required mapping-set identifier.
    member _.MappingSetId
        with get () = mappingSetId
        and set value = mappingSetId <- ModelValue.required (nameof value) value

    /// Gets or sets the required mapping-set license.
    member _.License
        with get () = license
        and set value = license <- ModelValue.required (nameof value) value

    /// Gets or sets the explicitly declared SSSOM version.
    member _.SssomVersion with get () = sssomVersion and set value = sssomVersion <- value

    /// Gets or sets document CURIE prefix entries.
    member _.CurieMap
        with get () = curieMap
        and set value = curieMap <- ModelValue.nonNullArray value

    /// Gets or sets the mapping-set version.
    member _.MappingSetVersion with get () = mappingSetVersion and set value = mappingSetVersion <- value

    /// Gets or sets mapping sets from which this set was derived.
    member _.MappingSetSource
        with get () = mappingSetSource
        and set value = mappingSetSource <- ModelValue.nonNullArray value

    /// Gets or sets the mapping-set title.
    member _.MappingSetTitle with get () = mappingSetTitle and set value = mappingSetTitle <- value
    /// Gets or sets the mapping-set description.
    member _.MappingSetDescription with get () = mappingSetDescription and set value = mappingSetDescription <- value
    /// Gets or sets mapping-set confidence without materializing a default.
    member _.MappingSetConfidence with get () = mappingSetConfidence and set value = mappingSetConfidence <- value

    /// Gets or sets creator identifiers.
    member _.CreatorId
        with get () = creatorId
        and set value = creatorId <- ModelValue.nonNullArray value

    /// Gets or sets creator labels.
    member _.CreatorLabel
        with get () = creatorLabel
        and set value = creatorLabel <- ModelValue.nonNullArray value

    /// Gets or sets the propagated subject type.
    member _.SubjectType with get () = subjectType and set value = subjectType <- value
    /// Gets or sets the propagated subject source.
    member _.SubjectSource with get () = subjectSource and set value = subjectSource <- value
    /// Gets or sets the propagated subject source version.
    member _.SubjectSourceVersion with get () = subjectSourceVersion and set value = subjectSourceVersion <- value
    /// Gets or sets the propagated object type.
    member _.ObjectType with get () = objectType and set value = objectType <- value
    /// Gets or sets the propagated object source.
    member _.ObjectSource with get () = objectSource and set value = objectSource <- value
    /// Gets or sets the propagated object source version.
    member _.ObjectSourceVersion with get () = objectSourceVersion and set value = objectSourceVersion <- value
    /// Gets or sets the v1.1 propagated predicate type.
    member _.PredicateType with get () = predicateType and set value = predicateType <- value
    /// Gets or sets the mapping provider.
    member _.MappingProvider with get () = mappingProvider and set value = mappingProvider <- value

    /// Gets or sets v1.1 cardinality-scope values.
    member _.CardinalityScope
        with get () = cardinalityScope
        and set value = cardinalityScope <- ModelValue.nonNullArray value

    /// Gets or sets the mapping tool name.
    member _.MappingTool with get () = mappingTool and set value = mappingTool <- value
    /// Gets or sets the v1.1 mapping tool identifier.
    member _.MappingToolId with get () = mappingToolId and set value = mappingToolId <- value
    /// Gets or sets the mapping tool version.
    member _.MappingToolVersion with get () = mappingToolVersion and set value = mappingToolVersion <- value
    /// Gets or sets the propagated mapping date.
    member _.MappingDate with get () = mappingDate and set value = mappingDate <- value
    /// Gets or sets the publication date.
    member _.PublicationDate with get () = publicationDate and set value = publicationDate <- value

    /// Gets or sets propagated subject match fields.
    member _.SubjectMatchField
        with get () = subjectMatchField
        and set value = subjectMatchField <- ModelValue.nonNullArray value

    /// Gets or sets propagated object match fields.
    member _.ObjectMatchField
        with get () = objectMatchField
        and set value = objectMatchField <- ModelValue.nonNullArray value

    /// Gets or sets propagated subject preprocessing operations.
    member _.SubjectPreprocessing
        with get () = subjectPreprocessing
        and set value = subjectPreprocessing <- ModelValue.nonNullArray value

    /// Gets or sets propagated object preprocessing operations.
    member _.ObjectPreprocessing
        with get () = objectPreprocessing
        and set value = objectPreprocessing <- ModelValue.nonNullArray value

    /// Gets or sets the v1.1 mapping-set similarity measure.
    member _.SimilarityMeasure with get () = similarityMeasure and set value = similarityMeasure <- value

    /// Gets or sets v1.1 mapping-set curation rules.
    member _.CurationRule
        with get () = curationRule
        and set value = curationRule <- ModelValue.nonNullArray value

    /// Gets or sets v1.1 mapping-set curation-rule text values.
    member _.CurationRuleText
        with get () = curationRuleText
        and set value = curationRuleText <- ModelValue.nonNullArray value

    /// Gets or sets related URI values.
    member _.SeeAlso
        with get () = seeAlso
        and set value = seeAlso <- ModelValue.nonNullArray value

    /// Gets or sets the issue tracker URI.
    member _.IssueTracker with get () = issueTracker and set value = issueTracker <- value
    /// Gets or sets the free-form other value.
    member _.Other with get () = other and set value = other <- value
    /// Gets or sets the mapping-set comment.
    member _.Comment with get () = comment and set value = comment <- value

    /// Gets or sets declared extension definitions.
    member _.ExtensionDefinitions
        with get () = extensionDefinitions
        and set value = extensionDefinitions <- ModelValue.nonNullArray value

    /// Gets or sets declared extension values retained in metadata.
    member _.ExtensionValues
        with get () = extensionValues
        and set value = extensionValues <- ModelValue.nonNullArray value

    /// Ensures that a prefix has the requested non-relative expansion.
    member _.EnsurePrefix(prefixName: string, prefixUrl: string) =
        let candidate = PrefixEntry(prefixName, UriReference.Create prefixUrl)

        if not candidate.PrefixUrl.IsNonRelative then
            invalidArg (nameof prefixUrl) $"Prefix expansion '{prefixUrl}' must be a non-relative URI."

        match
            curieMap
            |> Array.filter (fun entry -> not (isNull (box entry)))
            |> Array.tryFind (fun entry -> entry.PrefixName = candidate.PrefixName)
        with
        | Some existing when existing.PrefixUrl.Value = candidate.PrefixUrl.Value -> ()
        | Some existing ->
            invalidArg
                (nameof prefixName)
                $"Prefix '{prefixName}' already expands to '{existing.PrefixUrl.Value}'."
        | None when CurieMap.isBuiltIn candidate.PrefixName ->
            let builtIn =
                CurieMap.builtInEntries ()
                |> Array.find (fun entry -> entry.PrefixName = candidate.PrefixName)

            if builtIn.PrefixUrl.Value <> candidate.PrefixUrl.Value then
                invalidArg
                    (nameof prefixName)
                    $"Built-in prefix '{prefixName}' must expand to '{builtIn.PrefixUrl.Value}'."
        | None -> curieMap <- Array.append curieMap [| candidate |]

    /// Creates an independent copy of this mapping set and all mutable collections, prefixes, and extensions.
    member _.Clone() =
        MappingSet(
            mappingSetId,
            license,
            ?sssomVersion = sssomVersion,
            curieMap = (curieMap |> Array.map (fun entry -> PrefixEntry(entry.PrefixName, entry.PrefixUrl))),
            ?mappingSetVersion = mappingSetVersion,
            mappingSetSource = Array.copy mappingSetSource,
            ?mappingSetTitle = mappingSetTitle,
            ?mappingSetDescription = mappingSetDescription,
            ?mappingSetConfidence = mappingSetConfidence,
            creatorId = Array.copy creatorId,
            creatorLabel = Array.copy creatorLabel,
            ?subjectType = subjectType,
            ?subjectSource = subjectSource,
            ?subjectSourceVersion = subjectSourceVersion,
            ?objectType = objectType,
            ?objectSource = objectSource,
            ?objectSourceVersion = objectSourceVersion,
            ?predicateType = predicateType,
            ?mappingProvider = mappingProvider,
            cardinalityScope = Array.copy cardinalityScope,
            ?mappingTool = mappingTool,
            ?mappingToolId = mappingToolId,
            ?mappingToolVersion = mappingToolVersion,
            ?mappingDate = mappingDate,
            ?publicationDate = publicationDate,
            subjectMatchField = Array.copy subjectMatchField,
            objectMatchField = Array.copy objectMatchField,
            subjectPreprocessing = Array.copy subjectPreprocessing,
            objectPreprocessing = Array.copy objectPreprocessing,
            ?similarityMeasure = similarityMeasure,
            curationRule = Array.copy curationRule,
            curationRuleText = Array.copy curationRuleText,
            seeAlso = Array.copy seeAlso,
            ?issueTracker = issueTracker,
            ?other = other,
            ?comment = comment,
            extensionDefinitions =
                (extensionDefinitions
                 |> Array.map (fun definition ->
                     ExtensionDefinition(
                         definition.SlotName,
                         ?property = definition.Property,
                         ?typeHint = definition.TypeHint
                     ))),
            extensionValues = (extensionValues |> Array.map (fun value -> ExtensionValue(value.SlotName, value.Value)))
        )
