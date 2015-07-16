// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Etl.Test

open System


open IQ.Core.Etl
open IQ.Core.Framework.Test


type EtlContext() = class end
    

module Join =
    let cross s0 s1 =
        seq {
                for e0 in s0 do
                    for e1 in s1 do
                        yield e0 , e1
        }

            
module EtlVocabulary =

    type LogicTests(ctx,log) =
        inherit ProjectTestContainer(ctx,log)

        [<Fact>]
        let ``Applied CrossJoin``() =
            let s0 = [1; 2;]
            let s1 = ['A'; 'B'; 'D']
            s1 |> Join.cross s0 |> List.ofSeq |> Claim.equal [(1, 'A'); (1,'B'); (1, 'D'); (2, 'A'); (2,'B'); (2, 'D')]

        
    

