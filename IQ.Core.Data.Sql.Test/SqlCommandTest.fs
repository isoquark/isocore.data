// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql.Test

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Sql
open IQ.Core.Framework

module SqlCommandTest =

    type Tests(ctx, log) = 
        inherit ProjectTestContainer(ctx,log)
    
        let store = ctx.Store

        let allocateRange seq count = 
            (seq , count) 
            |> AllocateSequenceRange 
            |> store.ExecuteCommand
            |> fun x -> match x with | AllocateSequenceRangeResult i -> i :?> int |_ -> nosupport()
            

        [<Fact>]
        let ``Allocated sequence range``() =
            let seq = DataObjectName("SqlTest", "Seq01")
            let count = 5
            let startA = allocateRange seq count
            let startB = allocateRange seq count
            Claim.equal count (startB - startA)
            
            
           

