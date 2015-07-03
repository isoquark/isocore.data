namespace IQ.Core.Framework.Test
open System

open XUnit

type LangTest(ctx,log) =
    inherit ProjectTestContainer(ctx,log)
    [<Fact>]
    let ``Determined whether values were option values``() =
        Some(3) |> Option.isOptionValue |> Claim.isTrue
        3 |> Option.isOptionValue |> Claim.isFalse
        null |> Option.isOptionValue |> Claim.isFalse
    
    [<Fact>]
    let ``Unwrapped option values``() =
        let x = Some(3)
        x |> Option.unwrapValue |>Option.get |> Claim.equal (3 :> obj)
        let y = option<int>.None
        y |> Option.unwrapValue |> Option.isNone |> Claim.isTrue
        
    [<Fact>]
    let ``Created union values via reflection``() =
        3 |> Option.makeSome |> Claim.equal (Some(3) :> obj)
        typeof<int> |> Option.makeNone |> Claim.equal (option<int>.None :> obj)

    [<Fact>]
    let ``Determined collection value type``() =
        [1;2;3].GetType() |> Type.getCollectionValueType |> Claim.equal (Some(typeof<int>))

    [<Fact>]
    let ``Formatted connection string``() =
        ["A"; "B"; "C"] |> ConnectionString |> ConnectionString.format |> Claim.equal "A;B;C"

    [<Fact>]
    let ``Parsed connection string``() =
        "A;B;C" |> ConnectionString.parse |> Claim.equal (ConnectionString["A"; "B"; "C"])


       
             