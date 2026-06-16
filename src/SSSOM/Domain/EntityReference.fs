namespace SSSOM

open Fable.Core

[<AttachMembers>]
type EntityReference(TypeOf: string, Base: string, TypeURI: string, Representation: string) =
    let mutable _typeOf = TypeOf
    let mutable ``_base`` = Base
    let mutable _typeURI = TypeURI
    let mutable _representation = Representation

    member this.TypeOf
        with get() = _typeOf
        and set value = _typeOf <- value

    member this.Base
        with get() = ``_base``
        and set value = ``_base`` <- value

    member this.TypeURI
        with get() = _typeURI
        and set value = _typeURI <- value

    member this.Representation
        with get() = _representation
        and set value = _representation <- value

