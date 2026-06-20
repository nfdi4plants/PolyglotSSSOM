namespace SSSOM

type EncodeSssomDocument() =
    static member EncodeSssomDocument(doc: SssomDocument) =
        let mappingSet = EncodeMappingSet.EncodeMappingSet(doc.Metadata)
        let mapping = EncodeMapping.EncodeMapping(doc.Mappings)

        mappingSet + "\n" + mapping
