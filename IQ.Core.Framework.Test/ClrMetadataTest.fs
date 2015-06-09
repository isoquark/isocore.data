namespace IQ.Core.Framework

open IQ.Core.TestFramework

open System
open System.Reflection


[<TestFixture>]
module ClrMetadataTest =
    type private RecordA = {
        Field01 : int
        Field02 : decimal
        Field03 : DateTime
    }
    
    [<Test>]
    let ``Discovered record metadata``() =
        let info = typeof<RecordA> |> ClrRecord.describe
        info.Fields.Length |> Claim.equal 3         


    [<Test>]
    let ``Retrieved value map for record``() =
        let value = {
            Field01 = 16
            Field02 = 38.12m
            Field03 = DateTime(2015, 5, 6)
        }
        let expect = 
            [
            ("Field01", value.Field01 :> obj)
            ("Field02", value.Field02 :> obj)
            ("Field03", value.Field03:> obj)
            ] |> Map.ofList
        let actual = value |> ClrRecord.toValueMap 
        actual |> Claim.equal expect

    [<Test>]
    let ``Created value map from record``() =
        let values = [("Field01", 16 :> obj); ("Field02", 38.12m :> obj); ("Field03", DateTime(2015, 5, 6) :> obj)] |> Map.ofList
        let expect = {
            Field01 = 16
            Field02 = 38.12m
            Field03 = DateTime(2015, 5, 6)
        }
        let actual =  recinfo<RecordA> |> ClrRecord.fromValueMap values :?> RecordA
        actual |> Claim.equal expect

    [<Test>]
    let ``Created value array from record``() =
        let recordValue = {
            Field01 = 16
            Field02 = 38.12m
            Field03 = DateTime(2015, 5, 6)
        }
        let valueArray = recordValue |> ClrRecord.toValueArray
        Claim.equal (recordValue.Field01 :> obj) valueArray.[0]
        Claim.equal (recordValue.Field02 :> obj) valueArray.[1]
        Claim.equal (recordValue.Field03 :> obj) valueArray.[2]

    type private RecordB = {
        Field10 : int option
        Field11 : string
        Field12 : RecordA option
    }
    
    [<Test>]
    let ``Recognized option type``() =
        recinfo<RecordB>.Fields.[0].FieldType |> ClrType.isOptionType |> Claim.isTrue
        recinfo<RecordB>.Fields.[1].FieldType |> ClrType.isOptionType |> Claim.isFalse
        recinfo<RecordB>.Fields.[2].FieldType |> ClrType.isOptionType |> Claim.isTrue        


        
        

