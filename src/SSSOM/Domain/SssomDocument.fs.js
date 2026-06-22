
import { class_type } from "../fable_modules/fable-library-js.5.2.0/Reflection.js";

export class SssomDocument {
    constructor(metadata, mappings) {
        this._metadata = metadata;
        this._mappings = mappings;
    }
    get Metadata() {
        const this$ = this;
        return this$._metadata;
    }
    set Metadata(value) {
        const this$ = this;
        this$._metadata = value;
    }
    get Mappings() {
        const this$ = this;
        return this$._mappings;
    }
    set Mappings(value) {
        const this$ = this;
        this$._mappings = value;
    }
}

export function SssomDocument_$reflection() {
    return class_type("SSSOM.SssomDocument", undefined, SssomDocument);
}

export function SssomDocument_$ctor_Z54264FA0(metadata, mappings) {
    return new SssomDocument(metadata, mappings);
}

