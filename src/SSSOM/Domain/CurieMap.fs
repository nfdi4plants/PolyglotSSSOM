namespace SSSOM

open System
open Fable.Core

/// Declares one CURIE prefix and its IRI expansion.
[<AttachMembers>]
type PrefixEntry(prefixName: string, prefixUrl: UriReference) =
    let mutable prefixName =
        if LexicalValidation.isNcName prefixName then prefixName
        else invalidArg (nameof prefixName) $"'{prefixName}' is not a valid prefix name."

    let mutable prefixUrl = prefixUrl

    /// Gets or sets the prefix name without the trailing colon.
    member _.PrefixName
        with get () = prefixName
        and set value =
            if LexicalValidation.isNcName value then prefixName <- value
            else invalidArg (nameof value) $"'{value}' is not a valid prefix name."

    /// Gets or sets the IRI prefix expansion.
    member _.PrefixUrl
        with get () = prefixUrl
        and set value = prefixUrl <- value

/// Deterministic, lexical CURIE expansion and contraction helpers.
module CurieMap =

    let private builtIns =
        [|
            PrefixEntry("linkml", UriReference.Create "https://w3id.org/linkml/")
            PrefixEntry("owl", UriReference.Create "http://www.w3.org/2002/07/owl#")
            PrefixEntry("rdf", UriReference.Create "http://www.w3.org/1999/02/22-rdf-syntax-ns#")
            PrefixEntry("rdfs", UriReference.Create "http://www.w3.org/2000/01/rdf-schema#")
            PrefixEntry("semapv", UriReference.Create "https://w3id.org/semapv/vocab/")
            PrefixEntry("skos", UriReference.Create "http://www.w3.org/2004/02/skos/core#")
            PrefixEntry("sssom", UriReference.Create "https://w3id.org/sssom/")
            PrefixEntry("xsd", UriReference.Create "http://www.w3.org/2001/XMLSchema#")
        |]

    let private copyEntry (entry: PrefixEntry) =
        PrefixEntry(entry.PrefixName, UriReference.Create entry.PrefixUrl.Value)

    let private effectiveEntries (entries: PrefixEntry array) =
        let source = if isNull entries then [||] else entries

        Array.append builtIns source
        |> Array.rev
        |> Array.distinctBy (fun entry -> entry.PrefixName)
        |> Array.rev

    let private isUnambiguousAbsoluteIri (value: string) =
        value.Contains("://") || value.StartsWith("urn:")

    /// Gets fresh copies of the normative built-in SSSOM prefix entries.
    let builtInEntries () = builtIns |> Array.map copyEntry

    /// Returns true when the prefix is one of the normative built-ins.
    let isBuiltIn prefixName =
        builtIns |> Array.exists (fun entry -> entry.PrefixName = prefixName)

    /// Tries to expand a CURIE, returning absolute URI values unchanged.
    let tryExpand (entries: PrefixEntry array) (value: string) =
        if String.IsNullOrWhiteSpace value then
            None
        else
            let separator = value.IndexOf ':'

            if separator <= 0 || separator = value.Length - 1 then
                None
            else
                let prefixName = value.Substring(0, separator)
                let localName = value.Substring(separator + 1)

                effectiveEntries entries
                |> Array.tryFind (fun entry -> entry.PrefixName = prefixName)
                |> Option.map (fun entry -> entry.PrefixUrl.Value + localName)
                |> Option.orElseWith (fun () ->
                    if isUnambiguousAbsoluteIri value then Some value else None)

    /// Expands a CURIE or raises when its prefix is undeclared.
    let expand entries value =
        match tryExpand entries value with
        | Some expanded -> expanded
        | None -> invalidArg (nameof value) $"Cannot expand undeclared CURIE '{value}'."

    /// Tries to contract an IRI using the longest matching expansion.
    let tryContract (entries: PrefixEntry array) (value: string) =
        if String.IsNullOrWhiteSpace value then
            None
        else
            effectiveEntries entries
            |> Array.filter (fun entry -> value.StartsWith(entry.PrefixUrl.Value))
            |> Array.sortBy (fun entry -> -entry.PrefixUrl.Value.Length, entry.PrefixName)
            |> Array.tryHead
            |> Option.bind (fun entry ->
                let localName = value.Substring(entry.PrefixUrl.Value.Length)
                if localName = "" then None else Some(entry.PrefixName + ":" + localName))

    /// Contracts an IRI or raises when no prefix expansion matches it.
    let contract entries value =
        match tryContract entries value with
        | Some contracted -> contracted
        | None -> invalidArg (nameof value) $"Cannot contract IRI '{value}'."
