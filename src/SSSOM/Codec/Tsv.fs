namespace SSSOM

open System
open System.Text

type internal ParsedTsvRow =
    {
        Cells: string array
        Line: int
    }

type internal ParsedTsv =
    {
        Header: string array
        Rows: ParsedTsvRow array
    }

module internal TextCodec =

    let normalizeLineEndings (value: string) =
        if isNull value then ""
        else value.Replace("\r\n", "\n").Replace("\r", "\n")

    let splitEmbedded (content: string) (diagnostics: ResizeArray<SssomDiagnostic>) =
        let normalized = normalizeLineEndings content

        let hasByteOrderMark = normalized.Length > 0 && normalized.[0] = '\uFEFF'

        if hasByteOrderMark then
            diagnostics.Add(
                Diagnostics.error
                    DiagnosticCodes.MetadataStructure
                    "SSSOM/TSV content must not start with a UTF-8 byte-order mark."
                    (Some 1)
                    (Some 1)
                    None
                    None
            )

        let source = if hasByteOrderMark then normalized.Substring(1) else normalized
        let lines = source.Split('\n')
        let rawYamlLines = ResizeArray<string>()
        let mutable index = 0

        while index < lines.Length && lines.[index].StartsWith("#") do
            let line = lines.[index]
            rawYamlLines.Add(line.Substring(1))
            index <- index + 1

        if rawYamlLines.Count = 0 then
            diagnostics.Add(
                Diagnostics.error
                    DiagnosticCodes.MetadataStructure
                    "Embedded SSSOM/TSV content must begin with a '#' metadata block."
                    (Some 1)
                    (Some 1)
                    None
                    None
            )

        if index >= lines.Length || (index = lines.Length - 1 && lines.[index] = "") then
            diagnostics.Add(
                Diagnostics.error
                    DiagnosticCodes.TsvParse
                    "The mappings block and its header row are missing."
                    (Some(index + 1))
                    None
                    None
                    None
            )

        let commonSpaces =
            rawYamlLines
            |> Seq.filter (String.IsNullOrWhiteSpace >> not)
            |> Seq.map (fun line -> line.Length - line.TrimStart(' ').Length)
            |> Seq.fold (fun current value ->
                match current with
                | Some minimum -> Some(min minimum value)
                | None -> Some value) None
            |> Option.defaultValue 0

        let yamlLines =
            rawYamlLines
            |> Seq.map (fun line ->
                if line.Length >= commonSpaces then line.Substring(commonSpaces) else "")

        String.Join("\n", yamlLines), String.Join("\n", lines.[index..]), index + 1

    let private parseRows (content: string) (firstLine: int) (diagnostics: ResizeArray<SssomDiagnostic>) =
        let source = normalizeLineEndings content
        let rows = ResizeArray<ParsedTsvRow>()
        let cells = ResizeArray<string>()
        let field = StringBuilder()
        let mutable index = 0
        let mutable line = firstLine
        let mutable rowLine = firstLine
        let mutable quoted = false
        let mutable afterQuote = false
        let mutable atFieldStart = true
        let mutable rowHasContent = false

        let finishField () =
            cells.Add(field.ToString())
            field.Clear() |> ignore
            atFieldStart <- true
            afterQuote <- false

        let finishRow () =
            finishField ()
            rows.Add({ Cells = cells.ToArray(); Line = rowLine })
            cells.Clear()
            rowHasContent <- false
            rowLine <- line + 1

        while index < source.Length do
            let current = source.[index]

            if quoted then
                if current = '"' then
                    if index + 1 < source.Length && source.[index + 1] = '"' then
                        field.Append('"') |> ignore
                        index <- index + 2
                    else
                        quoted <- false
                        afterQuote <- true
                        index <- index + 1
                else
                    field.Append(current) |> ignore
                    if current = '\n' then line <- line + 1
                    index <- index + 1
            elif afterQuote then
                match current with
                | '\t' ->
                    finishField ()
                    rowHasContent <- true
                    index <- index + 1
                | '\n' ->
                    finishRow ()
                    line <- line + 1
                    rowLine <- line
                    index <- index + 1
                | _ ->
                    diagnostics.Add(
                        Diagnostics.error
                            DiagnosticCodes.TsvParse
                            "Only a tab or line break may follow a closing quote in a TSV field."
                            (Some line)
                            None
                            None
                            None
                    )
                    field.Append(current) |> ignore
                    afterQuote <- false
                    atFieldStart <- false
                    rowHasContent <- true
                    index <- index + 1
            else
                match current with
                | '"' when atFieldStart ->
                    quoted <- true
                    atFieldStart <- false
                    rowHasContent <- true
                    index <- index + 1
                | '"' ->
                    diagnostics.Add(
                        Diagnostics.error
                            DiagnosticCodes.TsvParse
                            "A quote inside an unquoted TSV field is malformed."
                            (Some line)
                            None
                            None
                            None
                    )
                    field.Append(current) |> ignore
                    atFieldStart <- false
                    rowHasContent <- true
                    index <- index + 1
                | '\t' ->
                    finishField ()
                    rowHasContent <- true
                    index <- index + 1
                | '\n' ->
                    finishRow ()
                    line <- line + 1
                    rowLine <- line
                    index <- index + 1
                | _ ->
                    field.Append(current) |> ignore
                    atFieldStart <- false
                    rowHasContent <- true
                    index <- index + 1

        if quoted then
            diagnostics.Add(
                Diagnostics.error
                    DiagnosticCodes.TsvParse
                    "A quoted TSV field was not closed before the end of the document."
                    (Some rowLine)
                    None
                    None
                    None
            )

        if rowHasContent || cells.Count > 0 || field.Length > 0 || afterQuote then
            finishField ()
            rows.Add({ Cells = cells.ToArray(); Line = rowLine })

        rows.ToArray()

    let parseTsv (content: string) firstLine (diagnostics: ResizeArray<SssomDiagnostic>) =
        let rows = parseRows content firstLine diagnostics

        if rows.Length = 0 then
            diagnostics.Add(
                Diagnostics.error DiagnosticCodes.TsvParse "The TSV header row is missing." (Some firstLine) None None None
            )

            { Header = [||]; Rows = [||] }
        else
            let header = rows.[0].Cells

            if header.Length = 0 || (header.Length = 1 && String.IsNullOrWhiteSpace header.[0]) then
                diagnostics.Add(
                    Diagnostics.error DiagnosticCodes.TsvParse "The TSV header row is empty." (Some firstLine) None None None
                )

            header
            |> Array.countBy id
            |> Array.filter (fun (_, count) -> count > 1)
            |> Array.iter (fun (name, _) ->
                diagnostics.Add(
                    Diagnostics.error
                        DiagnosticCodes.DuplicateHeader
                        $"The TSV header contains duplicate column '{name}'."
                        (Some firstLine)
                        None
                        None
                        (Some name)
                ))

            let dataRows = rows.[1..]

            dataRows
            |> Array.iteri (fun rowIndex row ->
                if row.Cells.Length <> header.Length then
                    diagnostics.Add(
                        Diagnostics.error
                            DiagnosticCodes.RowWidth
                            $"Mapping row {rowIndex + 1} has {row.Cells.Length} cells but the header has {header.Length} columns."
                            (Some row.Line)
                            None
                            (Some(rowIndex + 1))
                            None
                    ))

            { Header = header; Rows = dataRows }

    let splitMultivalue version (value: string) =
        if value = "" then
            [||]
        elif version = SssomVersion.V1_0 then
            value.Split('|')
        else
            let values = ResizeArray<string>()
            let current = StringBuilder()
            let mutable index = 0

            while index < value.Length do
                match value.[index] with
                | '\\' when index + 1 < value.Length && (value.[index + 1] = '|' || value.[index + 1] = '\\') ->
                    current.Append(value.[index + 1]) |> ignore
                    index <- index + 2
                | '|' ->
                    values.Add(current.ToString())
                    current.Clear() |> ignore
                    index <- index + 1
                | character ->
                    current.Append(character) |> ignore
                    index <- index + 1

            values.Add(current.ToString())
            values.ToArray()

    let private escapeMultivalueItem version (value: string) =
        if version = SssomVersion.V1_0 then
            value
        else
            value.Replace("\\", "\\\\").Replace("|", "\\|")

    let joinMultivalue version (values: string array) =
        values
        |> Array.map (escapeMultivalueItem version)
        |> fun escaped -> String.Join("|", escaped)

    let quoteTsv (value: string) =
        if value.IndexOfAny([| '\t'; '\n'; '\r'; '"' |]) >= 0 then
            "\"" + value.Replace("\"", "\"\"") + "\""
        else
            value
