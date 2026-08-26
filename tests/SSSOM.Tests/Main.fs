module Main

open Fable.Pyxpecto

let allTests = 
    testList "All SSSOM Tests" [
        SSSOM.Tests.DomainTests.tests
        SSSOM.Tests.CodecTests.tests
    ]

#if !FABLE_COMPILER_JAVASCRIPT && !FABLE_COMPILER_TYPESCRIPT
let (!!) (any: 'a) = any
#endif
#if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
open Fable.Core.JsInterop
#endif

[<EntryPoint>]
let main argv = !!Pyxpecto.runTests [||] allTests
