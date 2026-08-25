namespace SSSOM

open System
open System.Text.RegularExpressions
open Fable.Core

module internal LexicalValidation =

    let requireNonBlank argumentName (value: string) =
        if String.IsNullOrWhiteSpace value then
            invalidArg argumentName "The lexical value must not be empty or whitespace."

        value

    let isNcName (value: string) =
        not (String.IsNullOrWhiteSpace value)
        && Regex.IsMatch(value, "^[A-Za-z_][A-Za-z0-9._-]*$")

    let isNonRelativeUri (value: string) =
        not (String.IsNullOrWhiteSpace value)
        && Regex.IsMatch(value, "^[A-Za-z][A-Za-z0-9+.-]*:[^\\s]+$")

    let requireEntityReference argumentName (value: string) =
        let candidate = requireNonBlank argumentName value

        if Regex.IsMatch(candidate, "^[A-Za-z_][A-Za-z0-9_.-]*:[^\\s]+$")
           || isNonRelativeUri candidate then
            candidate
        else
            invalidArg argumentName $"'{value}' is not a valid URI or CURIE lexical value."

    let requireDate argumentName (value: string) =
        let invalid () =
            invalidArg argumentName $"'{value}' is not a valid calendar date in YYYY-MM-DD form."

        if isNull value || not (Regex.IsMatch(value, "^[0-9]{4}-[0-9]{2}-[0-9]{2}$")) then
            invalid ()

        let year = Int32.Parse(value.Substring(0, 4))
        let month = Int32.Parse(value.Substring(5, 2))
        let day = Int32.Parse(value.Substring(8, 2))
        let leap = year % 400 = 0 || (year % 4 = 0 && year % 100 <> 0)

        let daysInMonth =
            match month with
            | 1 | 3 | 5 | 7 | 8 | 10 | 12 -> 31
            | 4 | 6 | 9 | 11 -> 30
            | 2 when leap -> 29
            | 2 -> 28
            | _ -> 0

        if day < 1 || day > daysInMonth then invalid ()
        value

/// A validated lexical URI value that may retain v1.0 relative or string forms.
[<AttachMembers>]
type UriReference private (value: string) =

    let value = LexicalValidation.requireNonBlank (nameof value) value

    /// Gets the original lexical value.
    member _.Value = value

    /// Gets whether this value satisfies the v1.1 NonRelativeURI constraint.
    member _.IsNonRelative = LexicalValidation.isNonRelativeUri value

    /// Creates a non-blank URI lexical value.
    static member Create(value: string) =
        UriReference value

    /// Tries to create a URI lexical value.
    static member TryCreate(value: string) =
        try Some(UriReference value) with _ -> None

/// A validated URI or CURIE lexical identifier.
[<AttachMembers>]
type EntityReference private (value: string) =

    let value = LexicalValidation.requireEntityReference (nameof value) value

    /// Gets the original lexical value.
    member _.Value = value

    /// Gets whether the value has a URI/CURIE prefix and no whitespace.
    member _.IsValid =
        true

    /// Creates a URI or CURIE entity reference.
    static member Create(value: string) =
        EntityReference value

    /// Tries to create a URI or CURIE entity reference.
    static member TryCreate(value: string) =
        try Some(EntityReference value) with _ -> None

/// A validated calendar date in the SSSOM YYYY-MM-DD lexical form.
[<AttachMembers>]
type SssomDate private (value: string) =

    let value = LexicalValidation.requireDate (nameof value) value

    /// Gets the canonical YYYY-MM-DD value.
    member _.Value = value

    /// Creates a validated SSSOM date.
    static member Create(value: string) =
        SssomDate value

    /// Tries to create a validated SSSOM date.
    static member TryCreate(value: string) =
        try
            Some(SssomDate.Create value)
        with _ ->
            None
