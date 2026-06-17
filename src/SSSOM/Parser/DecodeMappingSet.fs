namespace SSSOM

open System
open System.Text
open YAMLicious
open YAMLiciousTypes
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

        let mutable isConsitent = true
        let mutable previousSpaceCount = 0
        let mutable previousOpenedBlock = false

        if yamlinputLines.Length > 0 then
            for i = 0 to yamlinputLines.Length - 1 do
                if isConsitent then
                    let line = yamlinputLines.[i]

                    let afterHash = line.Substring(1)
                    let withoutSpaces = afterHash.TrimStart(' ')
                    let currentSpaces = afterHash.Length - withoutSpaces.Length

                    let opensBlock = withoutSpaces.TrimEnd().EndsWith(":")

                    if i = 0 then
                        previousSpaceCount <- currentSpaces
                        previousOpenedBlock <- opensBlock
                    else
                        if previousOpenedBlock then
                            previousSpaceCount <- currentSpaces
                        else
                            if currentSpaces > previousSpaceCount then
                                isConsitent <- false
                            else
                                previousSpaceCount <- currentSpaces
                        
                        previousOpenedBlock <- opensBlock
        else
            isConsitent <- false

        isConsitent

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

        finalYamlBuilder.ToString()

    static member DecodeMappingSet (source: string) =
        let yamlInput = DecodeMappingSet.processYamlInput(source)
        let yamlElement = YAMLicious.Reader.read yamlInput

        let dataformatDecoder = 
            Decode.object (fun get ->
            let curieDict = get.Optional.Field "curie_map" (Decode.dict  id Decode.string)

            let curieList =
                curieDict
                |> Option.map (fun dict ->
                    dict
                    |> Seq.map (fun kvp -> CurieMap(kvp.Key, kvp.Value))
                    |> Seq.toList
                )
            ()

            // TODO: Call MappingSet constructor and fill in the fields
        )

        printfn "TODO"

