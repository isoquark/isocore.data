namespace IQ.Core.Framework.Test

open System
open System.Reflection
open System.Linq.Expressions

open IQ.Core.Data


module TestTransformations =
    
    [<Transformation>]
    let dateToString (x : BclDateTime) =
        x.ToShortDateString()

    [<Transformation>]
    let stringToDate x =
        x |> BclDateTime.Parse

    [<Transformation>]
    let uint32Toint64 (x : uint32) =
        int64(x)

    [<Transformation>]
    let int16ToInt64 (x : int16) =
        int64(x)
    
    [<Transformation>]
    let intToInt (x : int) =
        x

module Transformer =
    
    let getTransformerConfig() =
        let q = thisAssembly().AssemblyName |> FindAssemblyByName |> FindAssemblyElement |> List.singleton
        TransformerConfig(q, None)

    type LogicTests(ctx,log) = 
        inherit ProjectTestContainer(ctx,log)
    

        let transformer : ITransformer = getTransformerConfig() |>ctx.AppContext.Resolve
    
        [<Fact>]
        let ``Converted optional and non-optional values``() =
            Some(3) |> Transformer.convert (typeof<int>) |> Claim.equal (3 :> obj)
            Some(3) |> Transformer.convert (typeof<option<int>>) |> Claim.equal (Some(3) :> obj)
            3 |> Transformer.convert (typeof<option<int>>) |> Claim.equal (Some(3) :> obj)
            3 |> Transformer.convert (typeof<int>) |> Claim.equal (3 :> obj)
            Some(3L) |> Transformer.convert (typeof<int>) |> Claim.equal (3 :> obj)
            option<int>.None |> Transformer.convert typeof<int> |> Claim.isNull
            
        [<Fact>]
        let ``Discovered transformations``() =
            let transformations  = transformer.GetKnownTransformations()
        
            let c1Info = funcinfo<@fun () -> TestTransformations.stringToDate@>
            let c1Id =  TransformationIdentifier.createDefault<BclDateTime,string>()
            c1Id |> Claim.seqIn transformations

            let c2Info = funcinfo<@fun () -> TestTransformations.dateToString@>
            let c2Id = TransformationIdentifier.createDefault<string,BclDateTime>()
            c2Id |> Claim.seqIn transformations

        [<Fact>]
        let ``Executed transformations via untyped transformer``() =
            let val1 = BclDateTime(2015, 5, 15) :> obj
            let val2 = BclDateTime(2015, 5, 15).ToShortDateString() :> obj
            val1 |> transformer.Transform (typeof<string>) |> Claim.equal val2 
            val2 |> transformer.Transform (typeof<BclDateTime>) |> Claim.equal val1

            let x = [1u..4u] |> List.map( fun x -> x :> obj) 
            let y = [1s..4s] |> List.map( fun x -> x :> obj)
            let z = [1L..4L] |> List.map( fun x -> x :> obj)
            x |> transformer.TransformMany typeof<int64> |> List.ofSeq |> Claim.equal z
            y |> transformer.TransformMany typeof<int64> |> List.ofSeq |> Claim.equal z

        [<Fact>]
        let ``Executed transformations via typed transformer``() =
            let transformer = transformer :?> ITypedTransformer
            transformer.Transform(35u) |> Claim.equal 35L

    //The number of iterations to use for benchmarking
    let itcount = pown 10 6

    [<Category(Categories.Benchmark)>]
    type Benchmarks(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
    
        let transformer : ITransformer = getTransformerConfig() |>ctx.AppContext.Resolve

        [<Fact>]
        let ``Benchmark - Called Method Invoke 10^6 Times``() =
            let m = funcinfo<@fun () -> TestTransformations.intToInt@>

            let f() =
                for i in 0..itcount do
                    m.Invoke(null, [|i :> obj|]) |> ignore
                                   
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Called Method Directly 10^6 Times``() =
            let m = funcinfo<@fun () -> TestTransformations.intToInt@>
                    
            let f() =
                for i in 0..itcount do
                    Convert.ChangeType(i, typeof<int>) |> ignore
                       
            f |> Benchmark.capture ctx
    
        [<Fact>]
        let ``Benchmark - Executed Transformation Int32->Int32 with Transformer 10^6 Times``() =
                
            let f() =
                let dstType = typeof<int>
                for i in 0..itcount do
                    i |> transformer.Transform dstType |> ignore
        
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Executed Transformation Int32->Int32 with System Convert 10^6 Times``() =
                
            let f() =
                let dstType = typeof<int>
                for i in 0..itcount do
                    Convert.ChangeType(i, typeof<int>) |> ignore

            f |> Benchmark.capture ctx
        
        [<Fact>]
        let ``Benchmark - Executed Transformation Int32->Int32 Directly 10^6 Times``() =
                
            let f() =
                let dstType = typeof<int>
                for i in 0..itcount do
                    i |> TestTransformations.intToInt |> ignore

            f |> Benchmark.capture ctx
        

        