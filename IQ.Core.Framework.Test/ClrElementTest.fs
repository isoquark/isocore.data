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
    let FieldA2Name = propname<@fun (x : RecordA) -> x.FieldA2 @>
    
    
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
    

    type AttributeAAttribute() =
        inherit Attribute()

    module ModuleA =
        [<AttributeA>]
        let sum a b  =
            a + b
        [<AttributeA>]
        let subtract a b  =
            a - b

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
        let text = thisAssembly() |> Assembly.findTextResource "EmbeddedResource01.txt"
        text |> Claim.isSome
        text.Value.Trim() |> Claim.equal "This is an embedded text resource"
    

    [<Test>]
    let ``Discovered type child elements``() =
        let t = clrtype<ClassA>
        let children = t.Element |> ClrElement.getChildren 
        children |> Claim.listHasLength 6
    
    [<Test>]
    let ``Traversed CLR element hierarchy``() =
        let elements = ResizeArray<ClrElement>()
        let handler (e : ClrElement) =
            e |> elements.Add
        clrtype<RecordA>.Element |> ClrElement.walk handler
        elements |> Seq.tryFind(fun x -> x.Name = FieldA1Name) |> Claim.isSome
        elements |> Seq.tryFind(fun x -> x.Name = FieldA2Name) |> Claim.isSome

    [<Test>]
    let ``Discovered attributed functions``() =
        let functions = ResizeArray<ClrElement>()
        
        let handler (e : ClrElement) =
            match e |> ClrElement.tryGetAttributeT<AttributeAAttribute> with
            | Some(x) ->
                e |> functions.Add
            | None -> ()
        
        thisAssemblyElement().Element |> ClrElement.walk handler
        let x = 1
        ()
              
        
//    [<Test>]
//    let ``Discovered attributed types within an assembly``() =
//        ()


