namespace IQ.Core.Framework.Test

open IQ.Core.Framework
open IQ.Core.TestFramework

[<TestContainer>]
module LangTest =
    [<Test>]
    let ``Determined whether values were option values``() =
        Some(3) |> ClrOption.isOptionValue |> Claim.isTrue
        3 |> ClrOption.isOptionValue |> Claim.isFalse
        null |> ClrOption.isOptionValue |> Claim.isFalse
    
    [<Test>]
    let ``Unwrapped option values``() =
        let x = Some(3)
        x |> ClrOption.unwrapValue |>Option.get |> Claim.equal (3 :> obj)
        let y = option<int>.None
        y |> ClrOption.unwrapValue |> Option.isNone |> Claim.isTrue
        
    [<Test>]
    let ``Created union values via reflection``() =
        3 |> ClrOption.makeSome |> Claim.equal (Some(3) :> obj)
        typeof<int> |> ClrOption.makeNone |> Claim.equal (option<int>.None :> obj)

