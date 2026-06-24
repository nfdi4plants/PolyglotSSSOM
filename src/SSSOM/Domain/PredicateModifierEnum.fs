namespace SSSOM

open Fable.Core

[<StringEnum>]
type PredicateModifierEnum =
    | [<CompiledName("Not")>] Not

module PredicateModifierEnum =
    let create (text: string) =
        match text with
        | "Not" -> Not
        | unknown -> failwith $"Can't parse PredicateModifierEnum. Unknown value: '{unknown}'"

    let toString (enumValue: PredicateModifierEnum) =
        match enumValue with
        | Not -> "Not"

