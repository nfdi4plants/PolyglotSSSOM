namespace SSSOM

type EntityTypeEnum =
    | OwlClass
    | OwlObjectProperty
    | OwlDataProperty
    | OwlAnnotationProperty
    | OwlNamedIndividual
    | SkosConcept
    | RedfsResource
    | RdfsClass
    | RdfsLiteral
    | RdfsDatatype
    | RdfProperty
    | ComposedEntityExpression

module EntityTypeEnum =
    
    let create (text: string) =
        match text with
        | "owl class" -> OwlClass
        | "owl object property" -> OwlObjectProperty
        | "owl data property" -> OwlDataProperty
        | "owl annotation property" -> OwlAnnotationProperty
        | "owl named individual" -> OwlNamedIndividual
        | "skos concept" -> SkosConcept
        | "rdfs resource" -> RedfsResource
        | "rdfs class" -> RdfsClass
        | "rdfs literal" -> RdfsLiteral
        | "rdfs datatype" -> RdfsDatatype
        | "rdf property" -> RdfProperty
        | "composed entity expression" -> ComposedEntityExpression
        | unknown -> failwith $"Can't parse EntityTypeEnum. Unknown value: '{unknown}'"

    let toString (enumValue: EntityTypeEnum) =
        match enumValue with
        | OwlClass -> "owl class"
        | OwlObjectProperty -> "owl object property"
        | OwlDataProperty -> "owl data property"
        | OwlAnnotationProperty -> "owl annotation property"
        | OwlNamedIndividual -> "owl named individual"
        | SkosConcept -> "skos concept"
        | RedfsResource -> "rdfs resource"
        | RdfsClass -> "rdfs class"
        | RdfsLiteral -> "rdfs literal"
        | RdfsDatatype -> "rdfs datatype"
        | RdfProperty -> "rdf property"
        | ComposedEntityExpression -> "composed entity expression"
