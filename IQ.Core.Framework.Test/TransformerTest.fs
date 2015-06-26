namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework

open System

module TestTransformations =
    
    [<Transformation>]
    let dateToString (x : DateTime) =
        x.ToShortDateString()

    [<Transformation>]
    let stringToDate x =
        x |> DateTime.Parse

    [<Transformation>]
    let uint32Toint64 (x : uint32) =
        int64(x)

    [<Transformation>]
    let int16ToInt64 (x : int16) =
        int64(x)


[<TestContainer>]
module TransformerTest =
    [<Test>]
    let ``Converted optional and non-optional values``() =
        Some(3) |> Transformer.convert (typeof<int>) |> Claim.equal (3 :> obj)
        Some(3) |> Transformer.convert (typeof<option<int>>) |> Claim.equal (Some(3) :> obj)
        3 |> Transformer.convert (typeof<option<int>>) |> Claim.equal (Some(3) :> obj)
        3 |> Transformer.convert (typeof<int>) |> Claim.equal (3 :> obj)

        //Convert option<int64> to int32
        Some(3L) |> Transformer.convert (typeof<int>) |> Claim.equal (3 :> obj)
        option<int>.None |> Transformer.convert typeof<int> |> Claim.isNull

    let transformer = DataConverterConfig([thisAssemblyElement().Name],None) |> Transformer.get

    [<Test>]
    let ``Discovered transformations``() =
        let transformer = DataConverterConfig([thisAssemblyElement().Name],None) |> Transformer.get
        let transformations  = transformer.GetKnownTransformations()
        
        let c1Info = funcinfo<@fun () -> TestTransformations.stringToDate@>
        let c1Id =  TransformationIdentifier.createDefaultT<DateTime,string>()
        c1Id |> Claim.inList transformations

        let c2Info = funcinfo<@fun () -> TestTransformations.dateToString@>
        let c2Id = TransformationIdentifier.createDefaultT<string,DateTime>()
        c2Id |> Claim.inList transformations

    [<Test>]
    let ``Executed transformations via untyped transformer``() =
        let val1 = DateTime(2015, 5, 15) :> obj
        let val2 = DateTime(2015, 5, 15).ToShortDateString() :> obj
        val1 |> transformer.Transform (typeof<string>) |> Claim.equal val2 
        val2 |> transformer.Transform (typeof<DateTime>) |> Claim.equal val1

        let x = [1u..1000u] |> List.map( fun x -> x :> obj) 
        let y = [1s..1000s] |> List.map( fun x -> x :> obj)
        let z = [1l..1000l] |> List.map( fun x -> x :> obj)
        x |> transformer.TransformMany typeof<int64> |> List.ofSeq |> Claim.equal z
        y |> transformer.TransformMany typeof<int64> |> List.ofSeq |> Claim.equal z
        ()

    [<Test>]
    let ``Executed transformations via typed transformer``() =
        let transformer = transformer :?> ITypedTransformer
        transformer.Transform(35u) |> Claim.equal 35L


        