// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Synthetics.Test

open IQ.Core.Synthetics
open IQ.Core.Math


module NumberGeneration =

    let checkRange minValue maxValue values =
        values |> Seq.iter(fun value -> 
            value >= minValue |> Claim.isTrue
            value <= maxValue |> Claim.isTrue
            )        

    type LogicTests(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
    
        let generalProvider = ctx.AppContext.Resolve<IValueGeneratorProvider>()
        let specialProvider = ctx.AppContext.Resolve<INumberGeneratorProvider>()
        let calculator = Calculator.universal() 


        [<Fact>]
        let ``Generated distributed values - Int64 - Random Seed``() =
            let min = 100L
            let max = 500L
            let count = 50
            let gen1 = generalProvider.GetGenerator<int64>(min, max)
            let set1 = gen1.NextValues(count) |> List.ofSeq
            set1.Length |> Claim.equal count 
            set1 |> checkRange min max

            let gen2 = specialProvider.GetGenerator<int64>(min,max)
            let set2 = gen1.NextValues(count) |> List.ofSeq
            set1.Length |> Claim.equal count 
            set2 |> checkRange min max


        [<Fact>]
        let ``Generated distributed values - Int64 - Fixed Seed``() =
            let min = 100L
            let max = 500L
            let count = 50
            let seed = 101
            let gen1 = generalProvider.GetGenerator<int64>(min, max, seed)
            let set1 = gen1.NextValues(count) |> List.ofSeq
            let gen2 = generalProvider.GetGenerator<int64>(min, max, seed)
            let set2 = gen2.NextValues(count) |> List.ofSeq
            set1 |> Claim.equal set2
                        


