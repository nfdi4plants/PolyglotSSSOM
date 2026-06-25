module Main

open Fable.Pyxpecto

let allTests = 
    testList "All SSSOM Tests" [
        SSSOM.Tests.DecodeMappingTests.tests
        SSSOM.Tests.DecodeMappingSetTests.tests
        SSSOM.Tests.EncodeMappingTests.tests
        SSSOM.Tests.EncodeMappingSetTests.tests
        SSSOM.Tests.DecodeSssomDocumentTests.tests
        SSSOM.Tests.EncodeSssomDocumentTests.tests
    ]

#if !FABLE_COMPILER_JAVASCRIPT && !FABLE_COMPILER_TYPESCRIPT
let (!!) (any: 'a) = any
#endif
#if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
open Fable.Core.JsInterop
#endif

[<EntryPoint>]
let main argv = !!Pyxpecto.runTests [||] allTests
