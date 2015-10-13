// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.MetaCode.Shell

open System

open IQ.Core.Data
open IQ.Core.MetaCode
open IQ.Core.Framework;


type CodeGenerationContext() =
    member val Namespace = String.Empty with get, set

module Main = 

    let rec getMappedTypeName (dataType : DataTypeReference) =
        match dataType with
        | BitDataType -> typeof<Boolean>.FullName
        | UInt8DataType -> typeof<Byte>.FullName
        | UInt16DataType -> typeof<UInt16>.FullName
        | UInt32DataType -> typeof<UInt32>.FullName
        | UInt64DataType -> typeof<UInt64>.FullName
        | Int8DataType -> typeof<SByte>.FullName
        | Int16DataType -> typeof<Int16>.FullName
        | Int32DataType -> typeof<Int32>.FullName
        | Int64DataType -> typeof<Int64>.FullName
        | BinaryFixedDataType(len) -> typeof<Byte>.FullName + "[]"
        | BinaryVariableDataType(maxlen) -> typeof<Byte>.FullName + "[]"
        | BinaryMaxDataType -> typeof<Byte>.FullName + "[]"
        | AnsiTextFixedDataType(len) -> typeof<string>.FullName
        | AnsiTextVariableDataType(maxlen) -> typeof<string>.FullName
        | AnsiTextMaxDataType -> typeof<string>.FullName
        | UnicodeTextFixedDataType(len) -> typeof<string>.FullName
        | UnicodeTextVariableDataType(maxlen) -> typeof<string>.FullName
        | UnicodeTextMaxDataType  -> typeof<string>.FullName
        | DateTimeDataType(p,s) -> typeof<BclDateTime>.FullName
        | DateTimeOffsetDataType -> typeof<BclDateTimeOffset>.FullName
        | TimeOfDayDataType(p,s) -> typeof<UInt64>.FullName
        | DurationDataType -> typeof<UInt64>.FullName 
        | RowversionDataType-> typeof<Byte>.FullName + "[]"
        | DateDataType -> typeof<BclDateTime>.FullName
        | Float32DataType -> typeof<float32>.FullName
        | Float64DataType -> typeof<float>.FullName
        | DecimalDataType(p,s) -> typeof<Decimal>.FullName
        | MoneyDataType(p,s)  -> typeof<Decimal>.FullName
        | GuidDataType -> typeof<Guid>.FullName
        | XmlDataType(schema) -> typeof<string>.FullName
        | JsonDataType -> typeof<string>.FullName
        | VariantDataType -> typeof<Object>.FullName
        | TableDataType(name) -> name.LocalName
        | ObjectDataType(name, clrTypeName) -> clrTypeName
        | CustomPrimitiveDataType(name, baseType) -> baseType |> getMappedTypeName
        | TypedDocumentDataType(doctype) -> doctype.FullName
        
        
    let inferProxyDescription (context : CodeGenerationContext) (table : TableDescription) =
        let typeName = ClrTypeName(table.Name.LocalName, Some(sprintf "%s.%s" table.Name.SchemaName table.Name.LocalName), None)
        
        let typeAttributions = 
                         TableAttribute(table.Name.SchemaName, table.Name.LocalName) :> Attribute 
                         |> Seq.singleton
                         |> ClrAttribution.create (typeName |> TypeElementName)

        

        let members = [for col in table.Columns ->
                            let valueTypeName = ClrTypeName(String.Empty, Some(col.DataType |> getMappedTypeName), None)
                            {
                                ClrProperty.Name = col.Name |> ClrMemberName
                                Position = col.Position
                                ReflectedElement = None
                                Attributes = []
                                GetMethodAttributes = []
                                SetMethodAttributes = []
                                DeclaringType = typeName
                                ValueType = valueTypeName
                                IsOptional = false
                                IsNullable = false
                                CanRead = true
                                CanWrite = true
                                ReadAccess = ClrAccessKind.Public |> Some
                                WriteAccess = ClrAccessKind.Public |> Some
                                IsStatic = false
                            } |> PropertyMember
                         ]
        
        let typeInfo = {
            ClrTypeInfo.Access = ClrAccessKind.Public
            Name = typeName
            Position = 0
            ReflectedElement = None
            DeclaringType = None
            Kind = ClrTypeKind.Class
            IsOptionType = false
            Members = members
            IsStatic = false
            Attributes = typeAttributions
            ItemValueType = typeName
            Namespace = sprintf "%s.%s" context.Namespace table.Name.SchemaName
            DeclaredTypes = []
        }
        ClrClass(typeInfo) |> ClassType
    
    
    [<EntryPoint>]
    let main argv = 
        use context = new ShellContext()
        let cs = "csSqlDataStore" |> context.ConfigurationManager.GetValue 

        let dsProvider = context.AppContext.Resolve<IDataStoreProvider>()
        let store = dsProvider.GetDataStore<ISqlDataStore>(cs)
        let metadata = store.MetadataProvider

        let context = CodeGenerationContext(Namespace="MyNamespace")

        let table = metadata.DescribeTable(DataObjectName("SqlTest", "Table02"))
        let tableProxy = table |> inferProxyDescription context

//        let prop = {
//            Name = ClrMemberName("Prop1")
//            Position = 0
//            ReflectedElement = None
//            Attributes = []
//            GetMethodAttributes = []
//            SetMethodAttributes = []
//            DeclaringType = ClrTypeName("MyClass1", None, None)
//            ValueType = ClrTypeName("Int32", Some("System.Int32"), None)
//            IsOptional = false
//            IsNullable = false
//            CanRead = true
//            ReadAccess = ClrAccessKind.Public |> Some
//            CanWrite = true
//            WriteAccess = ClrAccessKind.Public |> Some
//            IsStatic = false
//        }
//
//        let info = {
//            Name = ClrTypeName("MyClass1", None, None)
//            Position = 0
//            ReflectedElement = None
//            DeclaringType = None
//            DeclaredTypes = []
//            Kind = ClrTypeKind.Class
//            IsOptionType = false
//            Members = [prop |> PropertyMember]
//            Access = ClrAccessKind.Public
//            IsStatic = false
//            Attributes = []
//            ItemValueType = ClrTypeName("MyClass1", None, None)
//            Namespace = "IQ.Types"
//        }



        let assembly = 
            {
                Name = ClrAssemblyName("IQ.Types", None)
                ReflectedElement = None
                Position = 0
                Types = [tableProxy]
                Attributes = []
                References = []
            }

        assembly |> CSharpGenerator.genProject @"C:\Temp"

        0 
