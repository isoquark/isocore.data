namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Generic

[<TestContainer>]
module ClrCollectionTest =

    [<Test>]
    let ``Determined whether type is a collection type``() =
        [1;2;3].GetType() |> ClrType.isCollectionType|> Claim.isTrue                      
        [|1;2;3|].GetType() |> ClrType.isCollectionType |> Claim.isTrue

    [<Test>]
    let ``Determined collection kind``() =
         [1;2;3].GetType() |>ClrType.getCollectionKind |> Claim.equal ClrCollectionKind.FSharpList
         [|1;2;3|].GetType() |>ClrType.getCollectionKind |> Claim.equal ClrCollectionKind.Array
         Some([|1;2;3|]).GetType()   |> ClrType.getCollectionKind |> Claim.equal ClrCollectionKind.Array


    [<Test>]
    let ``Determined collection value type``() =
        [1;2;3].GetType() |> ClrType.getCollectionValueType |> Claim.equal (Some(typeof<int>))


    [<Test>]
    let ``Referenced collection type``() =
        let ref1 = [1;2;3].GetType() |> ClrType.reference
        ref1.Type |> Claim.equal (typeof<list<int>>)
