namespace IQ.Core.Framework.Test

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
        

type ClrElementNameTests(ctx,log)  =
    inherit ProjectTestContainer(ctx,log)

    [<Fact>]
    let ``Retreived CLR type name from type``() =
        let t = typeof<RecordA>
        let actual = typeinfo<RecordA>.Name
        let expect = ClrTypeName(t.Name , t.FullName |> Some, t.AssemblyQualifiedName |> Some)
        actual |> Claim.equal expect

    [<Fact>]
    let ``Extracted embedded text resource from assembly``() =
        let text = thisAssembly() |> Assembly.findTextResource "EmbeddedResource01.txt"
        text |> Claim.isSome
        text.Value.Trim() |> Claim.equal "This is an embedded text resource"
    

    [<Fact>]
    let ``Discovered type child elements``() =
        typeinfo<ClassA>.Members.Length |> Claim.equal 6
    
    [<Fact>]
    let ``Traversed CLR element hierarchy``() =
        let elements = ResizeArray<ClrElement>()
        let handler (e : ClrElement) =
            e |> elements.Add
        typeinfo<RecordA> |> TypeElement |> ClrElement.walk handler



