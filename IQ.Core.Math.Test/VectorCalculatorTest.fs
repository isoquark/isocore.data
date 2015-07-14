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

        
            

