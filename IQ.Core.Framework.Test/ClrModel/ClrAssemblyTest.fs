namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework

open System
open System.Reflection


module ClrReflectionTestTypes =
    type Reflect_RecordTypeA = {
        FieldA1 : int
        FieldA2 : string
        FieldA3 : decimal        

    }

    let FieldA1Name = propname<@fun (x : Reflect_RecordTypeA) -> x.FieldA1 @>
    
    
    type Reflect_ClassTypeA() =
        let mutable f1 : int = 0    
        let mutable f2 : string = String.Empty
        let mutable f3 : decimal = 0m
    
        member this.Prop1
            with get() = f1
            and  set(value) = f1 <- value

        member this.Prop2
            with get() = f2
            and  set(value) = f2 <- value

        member this.Prop3
            with get() = f3
            and  set(value) = f3 <- value
    
open ClrReflectionTestTypes

[<TestContainer>]
module ClrAssemblyTest =
    [<Test>]
    let ``Extracted embedded text resource from assembly``() =
        let text = thisAssemblyElement() |> ClrAssembly.findTextResource "EmbeddedResource01.txt"
        text |> Claim.isSome
        text.Value.Trim() |> Claim.equal "This is an embedded text resource"

    

    [<Test>]
    let ``Discovered types from assembly``() =
        let types = thisAssemblyElement() |> ClrAssembly.getTypeElements
        clrtype<Reflect_RecordTypeA> |> Claim.inList types
        clrtype<Reflect_ClassTypeA> |> Claim.inList types


module ClrTypeTest =
    [<Test>]
    let ``Discovered type members``() =
        let a = clrtype<Reflect_RecordTypeA>
        let aMembers = a |> ClrTypeElement.getMembers
        FieldA1Name |> Claim.inList (aMembers |> List.map(fun x -> x.ElementName))
        
        
    