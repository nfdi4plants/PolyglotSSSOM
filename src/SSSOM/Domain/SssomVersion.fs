namespace SSSOM

open Fable.Core

[<StringEnum>]
type SssomVersion =
    | [<CompiledName("sssom:version1.0")>] V1_0
    | [<CompiledName("sssom:version1.1")>] V1_1

module SssomVersion =
    let toString (version: SssomVersion) =
        match version with
        | V1_0 -> "sssom:version1.0"
        | V1_1 -> "sssom:version1.1"