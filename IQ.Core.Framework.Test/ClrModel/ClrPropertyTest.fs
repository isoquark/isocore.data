namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework

open System
open System.Reflection

[<TestContainer>]

module ClrPropertyTest =
    type private RecordA = {
        Prop1 : int
        Prop2 : string
        Prop3 : decimal
    }

    [<Test>]
    let ``Created property descriptions from record``() =
        
        let infomap = propinfomap<RecordA> 
        let p1Name = propname<@ fun (x : RecordA) -> x.Prop1 @> 
        let p1Actual = infomap.[p1Name]
        let p1Expect = {
            ClrPropertyDescription.Subject = ClrSubjectDescription(p1Name, 0)
            DeclaringType = typeof<RecordA>.FullName |> FullTypeName
            ValueType = typeof<int>.FullName |> FullTypeName
            IsOptional = false
            CanWrite = false
            WriteAccess = None
            CanRead = true
            //Internal because record is private
            ReadAccess = InternalAccess |> Some
        }
        p1Actual |> Claim.equal p1Expect
     
    type ClassA() =   
        let mutable p2Val = 0
        let p3Val = Some(4L)
        member this.Prop1  = DateTime.Now
        member this.Prop2
            with get() = p2Val
            and  set(value) = p2Val <-value
        member this.Prop3 with get() = p3Val

    [<Test>]
    let ``Created property descriptions from class``() =
        let infomap = propinfomap<ClassA>
        let p1Name = propname<@ fun (x : ClassA) -> x.Prop1 @> 
        let p1Expect = {
            ClrPropertyDescription.Subject = ClrSubjectDescription(p1Name, 0)
            DeclaringType = typeof<ClassA>.FullName |> FullTypeName
            ValueType = typeof<DateTime>.FullName |> FullTypeName
            IsOptional = false
            CanWrite = false
            WriteAccess = None
            CanRead = true
            ReadAccess = PublicAccess|> Some
        }
        let p1Actual = infomap.[p1Name]
        p1Actual |> Claim.equal p1Expect

        let p2Name = propname<@ fun (x : ClassA) -> x.Prop2 @> 
        let p2Expect = {
            ClrPropertyDescription.Subject = ClrSubjectDescription(p2Name, 1)
            DeclaringType = typeof<ClassA>.FullName |> FullTypeName
            ValueType = typeof<int>.FullName |> FullTypeName
            IsOptional = false
            CanWrite = true
            WriteAccess = PublicAccess |> Some
            CanRead = true
            ReadAccess = PublicAccess|> Some
        }

        let p3Name = propname<@ fun (x : ClassA) -> x.Prop3 @>
        let p3Expect = {
            ClrPropertyDescription.Subject = ClrSubjectDescription(p3Name, 2)
            DeclaringType = typeof<ClassA>.FullName |> FullTypeName
            ValueType = typeof<int64>.ItemValueType.FullName |> FullTypeName
            IsOptional = true
            CanWrite = false
            WriteAccess = None
            CanRead = true
            ReadAccess = PublicAccess|> Some
        }
        let p3Actual = infomap.[p3Name]
        p3Actual |> Claim.equal p3Expect

        ()    

