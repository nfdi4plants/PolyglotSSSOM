
import { StringBuilder__Append_Z721C83C5, StringBuilder_$ctor } from "../fable_modules/fable-library-js.5.2.0/System.Text.js";
import { join, replace } from "../fable_modules/fable-library-js.5.2.0/String.js";
import { setItem, item as item_2, fill } from "../fable_modules/fable-library-js.5.2.0/Array.js";
import { disposeSafe, getEnumerator } from "../fable_modules/fable-library-js.5.2.0/Util.js";
import { toString } from "../fable_modules/fable-library-js.5.2.0/Types.js";
import { class_type } from "../fable_modules/fable-library-js.5.2.0/Reflection.js";

export class EncodeMapping {
    constructor() {
    }
    static EncodeMapping(mapping) {
        const tsvString = StringBuilder_$ctor();
        const allHeaders = ["predicate_id", "mapping_justification", "record_id", "subject_id", "subject_label", "subject_category", "predicate_label", "predicate_modifier", "object_id", "object_label", "object_category", "author_id", "author_label", "reviewer_id", "reviewer_label", "creator_id", "creator_label", "license", "subject_type", "subject_source", "subject_source_version", "object_type", "object_source", "object_source_version", "predicate_type", "mapping_provider", "mapping_source", "mapping_cardinality", "cardinality_scope", "mapping_tool", "mapping_tool_id", "mapping_tool_version", "mapping_date", "publication_date", "review_date", "confidence", "reviewer_agreement", "curation_role", "curation_role_text", "subject_match_field", "object_match_field", "match_string", "subject_preprocessing", "object_preprocessing", "similarity_score", "similarity_measure", "see_also", "issue_tracker_item", "derived_from", "other", "comment"];
        const numCols = allHeaders.length | 0;
        const getValue = (opt) => {
            if (opt != null) {
                return replace(replace(replace(opt, "\t", " "), "\n", " "), "\r", " ");
            }
            else {
                return "";
            }
        };
        const getFloatValue = (opt_1) => {
            if (opt_1 != null) {
                return String(opt_1);
            }
            else {
                return "";
            }
        };
        const getRowValues = (i) => [replace(replace(replace(i.Predicate_id, "\t", " "), "\n", " "), "\r", " "), replace(replace(replace(i.Mapping_justification, "\t", " "), "\n", " "), "\r", " "), getValue(i.Record_id), getValue(i.Subject_id), getValue(i.Subject_label), getValue(i.Subject_category), getValue(i.Predicate_label), getValue(i.Predicate_modifier), getValue(i.Object_id), getValue(i.Object_label), getValue(i.Object_category), getValue(i.author_id), getValue(i.Author_label), getValue(i.Reviewer_id), getValue(i.Reviewer_label), getValue(i.Creator_id), getValue(i.Creator_label), getValue(i.License), getValue(i.Subject_type), getValue(i.Subject_source), getValue(i.Subject_source_version), getValue(i.Object_type), getValue(i.Object_source), getValue(i.Object_source_version), getValue(i.Predicate_type), getValue(i.Mapping_provider), getValue(i.Mapping_source), getValue(i.Mapping_cardinality), getValue(i.Cardinality_scope), getValue(i.Mapping_tool), getValue(i.Mapping_tool_id), getValue(i.Mapping_tool_version), getValue(i.Mapping_date), getValue(i.Publication_date), getValue(i.Review_date), getFloatValue(i.Confidence), getFloatValue(i.Reviewer_agreement), getValue(i.Curation_rule), getValue(i.Curation_rule_text), getValue(i.Subject_match_field), getValue(i.Object_match_field), getValue(i.Match_string), getValue(i.Subject_preprocessing), getValue(i.Object_preprocessing), getFloatValue(i.Similarity_score), getValue(i.Similarity_measure), getValue(i.See_also), getValue(i.Issue_tracker_item), getValue(i.Derived_from), getValue(i.Other), getValue(i.Comment)];
        const columnHasData = fill(new Array(numCols), 0, numCols, false);
        const enumerator = getEnumerator(mapping);
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                const rowVals = getRowValues(enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]());
                for (let colIdx = 0; colIdx <= (numCols - 1); colIdx++) {
                    if (item_2(colIdx, rowVals) !== "") {
                        setItem(columnHasData, colIdx, true);
                    }
                }
            }
        }
        finally {
            disposeSafe(enumerator);
        }
        const activeHeaders = [];
        for (let colIdx_1 = 0; colIdx_1 <= (numCols - 1); colIdx_1++) {
            if (item_2(colIdx_1, columnHasData)) {
                void (activeHeaders.push(item_2(colIdx_1, allHeaders)));
            }
        }
        StringBuilder__Append_Z721C83C5(StringBuilder__Append_Z721C83C5(tsvString, join("\t", activeHeaders)), "\n");
        const enumerator_1 = getEnumerator(mapping);
        try {
            while (enumerator_1["System.Collections.IEnumerator.MoveNext"]()) {
                const rowVals_1 = getRowValues(enumerator_1["System.Collections.Generic.IEnumerator`1.get_Current"]());
                const activeValues = [];
                for (let colIdx_2 = 0; colIdx_2 <= (numCols - 1); colIdx_2++) {
                    if (item_2(colIdx_2, columnHasData)) {
                        void (activeValues.push(item_2(colIdx_2, rowVals_1)));
                    }
                }
                StringBuilder__Append_Z721C83C5(StringBuilder__Append_Z721C83C5(tsvString, join("\t", activeValues)), "\n");
            }
        }
        finally {
            disposeSafe(enumerator_1);
        }
        return toString(tsvString);
    }
}

export function EncodeMapping_$reflection() {
    return class_type("SSSOM.EncodeMapping", undefined, EncodeMapping);
}

export function EncodeMapping_$ctor() {
    return new EncodeMapping();
}

