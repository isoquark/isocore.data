// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.MetaCode.Shell

open System
open System.IO

open IQ.Core.Data
open IQ.Core.MetaCode
open IQ.Core.Framework;


type CodeGenerationContext() =
    member val Namespace = String.Empty with get, set

module Main = 

    type PlaceholderType() = class end

    let rec getMappedType (dataType : DataTypeReference) =
        match dataType with
        | BitDataType -> typeof<Boolean>
        | UInt8DataType -> typeof<Byte>
        | UInt16DataType -> typeof<UInt16>
        | UInt32DataType -> typeof<UInt32>
        | UInt64DataType -> typeof<UInt64>
        | Int8DataType -> typeof<SByte>
        | Int16DataType -> typeof<Int16>
        | Int32DataType -> typeof<Int32>
        | Int64DataType -> typeof<Int64>
        | BinaryFixedDataType(len) -> typeof<Byte[]>
        | BinaryVariableDataType(maxlen) -> typeof<Byte>
        | BinaryMaxDataType -> typeof<Byte[]>
        | AnsiTextFixedDataType(len) -> typeof<string>
        | AnsiTextVariableDataType(maxlen) -> typeof<string>
        | AnsiTextMaxDataType -> typeof<string>
        | UnicodeTextFixedDataType(len) -> typeof<string>
        | UnicodeTextVariableDataType(maxlen) -> typeof<string>
        | UnicodeTextMaxDataType  -> typeof<string>
        | DateTimeDataType(p,s) -> typeof<BclDateTime>
        | DateTimeOffsetDataType -> typeof<BclDateTimeOffset>
        | TimeOfDayDataType(p,s) -> typeof<UInt64>
        | DurationDataType -> typeof<UInt64>
        | RowversionDataType-> typeof<Byte[]>
        | DateDataType -> typeof<BclDateTime>
        | Float32DataType -> typeof<float32>
        | Float64DataType -> typeof<float>
        | DecimalDataType(p,s) -> typeof<Decimal>
        | MoneyDataType(p,s)  -> typeof<Decimal>
        | GuidDataType -> typeof<Guid>
        | XmlDataType(schema) -> typeof<string>
        | JsonDataType -> typeof<string>
        | VariantDataType -> typeof<Object>
        | TableDataType(name) -> typeof<PlaceholderType>
        | ObjectDataType(name, clrTypeName) -> typeof<PlaceholderType>
        | CustomPrimitiveDataType(name, baseType) -> baseType |> getMappedType
        | TypedDocumentDataType(doctype) -> doctype



    let defineClassType typeNamespace typeName  members attributions documentation =
        let typeInfo = {
            ClrTypeInfo.Access = ClrAccessKind.Public
            Name = typeName
            Position = 0
            Documentation = documentation
            ReflectedElement = None
            DeclaringType = None
            Kind = ClrTypeKind.Class
            IsOptionType = false
            Members = members
            IsStatic = false
            Attributes = attributions
            ItemValueType = typeName
            Namespace =  typeNamespace
            DeclaredTypes = []
        }
        ClrClass(typeInfo) |> ClassType

    let defineColumnMember typeName (col : ColumnDescription) =
        let mappedType = col.DataType |> getMappedType
        {
            ClrProperty.Name = col.Name |> ClrMemberName
            Position = col.Position
            Documentation = col.Documentation
            ReflectedElement = None
            Attributes = []
            GetMethodAttributes = []
            SetMethodAttributes = []
            DeclaringType = typeName
            ValueType = mappedType.TypeName
            IsOptional = false
            IsNullable = (col.Nullable && mappedType.IsValueType)
            CanRead = true
            CanWrite = true
            ReadAccess = ClrAccessKind.Public |> Some
            WriteAccess = ClrAccessKind.Public |> Some
            IsStatic = false
        } |> PropertyMember
        
    let defineTypeName (context : CodeGenerationContext) (dataobject : IDataObjectDescription) =
        ClrTypeName(dataobject.ObjectName.LocalName, Some(sprintf "%s.%s" dataobject.ObjectName.SchemaName dataobject.ObjectName.LocalName), None)
        
    let ExcludedColumnNames = ["DbCreateUser"; "DbCreateTime"; "DbUpdateUser"; "DbUpdateTime"]

    let isColumnExcluded colName =
        ExcludedColumnNames |>List.exists(fun c -> c = colName)

    let isColumnIncluded colName =
        colName |> isColumnExcluded |> not

    let defineColumnMembers typeName (dataobject : ITabularDescription) =
        dataobject.Columns |> List.filter(fun c -> c.Name |> isColumnIncluded) 
                           |>  List.map(fun c -> c |> defineColumnMember typeName)
        
    let defineTableTypeProxy (context : CodeGenerationContext) (dataobject : ITabularDescription) =
        let typeName = dataobject |> defineTypeName context
        let attributions = 
                         TableTypeAttribute(dataobject.ObjectName.SchemaName, dataobject.ObjectName.LocalName) :> Attribute 
                         |> Seq.singleton
                         |> ClrAttribution.create (typeName |> TypeElementName)
        let members = dataobject |> defineColumnMembers typeName
        defineClassType context.Namespace typeName members attributions dataobject.Documentation
        
    let defineTableProxy (context : CodeGenerationContext) (dataobject : ITabularDescription) =
        let typeName = dataobject |> defineTypeName context
        let attributions = 
                         TableAttribute(dataobject.ObjectName.SchemaName, dataobject.ObjectName.LocalName) :> Attribute 
                         |> Seq.singleton
                         |> ClrAttribution.create (typeName |> TypeElementName)
        
        let members = dataobject |> defineColumnMembers typeName
        defineClassType context.Namespace typeName members attributions dataobject.Documentation
    
    let defineViewProxy (context : CodeGenerationContext) (dataobject : ITabularDescription) =
        let typeName = dataobject |> defineTypeName context
        let attributions = 
                         ViewAttribute(dataobject.ObjectName.SchemaName, dataobject.ObjectName.LocalName) :> Attribute 
                         |> Seq.singleton
                         |> ClrAttribution.create (typeName |> TypeElementName)
        
        let members = dataobject |> defineColumnMembers typeName
        defineClassType context.Namespace typeName members attributions dataobject.Documentation

    
    let buildSchemaProxies outdir (metadata : ISqlMetadataProvider) nsRoot schemaName  =
        let context = CodeGenerationContext(Namespace= sprintf "%s.%s" nsRoot schemaName)

        let tableProxies = schemaName 
                         |> metadata.DescribeTablesInSchema 
                         |>List.map(fun x -> x |> defineTableProxy context)
        let tableTypeProxies = schemaName 
                            |> metadata.DescribeDataTypesInSchema |> List.filter(fun x -> x.IsTableType)
                            |>List.map(fun x -> x |> defineTableTypeProxy context)
        let viewProxies = schemaName 
                         |> metadata.DescribeViewsInSchema 
                         |> List.map(fun x -> x |> defineViewProxy context)

        let proxies = tableTypeProxies 
                    |> List.append tableProxies 
                    |> List.append viewProxies
                    |> List.sortBy(fun x -> x.Name.SimpleName)
                
        let filename = sprintf "%sStructures.cs" schemaName 
        let filepath = Path.Combine(outdir, filename)
        proxies |> CSharpGenerator.genFile filepath
        

    let csFmt = "Data Source={0};Integrated Security=True;Pooling=False; Initial Catalog={1}"

    [<EntryPoint>]
    let main argv = 
        let cs = ""
        let schemas = [""]
        let outdir = ""
        let nsRoot = ""

        
        use context = new ShellContext()
        let dsProvider = context.AppContext.Resolve<IDataStoreProvider>()
        let store = dsProvider.GetDataStore<ISqlDataStore>(cs)
        let metadata = store.MetadataProvider
        let build = buildSchemaProxies outdir metadata nsRoot
        schemas |> List.iter build 
        0 
