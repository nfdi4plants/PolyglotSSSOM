namespace SSSOM

open Fable.Core

/// Declares the meaning and optional lexical type of an extension slot.
[<AttachMembers>]
type ExtensionDefinition(slotName: string, ?property: EntityReference, ?typeHint: EntityReference) =
    let mutable slotName =
        if LexicalValidation.isNcName slotName then slotName
        else invalidArg (nameof slotName) $"'{slotName}' is not an NCName."

    let mutable property = property
    let mutable typeHint = typeHint

    /// Gets or sets the extension slot name.
    member _.SlotName
        with get () = slotName
        and set value =
            if LexicalValidation.isNcName value then slotName <- value
            else invalidArg (nameof value) $"'{value}' is not an NCName."

    /// Gets or sets the property defining the slot's meaning.
    member _.Property
        with get () = property
        and set value = property <- value

    /// Gets or sets the optional LinkML-compatible type hint.
    member _.TypeHint
        with get () = typeHint
        and set value = typeHint <- value

/// Retains one declared extension slot's lexical value without interpretation.
[<AttachMembers>]
type ExtensionValue(slotName: string, value: string) =
    let mutable slotName =
        if LexicalValidation.isNcName slotName then slotName
        else invalidArg (nameof slotName) $"'{slotName}' is not an NCName."

    let mutable value = value

    /// Gets or sets the extension slot name.
    member _.SlotName
        with get () = slotName
        and set newValue =
            if LexicalValidation.isNcName newValue then slotName <- newValue
            else invalidArg (nameof newValue) $"'{newValue}' is not an NCName."

    /// Gets or sets the uninterpreted lexical value.
    member _.Value
        with get () = value
        and set newValue = value <- newValue
