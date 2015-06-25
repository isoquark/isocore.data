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
            DeclaringType = typeof<RecordA>.ElementTypeName
            ValueType = typeof<int>.ElementTypeName
            IsOptional = false
            CanWrite = false
            WriteAccess = None
            CanRead = true
            //Internal because record is private
            ReadAccess = InternalAccess |> Some
        }
        p1Actual |> Claim.equal p1Expect
     

