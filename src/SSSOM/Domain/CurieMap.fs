namespace SSSOM

open Fable.Core

[<AttachMembers>]
type CurieMap(
    prefix_name: string,
    prefix_url: string
) =
    let mutable _prefix_name = prefix_name
    let mutable _prefix_url = prefix_url

    member this.Prefix_name
        with get() = _prefix_name
        and set value = _prefix_name <- value

    member this.Prefix_url
        with get() = _prefix_url
        and set value = _prefix_url <- value
