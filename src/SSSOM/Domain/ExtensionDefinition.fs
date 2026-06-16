namespace SSSOM

open Fable.Core

[<AttachMembers>]
type ExtensionDefinition(Slot_name: string, Property: string, Type_hint: string) =
    let mutable _slot_name = Slot_name
    let mutable _property = Property
    let mutable _type_hint = Type_hint

    member this.Slot_name
        with get() = _slot_name
        and set value = _slot_name <- value

    member this.Property
        with get() = _property
        and set value = _property <- value

    member this.Type_hint
        with get() = _type_hint
        and set value = _type_hint <- value
    

