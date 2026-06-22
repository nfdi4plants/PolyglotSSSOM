
import { class_type } from "../fable_modules/fable-library-js.5.2.0/Reflection.js";

export class EntityReference {
    constructor(TypeOf, Base, TypeURI, Representation) {
        this._typeOf = TypeOf;
        this._base = Base;
        this._typeURI = TypeURI;
        this._representation = Representation;
    }
    get TypeOf() {
        const this$ = this;
        return this$._typeOf;
    }
    set TypeOf(value) {
        const this$ = this;
        this$._typeOf = value;
    }
    get Base() {
        const this$ = this;
        return this$._base;
    }
    set Base(value) {
        const this$ = this;
        this$._base = value;
    }
    get TypeURI() {
        const this$ = this;
        return this$._typeURI;
    }
    set TypeURI(value) {
        const this$ = this;
        this$._typeURI = value;
    }
    get Representation() {
        const this$ = this;
        return this$._representation;
    }
    set Representation(value) {
        const this$ = this;
        this$._representation = value;
    }
}

export function EntityReference_$reflection() {
    return class_type("SSSOM.EntityReference", undefined, EntityReference);
}

export function EntityReference_$ctor_Z46998140(TypeOf, Base, TypeURI, Representation) {
    return new EntityReference(TypeOf, Base, TypeURI, Representation);
}

