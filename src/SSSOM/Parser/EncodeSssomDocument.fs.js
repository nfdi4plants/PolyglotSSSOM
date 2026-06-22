
import { EncodeMappingSet } from "./EncodeMappingSet.fs.js";
import { EncodeMapping } from "./EncodeMapping.fs.js";
import { class_type } from "../fable_modules/fable-library-js.5.2.0/Reflection.js";

export class EncodeSssomDocument {
    constructor() {
    }
    static EncodeSssomDocument(doc) {
        return (EncodeMappingSet.EncodeMappingSet(doc.Metadata) + "\n") + EncodeMapping.EncodeMapping(doc.Mappings);
    }
}

export function EncodeSssomDocument_$reflection() {
    return class_type("SSSOM.EncodeSssomDocument", undefined, EncodeSssomDocument);
}

export function EncodeSssomDocument_$ctor() {
    return new EncodeSssomDocument();
}

