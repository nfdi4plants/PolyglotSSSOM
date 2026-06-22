
import { printf, toText, isNullOrWhiteSpace, trimEnd, trimStart, split } from "../fable_modules/fable-library-js.5.2.0/String.js";
import { StringBuilder__Append_Z721C83C5, StringBuilder_$ctor } from "../fable_modules/fable-library-js.5.2.0/System.Text.js";
import { item } from "../fable_modules/fable-library-js.5.2.0/Array.js";
import { FSharpRef, toString } from "../fable_modules/fable-library-js.5.2.0/Types.js";
import { Exception } from "../fable_modules/fable-library-js.5.2.0/Util.js";
import { tryGetValue, addToDict } from "../fable_modules/fable-library-js.5.2.0/MapUtil.js";
import { tryParse } from "../fable_modules/fable-library-js.5.2.0/Double.js";
import { Mapping } from "../Domain/Mapping.fs.js";
import { unwrap } from "../fable_modules/fable-library-js.5.2.0/Option.js";
import { toList } from "../fable_modules/fable-library-js.5.2.0/Seq.js";
import { class_type } from "../fable_modules/fable-library-js.5.2.0/Reflection.js";

export class DecodeMapping {
    constructor() {
    }
    static extractMapping(source) {
        const lines = split(source, ["\n", "\r"], undefined, 1);
        const processYamlInput = StringBuilder_$ctor();
        for (let idx = 0; idx <= (lines.length - 1); idx++) {
            const line = item(idx, lines);
            const checkLine = trimStart(line, " ", "\t", "/");
            if (!checkLine.startsWith("#")) {
                StringBuilder__Append_Z721C83C5(processYamlInput, line + "\n");
            }
        }
        return trimEnd(toString(processYamlInput), "\n");
    }
    static isValidTsvInput(source) {
        const lines = split(DecodeMapping.extractMapping(source), ["\n", "\r"], undefined, 1);
        if (lines.length === 0) {
            return false;
        }
        else {
            let isValid = true;
            const headers = item(0, lines).split("\t");
            const expectedColumns = headers.length | 0;
            let hasPredicate_id = false;
            let hasMapping_justification = false;
            for (let idx = 0; idx <= (headers.length - 1); idx++) {
                const header = item(idx, headers);
                const lowercaseHeader = header.toLocaleLowerCase();
                if (lowercaseHeader.indexOf("predicate_id") >= 0) {
                    hasPredicate_id = true;
                }
                if (lowercaseHeader.indexOf("mapping_justification") >= 0) {
                    hasMapping_justification = true;
                }
            }
            if (hasPredicate_id && hasMapping_justification) {
                for (let i = 1; i <= (lines.length - 1); i++) {
                    if (isValid) {
                        const currentLine = item(i, lines);
                        if (currentLine.split("\t").length !== expectedColumns) {
                            isValid = false;
                        }
                    }
                }
            }
            else {
                isValid = false;
            }
            return isValid;
        }
    }
    static DecodeMapping(source) {
        const tsvString = DecodeMapping.extractMapping(source);
        if (!DecodeMapping.isValidTsvInput(tsvString)) {
            throw new Exception("Invalid TSV Input: Missing required columns or inconsistent column count.");
        }
        const lines = split(tsvString, ["\n", "\r"], undefined, 1);
        const headers = item(0, lines).split("\t");
        const headerIndexMap = new Map([]);
        for (let i = 0; i <= (headers.length - 1); i++) {
            addToDict(headerIndexMap, item(i, headers), i);
        }
        const mappings = [];
        for (let i_1 = 1; i_1 <= (lines.length - 1); i_1++) {
            const columns = item(i_1, lines).split("\t");
            const getOptionalString = (colName) => {
                let matchValue;
                let outArg = 0;
                matchValue = [tryGetValue(headerIndexMap, colName.toLocaleLowerCase(), new FSharpRef(() => (outArg | 0), (v) => {
                    outArg = (v | 0);
                })), outArg];
                if (matchValue[0]) {
                    const index = matchValue[1] | 0;
                    if ((index < columns.length) && !isNullOrWhiteSpace(item(index, columns))) {
                        return item(index, columns);
                    }
                    else {
                        return undefined;
                    }
                }
                else {
                    return undefined;
                }
            };
            const getRequiredString = (colName_1) => {
                const matchValue_1 = getOptionalString(colName_1);
                if (matchValue_1 == null) {
                    throw new Exception(toText(printf("Row %d: Mandatory field \'%s\' is missing or empty!"))(i_1)(colName_1));
                }
                else {
                    return matchValue_1;
                }
            };
            const getOptionalDouble = (colName_2) => {
                const matchValue_2 = getOptionalString(colName_2);
                if (matchValue_2 == null) {
                    return undefined;
                }
                else {
                    let matchValue_3;
                    let outArg_1 = 0;
                    matchValue_3 = [tryParse(matchValue_2, new FSharpRef(() => outArg_1, (v_1) => {
                        outArg_1 = v_1;
                    })), outArg_1];
                    if (matchValue_3[0]) {
                        return matchValue_3[1];
                    }
                    else {
                        return undefined;
                    }
                }
            };
            const newMapping = new Mapping(getRequiredString("predicate_id"), getRequiredString("mapping_justification"), unwrap(getOptionalString("record_id")), unwrap(getOptionalString("subject_id")), unwrap(getOptionalString("subject_label")), unwrap(getOptionalString("subject_category")), unwrap(getOptionalString("predicate_label")), unwrap(getOptionalString("predicate_modifier")), unwrap(getOptionalString("object_id")), unwrap(getOptionalString("object_label")), unwrap(getOptionalString("object_category")), unwrap(getOptionalString("author_id")), unwrap(getOptionalString("author_label")), unwrap(getOptionalString("reviewer_id")), unwrap(getOptionalString("reviewer_label")), unwrap(getOptionalString("creator_id")), unwrap(getOptionalString("creator_label")), unwrap(getOptionalString("license")), unwrap(getOptionalString("subject_type")), unwrap(getOptionalString("subject_source")), unwrap(getOptionalString("subject_source_version")), unwrap(getOptionalString("object_type")), unwrap(getOptionalString("object_source")), unwrap(getOptionalString("object_source_version")), unwrap(getOptionalString("predicate_type")), unwrap(getOptionalString("mapping_provider")), unwrap(getOptionalString("mapping_source")), unwrap(getOptionalString("mapping_cardinality")), unwrap(getOptionalString("cardinality_scope")), unwrap(getOptionalString("mapping_tool")), unwrap(getOptionalString("mapping_tool_id")), unwrap(getOptionalString("mapping_tool_version")), unwrap(getOptionalString("mapping_date")), unwrap(getOptionalString("publication_date")), unwrap(getOptionalString("review_date")), unwrap(getOptionalDouble("confidence")), unwrap(getOptionalDouble("reviewer_agreement")), unwrap(getOptionalString("curation_rule")), unwrap(getOptionalString("curation_rule_text")), unwrap(getOptionalString("subject_match_field")), unwrap(getOptionalString("object_match_field")), unwrap(getOptionalString("match_string")), unwrap(getOptionalString("subject_preprocessing")), unwrap(getOptionalString("object_preprocessing")), unwrap(getOptionalDouble("similarity_score")), unwrap(getOptionalString("similarity_measure")), unwrap(getOptionalString("see_also")), unwrap(getOptionalString("issue_tracker_item")), unwrap(getOptionalString("derived_from")), unwrap(getOptionalString("other")), unwrap(getOptionalString("comment")));
            void (mappings.push(newMapping));
        }
        return toList(mappings);
    }
}

export function DecodeMapping_$reflection() {
    return class_type("SSSOM.DecodeMapping", undefined, DecodeMapping);
}

export function DecodeMapping_$ctor() {
    return new DecodeMapping();
}

