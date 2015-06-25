namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework

open System
open System.Reflection

        

[<TestContainer>]
module ClrMethodTest =
    
    [<AttributeUsage(AttributeTargets.All)>]
    type MyAttribute() =
        inherit Attribute()
    
    type private IInterfaceA =        
        abstract Method01:p1 : string -> p2 : int -> [<return: MyAttribute>] int64
        abstract Method02:p1 : string * p2: int->unit
        abstract Method03:p1 : (string*int) -> unit
        abstract Method04:p1 : string -> p2 : int option -> DateTime

    let private methodmap = methodrefmap<IInterfaceA>

    let getMappedMethod(name) =  methodmap.[(MemberElementName(name))]
    
    [<Test>]
    let ``Described non-tupled method - variation 1``() =
        let mName = MemberElementName("Method01")
        let m = methodmap.[mName]
        m.Name |> Claim.equal (mName)
        m.Position |> Claim.equal 0
        m.Parameters.Length |> Claim.equal 2
        m.Parameters.[0].IsRequired |> Claim.isTrue
        m.Parameters.[0].Subject.Name.Text |> Claim.equal "p1"
        m.Parameters.[0].ValueType |> Claim.equal typeof<string>
        m.Parameters.[1].IsRequired |> Claim.isTrue
        m.Parameters.[1].Subject.Name.Text |> Claim.equal "p2"
        m.Parameters.[1].ValueType |> Claim.equal typeof<int>
        m.ReturnType |> Option.get |> Claim.equal typeof<int64>
        match m.Subject.Element with
        | MemberElement(x) -> 
            match x with
            | MethodElement(x) -> x |> MethodInfo.getReturnAttribute<MyAttribute> |> Claim.isSome
            | _ -> Claim.assertFail()
        | _ -> Claim.assertFail()

    [<Test>]
    let ``Described tupled methods``() =
        let m2Name = MemberElementName("Method02")
        let m2 = methodmap.[m2Name]
        m2.Name |> Claim.equal m2Name
        m2.Parameters.Length |> Claim.equal 2
        m2.ReturnType |> Claim.isNone

        let m3Name = MemberElementName("Method03")
        let m3 = methodmap.[m3Name]
        m3.Name |> Claim.equal m3Name
        m3.Parameters.Length |> Claim.equal 1
        m3.ReturnType |> Claim.isNone

    [<Test>]
    let ``Described non-tupled method - variation 2``() =
        let mName = MemberElementName("Method04")
        let m = methodmap.[mName]
        m.Name |> Claim.equal mName
        m.Parameters.Length |> Claim.equal 2
        m.Parameters.[0].IsRequired |> Claim.isTrue
        m.Parameters.[0].Subject.Name.Text |> Claim.equal "p1"
        m.Parameters.[0].ValueType |> Claim.equal typeof<string>
        m.Parameters.[1].IsRequired |> Claim.isTrue
        m.Parameters.[1].Subject.Name.Text |> Claim.equal "p2"
        m.Parameters.[1].ValueType |> Claim.equal typeof<int>
        m.Parameters.[1].ParameterType |> Claim.equal typeof<option<int>>         
        m.ReturnType |> Option.get |> Claim.equal typeof<DateTime>
    
    type private ClassA() =
        member this.Method01(p1 : int, ?p2 : int) = 0L

    [<Test>]
    let ``Described methods with optional parameters``() =
        let methodmap = methodrefmap<ClassA>
        let mName = MemberElementName("Method01")
        let m = methodmap.[mName]
        m.Name |> Claim.equal mName
        m.Parameters.Length |> Claim.equal 2
        m.Parameters.[1].IsRequired |> Claim.isFalse
        m.ReturnType|> Option.get |> Claim.equal typeof<int64>
        ()
        

    

        

        

                
            

