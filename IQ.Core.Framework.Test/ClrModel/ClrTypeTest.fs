namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Generic

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
        match typeref<RecordB> with
        | RecordTypeReference(subject, fields) ->
            fields.[0].PropertyType.IsOptionType |> Claim.isTrue
            fields.[1].PropertyType.IsOptionType |> Claim.isFalse
            fields.[2].PropertyType.IsOptionType |> Claim.isTrue  
        | _ ->
            Claim.assertFail()

    type private UnionA = UnionA of field01 : int * field02 : decimal * field03 : DateTime

    [<Test>]
    let ``Described single-case discriminated union``() =
        let u = unionref<UnionA>
        let unionName = typeof<UnionA>.ElementName
        match typeref<UnionA> with
        | UnionTypeReference(subject,cases) ->
            subject.Name |> Claim.equal unionName
            cases.Length |> Claim.equal 1
            subject.Type |> Claim.equal typeof<UnionA>
            
            let field01Case = cases.[0].[0]        
            let fieldCaseName = field01Case.Name
            cases.[0].[fieldCaseName] |> Claim.equal field01Case
            field01Case.Position |> Claim.equal 0
            field01Case.ValueType |> Claim.equal typeof<int>
            field01Case.Name.Text |> Claim.equal "field01"
        | _ ->
            Claim.assertFail()
        



    [<Test>]
    let ``Determined the item value type of a type``() =        
        typeof<List<string>>.ItemValueType |> Claim.equal typeof<string>
        typeof<option<string>>.ItemValueType |> Claim.equal typeof<string>
        typeof<option<List<string>>>.ItemValueType |> Claim.equal typeof<string>
        typeof<string>.ItemValueType |> Claim.equal typeof<string>
