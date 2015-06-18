namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework

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
        recordref<RecordB>.Fields.[0].PropertyType.IsOptionType |> Claim.isTrue
        recordref<RecordB>.Fields.[1].PropertyType.IsOptionType |> Claim.isFalse
        recordref<RecordB>.Fields.[2].PropertyType.IsOptionType |> Claim.isTrue        

    type private UnionA = UnionA of field01 : int * field02 : decimal * field03 : DateTime

    [<Test>]
    let ``Described single-case discriminated union``() =
        let u = unionref<UnionA>
        let unionName = typeof<UnionA>.ElementName
        u.Name |> Claim.equal unionName
        u.Cases.Length |> Claim.equal 1
        u.Type |> Claim.equal typeof<UnionA>
        u.[0] |> Claim.equal u.[BasicElementName(typeof<UnionA>.Name)]

        let field01Case = u.[0].[0]        
        let fieldCaseName = field01Case.Name
        u.[0].[fieldCaseName] |> Claim.equal field01Case
        field01Case.Position |> Claim.equal 0
        field01Case.ValueType |> Claim.equal typeof<int>
        field01Case.Name.Text |> Claim.equal "field01"
        


