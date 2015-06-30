namespace IQ.Core.Framework.Test
open System

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

        
    let assdesc = thisAssembly() |> ClrMetadataProvider.describeAssembly

    [<Test>]
    let ``Described records``() =
        //No optional fields
        let t1 = typeof<ModuleA.RecordA> |> ClrMetadataProvider.describeType
        t1.Name.SimpleName |> Claim.equal typeof<ModuleA.RecordA>.Name
        t1.Members.Length |> Claim.equal 3
        t1.Members.[0].Name.Text |> Claim.equal "FieldA1"
        t1.Members.[1].Name.Text |> Claim.equal "FieldA2"
        t1.Members.[2].Name.Text |> Claim.equal "FieldA3"

        //Optional fields
        let t2 = typeof<ModuleA.RecordB> |> ClrMetadataProvider.describeType
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

            
    type private IInterfaceB =
        abstract Method01:unit->unit
        abstract Method02:int->unit
        abstract Method03:int->int
        abstract Property01:DateTime
        abstract Property02:DateTime with get,set

    

    [<Test>]
    let ``Described interfaces``() =
        let t = typeof<IInterfaceB> |> ClrMetadataProvider.describeType
        t.Members.Length |> Claim.equal 5

    type ClassA() =   
        let mutable p2Val = 0
        let p3Val = Some(4L)
        member this.Prop1  = DateTime.Now
        member this.Prop2
            with get() = p2Val
            and  set(value) = p2Val <-value
        member this.Prop3 with get() = p3Val

    [<Test>]
    let ``Described classes``() =
        let infomap = propinfomap<ClassA>
        let p1Info = propinfo<@ fun (x : ClassA) -> x.Prop1 @> 
        let p1Name = p1Info.Name |> ClrMemberName
        let p1Expect = 
            {
                Name = p1Name
                Position = 0
                DeclaringType = typeof<ClassA>.ElementTypeName
                ValueType = typeof<DateTime>.ElementTypeName
                IsOptional = false
                CanWrite = false
                WriteAccess = None
                CanRead = true
                ReadAccess = PublicAccess |> Some
                ReflectedElement = p1Info |> Some
                IsStatic = false
            } |> PropertyDescription
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
            ReadAccess = PublicAccess |> Some
            ReflectedElement = p2Info |> Some
            IsStatic = false
        }

        let p3Info = propinfo<@ fun (x : ClassA) -> x.Prop3 @>
        let p3Name = p3Info.Name |> ClrMemberName
        let p3Expect = 
            {
                Name = p3Name
                Position = 2
                DeclaringType = typeof<ClassA>.ElementTypeName
                ValueType = typeof<int64>.ElementTypeName
                IsOptional = true
                CanWrite = false
                WriteAccess = None
                CanRead = true
                ReadAccess = PublicAccess |> Some
                ReflectedElement = p3Info |> Some
                IsStatic = false
            } |> PropertyDescription
        let p3Actual = infomap.[p3Name]
        p3Actual |> Claim.equal p3Expect

    type private UnionA = UnionA of field01 : int * field02 : decimal * field03 : DateTime        
    
    [<Test>]
    let ``Described union``() =
        let u = typeinfo<UnionA>
        u.ReflectedElement  |> Option.get|> Claim.equal typeof<UnionA>
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

    [<Test>]
    let ``Found types by name``() =
        let results = typeof<UnionA>.ElementTypeName |> FindTypeByName |> ClrMetadataProvider.findTypes
        results |> List.isEmpty |> Claim.isFalse
        Claim.equal typeof<UnionA>.ElementTypeName results.Head.Name