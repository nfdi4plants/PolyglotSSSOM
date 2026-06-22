namespace SSSOM

open Fable.Core
open System.Text.RegularExpressions

[<AttachMembers>]
type EntityReference private (value: string) =
    member this.Value
        with get() = value

    static member TypeClassUri = "rdfs:Resource"
    static member TypeClassCurie = "rdfs:Resource"
    static member TypeName = "EntityReference"
    static member TypeModelUri = "SSSOM.EntityReference"

    static member Create(text: string) =
        let isValid = Regex.IsMatch(text, @"^[a-zA-Z0-9_.-]+:")
        if isValid then
            EntityReference(text)
        else
           failwith $"Can't create EntityReference from '{text}'. It must be a valid URI or CURIE (e.g., 'prefix:value')." 

