namespace SSSOM

open Fable.Core

/// Provides strict SSSOM YAML-metadata-plus-TSV decoding, validation, and canonical encoding.
[<AbstractClass; Sealed; AttachMembers>]
type SssomCodec private () =

    static member private DecodeFailure(ex: System.Exception) =
        DecodeResult(
            None,
            [| Diagnostics.error
                   DiagnosticCodes.TsvParse
                   $"SSSOM decoding failed unexpectedly: {ex.Message}"
                   None
                   None
                   None
                   None |]
        )

    static member private ThrowDecode(result: DecodeResult) =
        match result.Document with
        | Some document when result.IsSuccess -> document
        | _ -> raise (SssomCodecException("SSSOM decoding failed.", result.Diagnostics))

    static member private ThrowEncode(result: EncodeResult) =
        match result.Content with
        | Some content when result.IsSuccess -> content
        | _ -> raise (SssomCodecException("SSSOM canonical encoding failed.", result.Diagnostics))

    /// Decodes an embedded YAML-metadata-plus-TSV document or raises SssomCodecException.
    static member DecodeEmbedded(content: string) =
        SssomCodec.TryDecodeEmbedded content |> SssomCodec.ThrowDecode

    /// Decodes an embedded YAML-metadata-plus-TSV document without throwing for document errors.
    static member TryDecodeEmbedded(content: string) =
        try Decoder.embedded content with ex -> SssomCodec.DecodeFailure ex

    /// Decodes separate YAML metadata and TSV mappings or raises SssomCodecException.
    static member DecodeExternal(metadataContent: string, tsvContent: string) =
        SssomCodec.TryDecodeExternal(metadataContent, tsvContent) |> SssomCodec.ThrowDecode

    /// Decodes separate YAML metadata and TSV mappings without throwing for document errors.
    static member TryDecodeExternal(metadataContent: string, tsvContent: string) =
        try Decoder.externalMetadata metadataContent tsvContent with ex -> SssomCodec.DecodeFailure ex

    /// Encodes a document as canonical embedded SSSOM/TSV or raises SssomCodecException.
    static member EncodeCanonical(document: SssomDocument) =
        Encoder.tryEncode document |> SssomCodec.ThrowEncode

    /// Encodes a document as canonical embedded SSSOM/TSV without throwing for document errors.
    static member TryEncodeCanonical(document: SssomDocument) =
        Encoder.tryEncode document

    /// Validates a document using its declared or lowest required supported SSSOM version.
    static member Validate(document: SssomDocument) =
        Validation.validateForPublicApi document
