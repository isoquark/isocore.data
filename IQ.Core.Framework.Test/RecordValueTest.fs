namespace IQ.Core.Framework.Test

open System


[<TestContainer>]
module RecordValueTest =

    type private RecordA = {
        FieldA1 : int
        FieldA2 : string
        FieldA3 : decimal
    }
    let private FieldA1Name = propname<@ fun (x : RecordA) -> x.FieldA1 @>
    let private FieldA2Name = propname<@ fun (x : RecordA) -> x.FieldA2 @>
    let private FieldA3Name = propname<@ fun (x : RecordA) -> x.FieldA3 @>
    
    type private RecordB = {
        FieldB1 : int
        FieldB2 : decimal
        FieldB3 : BclDateTime option    
    }
    let private FieldB1Name = propname<@ fun (x : RecordB) -> x.FieldB1 @>
    let private FieldB2Name = propname<@ fun (x : RecordB) -> x.FieldB2 @>
    let private FieldB3Name = propname<@ fun (x : RecordB) -> x.FieldB3 @>

    [<Fact>]
    let ``Created record value from array - No optional fields``() =
        let expect = {        
            FieldA1 = 15
            FieldA2 = "Hello"
            FieldA3 = 14.1m
        }
        let actual = typeof<RecordA> 
                  |> RecordValue.fromValueArray [|expect.FieldA1 :> obj; expect.FieldA2 :> obj; expect.FieldA3 :> obj|]
                  :?> RecordA
        actual |> Claim.equal expect

    [<Fact>]
    let ``Created value array from record - No optional fields``() =
        let recordValue = {
            FieldA1 = 16
            FieldA2 = "Hello"
            FieldA3 = 38.12m
        }
        let valueArray = recordValue |> RecordValue.toValueArray
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
        let actual =  typeof<RecordA> |> RecordValue.fromValueIndex valueMap :?> RecordA
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
        let actual1 =  typeof<RecordB> |> RecordValue.fromValueIndex valueMap1 :?> RecordB
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
        let actual2 =  typeof<RecordB> |> RecordValue.fromValueIndex valueMap2 :?> RecordB
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

        let actual1 = value1 |> RecordValue.toValueIndex 
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

        let actual2 = value1 |> RecordValue.toValueIndex 
        actual2 |> Claim.equal expect2
