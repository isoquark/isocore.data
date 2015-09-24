// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework.Test

open System
open System.Collections.Generic

open IQ.Core.Framework


module Type =
    module A = 
        type internal B = class end

        module internal C = 
            type internal D = class end

            module internal E = 
                type internal F = class end
    
    type Class1() =
        //This will never show up in the IL
        let LetValue1 = 3
        //This will never show up in the IL
        [<Literal>]
        let LetValue2 = 4u
        //This will show up as a field that gets initialized in the constructor
        let message1 = "hello"

        [<Literal>]
        let message2 = "world"

        //auto-implemented property
        member val public Property1 = 5
        //auto-implemented property
        member val private Property2 = 6u
        //auto-implemented property
        member val internal Property3 = 6L

        member this.Property4 = message1

        member this.Property5 = message2

        [<DefaultValue>]
        val mutable public Field1 : int

    type Enum1 =
        | Field01 = 1
        | Field02 = 2

    type Enum2 =
        | Field01 = 1u
        | Field02 = 2u 
    

    type LogicTests(ctx,log) =
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
                        |> CollectionBuilder.create ClrCollectionKind.FSharpList typeof<int> 
                        :?> list<int>
            let expect = [1; 2; 3;]
            actual |> Claim.equal expect

        [<Fact>]
        let ``Created array via reflection``() =
            let actual = [1 :> obj;2:> obj; 3:> obj]   
                        |> CollectionBuilder.create ClrCollectionKind.Array typeof<int> 
                        :?> array<int>
            let expect = [|1; 2; 3|]
            actual |> Claim.equal expect

        [<Fact>]
        let ``Created generic list via reflection``() =
            let actual = [1 :> obj;2:> obj; 3:> obj]   
                        |> CollectionBuilder.create ClrCollectionKind.GenericList typeof<int>
                        :?> List<int>
            let expect = List<int>([1;2;3])
            actual.[0] |> Claim.equal expect.[0]
            actual.[1] |> Claim.equal expect.[1]
            actual.[2] |> Claim.equal expect.[2]
       
        
        [<Fact>]
        let ``Discovered class field facets``() =
            let f1 = fieldinfo<@fun (t : Class1) -> t.Field1@>
            f1.Facets |> Claim.equal FieldFacetSet.Blank

        [<Fact>]
        let ``Discovered enumeration field facets``() =
            let fields = fields<Enum1>            
           
            let f1 = fields |> List.find(fun f -> f.MemberName = Enum1.Field01.MemberName)
            let f1FacetsExpect = {FieldFacetSet.Blank with IsStatic = true; IsLiteral = true; HasDefault = true} 
            let f1FacetsActual = f1.Facets
            Claim.equal f1FacetsExpect f1FacetsActual
            
            
            


