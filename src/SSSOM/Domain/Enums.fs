namespace SSSOM

/// Identifies a supported SSSOM specification version.
[<RequireQualifiedAccess>]
type SssomVersion =
    | V1_0
    | V1_1

/// Functions for converting SSSOM versions to and from their lexical form.
module SssomVersion =

    /// Tries to parse the lexical value used by the sssom_version slot.
    let tryParse (text: string) =
        match text with
        | "1.0" -> Some SssomVersion.V1_0
        | "1.1" -> Some SssomVersion.V1_1
        | _ -> None

    /// Parses the lexical value used by the sssom_version slot.
    let parse (text: string) =
        match tryParse text with
        | Some value -> value
        | None -> invalidArg (nameof text) $"Unsupported SSSOM version '{text}'."

    /// Returns the lexical value used by the sssom_version slot.
    let toLexical value =
        match value with
        | SssomVersion.V1_0 -> "1.0"
        | SssomVersion.V1_1 -> "1.1"

/// Describes the semantic kind of an entity participating in a mapping.
[<RequireQualifiedAccess>]
type EntityType =
    | OwlClass
    | OwlObjectProperty
    | OwlDataProperty
    | OwlAnnotationProperty
    | OwlNamedIndividual
    | SkosConcept
    | RdfsResource
    | RdfsClass
    | RdfsLiteral
    | RdfsDatatype
    | RdfProperty
    | ComposedEntityExpression

/// Functions for converting entity types to and from SSSOM lexical values.
module EntityType =

    /// Tries to parse an SSSOM entity-type value.
    let tryParse (text: string) =
        match text with
        | "owl class" -> Some EntityType.OwlClass
        | "owl object property" -> Some EntityType.OwlObjectProperty
        | "owl data property" -> Some EntityType.OwlDataProperty
        | "owl annotation property" -> Some EntityType.OwlAnnotationProperty
        | "owl named individual" -> Some EntityType.OwlNamedIndividual
        | "skos concept" -> Some EntityType.SkosConcept
        | "rdfs resource" -> Some EntityType.RdfsResource
        | "rdfs class" -> Some EntityType.RdfsClass
        | "rdfs literal" -> Some EntityType.RdfsLiteral
        | "rdfs datatype" -> Some EntityType.RdfsDatatype
        | "rdf property" -> Some EntityType.RdfProperty
        | "composed entity expression" -> Some EntityType.ComposedEntityExpression
        | _ -> None

    /// Parses an SSSOM entity-type value.
    let parse (text: string) =
        match tryParse text with
        | Some value -> value
        | None -> invalidArg (nameof text) $"Unknown SSSOM entity type '{text}'."

    /// Returns the SSSOM lexical value for an entity type.
    let toLexical value =
        match value with
        | EntityType.OwlClass -> "owl class"
        | EntityType.OwlObjectProperty -> "owl object property"
        | EntityType.OwlDataProperty -> "owl data property"
        | EntityType.OwlAnnotationProperty -> "owl annotation property"
        | EntityType.OwlNamedIndividual -> "owl named individual"
        | EntityType.SkosConcept -> "skos concept"
        | EntityType.RdfsResource -> "rdfs resource"
        | EntityType.RdfsClass -> "rdfs class"
        | EntityType.RdfsLiteral -> "rdfs literal"
        | EntityType.RdfsDatatype -> "rdfs datatype"
        | EntityType.RdfProperty -> "rdf property"
        | EntityType.ComposedEntityExpression -> "composed entity expression"

    /// Returns the earliest SSSOM version supporting the value.
    let minimumVersion value =
        match value with
        | EntityType.ComposedEntityExpression -> SssomVersion.V1_1
        | _ -> SssomVersion.V1_0

/// Modifies the interpretation of a mapping predicate.
[<RequireQualifiedAccess>]
type PredicateModifier =
    | Not

/// Functions for converting predicate modifiers to and from SSSOM values.
module PredicateModifier =

    /// Tries to parse an SSSOM predicate modifier.
    let tryParse (text: string) =
        match text with
        | "Not" -> Some PredicateModifier.Not
        | _ -> None

    /// Parses an SSSOM predicate modifier.
    let parse (text: string) =
        match tryParse text with
        | Some value -> value
        | None -> invalidArg (nameof text) $"Unknown predicate modifier '{text}'."

    /// Returns the SSSOM lexical value for a predicate modifier.
    let toLexical value =
        match value with
        | PredicateModifier.Not -> "Not"

/// Describes the cardinality asserted by a mapping record.
[<RequireQualifiedAccess>]
type MappingCardinality =
    | OneToOne
    | OneToMany
    | ManyToOne
    | ManyToMany
    | OneToNone
    | NoneToOne
    | NoneToNone

/// Functions for converting mapping cardinalities to and from SSSOM values.
module MappingCardinality =

    /// Tries to parse an SSSOM mapping-cardinality value.
    let tryParse (text: string) =
        match text with
        | "1:1" -> Some MappingCardinality.OneToOne
        | "1:n" -> Some MappingCardinality.OneToMany
        | "n:1" -> Some MappingCardinality.ManyToOne
        | "n:n" -> Some MappingCardinality.ManyToMany
        | "1:0" -> Some MappingCardinality.OneToNone
        | "0:1" -> Some MappingCardinality.NoneToOne
        | "0:0" -> Some MappingCardinality.NoneToNone
        | _ -> None

    /// Parses an SSSOM mapping-cardinality value.
    let parse (text: string) =
        match tryParse text with
        | Some value -> value
        | None -> invalidArg (nameof text) $"Unknown mapping cardinality '{text}'."

    /// Returns the SSSOM lexical value for a mapping cardinality.
    let toLexical value =
        match value with
        | MappingCardinality.OneToOne -> "1:1"
        | MappingCardinality.OneToMany -> "1:n"
        | MappingCardinality.ManyToOne -> "n:1"
        | MappingCardinality.ManyToMany -> "n:n"
        | MappingCardinality.OneToNone -> "1:0"
        | MappingCardinality.NoneToOne -> "0:1"
        | MappingCardinality.NoneToNone -> "0:0"

    /// Returns the earliest SSSOM version supporting the value.
    let minimumVersion value =
        match value with
        | MappingCardinality.NoneToNone -> SssomVersion.V1_1
        | _ -> SssomVersion.V1_0
