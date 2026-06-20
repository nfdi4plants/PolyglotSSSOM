namespace SSSOM

open System
open System.Text
open YAMLicious
open Fable.Core

[<AttachMembers>]
type DecodeMappingSet() =

    static member extractMappingSet (source: string) =
        let lines = source.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
        let processedYamlInput = StringBuilder()

        for line in lines do
            let checkLine = line.TrimStart([|' '; '\t'; '/'|])
            if checkLine.StartsWith("#") then
                processedYamlInput.Append(line + "\n") |> ignore
        processedYamlInput.ToString().TrimEnd('\n')

    static member isValidYamlInput (source: string) =
        let yamlinputLines = source.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)

        if yamlinputLines.Length = 0 then
            false
        else
            let mutable isConsistent = true
            
            let initialLine = yamlinputLines.[0]
            let afterHash = initialLine.Substring(1)
            let baseIndent = afterHash.Length - afterHash.TrimStart(' ').Length
            
            let mutable indentStack = [ baseIndent ]
            let mutable previousOpenedBlock = afterHash.TrimEnd().EndsWith(":")

            for i = 1 to yamlinputLines.Length - 1 do
                if isConsistent then
                    let line = yamlinputLines.[i]
                    let currentAfterHash = line.Substring(1)
                    let currentSpaceCount = currentAfterHash.Length - currentAfterHash.TrimStart(' ').Length
                    let opensBlock = currentAfterHash.TrimEnd().EndsWith(":")

                    let currentTop = indentStack.Head

                    if currentSpaceCount > currentTop then
                        if previousOpenedBlock then
                            indentStack <- currentSpaceCount :: indentStack
                        else
                            isConsistent <- false
                            
                    elif currentSpaceCount < currentTop then
                        let mutable tempStack = indentStack
                        while not tempStack.IsEmpty && currentSpaceCount < tempStack.Head do
                            tempStack <- tempStack.Tail
                            
                        if not tempStack.IsEmpty && currentSpaceCount = tempStack.Head then
                            indentStack <- tempStack
                        else
                            isConsistent <- false
                    else
                        ()

                    previousOpenedBlock <- opensBlock

            isConsistent
    
    static member processYamlInput (source: string) =
        let preprocessedYaml = DecodeMappingSet.extractMappingSet(source)
        let isValid = DecodeMappingSet.isValidYamlInput(preprocessedYaml)

        let finalYamlBuilder = StringBuilder()

        if isValid then
            let lines = preprocessedYaml.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)

            let firstAfterHash = lines.[0].Substring(1)
            let baseIndent = firstAfterHash.Length - firstAfterHash.TrimStart(' ').Length

            for line in lines do
                let afterHash = line.Substring(1)

                let cleanYamlLine =
                    if afterHash.Length >= baseIndent then
                        afterHash.Substring(baseIndent)
                    else
                        afterHash.TrimStart(' ')

                finalYamlBuilder.Append(cleanYamlLine + "\n") |> ignore
        else
            failwith "Yaml-input is not valid!"

        finalYamlBuilder.ToString().TrimEnd('\n')

    static member DecodeMappingSet (source: string) =
        let yamlInput = DecodeMappingSet.processYamlInput(source)
        let yamlElement = YAMLicious.Reader.read yamlInput

        let mappingSetDecoder = 
            Decode.object (fun get ->
            let curieDict = get.Optional.Field "curie_map" (Decode.dict id Decode.string)

            let curieList =
                match curieDict with
                | Some dict -> 
                    let tempList = ResizeArray<CurieMap>()

                    for kvp in dict do
                        let curieItem = CurieMap(kvp.Key, kvp.Value)
                        tempList.Add(curieItem)
                    let output = tempList.ToArray()
                    Some output
                | None -> Some [||]

            MappingSet(
                ?Sssom_version = get.Optional.Field "sssom_version" Decode.string,
                ?Curie_map = curieList,
                ?Mappings = get.Optional.Field "mappings" Decode.string,
                ?Mapping_set_id = get.Optional.Field "mapping_set_id" Decode.string,
                ?Mapping_set_version = get.Optional.Field "mapping_set_version" Decode.string,
                ?Mapping_set_source = get.Optional.Field "mapping_set_source" Decode.string,
                ?Mapping_set_title =get.Optional.Field "mapping_set_title" Decode.string,
                ?Mapping_set_description = get.Optional.Field "mapping_set_description" Decode.string,
                ?Mapping_set_confidence = get.Optional.Field "mapping_set_confidence" Decode.string,
                ?Creator_id = get.Optional.Field "creator_id" Decode.string,
                ?Creator_label = get.Optional.Field "creator_label" Decode.string,
                ?License = get.Optional.Field "license" Decode.string,
                ?Subject_type = get.Optional.Field "subject_type" Decode.string,
                ?Subject_source = get.Optional.Field "subject_source" Decode.string,
                ?Subject_source_version = get.Optional.Field "" Decode.string,
                ?Object_type = get.Optional.Field "object_type" Decode.string,
                ?Object_source = get.Optional.Field "object_source" Decode.string,
                ?Object_source_version = get.Optional.Field "object_source_version" Decode.string,
                ?Predicate_type = get.Optional.Field "predicate_type" Decode.string,
                ?Mapping_provider = get.Optional.Field "mapping_provider" Decode.string,
                ?Cardinality_scope = get.Optional.Field "cardinality_scope" Decode.string,
                ?Mapping_tool = get.Optional.Field "mapping_tool" Decode.string,
                ?Mapping_tool_id = get.Optional.Field "mapping_tool_id" Decode.string,
                ?Mapping_tool_version = get.Optional.Field "mapping_tool_version" Decode.string,
                ?Mapping_date = get.Optional.Field "mapping_date" Decode.string,
                ?Publication_date = get.Optional.Field "publication_date" Decode.string,
                ?Subject_match_field = get.Optional.Field "subject_match_field" Decode.string,
                ?Object_match_field = get.Optional.Field "object_match_field" Decode.string,
                ?Subject_preprocessing = get.Optional.Field "subject_preprocessing" Decode.string,
                ?Object_preprocessing = get.Optional.Field "object_preprocessing" Decode.string,
                ?Similarity_measure = get.Optional.Field "similarity_measure" Decode.string,
                ?Curation_rule = get.Optional.Field "curation_rule" Decode.string,
                ?Curation_rule_text = get.Optional.Field "curation_rule_text" Decode.string,
                ?See_also = get.Optional.Field "see_also" Decode.string,
                ?Issue_tracker = get.Optional.Field "issue_tracker" Decode.string,
                ?Other = get.Optional.Field "other" Decode.string,
                ?Comment = get.Optional.Field "comment" Decode.string,
                ?Extension_definitions = get.Optional.Field "extension_definitions" Decode.string
            )
        )

        let output = mappingSetDecoder yamlElement
        output