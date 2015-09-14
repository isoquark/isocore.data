// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Behavior

open System
open System.Reflection
//open System.Data
open System.Text.RegularExpressions

open FSharp.Data

open IQ.Core.Framework
open IQ.Core.Data.Contracts

type DescriptionAttribute = System.ComponentModel.DescriptionAttribute

/// <summary>
/// Defines operations that populate a Data Proxy Metamodel
/// </summary>
module DataProxyMetadata =    
    
    let private unsafeParse x = x |> DataType.parse |> Option.get
    let private typeMap = 
        [
            Type.GetType("System.Boolean"), unsafeParse "Bit"
            Type.GetType("System.Byte"), unsafeParse "UInt8"
            Type.GetType("System.Byte[]"), unsafeParse "BinaryMax"
            Type.GetType("System.DateTime"), unsafeParse "DateTime(27,7)"
            Type.GetType("System.DateTimeOffset"), unsafeParse "DateTimeOffset"
            Type.GetType("System.Decimal"), unsafeParse "Decimal(19,4)"
            Type.GetType("System.Double"), unsafeParse "Float64"
            Type.GetType("System.Guid"), unsafeParse "Guid"
            Type.GetType("System.Int16"), unsafeParse "Int16"
            Type.GetType("System.Int32"), unsafeParse "Int32"
            Type.GetType("System.Int64"), unsafeParse "Int64"
            Type.GetType("System.Object"), unsafeParse "Variant"
            Type.GetType("System.Single"), unsafeParse "Float32"
            Type.GetType("System.Char"), unsafeParse "UnicodeTextFixed(1)"
            Type.GetType("System.String"), unsafeParse "UnicodeTextVariable(250)"
            Type.GetType("System.TimeSpan"), unsafeParse "Int64"        
        ] |> dict
    
       
    /// <summary>
    /// Infers the dat type from a supplied attribute
    /// </summary>
    /// <param name="attrib">The attribute that describes the type of storage</param>
    let private inferDataTypeFromAttribute (attrib : DataTypeAttribute) =
        match attrib.DataKind with
        | DataKind.Bit ->BitDataType
        | DataKind.UInt8 -> UInt8DataType
        | DataKind.UInt16 -> UInt16DataType
        | DataKind.UInt32 -> UInt32DataType
        | DataKind.UInt64 -> UInt64DataType
        | DataKind.Int8 -> Int8DataType
        | DataKind.Int16 -> Int16DataType
        | DataKind.Int32 -> Int32DataType
        | DataKind.Int64 -> Int64DataType
        | DataKind.Float32 -> Float32DataType
        | DataKind.Float64 -> Float64DataType
        | DataKind.Money -> 
            MoneyDataType(
                defaultArg attrib.Precision DataKind.Money.DefaultPrecision, 
                defaultArg attrib.Scale DataKind.Money.DefaultScale)
        | DataKind.Guid -> GuidDataType
        | DataKind.AnsiTextMax -> AnsiTextMaxDataType
        | DataKind.DateTimeOffset -> DateTimeOffsetDataType
        | DataKind.TimeOfDay -> 
            TimeOfDayDataType(
                defaultArg attrib.Precision DataKind.TimeOfDay.DefaultPrecision,
                defaultArg attrib.Scale DataKind.TimeOfDay.DefaultScale
                )
        | DataKind.Flexible -> VariantDataType
        | DataKind.UnicodeTextMax -> UnicodeTextMaxDataType
        | DataKind.BinaryFixed -> 
            BinaryFixedDataType( defaultArg attrib.Length DataKind.BinaryFixed.DefaultLength)
        | DataKind.BinaryVariable -> 
            BinaryVariableDataType (defaultArg attrib.Length DataKind.BinaryVariable.DefaultLength)
        | DataKind.BinaryMax -> BinaryMaxDataType
        | DataKind.AnsiTextFixed -> 
            AnsiTextFixedDataType(defaultArg attrib.Length DataKind.AnsiTextFixed.DefaultLength)
        | DataKind.AnsiTextVariable -> 
            AnsiTextVariableDataType(defaultArg attrib.Length DataKind.AnsiTextVariable.DefaultLength)
        | DataKind.UnicodeTextFixed -> 
            UnicodeTextFixedDataType(defaultArg attrib.Length DataKind.UnicodeTextFixed.DefaultLength)
        | DataKind.UnicodeTextVariable -> 
            UnicodeTextVariableDataType(defaultArg attrib.Length DataKind.UnicodeTextVariable.DefaultLength)
        | DataKind.DateTime -> 
            DateTimeDataType(
                defaultArg attrib.Precision DataKind.DateTime.DefaultPrecision, 
                defaultArg attrib.Scale DataKind.DateTime.DefaultScale)  
        | DataKind.Date -> DateDataType
        | DataKind.Decimal -> 
            DecimalDataType(
                defaultArg attrib.Precision DataKind.Decimal.DefaultPrecision, 
                defaultArg attrib.Scale DataKind.Decimal.DefaultScale)
        | DataKind.Xml -> XmlDataType("")
        | DataKind.CustomTable -> 
            TableDataType(attrib.CustomTypeName |> Option.get)
        | DataKind.CustomPrimitive -> 
            //TODO: This cannot be calculated unless additional metadata is attached or we have access
            //to database metadata (!)
            CustomPrimitiveDataType(attrib.CustomTypeName |> Option.get, Int32DataType)
        | DataKind.CustomObject | DataKind.Geography | DataKind.Geometry | DataKind.Hierarchy ->          
            ObjectDataType(attrib.CustomTypeName |> Option.get, (attrib.ClrType |> Option.get).FullName)
        | _ ->
            NotSupportedException(sprintf "The data type %A is not recognized" attrib.DataKind) |> raise


//        member this.GetTypeReference(?length : int, ?precision : uint8, ?scale : uint8, ?objectName : DataObjectName) =
//            match this with
//            | DataKind.Bit -> BitDataType
//            | DataKind.UInt8 -> UInt8DataType
//            | DataKind.UInt16 -> UInt16DataType
//            | DataKind.UInt32 -> UInt32DataType
//            | DataKind.UInt64 -> UInt64DataType
//            | DataKind.Int8 -> Int8DataType
//            | DataKind.Int16-> Int16DataType
//            | DataKind.Int32 -> Int32DataType  
//            | DataKind.Int64 -> Int64DataType
//            | DataKind.BinaryFixed -> BinaryFixedDataType(length.Value)
//            | DataKind.BinaryVariable -> BinaryVariableDataType(length.Value)
//            | DataKind.BinaryMax -> BinaryMaxDataType
//            | DataKind.AnsiTextFixed -> AnsiTextFixedDataType(length.Value)
//            | DataKind.AnsiTextVariable -> AnsiTextVariableDataType(length.Value)
//            | DataKind.AnsiTextMax -> AnsiTextMaxDataType
//            | DataKind.UnicodeTextFixed -> UnicodeTextFixedDataType(length.Value)
//            | DataKind.UnicodeTextVariable -> UnicodeTextVariableDataType(length.Value)
//            | DataKind.UnicodeTextMax -> UnicodeTextMaxDataType
//            | DataKind.DateTime -> DateTimeDataType(precision.Value, scale.Value)
//            | DataKind.DateTimeOffset -> DateTimeOffsetDataType
//            | DataKind.TimeOfDay -> TimeOfDayDataType(precision.Value, scale.Value)
//            | DataKind.Date -> DateDataType
//            | DataKind.Duration -> TimespanDataType
//            | DataKind.Float32 -> Float32DataType
//            | DataKind.Float64 -> Float64DataType
//            | DataKind.Decimal -> DecimalDataType(precision.Value, scale.Value)
//            | DataKind.Money -> MoneyDataType(precision.Value, scale.Value)
//            | DataKind.Guid -> GuidDataType
//            | DataKind.Xml -> XmlDataType(objectName.Value.SchemaName)
//            | DataKind.Json -> JsonDataType
//            | DataKind.Flexible -> VariantDataType
//            | DataKind.Geography -> ObjectDataType()
//            | DataKind.Geometry -> nosupport()
//            | DataKind.Hierarchy -> nosupport()  
//            | DataKind.TypedDocument -> nosupport()
//            | DataKind.CustomTable -> nosupport()
//            | DataKind.CustomObject -> nosupport()
//            | DataKind.CustomPrimitive -> nosupport()
//            | _-> nosupport()

        
    let private inferDataTypeFromClrType (t : Type) =            
        if t |> Type.isCollectionType && typeMap.ContainsKey(t) then
            typeMap.[t]
        else if typeMap.ContainsKey(t.ItemValueType) then
            typeMap.[t.ItemValueType]
        else
            TypedDocumentDataType(t)

    let private inferDataType2(element : ClrElement) =
        let dataKindAttrib = element |> ClrElement.tryGetAttributeT<DataKindAttribute>
        let precisionAttrib = element |> ClrElement.tryGetAttributeT<PrecisionAttribute>
        let scaleAttrib = element |> ClrElement.tryGetAttributeT<ScaleAttribute> 
        match element with
        | MemberElement(m) -> 
            match m with
            | PropertyMember(p) ->
                match dataKindAttrib with
                | Some(attrib) ->
                    ()
                | None ->
                    ()
                
            | FieldMember(f) ->
                nosupport()
            | _ ->
                nosupport()
        | TypeElement(t) ->
            nosupport()
        | AssemblyElement(a) ->
            nosupport()
        | ParameterElement(p) ->
            nosupport()
        | UnionCaseElement(c) ->
            nosupport()


    let private inferDataType(description : ClrElement) =

        match description |> ClrElement.tryGetAttributeT<DataTypeAttribute>  with
        | Some(attrib) -> 
            attrib |> inferDataTypeFromAttribute
        | None ->            
            match description with
            | MemberElement(d) ->
                match d with
                | PropertyMember(x) -> x.ReflectedElement.Value.PropertyType |> inferDataTypeFromClrType
                | FieldMember(x) -> x.ReflectedElement.Value.FieldType |> inferDataTypeFromClrType
                | _ -> nosupport()
            | ParameterElement(d) ->
                d.ReflectedElement.Value.ParameterType |> inferDataTypeFromClrType
            | TypeElement(t) ->
                t.ReflectedElement.Value |> inferDataTypeFromClrType
            | _ -> nosupport()

    let private describeType(name : ClrTypeName) =        
        name |> ClrMetadataProvider.getDefault().FindType
        
         
    let private getMemberDescription(m : MemberInfo) =
        if Attribute.IsDefined(m, typeof<DescriptionAttribute>) then
            (Attribute.GetCustomAttribute(m, typeof<DescriptionAttribute>) :?> DescriptionAttribute).Description 
        else
            String.Empty

    /// <summary>
    /// Infers the name of the schema in which the element lives or represents
    /// </summary>
    /// <param name="clrElement">The element from which to infer the schema name</param>
    let inferSchemaName(description: ClrElement) =              
        let inferFromDeclaringType() =
                match description.DeclaringType with
                | Some(t) ->
                    let description = t |> describeType |> TypeElement
                   
                    match description |> ClrElement.tryGetAttributeT<SchemaAttribute>  with
                    | Some(a) ->
                        match a.Name with
                        | Some(n) ->
                            n
                        | None ->
                            t.SimpleName
                    | None ->
                        t.SimpleName
                | None ->
                    nosupport()                    
        
        match description |> ClrElement.tryGetAttributeT<SchemaAttribute> with        
        | Some(a) ->
            match a.Name with
            | Some(name) -> 
                name
            | None -> 
                inferFromDeclaringType()

        | None ->
            match description |> ClrElement.tryGetAttributeT<DataObjectAttribute> with        
            | Some(a) ->
                match a.SchemaName with
                | Some(schemaName) -> 
                    schemaName
                | None -> 
                    inferFromDeclaringType()
            | None ->                                
                inferFromDeclaringType()

    /// <summary>
    /// Infers a <see cref="DataObjectName"/> from a CLR element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the object name will be inferred</param>
    let inferDataObjectName(description : ClrElement) =        
        match description |> ClrElement.tryGetAttributeT<DataObjectAttribute> with
        | Some(a) -> 
            let schemaName = description |> inferSchemaName
            let localName = 
                match a.Name with
                | Some(x) -> 
                    x
                | None ->
                    description.Name.SimpleName
            DataObjectName(schemaName, localName)
                    
        | None ->
            let schemaName = description |> inferSchemaName
            DataObjectName(schemaName, description.Name.SimpleName)
    
    /// <summary>
    /// Infers a <see cref"ColumnDescription"/>  from a CLR element
    /// </summary>
    /// <param name="proxy">The CLR element from which the column description will be inferred</param>
    let describeColumn (parentName : DataObjectName) (proxy: ClrProperty) =
        let gDescription = proxy |> PropertyMember |> MemberElement
        let dataType = gDescription |> inferDataType
        let isNullable = proxy.IsOptional || proxy.IsNullable || proxy.HasAttribute<NullableAttribute>()
        match gDescription |> ClrElement.tryGetAttributeT<ColumnAttribute> with
        | Some(attrib) ->
            {
                ColumnDescription.Name = 
                    match attrib.Name with 
                    | Some(name) -> name
                    | None -> proxy.Name.Text
                Position = proxy.Position |> defaultArg attrib.Position 
                Documentation = String.Empty
                DataType = dataType
                Nullable = isNullable
                AutoValue = AutoValueKind.None
                ParentName = parentName
                Properties = []
            }

        | None ->
            {
                ColumnDescription.Name = proxy.Name.Text
                Position = proxy.Position
                Documentation = String.Empty
                DataType = dataType
                Nullable = isNullable
                AutoValue = AutoValueKind.None
                ParentName = parentName
                Properties = []
            }

    let private describeColumnProxy (parentName : DataObjectName) (description : ClrProperty) =
        let column = description |> describeColumn parentName
        ColumnProxyDescription(description, column)

    /// <summary>
    /// Infers a collection of <see cref="ClrColumnProxyDescription"/> instances from a CLR type element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the column descriptions will be inferred</param>
    let  private describeColumnProxies(parentName : DataObjectName) (description : ClrType) =
        let properties = description.Properties
        if properties.Length = 0 then
            NotSupportedException(sprintf "No columns were able to be inferred from the type %O" description) |> raise
        properties |> List.map (fun x -> x |> describeColumnProxy parentName)


    /// <summary>
    /// Infers a non-return RoutineParameterDescription from a CLR element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the parameter description will be inferred</param>
    let describeRoutineParameter(description  : ClrMethodParameter) =
        
        let name, direction, position = 
            match description |> ParameterElement |> ClrElement.tryGetAttributeT<RoutineParameterAttribute> with
            | Some(attrib) ->
                let position =  match attrib.Position with |Some(p) -> p |None -> description.Position
                match attrib.Name with
                |Some(name) ->
                    name, attrib.Direction, position
                | None ->
                    description.Name.Text, attrib.Direction, position                
            | None ->
                (description.Name.Text, RoutineParameterDirection.Input, description.Position)
        {
            RoutineParameterDescription.Name = name
            Position = position
            Direction = direction
            Documentation = String.Empty
            DataType = description |> ParameterElement|> inferDataType 
            Properties = []
        }
    
    
    /// <summary>
    /// Infers a return RoutineParameterDescription from a CLR element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the parameter description will be inferred</param>
    let describeReturnParameter(description : ClrMethod) =
        let eDescription   = description |> MethodMember |> MemberElement 
        let storageType = match eDescription |> ClrElement.tryGetAttributeT<DataTypeAttribute> with
                                | Some(attrib) -> 
                                    attrib |> inferDataTypeFromAttribute
                                | None -> 
                                    description.ReturnType  |> Option.get |> describeType |> TypeElement |>   inferDataType
        
        match description.ReturnAttributes |> List.tryFind(fun x -> x.AttributeName = typeinfo<RoutineParameterAttribute>.Name) with
        |Some(attrib) ->
            let attrib = attrib.AttributeInstance |> Option.get :?> RoutineParameterAttribute
            let position =  match attrib.Position with |Some(p) -> p |None -> -1
            {
                RoutineParameterDescription.Name = attrib.Name |> Option.get                   
                Direction = RoutineParameterDirection.Output //Must always be output so value in attribute can be ignored
                DataType = storageType
                Position = position
                Documentation = String.Empty
                Properties = []
            }
        | None ->
            //No attribute, so assume stored procedure return value
            {
                RoutineParameterDescription.Name = "Return"
                Position = -1
                Direction = RoutineParameterDirection.ReturnValue
                DataType = storageType
                Documentation = String.Empty
                Properties = []
            }
                                                                 
    /// <summary>
    /// Constructs a <see cref="ParameterProxyDescription"/> based on the CLR element that will proxy it
    /// </summary>
    /// <param name="clrElement">The field correlated with the proxy</param>
    let private describeParameterProxy (m : ClrMethod) (description : ClrMethodParameter) =
        let parameter, pos =
            match description.IsReturn with
            | true ->
                m |> describeReturnParameter, -1
            | false ->
                (description |> describeRoutineParameter), description.Position
        ParameterProxyDescription(description, pos, parameter)

                
    /// <summary>
    /// Describes a procedure proxy
    /// </summary>
    /// <param name="element">The CLR representation of the proxy</param>
    let describeProcedureProxy(element : ClrElement) =
        let objectName = element |> inferDataObjectName
        match element with
        | MemberElement(m) -> 
            match m with
            | MethodMember(m) ->
                let parameters = m.Parameters |> List.map(fun x -> x |> describeParameterProxy m) 
                let procedure = {
                    ProcedureDescription.Name = objectName
                    Parameters = parameters |> List.map(fun p -> p.DataElement) |> List.asReadOnlyList
                    Documentation = String.Empty
                    Properties = []
                }            
                ProcedureCallProxyDescription(m, procedure, parameters) |> ProcedureProxy
            | _ -> nosupport()

        | _ -> nosupport()
                                                                                                        
    /// <summary>
    /// Describes a table proxy
    /// </summary>
    /// <param name="t">The type of proxy</param>
    let describeTableProxy(t : ClrType) =
        let objectName = t |> TypeElement |> inferDataObjectName
        let columnProxies = t |> describeColumnProxies objectName
        let table = {
            TableDescription.Name = objectName
            Documentation = t.ReflectedElement.Value|> getMemberDescription
            Columns = columnProxies |> List.map(fun p -> p.DataElement) 
            Properties = []
        }
        TableProxyDescription(t, table, columnProxies) 
    
    /// <summary>
    /// Describes a view proxy
    /// </summary>
    /// <param name="t">The type of proxy</param>
    let describeViewProxy(t : ClrType) =
        let objectName = t |> TypeElement |> inferDataObjectName
        let columnProxies = t |> describeColumnProxies objectName
        let view = {
            ViewDescription.Name = objectName
            Documentation = t.ReflectedElement.Value|> getMemberDescription
            Columns = columnProxies |> List.map(fun p -> p.DataElement)
            Properties = []
        }
        ViewProxyDescription(t, view, columnProxies) 

    /// <summary>
    /// Describes a table function proxy
    /// </summary>
    /// <param name="element">The CLR representation of the proxy</param>
    let describeTableFunctionProxy(element : ClrElement) =
        let objectName = element |> inferDataObjectName
        let parameterProxies, columnProxies, clrMethod, returnType =
            match element with
            | MemberElement(m) -> 
                match m with
                | MethodMember(m) ->
                    let itemType = m.ReflectedElement.Value.ReturnType.ItemValueType.TypeName  |> describeType
                    let itemTypeProxies = itemType |> describeColumnProxies objectName                                       
                    m.InputParameters |> List.mapi (fun i x ->  x |> describeParameterProxy m ), 
                    itemTypeProxies,
                    m,
                    m.ReturnType |> Option.get |> describeType
                | _ ->
                    nosupport()
            | _ ->
                nosupport()
        let tableFunction = {
            TableFunctionDescription.Name = objectName   
            Parameters = parameterProxies|> List.map(fun p -> p.DataElement) |> List.asReadOnlyList
            Columns = columnProxies |> List.map(fun c -> c.DataElement) |> List.asReadOnlyList
            Documentation = String.Empty
            Properties = []
        }
        let callProxy = TableFunctionCallProxyDescription(clrMethod, tableFunction, parameterProxies)
        let resultProxy = TabularResultProxyDescription(returnType, tableFunction, columnProxies)
        TableFunctionProxyDescription(callProxy, resultProxy) |> TableFunctionProxy

    let describeRoutineProxy (description: ClrElement) =
        match description with
        | MemberElement(x) ->
                match x with
                | MethodMember(m) ->
                    if description |> ClrElement.hasAttributeT<ProcedureAttribute> then
                        description |> describeProcedureProxy
                    else if description |> ClrElement.hasAttributeT<TableFunctionAttribute> then
                        description |> describeTableFunctionProxy
                    else nosupport()                        
                | _ -> nosupport()
        | _ -> nosupport()

    let describeRoutineProxies(description : ClrElement ) =
        match description with
        | TypeElement(x) ->
            [for m in x.Methods do
                if m.HasAttribute<ProcedureAttribute>() then
                    yield m |> MethodMember |> MemberElement |> describeProcedureProxy
                else if m.HasAttribute<TableFunctionAttribute>() then
                    yield m |> MethodMember |> MemberElement |> describeTableFunctionProxy
            ]
                
        | _ ->
             nosupport()           

module DataProxyMetadataProvider =        
    
    let private describeTableProxy (clrElement : ClrElement) =
            match clrElement with
            | TypeElement(x) ->
                x |> DataProxyMetadata.describeTableProxy 
            | _ ->
                 nosupport()           

    
    let private describeProxies (dek : SqlElementKind) (clrElement : ClrElement) =
        match dek with
        | SqlElementKind.Procedure ->
            match clrElement with
            | MemberElement(x) ->
                clrElement |> DataProxyMetadata.describeProcedureProxy |> List.singleton
            
            | TypeElement(x) ->
                [for m in x.Methods do
                    if m.HasAttribute<ProcedureAttribute>() then
                        yield m |> MethodMember |> MemberElement |> DataProxyMetadata.describeProcedureProxy
                ]
            | _ -> nosupport()
                
        | SqlElementKind.TableFunction ->
            match clrElement with
            | MemberElement(x) ->
                clrElement |> DataProxyMetadata.describeTableFunctionProxy |> List.singleton
            | TypeElement(x) ->
                [for m in x.Methods do
                    if m.HasAttribute<TableFunctionAttribute>() then
                        yield m |> MethodMember |> MemberElement |> DataProxyMetadata.describeTableFunctionProxy
                ]
                
            | _ ->
                 nosupport()           
        | SqlElementKind.Table ->
            clrElement |> describeTableProxy |> TableProxy |> List.singleton
        | SqlElementKind.View ->
            match clrElement with
            | TypeElement(x) ->
                x |> DataProxyMetadata.describeViewProxy |> ViewProxy |> List.singleton
            | _ ->
                 nosupport()           
            
        | _ -> nosupport()
    
    let get() =
        { new IDataProxyMetadataProvider with
            member this.DescribeProxies dek clrElement =
                describeProxies dek clrElement
            member this.DescribeTableProxy<'T>() =  
                typeinfo<'T> |> TypeElement |> describeTableProxy
                
        }
                                   
module TypeProxy =
    let inferDataObjectName (typedesc : ClrType) =
        typedesc |> TypeElement |> DataProxyMetadata.inferDataObjectName

    let inferSchemaName (typedesc : ClrType) =
        typedesc |> TypeElement |> DataProxyMetadata.inferSchemaName

module TableFunctionProxy =    
    let describe (m : ClrMethod) =
        m |> MethodMember |> MemberElement  
                          |> DataProxyMetadata.describeTableFunctionProxy 
                          |> DataObjectProxy.unwrapTableFunctionProxy

/// <summary>
/// Convenience methods/operators intended to minimize syntactic clutter
/// </summary>
[<AutoOpen>]
module DataProxyOperators =    
    let tableproxy<'T> =
        typeinfo<'T> |> DataProxyMetadata.describeTableProxy

    let viewproxy<'T> =
        typeinfo<'T> |> DataProxyMetadata.describeViewProxy

    let routineproxies<'T> =
        typeinfo<'T> |> TypeElement |> DataProxyMetadata.describeRoutineProxies
            

           

    
               
           
        
                  



        