﻿namespace IQ.Core.Framework.Test
open System
open System.ComponentModel

open IQ.Core.Framework
open IQ.Core.TestFramework

[<TestContainer>]
module ClrMetadataProviderTest =
    module private ModuleA =
        type RecordA = {
            FieldA1 : int
            FieldA2 : string
            FieldA3 : decimal
        }

        type RecordB = {
            FieldB1 : int option
            FieldB2 : string
            FieldB3 : RecordA option
        }

        
    

    [<Test>]
    let ``Described records``() =
        //No optional fields
        let t1 = typeof<ModuleA.RecordA>.TypeName |> ClrMetadata().DescribeType
        t1.Name.SimpleName |> Claim.equal typeof<ModuleA.RecordA>.Name
        t1.Members.Length |> Claim.equal 3
        t1.Members.[0].Name.Text |> Claim.equal "FieldA1"
        t1.Members.[1].Name.Text |> Claim.equal "FieldA2"
        t1.Members.[2].Name.Text |> Claim.equal "FieldA3"

        //Optional fields
        let t2 = typeof<ModuleA.RecordB>.TypeName |> ClrMetadata().DescribeType
        let t2Props = t2.Properties
        t2Props.Length |> Claim.equal 3  
        

        t2Props.[0].Name.Text |> Claim.equal "FieldB1"
        t2Props.[0].ReflectedElement.Value.PropertyType |> Claim.equal typeof<option<int>>
        t2Props.[0].Position |> Claim.equal 0

        t2Props.[1].Name.Text |> Claim.equal "FieldB2"
        t2Props.[1].ReflectedElement.Value.PropertyType |> Claim.equal typeof<string>
        t2Props.[1].Position |> Claim.equal 1

        t2Props.[2].Name.Text|> Claim.equal "FieldB3"
        t2Props.[2].ReflectedElement.Value.PropertyType |> Claim.equal typeof<option<ModuleA.RecordA>>
        t2Props.[2].Position |> Claim.equal 2

            
    [<AttributeUsage(AttributeTargets.All)>]
    type MyAttribute() =
        inherit Attribute()

    type private IInterfaceA =        
        abstract Method01:p1 : string -> p2 : int -> [<return: MyAttribute>] int64
        abstract Method02:p1 : string * p2: int->unit
        abstract Method03:p1 : (string*int) -> unit
        abstract Method04:p1 : string -> p2 : int option -> DateTime

    type private IInterfaceB =
        abstract Method01:unit->unit
        abstract Method02:int->unit
        abstract Method03:int->int
        abstract Property01:DateTime
        abstract Property02:DateTime with get,set

    let methodmap = methinfos<IInterfaceA>    
    [<Test>]
    let ``Described non-tupled method - variation 1``() =
        let mName = "Method01" |> ClrMemberName 
        let m = methodmap.[mName]
        m.Name |> Claim.equal (mName)
        m.Position |> Claim.equal 0
        m.Parameters.Length |> Claim.equal 3
        m.Parameters.[0].CanOmit |> Claim.isFalse
        m.Parameters.[0].Name.Text |> Claim.equal "p1"
        m.Parameters.[0].ParameterType.SimpleName |> Claim.equal typeof<string>.Name
        m.Parameters.[1].CanOmit |> Claim.isFalse
        m.Parameters.[1].Name.Text |> Claim.equal "p2"
        m.Parameters.[1].ParameterType.SimpleName |> Claim.equal typeof<int>.Name
        m.ReturnType |> Option.get |> Claim.equal typeof<int64>.TypeName
        m.ReturnAttributes.Length |> Claim.equal 1
        m.ReturnAttributes.Head.AttributeName |> Claim.equal typeof<MyAttribute>.TypeName

    [<Test>]
    let ``Described tupled methods``() =
        let m2Name = "Method02" |> ClrMemberName 
        let m2 = methodmap.[m2Name]
        m2.Name |> Claim.equal m2Name
        m2.Parameters.Length |> Claim.equal 2
        m2.ReturnType |> Claim.isNone

        let m3Name = "Method03" |> ClrMemberName 
        let m3 = methodmap.[m3Name]
        m3.Name |> Claim.equal m3Name
        m3.Parameters.Length |> Claim.equal 1
        m3.ReturnType |> Claim.isNone

    [<Test>]
    let ``Described non-tupled method - variation 2``() =
        let mName = "Method04" |> ClrMemberName 
        let m = methodmap.[mName]
        m.Name |> Claim.equal mName
        m.Parameters.Length |> Claim.equal 3
        m.Parameters.[0].CanOmit |> Claim.isFalse
        m.Parameters.[0].Name.Text |> Claim.equal "p1"
        m.Parameters.[0].ParameterType |> Claim.equal typeof<string>.TypeName
        m.Parameters.[1].CanOmit |> Claim.isFalse
        m.Parameters.[1].Name.Text |> Claim.equal "p2"        
        m.Parameters.[1].ParameterType |> Claim.equal typeof<option<int>>.TypeName
        m.ReturnType |> Option.get |> Claim.equal typeof<DateTime>.TypeName

    [<Test>]
    let ``Described interfaces``() =
        let t = typeof<IInterfaceB>.TypeName |> ClrMetadata().DescribeType
        t.Members.Length |> Claim.equal 5

    type ClassA() =   
        let mutable p2Val = 0
        let p3Val = Some(4L)
        member this.Prop1  = DateTime.Now
        member this.Prop2
            with get() = p2Val
            and  set(value) = p2Val <-value
        member this.Prop3 with get() = p3Val

    type private ClassB() =
        member this.Method01(p1 : int, ?p2 : int) = 0L

    [<Test>]
    let ``Described class methods with optional parameters``() =
        let methodmap = methinfos<ClassB>
        let mName = "Method01" |> ClrMemberName 
        let m = methodmap.[mName]
        m.Name |> Claim.equal mName
        m.Parameters.Length |> Claim.equal 3
        m.Parameters.[1].CanOmit |> Claim.isTrue
        m.ReturnType|> Option.get |> Claim.equal typeof<int64>.TypeName


    [<Test>]
    let ``Described classes``() =
        let infomap = propinfos<ClassA>
        let p1Info = propinfo<@ fun (x : ClassA) -> x.Prop1 @> 
        let p1Name = p1Info.Name |> ClrMemberName
        let p1Expect = 
            {
                Name = p1Name
                Position = 0
                DeclaringType = typeof<ClassA>.TypeName
                ValueType = typeof<DateTime>.TypeName
                IsOptional = false
                CanWrite = false
                WriteAccess = None
                CanRead = true
                ReadAccess = PublicAccess |> Some
                ReflectedElement = p1Info |> Some
                IsStatic = false
                Attributes = []
                GetMethodAttributes = []
                SetMethodAttributes = []
            } 
        let p1Actual = infomap.[p1Name]
        p1Actual |> Claim.equal p1Expect

        let p2Info = propinfo<@ fun (x : ClassA) -> x.Prop2 @> 
        let p2Expect = {
            Name = p2Info.Name |> ClrMemberName
            Position = 1
            DeclaringType = typeof<ClassA>.TypeName
            ValueType = typeof<int>.TypeName
            IsOptional = false
            CanWrite = true
            WriteAccess = PublicAccess |> Some
            CanRead = true
            ReadAccess = PublicAccess |> Some
            ReflectedElement = p2Info |> Some
            IsStatic = false
            Attributes = []
            GetMethodAttributes = []
            SetMethodAttributes = []
        }

        let p3Info = propinfo<@ fun (x : ClassA) -> x.Prop3 @>
        let p3Name = p3Info.Name |> ClrMemberName
        let p3Expect = 
            {
                Name = p3Name
                Position = 2
                DeclaringType = typeof<ClassA>.TypeName
                ValueType = typeof<option<int64>>.TypeName
                IsOptional = true
                CanWrite = false
                WriteAccess = None
                CanRead = true
                ReadAccess = PublicAccess |> Some
                ReflectedElement = p3Info |> Some
                IsStatic = false
                Attributes = []
                GetMethodAttributes = []
                SetMethodAttributes = []
            } 
        let p3Actual = infomap.[p3Name] 
        p3Actual.Name |> Claim.equal p3Name
        p3Actual.Position |> Claim.equal 2
        p3Actual.DeclaringType |> Claim.equal typeof<ClassA>.TypeName
        p3Actual.ValueType |> Claim.equal typeof<option<int64>>.TypeName
        p3Actual.IsOptional |> Claim.isTrue
        p3Actual.CanWrite |> Claim.isFalse
        p3Actual.CanRead |> Claim.isTrue
        p3Actual.WriteAccess |> Claim.equal None
        p3Actual.ReadAccess |> Claim.equal (Some(PublicAccess))
        p3Actual.ReflectedElement |> Claim.equal (p3Info |> Some)
        p3Actual.IsStatic |> Claim.isFalse
        p3Actual.Attributes |> Claim.seqIsEmpty
        p3Actual.GetMethodAttributes |> Claim.seqIsEmpty
        p3Actual.SetMethodAttributes |> Claim.seqIsEmpty
                                   

    type private UnionA = UnionA of field01 : int * field02 : decimal * field03 : DateTime        
    
    [<Test>]
    let ``Described union``() =
        let u = typeinfo<UnionA>
        u.ReflectedElement  |> Option.get|> Claim.equal typeof<UnionA>        
        let unioninfo = typeinfo<UnionA>
        unioninfo.Name.SimpleName |> Claim.equal typeof<UnionA>.Name

    [<Test>]
    let ``Found types by name``() =
        typeof<UnionA>.TypeName |> ClrMetadata().DescribeType 
                                |> fun x -> x.Name 
                                |> Claim.equal (typeof<UnionA>.TypeName)

    module ModuleB =
        [<Description("This is RecordA")>]
        type RecordA = {
            FieldA1 : decimal
            FieldA2 : int64
            FieldA3 : DateTime option   
        }

    [<Test>]
    let ``Discovered attributes on type``() =
        let t = typeinfo<ModuleB.RecordA>
        let desc = t.Attributes 
                |> List.find(fun a -> a.AttributeName = typeof<DescriptionAttribute>.TypeName)
        desc.AppliedValues.["Description"] :?> string |> Claim.equal "This is RecordA"

    module Literals = 
        type Dummy() = class end
        [<Literal>]
        let Literal1 = 3u
        [<Literal>]
        let Literal2 = "Hello"
        let NotEvenAField = 46m

    [<Test>]
    let ``Discovered literals defined in a module``() =
        let m = typeof<Literals.Dummy>.DeclaringType.TypeName |> ClrMetadata().DescribeType
        m.Fields |> Claim.seqCount 2
        m.Fields.[0].IsLiteral |> Claim.isTrue
        m.Fields.[0].LiteralValue |> Option.get |> Claim.equal (Literals.Literal1.ToString())
        m.Fields.[1].IsLiteral |> Claim.isTrue
        m.Fields.[1].LiteralValue |> Option.get |> Claim.equal (Literals.Literal2.ToString())
