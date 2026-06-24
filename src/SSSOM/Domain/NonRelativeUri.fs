namespace SSSOM

open Fable.Core
open System
open System.Text.RegularExpressions

[<AttachMembers>]
type NonRelativeURI private (uri: string) =

    member this.Value = uri

    static member TypeClassUri = "xsd:anyURI"
    static member TypeClassCurie = "xsd:anyURI"
    static member TypeName = "NonRelativeURI"
    static member TypeModelUri = "SSSOM.NonRelativeURI"

    static member create (uriText: string) =
        let hasScheme = Regex.IsMatch(uriText, @"^[a-zA-Z][a-zA-Z0-9+.-]*:")

        if hasScheme then
            NonRelativeURI(uriText)
        else
            failwith "Can't create Uri from '{uriText}'. SSSOM requires a NonRelativeURI (must start with a valid scheme like http: or urn:)."