namespace SSSOM

open Fable.Core

[<AttachMembers>]
type SssomDocument(
    metadata: MappingSet,
    mappings: list<Mapping>
) =
    let mutable _metadata = metadata
    let mutable _mappings = mappings

    member this.Metadata
        with get() = _metadata
        and set value = _metadata <- value

    member this.Mappings
        with get() = _mappings
        and set value = _mappings <- value