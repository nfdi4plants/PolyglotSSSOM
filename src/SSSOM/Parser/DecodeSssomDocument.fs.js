
import { SssomDocument } from "../Domain/SssomDocument.fs.js";
import { DecodeMappingSet } from "./DecodeMappingSet.fs.js";
import { DecodeMapping } from "./DecodeMapping.fs.js";
import { class_type } from "../fable_modules/fable-library-js.5.2.0/Reflection.js";

export class DecodeSssomDocument {
    constructor() {
    }
    static DecodeSssomDocument(document$) {
        return new SssomDocument(DecodeMappingSet.DecodeMappingSet(document$), DecodeMapping.DecodeMapping(document$));
    }
}

export function DecodeSssomDocument_$reflection() {
    return class_type("SSSOM.DecodeSssomDocument", undefined, DecodeSssomDocument);
}

export function DecodeSssomDocument_$ctor() {
    return new DecodeSssomDocument();
}

