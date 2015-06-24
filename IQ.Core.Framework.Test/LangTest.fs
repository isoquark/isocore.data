namespace IQ.Core.Framework.Test

open IQ.Core.Framework
open IQ.Core.TestFramework

[<TestContainer>]
module LangTest =
    [<Test>]
    let ``Determined whether values were option values``() =
        Some(3) |> Option.isOptionValue |> Claim.isTrue
        3 |> Option.isOptionValue |> Claim.isFalse
        null |> Option.isOptionValue |> Claim.isFalse
    
    [<Test>]
    let ``Unwrapped option values``() =
        let x = Some(3)
        x |> Option.unwrapValue |>Option.get |> Claim.equal (3 :> obj)
        let y = option<int>.None
        y |> Option.unwrapValue |> Option.isNone |> Claim.isTrue
        
    [<Test>]
    let ``Created union values via reflection``() =
        3 |> Option.makeSome |> Claim.equal (Some(3) :> obj)
        typeof<int> |> Option.makeNone |> Claim.equal (option<int>.None :> obj)


        