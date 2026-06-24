namespace SSSOM

open Fable.Core
open YAMLicious

[<AttachMembers>]
type DecodeCurieMap() =

    static member Decode =
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
        curieList
        )