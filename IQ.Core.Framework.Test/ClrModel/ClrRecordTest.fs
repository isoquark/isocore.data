namespace IQ.Core.Framework.Test

open IQ.Core.Framework
open IQ.Core.TestFramework


open System
open System.Reflection

[<TestContainer>]
module ClrRecordTest =
    type private RecordA = {
        AField01 : int
        AField02 : decimal
        AField03 : DateTime
    }
    let private AField01Name = propname<@ fun (x : RecordA) -> x.AField01 @>
    let private AField02Name = propname<@ fun (x : RecordA) -> x.AField02 @>
    let private AField03Name = propname<@ fun (x : RecordA) -> x.AField03 @>

    type private RecordB = {
        BField01 : int
        BField02 : decimal
        BField03 : DateTime option    
    }
    let private BField01Name = propname<@ fun (x : RecordB) -> x.BField01 @>
    let private BField02Name = propname<@ fun (x : RecordB) -> x.BField02 @>
    let private BField03Name = propname<@ fun (x : RecordB) -> x.BField03 @>
    
    [<Test>]
    let ``Discovered record metadata - with no optional fields``() =
        let info = typeof<RecordA> |> ClrRecord.describe
        info.Fields.Length |> Claim.equal 3  
        info.Fields.[0].Name |> Claim.equal AField01Name
        info.Fields.[0].FieldType |> Claim.equal typeof<int>
        info.Fields.[0].Position |> Claim.equal 0

    [<Test>]
    let ``Discovered record metadata - with optional fields``() =
        let info = recordinfo<RecordB>
        info.Fields.Length |> Claim.equal 3  
        
        info.Fields.[0].Name |> Claim.equal BField01Name
        info.Fields.[0].FieldType |> Claim.equal typeof<int>
        info.Fields.[0].Position |> Claim.equal 0

        info.Fields.[1].Name |> Claim.equal BField02Name
        info.Fields.[1].FieldType |> Claim.equal typeof<decimal>
        info.Fields.[1].Position |> Claim.equal 1

        info.Fields.[2].Name |> Claim.equal BField03Name
        info.Fields.[2].FieldType |> Claim.equal typeof<option<DateTime>>
        info.Fields.[2].Position |> Claim.equal 2



    [<Test>]
    let ``Created value map from record - without optional fields``() =
        let value = {
            AField01 = 16
            AField02 = 38.12m
            AField03 = DateTime(2015, 5, 6)
        }
        let expect = 
            [
            (AField01Name, value.AField01 :> obj)
            (AField02Name, value.AField02 :> obj)
            (AField03Name, value.AField03:> obj)
            ] |> ValueMap.fromNamedItems
        let actual = value |> ClrRecord.toValueMap 
        actual |> Claim.equal expect

    [<Test>]
    let ``Create value map from record - with optional fields``() =
        let value1 = {
            BField01 = 16
            BField02 = 38.12m
            BField03 = Some(DateTime(2015, 5, 6))
        }
        let expect1 = 
            [
            (BField01Name, value1.BField01 :> obj)
            (BField02Name, value1.BField02 :> obj)
            (BField03Name, value1.BField03:> obj)
            ] |> ValueMap.fromNamedItems

        let actual1 = value1 |> ClrRecord.toValueMap 
        actual1 |> Claim.equal expect1

        let value2 = {
            BField01 = 16
            BField02 = 38.12m
            BField03 = None
        }
        let expect2 = 
            [
            (BField01Name, value1.BField01 :> obj)
            (BField02Name, value1.BField02 :> obj)
            (BField03Name, value1.BField03:> obj)
            ] |> ValueMap.fromNamedItems

        let actual2 = value1 |> ClrRecord.toValueMap 
        actual2 |> Claim.equal expect2

    
    [<Test>]
    let ``Created record from value map - with no optional fields``() =
        let valueMap = 
            [(AField01Name, 16 :> obj)
             (AField02Name, 38.12m :> obj)
             (AField03Name, DateTime(2015, 5, 6) :> obj)
            ]|> ValueMap.fromNamedItems
        
        let expect = {
            AField01 = 16
            AField02 = 38.12m
            AField03 = DateTime(2015, 5, 6)
        }
        let actual =  recordinfo<RecordA> |> ClrRecord.fromValueMap valueMap :?> RecordA
        actual |> Claim.equal expect

    [<Test>]
    let ``Created record from value map - with optional fields``() =
        let valueMap1 = 
            [(BField01Name, 16 :> obj)
             (BField02Name, 38.12m :> obj)
             (BField03Name, DateTime(2015, 5, 6) |> Some :> obj)
            ]|> ValueMap.fromNamedItems
        
        let expect1 = {
            BField01 = 16
            BField02 = 38.12m
            BField03 = DateTime(2015, 5, 6) |> Some
        }
        let actual1 =  recordinfo<RecordB> |> ClrRecord.fromValueMap valueMap1 :?> RecordB
        actual1 |> Claim.equal expect1

        let valueMap2 = 
            [(BField01Name, 16 :> obj)
             (BField02Name, 38.12m :> obj)
             (BField03Name, option<DateTime>.None :> obj)
            ]|> ValueMap.fromNamedItems
        
        let expect2 = {
            BField01 = 16
            BField02 = 38.12m
            BField03 = None
        }
        let actual2 =  recordinfo<RecordB> |> ClrRecord.fromValueMap valueMap2 :?> RecordB
        actual2 |> Claim.equal expect2


    [<Test>]
    let ``Created value array from record - with no optional fields``() =
        let recordValue = {
            AField01 = 16
            AField02 = 38.12m
            AField03 = DateTime(2015, 5, 6)
        }
        let valueArray = recordValue |> ClrRecord.toValueArray
        Claim.equal (recordValue.AField01 :> obj) valueArray.[0]
        Claim.equal (recordValue.AField02 :> obj) valueArray.[1]
        Claim.equal (recordValue.AField03 :> obj) valueArray.[2]


