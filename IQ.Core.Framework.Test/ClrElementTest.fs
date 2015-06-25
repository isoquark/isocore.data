namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework

open System
open System.Reflection


//// <summary>
/// Defines the types that are used to verify the correct function of the ClrElement module
/// </summary>
module ClrElementTestTypes =
    
    type RecordA = {
        FieldA1 : int
        FieldA2 : string
        FieldA3 : decimal        

    }

    let FieldA1Name = propname<@fun (x : RecordA) -> x.FieldA1 @>
    
    
    type ClassA() =
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
    
open ClrElementTestTypes

module ClrElementTest = ()
    

        
[<TestContainer>]
module ClrElementNameTests =
    [<Test>]
    let ``Retreived CLR type name from type``() =
        let t = typeof<RecordA>
        let actual = clrtype<RecordA>.ElementTypeName
        let expect = ClrTypeName(t.Name , t.FullName |> Some, t.AssemblyQualifiedName |> Some)
        actual |> Claim.equal expect

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
        clrtype<RecordA> |> Claim.inList types
        clrtype<ClassA> |> Claim.inList types

//    [<Test>]
//    let ``Discovered attributed types within an assembly``() =
//        ()

    [<Test>]
    let ``Discovered type child elements``() =
        let t = clrtype<ClassA>
        let children = t.Element |> ClrElement.getChildren 
        ()
        //children |> Claim.listHasLength 3
    


//[<TestContainer>]
//module ClrTypeTest =
//    [<Test>]
//    let ``Discovered type members``() =
//        let a = clrtype<RecordA>
//        let aMembers = a |> ClrType.getMembers
//        FieldA1Name |> Claim.inList (aMembers |> List.map(fun x -> x.ElementName))
        
        
    