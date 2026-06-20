namespace SSSOM

type DecodeSssomDocument() =
    static member DecodeSssomDocument(document: string) =
        SssomDocument(
            DecodeMappingSet.DecodeMappingSet(document),
            DecodeMapping.DecodeMapping(document)
        )