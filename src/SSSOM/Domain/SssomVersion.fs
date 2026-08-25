namespace SSSOM

type SssomVersion =
    | V1_0
    | V1_1

module SssomVersion =
    let toString (version: SssomVersion) =
        match version with
        | V1_0 -> "sssom:version1.0"
        | V1_1 -> "sssom:version1.1"
