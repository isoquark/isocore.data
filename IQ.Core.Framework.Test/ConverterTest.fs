namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework

open System

[<TestContainer>]
module ConverterTest =
    [<Test>]
    let ``Converted optional and non-optional values``() =
        Some(3) |> Converter.convert (typeof<int>) |> Claim.equal (3 :> obj)
        Some(3) |> Converter.convert (typeof<option<int>>) |> Claim.equal (Some(3) :> obj)
        3 |> Converter.convert (typeof<option<int>>) |> Claim.equal (Some(3) :> obj)
        3 |> Converter.convert (typeof<int>) |> Claim.equal (3 :> obj)

        //Convert option<int64> to int32
        Some(3L) |> Converter.convert (typeof<int>) |> Claim.equal (3 :> obj)
        let value = option<int>.None |> Converter.convert typeof<int> 
        ()