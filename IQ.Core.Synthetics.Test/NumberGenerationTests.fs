// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Synthetics.Test

open IQ.Core.Synthetics
open IQ.Core.Math


module NumberGeneration =

    type LogicTests(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
    
        let generators = ctx.AppContext.Resolve<IValueGeneratorProvider>()
        let calculator = Calculator.universal() 

        [<Fact>]
        let ``Generated distributed values - Int64``() =
            let min = 100L
            let max = 500L
            let count = 50
            let generator = generators.GetGenerator<int64>(min, max)
            let values = generator.NextValues(50) |> List.ofSeq
            values.Length |> Claim.equal count 
            values |> Seq.iter(fun value ->
                (calculator.LessThanOrEqual(value,max) && calculator.GreaterThanOrEqual(value, min)) |> Claim.isTrue)


