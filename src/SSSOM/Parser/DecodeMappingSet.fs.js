
import { substring, trimEnd, trimStart, split } from "../fable_modules/fable-library-js.5.2.0/String.js";
import { StringBuilder__Append_Z721C83C5, StringBuilder_$ctor } from "../fable_modules/fable-library-js.5.2.0/System.Text.js";
import { item } from "../fable_modules/fable-library-js.5.2.0/Array.js";
import { toString } from "../fable_modules/fable-library-js.5.2.0/Types.js";
import { tail, isEmpty, cons, head, singleton } from "../fable_modules/fable-library-js.5.2.0/List.js";
import { disposeSafe, getEnumerator, Exception } from "../fable_modules/fable-library-js.5.2.0/Util.js";
import { CurieMap } from "../Domain/CurieMap.fs.js";
import { MappingSet } from "../Domain/MappingSet.fs.js";
import { unwrap } from "../fable_modules/fable-library-js.5.2.0/Option.js";
import { class_type } from "../fable_modules/fable-library-js.5.2.0/Reflection.js";

export class DecodeMappingSet {
    constructor() {
    }
    static extractMappingSet(source) {
        const lines = split(source, ["\n", "\r"], undefined, 1);
        const processedYamlInput = StringBuilder_$ctor();
        for (let idx = 0; idx <= (lines.length - 1); idx++) {
            const line = item(idx, lines);
            const checkLine = trimStart(line, " ", "\t", "/");
            if (checkLine.startsWith("#")) {
                StringBuilder__Append_Z721C83C5(processedYamlInput, line + "\n");
            }
        }
        return trimEnd(toString(processedYamlInput), "\n");
    }
    static isValidYamlInput(source) {
        const yamlinputLines = split(source, ["\n", "\r"], undefined, 1);
        if (yamlinputLines.length === 0) {
            return false;
        }
        else {
            let isConsistent = true;
            const afterHash = substring(item(0, yamlinputLines), 1);
            let indentStack = singleton(afterHash.length - trimStart(afterHash, " ").length);
            let previousOpenedBlock = afterHash.trimEnd().endsWith(":");
            for (let i = 1; i <= (yamlinputLines.length - 1); i++) {
                if (isConsistent) {
                    const currentAfterHash = substring(item(i, yamlinputLines), 1);
                    const currentSpaceCount = (currentAfterHash.length - trimStart(currentAfterHash, " ").length) | 0;
                    const opensBlock = currentAfterHash.trimEnd().endsWith(":");
                    const currentTop = head(indentStack) | 0;
                    if (currentSpaceCount > currentTop) {
                        if (previousOpenedBlock) {
                            indentStack = cons(currentSpaceCount, indentStack);
                        }
                        else {
                            isConsistent = false;
                        }
                    }
                    else if (currentSpaceCount < currentTop) {
                        let tempStack = indentStack;
                        while (!isEmpty(tempStack) && (currentSpaceCount < head(tempStack))) {
                            tempStack = tail(tempStack);
                        }
                        if (!isEmpty(tempStack) && (currentSpaceCount === head(tempStack))) {
                            indentStack = tempStack;
                        }
                        else {
                            isConsistent = false;
                        }
                    }
                    previousOpenedBlock = opensBlock;
                }
            }
            return isConsistent;
        }
    }
    static processYamlInput(source) {
        const preprocessedYaml = DecodeMappingSet.extractMappingSet(source);
        const isValid = DecodeMappingSet.isValidYamlInput(preprocessedYaml);
        const finalYamlBuilder = StringBuilder_$ctor();
        if (isValid) {
            const lines = split(preprocessedYaml, ["\n", "\r"], undefined, 1);
            const firstAfterHash = substring(item(0, lines), 1);
            const baseIndent = (firstAfterHash.length - trimStart(firstAfterHash, " ").length) | 0;
            for (let idx = 0; idx <= (lines.length - 1); idx++) {
                const afterHash = substring(item(idx, lines), 1);
                StringBuilder__Append_Z721C83C5(finalYamlBuilder, ((afterHash.length >= baseIndent) ? substring(afterHash, baseIndent) : trimStart(afterHash, " ")) + "\n");
            }
        }
        else {
            throw new Exception("Yaml-input is not valid!");
        }
        return trimEnd(toString(finalYamlBuilder), "\n");
    }
    static DecodeMappingSet(source) {
        return YAMLicious_Decode_object((get$) => {
            let objectArg_1, objectArg_2, objectArg_3, objectArg_4, objectArg_5, objectArg_6, objectArg_7, objectArg_8, objectArg_9, objectArg_10, objectArg_11, objectArg_12, objectArg_13, objectArg_14, objectArg_15, objectArg_16, objectArg_17, objectArg_18, objectArg_19, objectArg_20, objectArg_21, objectArg_22, objectArg_23, objectArg_24, objectArg_25, objectArg_26, objectArg_27, objectArg_28, objectArg_29, objectArg_30, objectArg_31, objectArg_32, objectArg_33, objectArg_34, objectArg_35, objectArg_36, objectArg_37;
            let curieDict;
            const objectArg = get$.Optional;
            curieDict = objectArg.Field("curie_map", (value) => YAMLicious_Decode_dict((x) => x, YAMLicious_Decode_string, value));
            let curieList;
            if (curieDict == null) {
                curieList = [];
            }
            else {
                const tempList = [];
                let enumerator = getEnumerator(curieDict);
                try {
                    while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                        const kvp = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                        const curieItem = new CurieMap(kvp[0], kvp[1]);
                        void (tempList.push(curieItem));
                    }
                }
                finally {
                    disposeSafe(enumerator);
                }
                curieList = tempList.slice();
            }
            return new MappingSet(unwrap((objectArg_1 = get$.Optional, objectArg_1.Field("sssom_version", YAMLicious_Decode_string))), unwrap(curieList), unwrap((objectArg_2 = get$.Optional, objectArg_2.Field("mappings", YAMLicious_Decode_string))), unwrap((objectArg_3 = get$.Optional, objectArg_3.Field("mapping_set_id", YAMLicious_Decode_string))), unwrap((objectArg_4 = get$.Optional, objectArg_4.Field("mapping_set_version", YAMLicious_Decode_string))), unwrap((objectArg_5 = get$.Optional, objectArg_5.Field("mapping_set_source", YAMLicious_Decode_string))), unwrap((objectArg_6 = get$.Optional, objectArg_6.Field("mapping_set_title", YAMLicious_Decode_string))), unwrap((objectArg_7 = get$.Optional, objectArg_7.Field("mapping_set_description", YAMLicious_Decode_string))), unwrap((objectArg_8 = get$.Optional, objectArg_8.Field("mapping_set_confidence", YAMLicious_Decode_string))), unwrap((objectArg_9 = get$.Optional, objectArg_9.Field("creator_id", YAMLicious_Decode_string))), unwrap((objectArg_10 = get$.Optional, objectArg_10.Field("creator_label", YAMLicious_Decode_string))), unwrap((objectArg_11 = get$.Optional, objectArg_11.Field("license", YAMLicious_Decode_string))), unwrap((objectArg_12 = get$.Optional, objectArg_12.Field("subject_type", YAMLicious_Decode_string))), unwrap((objectArg_13 = get$.Optional, objectArg_13.Field("subject_source", YAMLicious_Decode_string))), unwrap((objectArg_14 = get$.Optional, objectArg_14.Field("", YAMLicious_Decode_string))), unwrap((objectArg_15 = get$.Optional, objectArg_15.Field("object_type", YAMLicious_Decode_string))), unwrap((objectArg_16 = get$.Optional, objectArg_16.Field("object_source", YAMLicious_Decode_string))), unwrap((objectArg_17 = get$.Optional, objectArg_17.Field("object_source_version", YAMLicious_Decode_string))), unwrap((objectArg_18 = get$.Optional, objectArg_18.Field("predicate_type", YAMLicious_Decode_string))), unwrap((objectArg_19 = get$.Optional, objectArg_19.Field("mapping_provider", YAMLicious_Decode_string))), unwrap((objectArg_20 = get$.Optional, objectArg_20.Field("cardinality_scope", YAMLicious_Decode_string))), unwrap((objectArg_21 = get$.Optional, objectArg_21.Field("mapping_tool", YAMLicious_Decode_string))), unwrap((objectArg_22 = get$.Optional, objectArg_22.Field("mapping_tool_id", YAMLicious_Decode_string))), unwrap((objectArg_23 = get$.Optional, objectArg_23.Field("mapping_tool_version", YAMLicious_Decode_string))), unwrap((objectArg_24 = get$.Optional, objectArg_24.Field("mapping_date", YAMLicious_Decode_string))), unwrap((objectArg_25 = get$.Optional, objectArg_25.Field("publication_date", YAMLicious_Decode_string))), unwrap((objectArg_26 = get$.Optional, objectArg_26.Field("subject_match_field", YAMLicious_Decode_string))), unwrap((objectArg_27 = get$.Optional, objectArg_27.Field("object_match_field", YAMLicious_Decode_string))), unwrap((objectArg_28 = get$.Optional, objectArg_28.Field("subject_preprocessing", YAMLicious_Decode_string))), unwrap((objectArg_29 = get$.Optional, objectArg_29.Field("object_preprocessing", YAMLicious_Decode_string))), unwrap((objectArg_30 = get$.Optional, objectArg_30.Field("similarity_measure", YAMLicious_Decode_string))), unwrap((objectArg_31 = get$.Optional, objectArg_31.Field("curation_rule", YAMLicious_Decode_string))), unwrap((objectArg_32 = get$.Optional, objectArg_32.Field("curation_rule_text", YAMLicious_Decode_string))), unwrap((objectArg_33 = get$.Optional, objectArg_33.Field("see_also", YAMLicious_Decode_string))), unwrap((objectArg_34 = get$.Optional, objectArg_34.Field("issue_tracker", YAMLicious_Decode_string))), unwrap((objectArg_35 = get$.Optional, objectArg_35.Field("other", YAMLicious_Decode_string))), unwrap((objectArg_36 = get$.Optional, objectArg_36.Field("comment", YAMLicious_Decode_string))), unwrap((objectArg_37 = get$.Optional, objectArg_37.Field("extension_definitions", YAMLicious_Decode_string))));
        }, YAMLicious_Reader_read(DecodeMappingSet.processYamlInput(source)));
    }
}

export function DecodeMappingSet_$reflection() {
    return class_type("SSSOM.DecodeMappingSet", undefined, DecodeMappingSet);
}

export function DecodeMappingSet_$ctor() {
    return new DecodeMappingSet();
}

