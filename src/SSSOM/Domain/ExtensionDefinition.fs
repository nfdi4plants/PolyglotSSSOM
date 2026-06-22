namespace SSSOM

open Fable.Core

[<AttachMembers>]
type ExtensionDefinition(
    ?property: EntityReference,
    ?slot_name: string,
    ?type_hint: string
) =
    let mutable _property = property
    let mutable _slot_name = slot_name
    let mutable _type_hint = type_hint

    member this.Property
        with get() = _property
        and set value = _property <- value

    member this.Slot_Name
        with get() = _slot_name
        and set value = _slot_name <- value

    member this.Type_hint
        with get() = _type_hint
        and set value = _type_hint <- value
