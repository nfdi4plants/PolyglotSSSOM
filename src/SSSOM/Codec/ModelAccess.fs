namespace SSSOM

open System
open System.Globalization

module internal LexicalCodec =

    let optionValue formatter value =
        match value with
        | Some item -> [| formatter item |]
        | None -> [||]

    let arrayValues formatter (values: 'a array) =
        if isNull values then [||] else values |> Array.map formatter

    let identity (value: string) = value
    let entity (value: EntityReference) = value.Value
    let uri (value: UriReference) = value.Value
    let date (value: SssomDate) = value.Value

    let tryParseDouble (value: string) =
        let mutable parsed = 0.0

#if FABLE_COMPILER
        if Double.TryParse(value, &parsed) && not (Double.IsNaN parsed || Double.IsInfinity parsed) then Some parsed else None
#else
        if Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, &parsed)
           && not (Double.IsNaN parsed || Double.IsInfinity parsed) then
            Some parsed
        else
            None
#endif

    let parseDouble value =
        match tryParseDouble value with
        | Some parsed -> parsed
        | None -> invalidArg (nameof value) $"'{value}' is not a finite number."

    let formatNumber value =
        let scaled =
            if value >= 0.0 then
                int (Math.Floor(value * 1000.0 + 0.5))
            else
                int (Math.Ceiling(value * 1000.0 - 0.5))

        let absolute = Math.Abs scaled
        let whole = absolute / 1000
        let fraction = absolute % 1000
        let sign = if scaled < 0 then "-" else ""

        if fraction = 0 then
            sign + string whole
        else
            let digits =
                if fraction < 10 then $"00{fraction}"
                elif fraction < 100 then $"0{fraction}"
                else string fraction

            sign + string whole + "." + digits.TrimEnd('0')

    let first values = values |> Array.tryHead

module internal MappingAccess =

    open LexicalCodec

    let getValues (mapping: Mapping) slot =
        match slot with
        | "record_id" -> optionValue entity mapping.RecordId
        | "subject_id" -> optionValue entity mapping.SubjectId
        | "subject_label" -> optionValue identity mapping.SubjectLabel
        | "subject_category" -> optionValue identity mapping.SubjectCategory
        | "predicate_id" -> [| entity mapping.PredicateId |]
        | "predicate_label" -> optionValue identity mapping.PredicateLabel
        | "predicate_modifier" -> optionValue PredicateModifier.toLexical mapping.PredicateModifier
        | "object_id" -> optionValue entity mapping.ObjectId
        | "object_label" -> optionValue identity mapping.ObjectLabel
        | "object_category" -> optionValue identity mapping.ObjectCategory
        | "mapping_justification" -> [| entity mapping.MappingJustification |]
        | "author_id" -> arrayValues entity mapping.AuthorId
        | "author_label" -> arrayValues identity mapping.AuthorLabel
        | "reviewer_id" -> arrayValues entity mapping.ReviewerId
        | "reviewer_label" -> arrayValues identity mapping.ReviewerLabel
        | "creator_id" -> arrayValues entity mapping.CreatorId
        | "creator_label" -> arrayValues identity mapping.CreatorLabel
        | "license" -> optionValue uri mapping.License
        | "subject_type" -> optionValue EntityType.toLexical mapping.SubjectType
        | "subject_source" -> optionValue entity mapping.SubjectSource
        | "subject_source_version" -> optionValue identity mapping.SubjectSourceVersion
        | "object_type" -> optionValue EntityType.toLexical mapping.ObjectType
        | "object_source" -> optionValue entity mapping.ObjectSource
        | "object_source_version" -> optionValue identity mapping.ObjectSourceVersion
        | "predicate_type" -> optionValue EntityType.toLexical mapping.PredicateType
        | "mapping_provider" -> optionValue uri mapping.MappingProvider
        | "mapping_source" -> optionValue entity mapping.MappingSource
        | "mapping_cardinality" -> optionValue MappingCardinality.toLexical mapping.MappingCardinality
        | "cardinality_scope" -> arrayValues identity mapping.CardinalityScope
        | "mapping_tool" -> optionValue identity mapping.MappingTool
        | "mapping_tool_id" -> optionValue entity mapping.MappingToolId
        | "mapping_tool_version" -> optionValue identity mapping.MappingToolVersion
        | "mapping_date" -> optionValue date mapping.MappingDate
        | "publication_date" -> optionValue date mapping.PublicationDate
        | "review_date" -> optionValue date mapping.ReviewDate
        | "confidence" -> optionValue formatNumber mapping.Confidence
        | "reviewer_agreement" -> optionValue formatNumber mapping.ReviewerAgreement
        | "curation_rule" -> arrayValues entity mapping.CurationRule
        | "curation_rule_text" -> arrayValues identity mapping.CurationRuleText
        | "subject_match_field" -> arrayValues entity mapping.SubjectMatchField
        | "object_match_field" -> arrayValues entity mapping.ObjectMatchField
        | "match_string" -> arrayValues identity mapping.MatchString
        | "subject_preprocessing" -> arrayValues entity mapping.SubjectPreprocessing
        | "object_preprocessing" -> arrayValues entity mapping.ObjectPreprocessing
        | "similarity_score" -> optionValue formatNumber mapping.SimilarityScore
        | "similarity_measure" -> optionValue identity mapping.SimilarityMeasure
        | "see_also" -> arrayValues uri mapping.SeeAlso
        | "issue_tracker_item" -> optionValue entity mapping.IssueTrackerItem
        | "derived_from" -> arrayValues entity mapping.DerivedFrom
        | "other" -> optionValue identity mapping.Other
        | "comment" -> optionValue identity mapping.Comment
        | _ -> [||]

    let setValues (mapping: Mapping) slot (values: string array) =
        let entityValue = EntityReference.Create
        let uriValue = UriReference.Create
        let dateValue = SssomDate.Create

        match slot with
        | "record_id" -> mapping.RecordId <- first values |> Option.map entityValue
        | "subject_id" -> mapping.SubjectId <- first values |> Option.map entityValue
        | "subject_label" -> mapping.SubjectLabel <- first values
        | "subject_category" -> mapping.SubjectCategory <- first values
        | "predicate_id" -> first values |> Option.iter (fun value -> mapping.PredicateId <- entityValue value)
        | "predicate_label" -> mapping.PredicateLabel <- first values
        | "predicate_modifier" -> mapping.PredicateModifier <- first values |> Option.map PredicateModifier.parse
        | "object_id" -> mapping.ObjectId <- first values |> Option.map entityValue
        | "object_label" -> mapping.ObjectLabel <- first values
        | "object_category" -> mapping.ObjectCategory <- first values
        | "mapping_justification" -> first values |> Option.iter (fun value -> mapping.MappingJustification <- entityValue value)
        | "author_id" -> mapping.AuthorId <- values |> Array.map entityValue
        | "author_label" -> mapping.AuthorLabel <- Array.copy values
        | "reviewer_id" -> mapping.ReviewerId <- values |> Array.map entityValue
        | "reviewer_label" -> mapping.ReviewerLabel <- Array.copy values
        | "creator_id" -> mapping.CreatorId <- values |> Array.map entityValue
        | "creator_label" -> mapping.CreatorLabel <- Array.copy values
        | "license" -> mapping.License <- first values |> Option.map uriValue
        | "subject_type" -> mapping.SubjectType <- first values |> Option.map EntityType.parse
        | "subject_source" -> mapping.SubjectSource <- first values |> Option.map entityValue
        | "subject_source_version" -> mapping.SubjectSourceVersion <- first values
        | "object_type" -> mapping.ObjectType <- first values |> Option.map EntityType.parse
        | "object_source" -> mapping.ObjectSource <- first values |> Option.map entityValue
        | "object_source_version" -> mapping.ObjectSourceVersion <- first values
        | "predicate_type" -> mapping.PredicateType <- first values |> Option.map EntityType.parse
        | "mapping_provider" -> mapping.MappingProvider <- first values |> Option.map uriValue
        | "mapping_source" -> mapping.MappingSource <- first values |> Option.map entityValue
        | "mapping_cardinality" -> mapping.MappingCardinality <- first values |> Option.map MappingCardinality.parse
        | "cardinality_scope" -> mapping.CardinalityScope <- Array.copy values
        | "mapping_tool" -> mapping.MappingTool <- first values
        | "mapping_tool_id" -> mapping.MappingToolId <- first values |> Option.map entityValue
        | "mapping_tool_version" -> mapping.MappingToolVersion <- first values
        | "mapping_date" -> mapping.MappingDate <- first values |> Option.map dateValue
        | "publication_date" -> mapping.PublicationDate <- first values |> Option.map dateValue
        | "review_date" -> mapping.ReviewDate <- first values |> Option.map dateValue
        | "confidence" -> mapping.Confidence <- first values |> Option.map parseDouble
        | "reviewer_agreement" -> mapping.ReviewerAgreement <- first values |> Option.map parseDouble
        | "curation_rule" -> mapping.CurationRule <- values |> Array.map entityValue
        | "curation_rule_text" -> mapping.CurationRuleText <- Array.copy values
        | "subject_match_field" -> mapping.SubjectMatchField <- values |> Array.map entityValue
        | "object_match_field" -> mapping.ObjectMatchField <- values |> Array.map entityValue
        | "match_string" -> mapping.MatchString <- Array.copy values
        | "subject_preprocessing" -> mapping.SubjectPreprocessing <- values |> Array.map entityValue
        | "object_preprocessing" -> mapping.ObjectPreprocessing <- values |> Array.map entityValue
        | "similarity_score" -> mapping.SimilarityScore <- first values |> Option.map parseDouble
        | "similarity_measure" -> mapping.SimilarityMeasure <- first values
        | "see_also" -> mapping.SeeAlso <- values |> Array.map uriValue
        | "issue_tracker_item" -> mapping.IssueTrackerItem <- first values |> Option.map entityValue
        | "derived_from" -> mapping.DerivedFrom <- values |> Array.map entityValue
        | "other" -> mapping.Other <- first values
        | "comment" -> mapping.Comment <- first values
        | _ -> ()

module internal MappingSetAccess =

    open LexicalCodec

    let getValues (metadata: MappingSet) slot =
        match slot with
        | "sssom_version" -> optionValue SssomVersion.toLexical metadata.SssomVersion
        | "mapping_set_id" -> [| uri metadata.MappingSetId |]
        | "mapping_set_version" -> optionValue identity metadata.MappingSetVersion
        | "mapping_set_source" -> arrayValues uri metadata.MappingSetSource
        | "mapping_set_title" -> optionValue identity metadata.MappingSetTitle
        | "mapping_set_description" -> optionValue identity metadata.MappingSetDescription
        | "mapping_set_confidence" -> optionValue formatNumber metadata.MappingSetConfidence
        | "creator_id" -> arrayValues entity metadata.CreatorId
        | "creator_label" -> arrayValues identity metadata.CreatorLabel
        | "license" -> [| uri metadata.License |]
        | "subject_type" -> optionValue EntityType.toLexical metadata.SubjectType
        | "subject_source" -> optionValue entity metadata.SubjectSource
        | "subject_source_version" -> optionValue identity metadata.SubjectSourceVersion
        | "object_type" -> optionValue EntityType.toLexical metadata.ObjectType
        | "object_source" -> optionValue entity metadata.ObjectSource
        | "object_source_version" -> optionValue identity metadata.ObjectSourceVersion
        | "predicate_type" -> optionValue EntityType.toLexical metadata.PredicateType
        | "mapping_provider" -> optionValue uri metadata.MappingProvider
        | "cardinality_scope" -> arrayValues identity metadata.CardinalityScope
        | "mapping_tool" -> optionValue identity metadata.MappingTool
        | "mapping_tool_id" -> optionValue entity metadata.MappingToolId
        | "mapping_tool_version" -> optionValue identity metadata.MappingToolVersion
        | "mapping_date" -> optionValue date metadata.MappingDate
        | "publication_date" -> optionValue date metadata.PublicationDate
        | "subject_match_field" -> arrayValues entity metadata.SubjectMatchField
        | "object_match_field" -> arrayValues entity metadata.ObjectMatchField
        | "subject_preprocessing" -> arrayValues entity metadata.SubjectPreprocessing
        | "object_preprocessing" -> arrayValues entity metadata.ObjectPreprocessing
        | "similarity_measure" -> optionValue identity metadata.SimilarityMeasure
        | "curation_rule" -> arrayValues entity metadata.CurationRule
        | "curation_rule_text" -> arrayValues identity metadata.CurationRuleText
        | "see_also" -> arrayValues uri metadata.SeeAlso
        | "issue_tracker" -> optionValue uri metadata.IssueTracker
        | "other" -> optionValue identity metadata.Other
        | "comment" -> optionValue identity metadata.Comment
        | _ -> [||]

    let setValues (metadata: MappingSet) slot (values: string array) =
        let entityValue = EntityReference.Create
        let uriValue = UriReference.Create
        let dateValue = SssomDate.Create

        match slot with
        | "sssom_version" -> metadata.SssomVersion <- first values |> Option.map SssomVersion.parse
        | "mapping_set_id" -> first values |> Option.iter (fun value -> metadata.MappingSetId <- uriValue value)
        | "mapping_set_version" -> metadata.MappingSetVersion <- first values
        | "mapping_set_source" -> metadata.MappingSetSource <- values |> Array.map uriValue
        | "mapping_set_title" -> metadata.MappingSetTitle <- first values
        | "mapping_set_description" -> metadata.MappingSetDescription <- first values
        | "mapping_set_confidence" -> metadata.MappingSetConfidence <- first values |> Option.map parseDouble
        | "creator_id" -> metadata.CreatorId <- values |> Array.map entityValue
        | "creator_label" -> metadata.CreatorLabel <- Array.copy values
        | "license" -> first values |> Option.iter (fun value -> metadata.License <- uriValue value)
        | "subject_type" -> metadata.SubjectType <- first values |> Option.map EntityType.parse
        | "subject_source" -> metadata.SubjectSource <- first values |> Option.map entityValue
        | "subject_source_version" -> metadata.SubjectSourceVersion <- first values
        | "object_type" -> metadata.ObjectType <- first values |> Option.map EntityType.parse
        | "object_source" -> metadata.ObjectSource <- first values |> Option.map entityValue
        | "object_source_version" -> metadata.ObjectSourceVersion <- first values
        | "predicate_type" -> metadata.PredicateType <- first values |> Option.map EntityType.parse
        | "mapping_provider" -> metadata.MappingProvider <- first values |> Option.map uriValue
        | "cardinality_scope" -> metadata.CardinalityScope <- Array.copy values
        | "mapping_tool" -> metadata.MappingTool <- first values
        | "mapping_tool_id" -> metadata.MappingToolId <- first values |> Option.map entityValue
        | "mapping_tool_version" -> metadata.MappingToolVersion <- first values
        | "mapping_date" -> metadata.MappingDate <- first values |> Option.map dateValue
        | "publication_date" -> metadata.PublicationDate <- first values |> Option.map dateValue
        | "subject_match_field" -> metadata.SubjectMatchField <- values |> Array.map entityValue
        | "object_match_field" -> metadata.ObjectMatchField <- values |> Array.map entityValue
        | "subject_preprocessing" -> metadata.SubjectPreprocessing <- values |> Array.map entityValue
        | "object_preprocessing" -> metadata.ObjectPreprocessing <- values |> Array.map entityValue
        | "similarity_measure" -> metadata.SimilarityMeasure <- first values
        | "curation_rule" -> metadata.CurationRule <- values |> Array.map entityValue
        | "curation_rule_text" -> metadata.CurationRuleText <- Array.copy values
        | "see_also" -> metadata.SeeAlso <- values |> Array.map uriValue
        | "issue_tracker" -> metadata.IssueTracker <- first values |> Option.map uriValue
        | "other" -> metadata.Other <- first values
        | "comment" -> metadata.Comment <- first values
        | _ -> ()

module internal ModelVersion =

    let descriptorFor version (descriptor: SlotDescriptor) =
        match version with
        | SssomVersion.V1_0 -> descriptor.V1_0
        | SssomVersion.V1_1 -> descriptor.V1_1

    let private containsV1_1Multivalue (descriptor: SlotDescriptor) (values: string array) =
        match descriptor.V1_0, descriptor.V1_1 with
        | Some v1_0, Some v1_1
            when v1_0.Cardinality = SlotCardinality.Multivalued
                 && v1_1.Cardinality = SlotCardinality.Multivalued ->
            values |> Array.exists (fun value -> value.Contains("|"))
        | _ -> false

    let minimumVersion (document: SssomDocument) =
        let metadata = document.Metadata
        let mappings = if isNull document.Mappings then [||] else document.Mappings

        let metadataNeedsV1_1 =
            MappingSetDescriptors.allDescriptors ()
            |> Array.exists (fun descriptor ->
                let values = MappingSetAccess.getValues metadata descriptor.Name

                (descriptor.Name <> "sssom_version" && descriptor.V1_0.IsNone && values.Length > 0)
                || containsV1_1Multivalue descriptor values)

        let mappingsNeedV1_1 =
            MappingDescriptors.allDescriptors ()
            |> Array.exists (fun descriptor ->
                mappings
                |> Array.exists (fun mapping ->
                    let values = MappingAccess.getValues mapping descriptor.Name
                    (descriptor.V1_0.IsNone && values.Length > 0)
                    || containsV1_1Multivalue descriptor values))

        let enumNeedsV1_1 =
            metadata.SubjectType = Some EntityType.ComposedEntityExpression
            || metadata.ObjectType = Some EntityType.ComposedEntityExpression
            || metadata.PredicateType = Some EntityType.ComposedEntityExpression
            || (mappings
                |> Array.exists (fun mapping ->
                    mapping.SubjectType = Some EntityType.ComposedEntityExpression
                    || mapping.ObjectType = Some EntityType.ComposedEntityExpression
                    || mapping.PredicateType = Some EntityType.ComposedEntityExpression
                    || mapping.MappingCardinality = Some MappingCardinality.NoneToNone))

        if metadata.SssomVersion = Some SssomVersion.V1_1
           || metadataNeedsV1_1
           || mappingsNeedV1_1
           || enumNeedsV1_1 then
            SssomVersion.V1_1
        else
            SssomVersion.V1_0
