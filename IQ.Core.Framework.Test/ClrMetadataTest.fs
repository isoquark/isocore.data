namespace IQ.Core.Framework

open IQ.Core.TestFramework

open System
open System.Reflection

[<TestContainer>]
module ClrTypeTest =     
    type private RecordA = {
        Field01 : int
        Field02 : decimal
        Field03 : DateTime
    }

    type private RecordB = {
        Field10 : int option
        Field11 : string
        Field12 : RecordA option
    }
    
    [<Test>]
    let ``Recognized option type``() =
        recordinfo<RecordB>.Fields.[0].FieldType |> ClrType.isOptionType |> Claim.isTrue
        recordinfo<RecordB>.Fields.[1].FieldType |> ClrType.isOptionType |> Claim.isFalse
        recordinfo<RecordB>.Fields.[2].FieldType |> ClrType.isOptionType |> Claim.isTrue        


[<TestContainer>]
module ClrAssemblyTest =
    [<Test>]
    let ``Extracted embedded text resource from assembly``() =
        let text = thisAssembly() |> ClrAssembly.findTextResource "EmbeddedResource01.txt"
        text |> Claim.isSome
        text.Value.Trim() |> Claim.equal "This is an embedded text resource"
        

        
        

