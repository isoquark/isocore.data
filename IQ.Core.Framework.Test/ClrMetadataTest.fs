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
        recordinfo<RecordB>.Fields.[0].FieldType.IsOptionType |> Claim.isTrue
        recordinfo<RecordB>.Fields.[1].FieldType.IsOptionType |> Claim.isFalse
        recordinfo<RecordB>.Fields.[2].FieldType.IsOptionType |> Claim.isTrue        

    type private UnionA = UnionA of field01 : int * field02 : decimal * field03 : DateTime

    [<Test>]
    let ``Described single-case discriminated union``() =
        let u = unioninfo<UnionA>
        u.Name |> Claim.equal "UnionA"
        u.Cases.Length |> Claim.equal 1
        u.Type |> Claim.equal typeof<UnionA>
        u.[0] |> Claim.equal u.["UnionA"]

        let field01Case = u.[0].[0]        
        u.[0].["field01"] |> Claim.equal field01Case
        field01Case.Position |> Claim.equal 0
        field01Case.ValueType |> Claim.equal typeof<int>
        field01Case.Property.Name |> Claim.equal "field01"
        
        

[<TestContainer>]
module ClrMethodTest =
    
    [<AttributeUsage(AttributeTargets.All)>]
    type MyAttribute() =
        inherit Attribute()
    
    type private IInterfaceA =        
        abstract Method01:p1 : string -> p2 : int -> [<return: MyAttribute>] int64
        abstract Method02:p1 : string * p2: int->unit
        abstract Method03:p1 : (string*int) -> int64
        abstract Method04:p1 : string -> p2 : int option -> DateTime

    [<Test>]
    let ``Read method return attribute``() =
        typeof<IInterfaceA>.GetMethod("Method01")|> MethodInfo.getReturnAttribute<MyAttribute> |> Claim.isSome
            

    [<Test>]
    let ``Described non-tupled method - variation 1``() =
        let m = typeof<IInterfaceA>.GetMethod("Method01")
        let description = m |> ClrMethod.describe
        description.Name |> Claim.equal "Method01"
        description.Parameters.Length |> Claim.equal 2
        description.Parameters.[0].IsRequired |> Claim.isTrue
        description.Parameters.[0].Name |> Claim.equal "p1"
        description.Parameters.[0].ValueType |> Claim.equal typeof<string>
        description.Parameters.[1].IsRequired |> Claim.isTrue
        description.Parameters.[1].Name |> Claim.equal "p2"
        description.Parameters.[1].ValueType |> Claim.equal typeof<int>


        description.Return.ReturnType |> Option.get |> Claim.equal typeof<int64>


    [<Test>]
    let ``Described non-tupled method - variation 2``() =
        let description = typeof<IInterfaceA>.GetMethod("Method04") |> ClrMethod.describe
        description.Name |> Claim.equal "Method04"
        description.Parameters.Length |> Claim.equal 2
        description.Parameters.[0].IsRequired |> Claim.isTrue
        description.Parameters.[0].Name |> Claim.equal "p1"
        description.Parameters.[0].ValueType |> Claim.equal typeof<string>
        description.Parameters.[1].IsRequired |> Claim.isTrue
        description.Parameters.[1].Name |> Claim.equal "p2"
        description.Parameters.[1].ValueType |> Claim.equal typeof<int>
        description.Parameters.[1].ParameterType |> Claim.equal typeof<option<int>> 
        
        description.Return.ReturnType |> Option.get |> Claim.equal typeof<DateTime>


    [<Test>]
    let ``Described tupled method - type 1``() =
        let description = typeof<IInterfaceA>.GetMethod("Method02") |> ClrMethod.describe
        description.Name |> Claim.equal "Method02"        
        description.Parameters.Length |> Claim.equal 2
        description.Return.ReturnType |> Claim.isNone
        ()

    [<Test>]
    let ``Described tupled method - type 2``() =
        let description = typeof<IInterfaceA>.GetMethod("Method03") |> ClrMethod.describe
        description.Name |> Claim.equal "Method03"
        description.Parameters.Length |> Claim.equal 1
        ()


    
    type private ClassA() =
        member this.Method01(p1 : int, ?p2 : int) = 0L

    [<Test>]
    let ``Described methods with optional parameters``() =
        let description = typeof<ClassA> |> Type.getMethod "Method01" |> ClrMethod.describe
        description.Name |> Claim.equal "Method01"
        description.Parameters.Length |> Claim.equal 2
        description.Parameters.[1].IsRequired |> Claim.isFalse
        description.Return.ReturnType|> Option.get |> Claim.equal typeof<int64>
        ()
        

[<TestContainer>]
module ClrAssemblyTest =
    [<Test>]
    let ``Extracted embedded text resource from assembly``() =
        let text = thisAssembly() |> Assembly.findTextResource "EmbeddedResource01.txt"
        text |> Claim.isSome
        text.Value.Trim() |> Claim.equal "This is an embedded text resource"
    

        
[<TestContainer>]
module ClrInterfaceTest =
    type private IMyInterfaceA =
        abstract Method01:unit->unit
        abstract Method02:int->unit
        abstract Method03:int->int
        abstract Property01:DateTime
        abstract Property02:DateTime with get,set

    [<Test>]
    let ``Described interface methods``() =
        let description = interfaceinfo<IMyInterfaceA>
        let methods = [for m in description.Members do  match m with | InterfaceMethodReference(x) -> yield x | _ -> ()]
        methods.Length |> Claim.equal 3
        let m1 = methods |> List.find(fun x -> x.Name = "Method01")
        m1.Return.ReturnType |> Claim.isNone
        m1.Parameters.Length |> Claim.equal 0

        let m2 = methods |> List.find(fun x -> x.Name = "Method02")
        m2.Return.ReturnType |> Claim.isNone
        m2.Parameters.Length |> Claim.equal 1
        m2.Parameters.[0].Name |> String.IsNullOrWhiteSpace |> Claim.isTrue
        m2.Parameters.[0].ParameterType |> Claim.equal typeof<int>

        

                
            

