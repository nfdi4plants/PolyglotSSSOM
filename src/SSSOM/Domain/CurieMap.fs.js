
import { class_type } from "../fable_modules/fable-library-js.5.2.0/Reflection.js";

export class CurieMap {
    constructor(prefix_name, prefix_url) {
        this._prefix_name = prefix_name;
        this._prefix_url = prefix_url;
    }
    get Prefix_name() {
        const this$ = this;
        return this$._prefix_name;
    }
    set Prefix_name(value) {
        const this$ = this;
        this$._prefix_name = value;
    }
    get Prefix_url() {
        const this$ = this;
        return this$._prefix_url;
    }
    set Prefix_url(value) {
        const this$ = this;
        this$._prefix_url = value;
    }
}

export function CurieMap_$reflection() {
    return class_type("SSSOM.CurieMap", undefined, CurieMap);
}

export function CurieMap_$ctor_Z384F8060(prefix_name, prefix_url) {
    return new CurieMap(prefix_name, prefix_url);
}

