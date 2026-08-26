namespace SSSOM

open System
open Fable.Core

/// Identifies whether a codec diagnostic prevents a result from being returned.
[<RequireQualifiedAccess>]
type DiagnosticSeverity =
    | Warning
    | Error

/// Describes one portable validation, decoding, or encoding diagnostic.
[<AttachMembers>]
type SssomDiagnostic(
    severity: DiagnosticSeverity,
    code: string,
    message: string,
    ?line: int,
    ?column: int,
    ?row: int,
    ?slot: string
) =

    /// Gets the diagnostic severity.
    member _.Severity = severity

    /// Gets the stable machine-readable diagnostic code.
    member _.Code = code

    /// Gets the human-readable diagnostic message.
    member _.Message = message

    /// Gets the one-based source line, when available.
    member _.Line = line

    /// Gets the one-based source column, when available.
    member _.Column = column

    /// Gets the one-based mapping row, excluding the header, when available.
    member _.Row = row

    /// Gets the SSSOM slot name associated with the diagnostic, when available.
    member _.Slot = slot

/// Contains the non-throwing result of a decode operation.
[<AttachMembers>]
type DecodeResult(document: SssomDocument option, diagnostics: SssomDiagnostic array) =
    let diagnostics = ModelValue.nonNullArray diagnostics

    /// Gets the decoded document when no errors occurred.
    member _.Document = document

    /// Gets all errors and warnings produced by the operation.
    member _.Diagnostics = diagnostics

    /// Gets whether a document was produced without error diagnostics.
    member _.IsSuccess =
        document.IsSome
        && diagnostics
           |> Array.exists (fun diagnostic -> diagnostic.Severity = DiagnosticSeverity.Error)
           |> not

/// Contains the non-throwing result of a canonical encode operation.
[<AttachMembers>]
type EncodeResult(content: string option, diagnostics: SssomDiagnostic array) =
    let diagnostics = ModelValue.nonNullArray diagnostics

    /// Gets canonical embedded SSSOM/TSV content when no errors occurred.
    member _.Content = content

    /// Gets all errors and warnings produced by the operation.
    member _.Diagnostics = diagnostics

    /// Gets whether content was produced without error diagnostics.
    member _.IsSuccess =
        content.IsSome
        && diagnostics
           |> Array.exists (fun diagnostic -> diagnostic.Severity = DiagnosticSeverity.Error)
           |> not

/// Raised by throwing codec operations when diagnostics contain an error.
[<AttachMembers>]
type SssomCodecException(message: string, diagnostics: SssomDiagnostic array) =
    inherit Exception(message)

    let diagnostics = ModelValue.nonNullArray diagnostics

    /// Gets the structured diagnostics that caused the operation to fail.
    member _.Diagnostics = diagnostics

module internal DiagnosticCodes =
    [<Literal>]
    let YamlParse = "SSSOM_YAML_PARSE"

    [<Literal>]
    let MetadataStructure = "SSSOM_METADATA_STRUCTURE"

    [<Literal>]
    let DuplicateMetadata = "SSSOM_DUPLICATE_METADATA"

    [<Literal>]
    let MissingRequired = "SSSOM_MISSING_REQUIRED"

    [<Literal>]
    let TsvParse = "SSSOM_TSV_PARSE"

    [<Literal>]
    let DuplicateHeader = "SSSOM_DUPLICATE_HEADER"

    [<Literal>]
    let RowWidth = "SSSOM_ROW_WIDTH"

    [<Literal>]
    let UnknownSlot = "SSSOM_UNKNOWN_SLOT"

    [<Literal>]
    let InvalidValue = "SSSOM_INVALID_VALUE"

    [<Literal>]
    let UnsupportedVersion = "SSSOM_UNSUPPORTED_VERSION"

    [<Literal>]
    let VersionConflict = "SSSOM_VERSION_CONFLICT"

    [<Literal>]
    let CuriePrefix = "SSSOM_CURIE_PREFIX"

    [<Literal>]
    let ConditionalRequirement = "SSSOM_CONDITIONAL_REQUIREMENT"

    [<Literal>]
    let RecordId = "SSSOM_RECORD_ID"

    [<Literal>]
    let ExtensionDefinition = "SSSOM_EXTENSION_DEFINITION"

    [<Literal>]
    let ExtensionValue = "SSSOM_EXTENSION_VALUE"

    [<Literal>]
    let Cardinality = "SSSOM_CARDINALITY"

module internal Diagnostics =

    let create severity code message line column row slot =
        SssomDiagnostic(severity, code, message, ?line = line, ?column = column, ?row = row, ?slot = slot)

    let error code message line column row slot =
        create DiagnosticSeverity.Error code message line column row slot

    let warning code message line column row slot =
        create DiagnosticSeverity.Warning code message line column row slot

    let hasErrors (diagnostics: seq<SssomDiagnostic>) =
        diagnostics |> Seq.exists (fun diagnostic -> diagnostic.Severity = DiagnosticSeverity.Error)
