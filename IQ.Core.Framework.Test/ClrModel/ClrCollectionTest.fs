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
        [1;2;3].GetType() |> ClrType.getKind|> Claim.equal ClrTypeKind.Collection                    
        [|1;2;3|].GetType() |> ClrType.getKind|> Claim.equal ClrTypeKind.Collection

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

    [<Test>]
    let ``Created F# list via reflection``() =
        let actual = [1 :> obj;2:> obj; 3:> obj]   
                   |> ClrCollection.create ClrCollectionKind.FSharpList typeof<int> 
                   :?> list<int>
        let expect = [1; 2; 3;]
        actual |> Claim.equal expect

    [<Test>]
    let ``Created array via reflection``() =
        let actual = [1 :> obj;2:> obj; 3:> obj]   
                   |> ClrCollection.create ClrCollectionKind.Array typeof<int> 
                   :?> array<int>
        let expect = [|1; 2; 3|]
        actual |> Claim.equal expect

    [<Test>]
    let ``Created generic list via reflection``() =
        let actual = [1 :> obj;2:> obj; 3:> obj]   
                   |> ClrCollection.create ClrCollectionKind.GenericList typeof<int>
                   :?> List<int>
        let expect = List<int>([1;2;3])
        actual.[0] |> Claim.equal expect.[0]
        actual.[1] |> Claim.equal expect.[1]
        actual.[2] |> Claim.equal expect.[2]