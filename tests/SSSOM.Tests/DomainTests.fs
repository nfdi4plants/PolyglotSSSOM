module SSSOM.Tests.DomainTests

open SSSOM
open Fable.Pyxpecto

let private entity value = EntityReference.Create value
let private uri value = UriReference.Create value
let private date value = SssomDate.Create value

let private expectEntity actual expected message =
    actual
    |> Option.map (fun (value: EntityReference) -> value.Value)
    |> fun value -> Expect.equal value (Some expected) message

let private expectUri actual expected message =
    actual
    |> Option.map (fun (value: UriReference) -> value.Value)
    |> fun value -> Expect.equal value (Some expected) message

let private expectDate actual expected message =
    actual
    |> Option.map (fun (value: SssomDate) -> value.Value)
    |> fun value -> Expect.equal value (Some expected) message

let private lexicalTests =
    testList "Lexical values and enums" [
        testCase "entity references retain CURIE and absolute URI values" <| fun _ ->
            Expect.equal (entity "CHEBI:15377").Value "CHEBI:15377" "CURIE should be retained"
            Expect.equal (entity "https://example.org/id/1").Value "https://example.org/id/1" "IRI should be retained"
            Expect.isNone (EntityReference.TryCreate "not-an-identifier") "missing colon should be rejected"

        testCase "URI references preserve v1.0 relative forms and identify v1.1 forms" <| fun _ ->
            let relative = uri "mappings/example.tsv"
            let absolute = uri "https://example.org/mappings/example.tsv"
            Expect.isFalse relative.IsNonRelative "relative v1.0 form should remain distinguishable"
            Expect.isTrue absolute.IsNonRelative "absolute v1.1 form should be recognized"

        testCase "dates validate calendar days without runtime-specific parsing" <| fun _ ->
            Expect.equal (date "2024-02-29").Value "2024-02-29" "leap day should be accepted"
            Expect.isNone (SssomDate.TryCreate "2023-02-29") "invalid calendar day should be rejected"
            Expect.isNone (SssomDate.TryCreate "2024-2-9") "noncanonical date should be rejected"

        testCase "enum values preserve version distinctions" <| fun _ ->
            Expect.equal (SssomVersion.toLexical SssomVersion.V1_1) "1.1" "version lexical form"
            Expect.equal (EntityType.minimumVersion EntityType.ComposedEntityExpression) SssomVersion.V1_1 "composed entities are v1.1"
            Expect.equal (MappingCardinality.minimumVersion MappingCardinality.NoneToNone) SssomVersion.V1_1 "0:0 is v1.1"
            Expect.equal (MappingCardinality.toLexical MappingCardinality.NoneToNone) "0:0" "0:0 should round trip"
    ]

let private curieTests =
    testList "CURIE map" [
        testCase "built-in and document prefixes expand lexically" <| fun _ ->
            let entries = [| PrefixEntry("ex", uri "https://example.org/") |]
            Expect.equal (CurieMap.expand entries "skos:exactMatch") "http://www.w3.org/2004/02/skos/core#exactMatch" "built-in prefix"
            Expect.equal (CurieMap.expand entries "ex:item") "https://example.org/item" "document prefix"
            Expect.equal (CurieMap.expand entries "https://example.org/item") "https://example.org/item" "absolute IRI"
            Expect.isNone (CurieMap.tryExpand entries "unknown:item") "unknown lexical prefix"

        testCase "contraction prefers the longest expansion then prefix name" <| fun _ ->
            let entries =
                [|
                    PrefixEntry("base", uri "https://example.org/")
                    PrefixEntry("terms", uri "https://example.org/terms/")
                    PrefixEntry("zterms", uri "https://example.org/terms/")
                |]

            Expect.equal (CurieMap.contract entries "https://example.org/terms/item") "terms:item" "longest expansion and lexical tie-break"

        testCase "built-in entries are returned as defensive copies" <| fun _ ->
            let first = CurieMap.builtInEntries ()
            first.[0].PrefixName <- "changed"
            let second = CurieMap.builtInEntries ()
            Expect.equal second.[0].PrefixName "linkml" "built-in state must not be caller mutable"
    ]

let private descriptorTests =
    testList "Descriptor catalogs" [
        testCase "catalogs contain every standard model property exactly once" <| fun _ ->
            let mappingSet = MappingSetDescriptors.allDescriptors ()
            let mapping = MappingDescriptors.allDescriptors ()
            Expect.equal mappingSet.Length 37 "mapping-set descriptor count"
            Expect.equal mapping.Length 51 "mapping descriptor count"
            Expect.equal (mappingSet |> Array.distinctBy (fun descriptor -> descriptor.Name) |> Array.length) mappingSet.Length "mapping-set names unique"
            Expect.equal (mapping |> Array.distinctBy (fun descriptor -> descriptor.Name) |> Array.length) mapping.Length "mapping names unique"
            Expect.equal (mappingSet |> Array.map (fun descriptor -> descriptor.Order)) [| 0 .. 36 |] "mapping-set order"
            Expect.equal (mapping |> Array.map (fun descriptor -> descriptor.Order)) [| 0 .. 50 |] "mapping order"

        testCase "required and version-specific constraints match the pinned schemas" <| fun _ ->
            let mappingSetId = MappingSetDescriptors.tryFind "mapping_set_id" |> Option.get
            Expect.equal mappingSetId.V1_0.Value.Range SlotRange.Uri "v1.0 permits legacy URI forms"
            Expect.equal mappingSetId.V1_1.Value.Range SlotRange.NonRelativeUri "v1.1 requires non-relative URI"
            Expect.isTrue mappingSetId.V1_0.Value.IsRequired "mapping_set_id required in v1.0"
            Expect.isTrue mappingSetId.V1_1.Value.IsRequired "mapping_set_id required in v1.1"

            let derivedFrom = MappingDescriptors.tryFind "derived_from" |> Option.get
            Expect.isNone derivedFrom.V1_0 "derived_from is absent in v1.0"
            Expect.equal derivedFrom.V1_1.Value.Cardinality SlotCardinality.Multivalued "derived_from is multivalued"

            let recordId = MappingDescriptors.tryFind "record_id" |> Option.get
            Expect.isNone recordId.V1_0 "record_id is native v1.1"
            Expect.isSome recordId.V1_1 "record_id exists in v1.1"

        testCase "propagation metadata preserves version changes" <| fun _ ->
            let curationRule = MappingDescriptors.tryFind "curation_rule" |> Option.get
            Expect.isFalse curationRule.V1_0.Value.IsPropagated "curation_rule was not propagated in v1.0"
            Expect.isTrue curationRule.V1_1.Value.IsPropagated "curation_rule propagates in v1.1"

            let similarityMeasure = MappingSetDescriptors.tryFind "similarity_measure" |> Option.get
            Expect.isNone similarityMeasure.V1_0 "mapping-set similarity_measure is absent in v1.0"
            Expect.isTrue similarityMeasure.V1_1.Value.IsPropagated "mapping-set similarity_measure propagates in v1.1"
    ]

let private mappingPropertyIsolationTest =
    testCase "every mapping setter retains its own value" <| fun _ ->
        let mapping = Mapping(entity "skos:exactMatch", entity "semapv:ManualMappingCuration")
        mapping.RecordId <- Some(entity "mapping:record")
        mapping.SubjectId <- Some(entity "ex:subject")
        mapping.SubjectLabel <- Some "subject label"
        mapping.SubjectCategory <- Some "subject category"
        mapping.PredicateLabel <- Some "predicate label"
        mapping.PredicateModifier <- Some PredicateModifier.Not
        mapping.ObjectId <- Some(entity "ex:object")
        mapping.ObjectLabel <- Some "object label"
        mapping.ObjectCategory <- Some "object category"
        mapping.AuthorId <- [| entity "orcid:author" |]
        mapping.AuthorLabel <- [| "author label" |]
        mapping.ReviewerId <- [| entity "orcid:reviewer" |]
        mapping.ReviewerLabel <- [| "reviewer label" |]
        mapping.CreatorId <- [| entity "orcid:creator" |]
        mapping.CreatorLabel <- [| "creator label" |]
        mapping.License <- Some(uri "https://example.org/license")
        mapping.SubjectType <- Some EntityType.OwlClass
        mapping.SubjectSource <- Some(entity "ex:subject-source")
        mapping.SubjectSourceVersion <- Some "subject source version"
        mapping.ObjectType <- Some EntityType.SkosConcept
        mapping.ObjectSource <- Some(entity "ex:object-source")
        mapping.ObjectSourceVersion <- Some "object source version"
        mapping.PredicateType <- Some EntityType.RdfProperty
        mapping.MappingProvider <- Some(uri "https://example.org/provider")
        mapping.MappingSource <- Some(entity "ex:mapping-source")
        mapping.MappingCardinality <- Some MappingCardinality.OneToMany
        mapping.CardinalityScope <- [| "scope" |]
        mapping.MappingTool <- Some "mapping tool"
        mapping.MappingToolId <- Some(entity "ex:mapping-tool")
        mapping.MappingToolVersion <- Some "mapping tool version"
        mapping.MappingDate <- Some(date "2024-01-01")
        mapping.PublicationDate <- Some(date "2024-01-02")
        mapping.ReviewDate <- Some(date "2024-01-03")
        mapping.Confidence <- Some 0.91
        mapping.ReviewerAgreement <- Some 0.82
        mapping.CurationRule <- [| entity "ex:curation-rule" |]
        mapping.CurationRuleText <- [| "curation text" |]
        mapping.SubjectMatchField <- [| entity "rdfs:label" |]
        mapping.ObjectMatchField <- [| entity "skos:prefLabel" |]
        mapping.MatchString <- [| "match" |]
        mapping.SubjectPreprocessing <- [| entity "semapv:LowerCase" |]
        mapping.ObjectPreprocessing <- [| entity "semapv:Stemming" |]
        mapping.SimilarityScore <- Some 0.73
        mapping.SimilarityMeasure <- Some "Jaccard"
        mapping.SeeAlso <- [| uri "https://example.org/mapping" |]
        mapping.IssueTrackerItem <- Some(entity "issue:42")
        mapping.DerivedFrom <- [| entity "mapping:source-record" |]
        mapping.Other <- Some "other"
        mapping.Comment <- Some "comment"
        mapping.ExtensionValues <- [| ExtensionValue("ext_mapping", "mapping extension") |]

        Expect.equal mapping.PredicateId.Value "skos:exactMatch" "PredicateId"
        Expect.equal mapping.MappingJustification.Value "semapv:ManualMappingCuration" "MappingJustification"
        expectEntity mapping.RecordId "mapping:record" "RecordId"
        expectEntity mapping.SubjectId "ex:subject" "SubjectId"
        Expect.equal mapping.SubjectLabel (Some "subject label") "SubjectLabel"
        Expect.equal mapping.SubjectCategory (Some "subject category") "SubjectCategory"
        Expect.equal mapping.PredicateLabel (Some "predicate label") "PredicateLabel"
        Expect.equal mapping.PredicateModifier (Some PredicateModifier.Not) "PredicateModifier"
        expectEntity mapping.ObjectId "ex:object" "ObjectId"
        Expect.equal mapping.ObjectLabel (Some "object label") "ObjectLabel"
        Expect.equal mapping.ObjectCategory (Some "object category") "ObjectCategory"
        Expect.equal mapping.AuthorId.[0].Value "orcid:author" "AuthorId"
        Expect.equal mapping.AuthorLabel.[0] "author label" "AuthorLabel"
        Expect.equal mapping.ReviewerId.[0].Value "orcid:reviewer" "ReviewerId"
        Expect.equal mapping.ReviewerLabel.[0] "reviewer label" "ReviewerLabel"
        Expect.equal mapping.CreatorId.[0].Value "orcid:creator" "CreatorId"
        Expect.equal mapping.CreatorLabel.[0] "creator label" "CreatorLabel"
        expectUri mapping.License "https://example.org/license" "License"
        Expect.equal mapping.SubjectType (Some EntityType.OwlClass) "SubjectType"
        expectEntity mapping.SubjectSource "ex:subject-source" "SubjectSource"
        Expect.equal mapping.SubjectSourceVersion (Some "subject source version") "SubjectSourceVersion"
        Expect.equal mapping.ObjectType (Some EntityType.SkosConcept) "ObjectType"
        expectEntity mapping.ObjectSource "ex:object-source" "ObjectSource must not alias ObjectType"
        Expect.equal mapping.ObjectSourceVersion (Some "object source version") "ObjectSourceVersion"
        Expect.equal mapping.PredicateType (Some EntityType.RdfProperty) "PredicateType"
        expectUri mapping.MappingProvider "https://example.org/provider" "MappingProvider"
        expectEntity mapping.MappingSource "ex:mapping-source" "MappingSource"
        Expect.equal mapping.MappingCardinality (Some MappingCardinality.OneToMany) "MappingCardinality"
        Expect.equal mapping.CardinalityScope.[0] "scope" "CardinalityScope"
        Expect.equal mapping.MappingTool (Some "mapping tool") "MappingTool"
        expectEntity mapping.MappingToolId "ex:mapping-tool" "MappingToolId"
        Expect.equal mapping.MappingToolVersion (Some "mapping tool version") "MappingToolVersion"
        expectDate mapping.MappingDate "2024-01-01" "MappingDate"
        expectDate mapping.PublicationDate "2024-01-02" "PublicationDate"
        expectDate mapping.ReviewDate "2024-01-03" "ReviewDate"
        Expect.equal mapping.Confidence (Some 0.91) "Confidence"
        Expect.equal mapping.ReviewerAgreement (Some 0.82) "ReviewerAgreement"
        Expect.equal mapping.CurationRule.[0].Value "ex:curation-rule" "CurationRule"
        Expect.equal mapping.CurationRuleText.[0] "curation text" "CurationRuleText"
        Expect.equal mapping.SubjectMatchField.[0].Value "rdfs:label" "SubjectMatchField"
        Expect.equal mapping.ObjectMatchField.[0].Value "skos:prefLabel" "ObjectMatchField"
        Expect.equal mapping.MatchString.[0] "match" "MatchString"
        Expect.equal mapping.SubjectPreprocessing.[0].Value "semapv:LowerCase" "SubjectPreprocessing"
        Expect.equal mapping.ObjectPreprocessing.[0].Value "semapv:Stemming" "ObjectPreprocessing"
        Expect.equal mapping.SimilarityScore (Some 0.73) "SimilarityScore"
        Expect.equal mapping.SimilarityMeasure (Some "Jaccard") "SimilarityMeasure"
        Expect.equal mapping.SeeAlso.[0].Value "https://example.org/mapping" "SeeAlso"
        expectEntity mapping.IssueTrackerItem "issue:42" "IssueTrackerItem"
        Expect.equal mapping.DerivedFrom.[0].Value "mapping:source-record" "DerivedFrom"
        Expect.equal mapping.Other (Some "other") "Other"
        Expect.equal mapping.Comment (Some "comment") "Comment"
        Expect.equal mapping.ExtensionValues.[0].Value "mapping extension" "ExtensionValues"

let private mappingSetPropertyIsolationTest =
    testCase "every mapping-set setter retains its own value" <| fun _ ->
        let metadata = MappingSet(uri "mapping-set.json", uri "license.txt")
        metadata.SssomVersion <- Some SssomVersion.V1_1
        metadata.CurieMap <- [| PrefixEntry("ex", uri "https://example.org/") |]
        metadata.MappingSetVersion <- Some "set version"
        metadata.MappingSetSource <- [| uri "source.tsv" |]
        metadata.MappingSetTitle <- Some "title"
        metadata.MappingSetDescription <- Some "description"
        metadata.MappingSetConfidence <- Some 0.65
        metadata.CreatorId <- [| entity "orcid:metadata-creator" |]
        metadata.CreatorLabel <- [| "metadata creator" |]
        metadata.SubjectType <- Some EntityType.OwlClass
        metadata.SubjectSource <- Some(entity "ex:metadata-subject-source")
        metadata.SubjectSourceVersion <- Some "metadata subject version"
        metadata.ObjectType <- Some EntityType.SkosConcept
        metadata.ObjectSource <- Some(entity "ex:metadata-object-source")
        metadata.ObjectSourceVersion <- Some "metadata object version"
        metadata.PredicateType <- Some EntityType.RdfProperty
        metadata.MappingProvider <- Some(uri "provider.txt")
        metadata.CardinalityScope <- [| "metadata scope" |]
        metadata.MappingTool <- Some "metadata tool"
        metadata.MappingToolId <- Some(entity "ex:metadata-tool")
        metadata.MappingToolVersion <- Some "metadata tool version"
        metadata.MappingDate <- Some(date "2024-02-01")
        metadata.PublicationDate <- Some(date "2024-02-02")
        metadata.SubjectMatchField <- [| entity "rdfs:label" |]
        metadata.ObjectMatchField <- [| entity "skos:prefLabel" |]
        metadata.SubjectPreprocessing <- [| entity "semapv:LowerCase" |]
        metadata.ObjectPreprocessing <- [| entity "semapv:Stemming" |]
        metadata.SimilarityMeasure <- Some "Cosine"
        metadata.CurationRule <- [| entity "ex:metadata-rule" |]
        metadata.CurationRuleText <- [| "metadata rule text" |]
        metadata.SeeAlso <- [| uri "related.txt" |]
        metadata.IssueTracker <- Some(uri "issues")
        metadata.Other <- Some "metadata other"
        metadata.Comment <- Some "metadata comment"
        metadata.ExtensionDefinitions <- [| ExtensionDefinition("ext_metadata") |]
        metadata.ExtensionValues <- [| ExtensionValue("ext_metadata", "metadata extension") |]

        Expect.equal metadata.MappingSetId.Value "mapping-set.json" "MappingSetId"
        Expect.equal metadata.License.Value "license.txt" "License"
        Expect.equal metadata.SssomVersion (Some SssomVersion.V1_1) "SssomVersion"
        Expect.equal metadata.CurieMap.[0].PrefixName "ex" "CurieMap"
        Expect.equal metadata.MappingSetVersion (Some "set version") "MappingSetVersion"
        Expect.equal metadata.MappingSetSource.[0].Value "source.tsv" "MappingSetSource"
        Expect.equal metadata.MappingSetTitle (Some "title") "MappingSetTitle"
        Expect.equal metadata.MappingSetDescription (Some "description") "MappingSetDescription"
        Expect.equal metadata.MappingSetConfidence (Some 0.65) "MappingSetConfidence"
        Expect.equal metadata.CreatorId.[0].Value "orcid:metadata-creator" "CreatorId"
        Expect.equal metadata.CreatorLabel.[0] "metadata creator" "CreatorLabel"
        Expect.equal metadata.SubjectType (Some EntityType.OwlClass) "SubjectType"
        expectEntity metadata.SubjectSource "ex:metadata-subject-source" "SubjectSource"
        Expect.equal metadata.SubjectSourceVersion (Some "metadata subject version") "SubjectSourceVersion"
        Expect.equal metadata.ObjectType (Some EntityType.SkosConcept) "ObjectType"
        expectEntity metadata.ObjectSource "ex:metadata-object-source" "ObjectSource"
        Expect.equal metadata.ObjectSourceVersion (Some "metadata object version") "ObjectSourceVersion"
        Expect.equal metadata.PredicateType (Some EntityType.RdfProperty) "PredicateType"
        expectUri metadata.MappingProvider "provider.txt" "MappingProvider"
        Expect.equal metadata.CardinalityScope.[0] "metadata scope" "CardinalityScope"
        Expect.equal metadata.MappingTool (Some "metadata tool") "MappingTool"
        expectEntity metadata.MappingToolId "ex:metadata-tool" "MappingToolId"
        Expect.equal metadata.MappingToolVersion (Some "metadata tool version") "MappingToolVersion"
        expectDate metadata.MappingDate "2024-02-01" "MappingDate"
        expectDate metadata.PublicationDate "2024-02-02" "PublicationDate"
        Expect.equal metadata.SubjectMatchField.[0].Value "rdfs:label" "SubjectMatchField"
        Expect.equal metadata.ObjectMatchField.[0].Value "skos:prefLabel" "ObjectMatchField"
        Expect.equal metadata.SubjectPreprocessing.[0].Value "semapv:LowerCase" "SubjectPreprocessing"
        Expect.equal metadata.ObjectPreprocessing.[0].Value "semapv:Stemming" "ObjectPreprocessing"
        Expect.equal metadata.SimilarityMeasure (Some "Cosine") "SimilarityMeasure"
        Expect.equal metadata.CurationRule.[0].Value "ex:metadata-rule" "CurationRule"
        Expect.equal metadata.CurationRuleText.[0] "metadata rule text" "CurationRuleText"
        Expect.equal metadata.SeeAlso.[0].Value "related.txt" "SeeAlso"
        expectUri metadata.IssueTracker "issues" "IssueTracker"
        Expect.equal metadata.Other (Some "metadata other") "Other"
        Expect.equal metadata.Comment (Some "metadata comment") "Comment"
        Expect.equal metadata.ExtensionDefinitions.[0].SlotName "ext_metadata" "ExtensionDefinitions"
        Expect.equal metadata.ExtensionValues.[0].Value "metadata extension" "ExtensionValues"

let private absenceAndDocumentTests =
    testList "Absence and document shape" [
        testCase "optional scalars remain absent and multivalues default to empty arrays" <| fun _ ->
            let mapping = Mapping(entity "skos:exactMatch", entity "semapv:ManualMappingCuration")
            Expect.isNone mapping.Confidence "confidence must not acquire a default"
            Expect.isNone mapping.RecordId "record_id should be absent"
            Expect.isEmpty mapping.AuthorId "author_id should be an empty array"
            Expect.isEmpty mapping.DerivedFrom "derived_from should be an empty array"
            Expect.isEmpty mapping.ExtensionValues "extensions should be an empty array"

            let metadata = MappingSet(uri "set.tsv", uri "license")
            Expect.isNone metadata.SssomVersion "versionless metadata remains versionless"
            Expect.isNone metadata.MappingSetConfidence "mapping-set confidence remains absent"
            Expect.isEmpty metadata.CurieMap "CURIE map should be an empty array"
            Expect.isEmpty metadata.ExtensionDefinitions "extension definitions should be an empty array"

        testCase "documents use arrays and retain mapping-set and mapping extensions" <| fun _ ->
            let metadata = MappingSet(uri "set.tsv", uri "license", extensionValues = [| ExtensionValue("ext_set", "set") |])
            let mapping = Mapping(entity "skos:exactMatch", entity "semapv:ManualMappingCuration", extensionValues = [| ExtensionValue("ext_row", "row") |])
            let document = SssomDocument(metadata, [| mapping |])
            Expect.equal document.Mappings.Length 1 "mapping array"
            Expect.equal document.Metadata.ExtensionValues.[0].Value "set" "mapping-set extension"
            Expect.equal document.Mappings.[0].ExtensionValues.[0].Value "row" "mapping extension"
    ]

let tests =
    testList "Portable domain model" [
        lexicalTests
        curieTests
        descriptorTests
        mappingPropertyIsolationTest
        mappingSetPropertyIsolationTest
        absenceAndDocumentTests
    ]
