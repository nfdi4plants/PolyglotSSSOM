
import { StringBuilder__Append_Z721C83C5, StringBuilder_$ctor } from "../fable_modules/fable-library-js.5.2.0/System.Text.js";
import { trimEnd, isNullOrWhiteSpace, replace } from "../fable_modules/fable-library-js.5.2.0/String.js";
import { item } from "../fable_modules/fable-library-js.5.2.0/Array.js";
import { toString } from "../fable_modules/fable-library-js.5.2.0/Types.js";
import { class_type } from "../fable_modules/fable-library-js.5.2.0/Reflection.js";

export class EncodeMappingSet {
    constructor() {
    }
    static EncodeMappingSet(mappingSet) {
        let curies;
        const builder = StringBuilder_$ctor();
        const addField = (yamlKey, optValue) => {
            let matchResult, value_1;
            if (optValue != null) {
                if (!isNullOrWhiteSpace(optValue)) {
                    matchResult = 0;
                    value_1 = optValue;
                }
                else {
                    matchResult = 1;
                }
            }
            else {
                matchResult = 1;
            }
            switch (matchResult) {
                case 0: {
                    const cleanValue = replace(replace(replace(value_1, "\t", " "), "\n", " "), "\r", " ");
                    StringBuilder__Append_Z721C83C5(StringBuilder__Append_Z721C83C5(StringBuilder__Append_Z721C83C5(StringBuilder__Append_Z721C83C5(StringBuilder__Append_Z721C83C5(builder, "#"), yamlKey), ": "), cleanValue), "\n");
                    break;
                }
                case 1: {
                    break;
                }
            }
        };
        const matchValue = mappingSet.Curie_map;
        let matchResult_1, curies_1;
        if (matchValue != null) {
            if ((curies = matchValue, curies.length > 0)) {
                matchResult_1 = 0;
                curies_1 = matchValue;
            }
            else {
                matchResult_1 = 1;
            }
        }
        else {
            matchResult_1 = 1;
        }
        switch (matchResult_1) {
            case 0: {
                StringBuilder__Append_Z721C83C5(builder, "#curie_map:\n");
                for (let idx = 0; idx <= (curies_1.length - 1); idx++) {
                    const curie = item(idx, curies_1);
                    StringBuilder__Append_Z721C83C5(StringBuilder__Append_Z721C83C5(StringBuilder__Append_Z721C83C5(StringBuilder__Append_Z721C83C5(StringBuilder__Append_Z721C83C5(builder, "#  "), curie.Prefix_name), ": "), curie.Prefix_url), "\n");
                }
                break;
            }
        }
        addField("sssom_version", mappingSet.Sssom_version);
        addField("mappings", mappingSet.Mappings);
        addField("mapping_set_id", mappingSet.Mapping_set_id);
        addField("mapping_set_version", mappingSet.Mapping_set_version);
        addField("mapping_set_source", mappingSet.Mapping_set_source);
        addField("mapping_set_title", mappingSet.Mapping_set_title);
        addField("mapping_set_description", mappingSet.Mapping_set_description);
        addField("mapping_set_confidence", mappingSet.Mapping_set_confidence);
        addField("creator_id", mappingSet.Creator_id);
        addField("creator_label", mappingSet.Creator_label);
        addField("license", mappingSet.License);
        addField("subject_type", mappingSet.Subject_type);
        addField("subject_source", mappingSet.Subject_source);
        addField("object_type", mappingSet.Object_type);
        addField("object_source", mappingSet.Object_Source);
        addField("object_source_version", mappingSet.Object_source_version);
        addField("predicate_type", mappingSet.Predicate_type);
        addField("cardinality_scope", mappingSet.Cardinality_scope);
        addField("mapping_tool", mappingSet.Mapping_tool);
        addField("mapping_tool_id", mappingSet.Mapping_tool_id);
        addField("mapping_tool_version", mappingSet.Mapping_tool_version);
        addField("mapping_date", mappingSet.Mapping_date);
        addField("publication_date", mappingSet.Publication_date);
        addField("subject_match_field", mappingSet.Subject_match_field);
        addField("object_match_field", mappingSet.Object_match_field);
        addField("subject_preprocessing", mappingSet.Subject_preprocessing);
        addField("object_preprocessing", mappingSet.Object_preprocessing);
        addField("similarity_measure", mappingSet.Similarity_measure);
        addField("curation_rule", mappingSet.Curation_rule);
        addField("curation_rule_text", mappingSet.Curation_rule_text);
        addField("see_also", mappingSet.See_also);
        addField("issue_tracker", mappingSet.Issue_tracker);
        addField("other", mappingSet.Other);
        addField("extension_definitions", mappingSet.Extension_definitions);
        addField("comment", mappingSet.Comment);
        return trimEnd(toString(builder), "\n");
    }
}

export function EncodeMappingSet_$reflection() {
    return class_type("SSSOM.EncodeMappingSet", undefined, EncodeMappingSet);
}

export function EncodeMappingSet_$ctor() {
    return new EncodeMappingSet();
}

