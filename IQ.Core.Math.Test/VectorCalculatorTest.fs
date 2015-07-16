// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Math.Test

open System
open System.Diagnostics
open System.Linq

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Math

module VectorCalculator =
    
    [<Literal>]
    let opcount = 1000000

    [<Benchmark(opcount)>]
    type Benchmarks(ctx,log) =
        inherit ProjectTestContainer(ctx,log)

        [<Fact>]
        let ``Benchmark - Int32 - Vector Scalar Product - Baseline``() =            
            let f() =
                let x = [|2; 3; 4; 5|] |> System.Numerics.Vector
                let y = [|1; 2; 6; 2|] |> System.Numerics.Vector
                for i in 1..opcount do
                    x * y |> ignore

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int32 - Vector Scalar Product - CPP``() =            
            let calc = MathServices.VectorCalcs<int32>()

            let f() =
                let x = [|2; 3; 4; 5|] |> Vector
                let y = [|1; 2; 6; 2|] |> Vector
                for i in 1..opcount do
                    calc.Dot(x, y) |> ignore

            f |> Benchmark.capture ctx
        

        [<Fact>]
        let ``Benchmark - Int32 - Vector Scalar Product``() =            
            let f() =
                let x = [|2; 3; 4; 5|] |> Vector
                let y = [|1; 2; 6; 2|] |> Vector
                for i in 1..opcount do
                    VectorCalcs.dot x y |> ignore

            f |> Benchmark.capture ctx
                
            

