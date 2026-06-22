namespace SSSOM

open YAMLicious
open Fable.Core

[<AttachMembers>]
type DecodeSssomVersion() =
    static member DecodeSssomVersion =
        Decode.object (fun get ->
            let rawVersion = get.Optional.Field "sssom_version" Decode.string

            let parsedVersion =
                match rawVersion with
                | Some "sssom:version1.0" 
                | Some "1.0" -> Some SssomVersion.V1_0
                | Some "sssom:version1.1"
                | Some "1.1" -> Some SssomVersion.V1_1
                | Some unknown -> failwith $"Error: SSSOM Version not found {unknown}"
                | None -> None
            parsedVersion
        )