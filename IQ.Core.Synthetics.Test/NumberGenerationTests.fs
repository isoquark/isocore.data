// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Synthetics.Test

open IQ.Core.Synthetics


module NumberGeneration =

    type LogicTests(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
    
        let generators = ctx.AppContext.Resolve<IValueGeneratorProvider>()

        [<Fact>]
        let ``Generated distributed values - Int64``() =
            let generator = generators.GetGenerator<int64>()
            let values = generator.NextValues(50) |> List.ofSeq
            Claim.equal 50 values.Length
            ()

