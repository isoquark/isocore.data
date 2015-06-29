﻿namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Generic

[<TestContainer>]
module ClrTypeReferenceTest =     
    type private RecordA = {
        Field01 : int
        Field02 : decimal
        Field03 : DateTime
    }
    let private AField01Name = propname<@ fun (x : RecordA) -> x.Field01 @>
    let private AField02Name = propname<@ fun (x : RecordA) -> x.Field02 @>
    let private AField03Name = propname<@ fun (x : RecordA) -> x.Field03 @>

    type private RecordB = {
        FieldB1 : int option
        FieldB2 : string
        FieldB3 : RecordA option
    }
    let private BField01Name = propname<@ fun (x : RecordB) -> x.FieldB1 @>
    let private BField02Name = propname<@ fun (x : RecordB) -> x.FieldB2 @>
    let private BField03Name = propname<@ fun (x : RecordB) -> x.FieldB3 @>
    
    type private UnionA = UnionA of field01 : int * field02 : decimal * field03 : DateTime

    [<Test>]
    let ``Created record reference - No optional fields``() =
        match typeref<RecordA> with
        | RecordTypeReference(subject, fields) ->
            fields.Length |> Claim.equal 3  
            fields.[0].ReferentName |> Claim.equal AField01Name
            fields.[0].PropertyType |> Claim.equal typeof<int>
            fields.[0].ReferentPosition |> Claim.equal 0

        | _ ->
            Claim.assertFail()



    [<Test>]
    let ``Created record reference - Optional fields``() =
        match typeref<RecordB> with
        | RecordTypeReference(subject, fields) ->
            fields.Length |> Claim.equal 3  
        
            fields.[0].ReferentName |> Claim.equal BField01Name
            fields.[0].PropertyType |> Claim.equal typeof<option<int>>
            fields.[0].ReferentPosition |> Claim.equal 0

            fields.[1].ReferentName |> Claim.equal BField02Name
            fields.[1].PropertyType |> Claim.equal typeof<string>
            fields.[1].ReferentPosition |> Claim.equal 1

            fields.[2].ReferentName |> Claim.equal BField03Name
            fields.[2].PropertyType |> Claim.equal typeof<option<RecordA>>
            fields.[2].ReferentPosition |> Claim.equal 2
        | _ ->
            Claim.assertFail()



    [<Test>]
    let ``Created union reference - single-case``() =
        let u = typeref<UnionA>
        u.ReferentType |> Claim.equal typeof<UnionA>.TypeElement
        let unionName = typeof<UnionA>.ElementName
        match typeref<UnionA> with
        | UnionTypeReference(subject,cases) ->
            subject.Name |> Claim.equal unionName
            cases.Length |> Claim.equal 1
            
            let field01Case = cases.[0].[0]        
            let fieldCaseName = field01Case.ReferentName
            cases.[0].[fieldCaseName] |> Claim.equal field01Case
            field01Case.ReferentPosition |> Claim.equal 0
            field01Case.ValueType |> Claim.equal typeof<int>
            field01Case.ReferentName.Text |> Claim.equal "field01"
        | _ ->
            Claim.assertFail()
        

    type ClassA() =   
        let mutable p2Val = 0
        let p3Val = Some(4L)
        member this.Prop1  = DateTime.Now
        member this.Prop2
            with get() = p2Val
            and  set(value) = p2Val <-value
        member this.Prop3 with get() = p3Val

    [<Test>]
    let ``Created class reference``() =
        let infomap = propinfomap<ClassA>
        let p1Info = propinfo<@ fun (x : ClassA) -> x.Prop1 @> 
        let p1Name = p1Info.Name |> ClrMemberName
        let p1Expect = {
            Name = p1Name
            Position = 0
            DeclaringType = typeof<ClassA>.ElementTypeName
            ValueType = typeof<DateTime>.ElementTypeName
            IsOptional = false
            CanWrite = false
            WriteAccess = None
            CanRead = true
            ReadAccess = PublicAccess|> Some
        }
        let p1Actual = infomap.[p1Name]
        p1Actual |> Claim.equal p1Expect

        let p2Info = propinfo<@ fun (x : ClassA) -> x.Prop2 @> 
        let p2Expect = {
            Name = p2Info.Name |> ClrMemberName
            Position = 1
            DeclaringType = typeof<ClassA>.ElementTypeName
            ValueType = typeof<int>.ElementTypeName
            IsOptional = false
            CanWrite = true
            WriteAccess = PublicAccess |> Some
            CanRead = true
            ReadAccess = PublicAccess|> Some
        }

        let p3Info = propinfo<@ fun (x : ClassA) -> x.Prop3 @>
        let p3Name = p3Info.Name |> ClrMemberName
        let p3Expect = {
            Name = p3Name
            Position = 2
            DeclaringType = typeof<ClassA>.ElementTypeName
            ValueType = typeof<int64>.ElementTypeName
            IsOptional = true
            CanWrite = false
            WriteAccess = None
            CanRead = true
            ReadAccess = PublicAccess|> Some
        }
        let p3Actual = infomap.[p3Name]
        p3Actual |> Claim.equal p3Expect

        ()    
    
    [<Test>]
    let ``Determined the item value type of a type``() =        
        typeof<List<string>>.ItemValueType |> Claim.equal typeof<string>
        typeof<option<string>>.ItemValueType |> Claim.equal typeof<string>
        typeof<option<List<string>>>.ItemValueType |> Claim.equal typeof<string>
        typeof<string>.ItemValueType |> Claim.equal typeof<string>
