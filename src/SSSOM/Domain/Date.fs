namespace SSSOM

open Fable.Core
open System
open System.Text.RegularExpressions

[<AttachMembers>]
type Date private (dateStr: string) =

    let mutable _dateStr = dateStr

    member this.Value
        with get() = _dateStr
        and set value = _dateStr <- value

    static member TypeClassUri = "xsd:date"
    static member TypeName = "date"

    static member create (text: string) =
        let isRightFormat = Regex.IsMatch(text, @"^\d{4}-\d{2}-\d{2}$")

        if isRightFormat then
            match DateTime.TryParse(text) with
            | true, _ -> 
                Date(text)
            | false, _ ->
                failwith $"Can't create Date from '{text}'. It matches the format, but the calendar day does not exist (e.g., February 30th)"
        else
            failwith $"Can't create Date from '{text}'. SSSOM requires the format YYYY-MM-DD (e.g., '2023-10-25')."

