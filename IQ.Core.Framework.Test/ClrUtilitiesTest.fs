namespace IQ.Core.Framework.Test

open System
open System.Collections.Generic


module TypeTestTypes =    
    module A = 
        type internal B = class end

        module internal C = 
            type internal D = class end

            module internal E = 
                type internal F = class end

open TypeTestTypes
type TypeTest(ctx,log) =
    inherit ProjectTestContainer(ctx,log)
    [<Fact>]
    let ``Discovered nested types``() =
        let actualA = typeof<A.B>.DeclaringType |> Type.getNestedTypes
        let expectA = [typeof<A.B>; typeof<A.C.D>.DeclaringType] 
        actualA |> Claim.equal expectA

    [<Fact>]
    let ``Loaded type from name``() =
        ClrTypeName("B", typeof<A.B>.FullName |> Some, None) |> Type.fromName |> Claim.equal typeof<A.B>

    [<Fact>]
    let ``Determined the item value type of a type``() =        
        typeof<List<string>>.ItemValueType |> Claim.equal typeof<string>
        typeof<option<string>>.ItemValueType |> Claim.equal typeof<string>
        typeof<option<List<string>>>.ItemValueType |> Claim.equal typeof<string>
        typeof<string>.ItemValueType |> Claim.equal typeof<string>

    [<Fact>]
    let ``Classified types as collection types``() =
        [1;2;3].GetType().Kind |> Claim.equal ClrTypeKind.Collection                    
        [|1;2;3|].GetType().Kind |> Claim.equal ClrTypeKind.Collection

    [<Fact>]
    let ``Classified collection types``() =
            [1;2;3].GetType() |>Type.getCollectionKind |> Claim.equal ClrCollectionKind.FSharpList
            [|1;2;3|].GetType() |>Type.getCollectionKind |> Claim.equal ClrCollectionKind.Array
            Some([|1;2;3|]).GetType()   |> Type.getCollectionKind |> Claim.equal ClrCollectionKind.Array

    [<Fact>]
    let ``Created F# list via reflection``() =
        let actual = [1 :> obj;2:> obj; 3:> obj]   
                    |> Collection.create ClrCollectionKind.FSharpList typeof<int> 
                    :?> list<int>
        let expect = [1; 2; 3;]
        actual |> Claim.equal expect

    [<Fact>]
    let ``Created array via reflection``() =
        let actual = [1 :> obj;2:> obj; 3:> obj]   
                    |> Collection.create ClrCollectionKind.Array typeof<int> 
                    :?> array<int>
        let expect = [|1; 2; 3|]
        actual |> Claim.equal expect

    [<Fact>]
    let ``Created generic list via reflection``() =
        let actual = [1 :> obj;2:> obj; 3:> obj]   
                    |> Collection.create ClrCollectionKind.GenericList typeof<int>
                    :?> List<int>
        let expect = List<int>([1;2;3])
        actual.[0] |> Claim.equal expect.[0]
        actual.[1] |> Claim.equal expect.[1]
        actual.[2] |> Claim.equal expect.[2]



