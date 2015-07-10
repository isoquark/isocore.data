namespace IQ.Core.MetaCode.Test

open System
open System.Reflection

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.MetaCode


open IQ.Core.MetaCode.Test.Prototypes

module ClrEnumGenerator =
    
    module ClrTypeInfo =
        let stripReflectedElement (i : ClrTypeInfo) =
            {i with ReflectedElement = None}

    type LogicTests(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
        
        let describeAssembly simpleName types =
                {
                    Name = ClrAssemblyName(simpleName, None)
                    ReflectedElement = None
                    Position = 0
                    Types = types
                    Attributes = []
                }


            

//        [<Fact>]
//        let ``Generated Enum1``() =
//            let expect = typeinfo<Enum1> 
//            let actual = [expect] |> describeAssembly "GeneratedEnum1" 
//                                  |> ClrAssemblyGenerator.generate 
//                                  |> Assembly.LoadFrom
//                                  |> fun x -> x.GetTypes() 
//                                  |> Array.find(fun x -> x.TypeName.FullName = expect.Name.FullName)
//                                  |> ClrType.describe expect.Position                   
//            
//            let attribs = typeof<Enum1>.GetCustomAttributes()
//            
//            
//            let info1 = expect.Info |> ClrTypeInfo.stripReflectedElement
//            let info2 = actual.Info |> ClrTypeInfo.stripReflectedElement
//            let same = info1 = info2
//            
//            ()
        
        [<Fact>]
        let ``Generated Enum - MyEnum``() =

            let numericType = typeof<int32>.TypeName
            let enumTypeName = ClrTypeName("MyEnum", Some("SomeNamespace.MyEnum"), None)
            let literalA = 
                {
                    ClrField.Name = ClrMemberName("LiteralA")
                    Access = ClrAccessKind.Public
                    ReflectedElement = None
                    Position = 0
                    Attributes = []
                    IsStatic = true
                    FieldType = numericType
                    DeclaringType = enumTypeName
                    IsLiteral = true
                    LiteralValue = Some(10 :> obj)
                } |> FieldMember
            

            let info = 
                {
                    ClrTypeInfo.Name = enumTypeName
                    Position = 1
                    ReflectedElement = None
                    DeclaringType = None
                    DeclaredTypes = []
                    Kind = ClrTypeKind.Enum
                    IsOptionType = false
                    Members = [literalA]
                    Access = ClrAccessKind.Public
                    IsStatic = true
                    Attributes = []
                    ItemValueType = enumTypeName
                }
                        
            let assembly = 
                {
                    Name = ClrAssemblyName("MetaCodeTest", None)
                    ReflectedElement = None
                    Position = 0
                    Types = [ClrEnum(numericType, info) |> EnumType]
                    Attributes = []
                }

            let result = assembly |> ClrAssemblyGenerator.generate |> Assembly.LoadFrom
            
            result.GetTypes() |> Array.exists(fun x -> x.TypeName.FullName = enumTypeName.FullName) |> Claim.isTrue

            
            ()

