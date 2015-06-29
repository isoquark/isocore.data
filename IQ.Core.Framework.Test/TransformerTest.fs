namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework

open System
open System.Reflection
open System.Linq.Expressions


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
    
    [<Transformation>]
    let intToInt (x : int) =
        x
               

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
        let c1Id =  TransformationIdentifier.createDefault<DateTime,string>()
        c1Id |> Claim.inList transformations

        let c2Info = funcinfo<@fun () -> TestTransformations.dateToString@>
        let c2Id = TransformationIdentifier.createDefault<string,DateTime>()
        c2Id |> Claim.inList transformations

    [<Test>]
    let ``Executed transformations via untyped transformer``() =
        let val1 = DateTime(2015, 5, 15) :> obj
        let val2 = DateTime(2015, 5, 15).ToShortDateString() :> obj
        val1 |> transformer.Transform (typeof<string>) |> Claim.equal val2 
        val2 |> transformer.Transform (typeof<DateTime>) |> Claim.equal val1

        let x = [1u..15u] |> List.map( fun x -> x :> obj) 
        let y = [1s..15s] |> List.map( fun x -> x :> obj)
        let z = [1l..15l] |> List.map( fun x -> x :> obj)
        x |> transformer.TransformMany typeof<int64> |> List.ofSeq |> Claim.equal z
        y |> transformer.TransformMany typeof<int64> |> List.ofSeq |> Claim.equal z

    [<Test>]
    let ``Executed transformations via typed transformer``() =
        let transformer = transformer :?> ITypedTransformer
        transformer.Transform(35u) |> Claim.equal 35L


    [<Test; BenchmarkTrait>]
    let ``Executed delegate invocation benchmarks``() =
        let m = funcinfo<@fun () -> TestTransformations.intToInt@>
        let count = pown 10 6


        let bm1() =
            for i in 0..count do
                m.Invoke(null, [|i :> obj|]) |> ignore
            
        let bm2() =
            for i in 0..count do
                Convert.ChangeType(i, typeof<int>) |> ignore
                       
        let bm1Result = Benchmark.run "Delegate Invocation 1" bm1
        let bm2Result = Benchmark.run "Baseline Comparision" bm2
                       
        ()

    [<Test; BenchmarkTrait>]
    let ``Executed transformation benchmarks``() =
                
        let count = pown 10 6
        let bm1() =
            let dstType = typeof<int>
            for i in 0..count do
                i |> transformer.Transform dstType |> ignore
        
        let bm2() =
            let dstType = typeof<int>
            for i in 0..count do
                Convert.ChangeType(i, typeof<int>) |> ignore

        let bm3() =
            let dstType = typeof<int>
            for i in 0..count do
                i |> TestTransformations.intToInt |> ignore

        let bm1Result = Benchmark.run "Transform1" bm1 
        let bm2Result = Benchmark.run "Transform2" bm2
        let bm3Result = Benchmark.run "Transform2" bm3
        ()

        
        

        