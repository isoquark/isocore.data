// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Math.Test

open System
open System.Diagnostics
open System.Linq

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Math

open MathNet.Numerics.Distributions

module Stats =
    
    [<Literal>]
    let opcount = 1000000
        
    let stats = MathServices.Stats()

    let runifd<'T>(count) =
        stats.runifd(count, Number<'T>.MinValue, Number<'T>.MaxValue) |> ignore
        
    let init() = 
        runifd<uint8>(1)
        runifd<int8>(1)
        runifd<int16>(1)
        runifd<uint16>(1)
        runifd<int32>(1)
        runifd<uint32>(1)
        runifd<int64>(1)
        runifd<uint64>(1)

        let d = DiscreteUniform(Number<int32>.MinValue, Number<int32>.MaxValue)
        d.Samples().Take(1) |> ignore


    [<Benchmark(opcount)>]
    type Benchmarks(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
       
        do
            init()

        [<Fact>]
        let ``Benchmark - Int32 - Uniform Distribution Sample - Baseline``() =            
            let f() =
                let d = DiscreteUniform(Number<int32>.MinValue, Number<int32>.MaxValue)
                d.Samples().Take(opcount) |> Seq.iter(fun x -> ())
            f |> Benchmark.capture ctx
        
        [<Fact>]
        let ``Benchmark - UInt8 - Uniform Distribution Sample``() =            
            (fun () -> runifd<uint8>(opcount)) |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int8 - Uniform Distribution Sample``() =            
            (fun () -> runifd<int8>(opcount)) |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int16 - Uniform Distribution Sample``() =            
            (fun () -> runifd<int16>(opcount)) |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt16 - Uniform Distribution Sample``() =            
            (fun () -> runifd<uint16>(opcount)) |> Benchmark.capture ctx


        [<Fact>]
        let ``Benchmark - Int32 - Uniform Distribution Sample``() = 
            (fun () -> runifd<int32>(opcount)) |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt32 - Uniform Distribution Sample``() =
            (fun () -> runifd<uint32>(opcount)) |> Benchmark.capture ctx


        [<Fact>]
        let ``Benchmark - UInt64 - Uniform Distribution Sample``() =
            (fun () -> runifd<uint64>(opcount)) |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int64 - Uniform Distribution Sample``() =
            (fun () -> runifd<int64>(opcount)) |> Benchmark.capture ctx

                


        

