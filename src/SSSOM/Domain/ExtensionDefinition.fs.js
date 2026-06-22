
import { class_type } from "../fable_modules/fable-library-js.5.2.0/Reflection.js";

export class ExtensionDefinition {
    constructor(Slot_name, Property, Type_hint) {
        this._slot_name = Slot_name;
        this._property = Property;
        this._type_hint = Type_hint;
    }
    get Slot_name() {
        const this$ = this;
        return this$._slot_name;
    }
    set Slot_name(value) {
        const this$ = this;
        this$._slot_name = value;
    }
    get Property() {
        const this$ = this;
        return this$._property;
    }
    set Property(value) {
        const this$ = this;
        this$._property = value;
    }
    get Type_hint() {
        const this$ = this;
        return this$._type_hint;
    }
    set Type_hint(value) {
        const this$ = this;
        this$._type_hint = value;
    }
}

export function ExtensionDefinition_$reflection() {
    return class_type("SSSOM.ExtensionDefinition", undefined, ExtensionDefinition);
}

export function ExtensionDefinition_$ctor_30230F9B(Slot_name, Property, Type_hint) {
    return new ExtensionDefinition(Slot_name, Property, Type_hint);
}

