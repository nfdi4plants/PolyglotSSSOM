namespace SSSOM

open Fable.Core
open YAMLicious

[<AttachMembers>]
type DecodeExtensionDefinition() =
    
    static member Decode =
        Decode.object (fun get -> 
            
            let parseEntityReference reference =
                match reference with
                | Some x -> Some (EntityReference.create x)
                | None -> None

            ExtensionDefinition(
                ?property  = parseEntityReference (get.Optional.Field "property" Decode.string),
                ?slot_name = get.Optional.Field "slot_name" Decode.string,
                ?type_hint = get.Optional.Field "type_hint" Decode.string
            )
        )