// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework.Test

open System

open IQ.Core.Framework

module RecordValue =

    type private RecordA = {
        FieldA1 : int
        FieldA2 : string
        FieldA3 : decimal
    }

    type private RecordB = {
        FieldB1 : int
        FieldB2 : decimal
        FieldB3 : BclDateTime option    
    }

    let private FieldA1Name = propname<@ fun (x : RecordA) -> x.FieldA1 @>
    let private FieldA2Name = propname<@ fun (x : RecordA) -> x.FieldA2 @>
    let private FieldA3Name = propname<@ fun (x : RecordA) -> x.FieldA3 @>
    
    let private FieldB1Name = propname<@ fun (x : RecordB) -> x.FieldB1 @>
    let private FieldB2Name = propname<@ fun (x : RecordB) -> x.FieldB2 @>
    let private FieldB3Name = propname<@ fun (x : RecordB) -> x.FieldB3 @>

    type Tests(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
     
        let converter =  PocoConverter.getDefault()

        [<Fact>]
        let ``Created record value from array - No optional fields``() =
            let expect = {        
                FieldA1 = 15
                FieldA2 = "Hello"
                FieldA3 = 14.1m
            }
            
            let values = [|expect.FieldA1 :> obj; expect.FieldA2 :> obj; expect.FieldA3 :> obj|]
            let actual = converter.FromValueArray(values, typeof<RecordA>) :?> RecordA
            actual |> Claim.equal expect

        [<Fact>]
        let ``Created value array from record - No optional fields``() =
            let recordValue = {
                FieldA1 = 16
                FieldA2 = "Hello"
                FieldA3 = 38.12m
            }
            let valueArray = recordValue |> converter.ToValueArray
            Claim.equal (recordValue.FieldA1 :> obj) valueArray.[0]
            Claim.equal (recordValue.FieldA2 :> obj) valueArray.[1]
            Claim.equal (recordValue.FieldA3 :> obj) valueArray.[2]
    
        [<Fact>]
        let ``Created record from value map - No optional fields``() =
            let valueMap = 
                [(FieldA1Name.Text, 0, 16 :> obj)
                 (FieldA2Name.Text, 1, "Hello" :> obj)
                 (FieldA3Name.Text, 2, 38.12m :> obj)
                ]|> ValueIndex.create
        
            let expect = {
                FieldA1 = 16
                FieldA2 = "Hello"
                FieldA3 = 38.12m
            }
            let actual = converter.FromValueIndex(valueMap, typeof<RecordA>) :?> RecordA
            
            actual |> Claim.equal expect


        [<Fact>]
        let ``Created record from value map - Optional fields``() =
            let valueMap1 = 
                [(FieldB1Name.Text, 0, 16 :> obj)
                 (FieldB2Name.Text, 1, 38.12m :> obj)
                 (FieldB3Name.Text, 2, BclDateTime(2015, 5, 6) |> Some :> obj)
                ]|> ValueIndex.create
        
            let expect1 = {
                FieldB1 = 16
                FieldB2 = 38.12m
                FieldB3 = BclDateTime(2015, 5, 6) |> Some
            }
            let actual1 = converter.FromValueIndex(valueMap1, typeof<RecordB>) :?> RecordB            
            actual1 |> Claim.equal expect1

            let valueMap2 = 
                [(FieldB1Name.Text, 0, 16 :> obj)
                 (FieldB2Name.Text, 1, 38.12m :> obj)
                 (FieldB3Name.Text, 2, option<BclDateTime>.None :> obj)
                ]|> ValueIndex.create
        
            let expect2 = {
                FieldB1 = 16
                FieldB2 = 38.12m
                FieldB3 = None
            }
            let actual2 = converter.FromValueIndex(valueMap2, typeof<RecordB>) :?> RecordB            
            actual2 |> Claim.equal expect2

        [<Fact>]
        let ``Create value map from record - Optional fields``() =
            let value1 = {
                FieldB1 = 16
                FieldB2 = 38.12m
                FieldB3 = Some(BclDateTime(2015, 5, 6))
            }
            let expect1 = 
                [
                (FieldB1Name.Text, 0, value1.FieldB1 :> obj)
                (FieldB2Name.Text, 1, value1.FieldB2 :> obj)
                (FieldB3Name.Text, 2, value1.FieldB3:> obj)
                ] |> ValueIndex.create

            let actual1 = value1 |> converter.ToValueIndex
            actual1 |> Claim.equal expect1

            let value2 = {
                FieldB1 = 16
                FieldB2 = 38.12m
                FieldB3 = None
            }
            let expect2 = 
                [
                (FieldB1Name.Text, 0, value1.FieldB1 :> obj)
                (FieldB2Name.Text, 1, value1.FieldB2 :> obj)
                (FieldB3Name.Text, 2, value1.FieldB3:> obj)
                ] |> ValueIndex.create

            let actual2 = value1 |> converter.ToValueIndex
            actual2 |> Claim.equal expect2
