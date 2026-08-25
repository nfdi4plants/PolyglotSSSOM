namespace SSSOM

open Fable.Core

/// Represents one SSSOM document as metadata plus mapping rows.
[<AttachMembers>]
type SssomDocument(metadata: MappingSet, mappings: Mapping array) =
    let mutable metadata = ModelValue.required (nameof metadata) metadata
    let mutable mappings = ModelValue.nonNullArray mappings

    /// Gets or sets the required mapping-set metadata.
    member _.Metadata
        with get () = metadata
        and set value = metadata <- ModelValue.required (nameof value) value

    /// Gets or sets the mapping rows; null is normalized to an empty array.
    member _.Mappings
        with get () = mappings
        and set value = mappings <- ModelValue.nonNullArray value
