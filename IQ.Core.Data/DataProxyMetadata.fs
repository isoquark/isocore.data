// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Reflection
//open System.Data
open System.Text.RegularExpressions

open FSharp.Data

open IQ.Core.Framework


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
    
       
        
    let inferStorageType(description : ClrElement) =
        let fromClrType (t : Type) =
            if typeMap.ContainsKey(t.ItemValueType) then
                typeMap.[t.ItemValueType]
            else
                TypedDocumentDataType(t)

        match description |> ClrElement.tryGetAttributeT<DataTypeAttribute>  with
        | Some(attrib) -> 
            attrib.DataType
        | None ->            
            match description with
            | MemberElement(d) ->
                match d with
                | PropertyMember(x) -> x.ReflectedElement.Value.PropertyType |> fromClrType
                | FieldMember(x) -> x.ReflectedElement.Value.FieldType |> fromClrType
                | _ -> nosupport()
            | ParameterElement(d) ->
                d.ReflectedElement.Value.ParameterType |> fromClrType
            | TypeElement(t) ->
                t.ReflectedElement.Value |> fromClrType
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
    /// <param name="clrElement">The CLR element from which the column description will be inferred</param>
    let describeColumn(description: ClrProperty) =
        let gDescription = description |> PropertyMember |> MemberElement
        let storageType = gDescription |> inferStorageType
        match gDescription |> ClrElement.tryGetAttributeT<ColumnAttribute> with
        | Some(attrib) ->
            {
                ColumnDescription.Name = 
                    match attrib.Name with 
                    | Some(name) -> name
                    | None -> description.Name.Text
                Position = description.Position |> defaultArg attrib.Position 
                Documentation = String.Empty
                StorageType = storageType
                Nullable = description.IsOptional 
                AutoValue = AutoValueKind.None
            }

        | None ->
            {
                ColumnDescription.Name = description.Name.Text
                Position = description.Position
                Documentation = String.Empty
                StorageType = storageType
                Nullable = description.IsOptional 
                AutoValue = AutoValueKind.None
            }

    let private describeColumnProxy(description : ClrProperty) =
        let column = description |> describeColumn
        ColumnProxyDescription(description, column)

    /// <summary>
    /// Infers a collection of <see cref="ClrColumnProxyDescription"/> instances from a CLR type element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the column descriptions will be inferred</param>
    let  private describeColumnProxies(description : ClrType) =
        if description.Properties.Length = 0 then
            NotSupportedException(sprintf "No columns were able to be inferred from the type %O" description) |> raise
        description.Properties |> List.map describeColumnProxy


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
            StorageType = description |> ParameterElement|> inferStorageType 
        }
    
    
    /// <summary>
    /// Infers a return RoutineParameterDescription from a CLR element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the parameter description will be inferred</param>
    let describeReturnParameter(description : ClrMethod) =
        let eDescription   = description |> MethodMember |> MemberElement 
        let storageType = match eDescription |> ClrElement.tryGetAttributeT<DataTypeAttribute> with
                                | Some(attrib) -> 
                                    attrib.DataType
                                | None -> 
                                    description.ReturnType  |> Option.get |> describeType |> TypeElement |>   inferStorageType
        
        match description.ReturnAttributes |> List.tryFind(fun x -> x.AttributeName = typeinfo<RoutineParameterAttribute>.Name) with
        |Some(attrib) ->
            let attrib = attrib.AttributeInstance |> Option.get :?> RoutineParameterAttribute
            let position =  match attrib.Position with |Some(p) -> p |None -> -1
            {
                RoutineParameterDescription.Name = attrib.Name |> Option.get                   
                Direction = RoutineParameterDirection.Output //Must always be output so value in attribute can be ignored
                StorageType = storageType
                Position = position
                Documentation = String.Empty
            }
        | None ->
            //No attribute, so assume stored procedure return value
            {
                RoutineParameterDescription.Name = "Return"
                Position = -1
                Direction = RoutineParameterDirection.ReturnValue
                StorageType = storageType
                Documentation = String.Empty
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

                
    let describeProcedureProxy(description : ClrElement) =
        let objectName = description |> inferDataObjectName
        match description with
        | MemberElement(m) -> 
            match m with
            | MethodMember(m) ->
                let parameters = m.Parameters |> List.map(fun x -> x |> describeParameterProxy m)
                let procedure = {
                    ProcedureDescription.Name = objectName
                    Parameters = parameters |> List.map(fun p -> p.DataElement)
                    Documentation = String.Empty
                }            
                ProcedureCallProxyDescription(m, procedure, parameters) |> ProcedureProxy
            | _ -> nosupport()

        | _ -> nosupport()
                                                                                                        
    /// <summary>
    /// Infers a Table Proxy Description
    /// </summary>
    /// <param name="proxyType">The type of proxy</param>
    let describeTablularProxy(description : ClrType) =
        let objectName = description |> TypeElement |> inferDataObjectName
        let columnProxies = description |> describeColumnProxies
        let table = {
            TabularDescription.Name = objectName
            Documentation = description.ReflectedElement.Value|> getMemberDescription
            Columns = columnProxies |> List.map(fun p -> p.DataElement)
        }
        TablularProxyDescription(description, table, columnProxies) 
    
    let describeTableFunctionProxy(description : ClrElement) =
        let objectName = description |> inferDataObjectName
        let parameterProxies, columnProxies, clrMethod, returnType =
            match description with
            | MemberElement(m) -> 
                match m with
                | MethodMember(m) ->
                    let itemType = m.ReflectedElement.Value.ReturnType.ItemValueType.TypeName  |> describeType
                    let itemTypeProxies = itemType |> describeColumnProxies                                        
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
            Parameters = parameterProxies|> List.map(fun p -> p.DataElement)
            Columns = columnProxies |> List.map(fun c -> c.DataElement)
            Documentation = String.Empty
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
        | SqlElementKind.Table | SqlElementKind.View ->
            match clrElement with
            | TypeElement(x) ->
                x |> DataProxyMetadata.describeTablularProxy |> TabularProxy |> List.singleton
            | _ ->
                 nosupport()           
            
        | _ -> nosupport()
    
    let get() =
        { new IDataProxyMetadataProvider with
            member this.DescribeProxies dek clrElement =
                describeProxies dek clrElement        
        }
                                   
module TypeProxy =
    let inferDataObjectName (typedesc : ClrType) =
        typedesc |> TypeElement |> DataProxyMetadata.inferDataObjectName

    let inferSchemaName (typedesc : ClrType) =
        typedesc |> TypeElement |> DataProxyMetadata.inferSchemaName

module TableFunctionProxy =    
    let describe (m : ClrMethod) =
        m |> MethodMember |> MemberElement  |> DataProxyMetadata.describeTableFunctionProxy |> DataObjectProxy.unwrapTableFunctionProxy

/// <summary>
/// Convenience methods/operators intended to minimize syntactic clutter
/// </summary>
[<AutoOpen>]
module DataProxyOperators =    
    let tabularproxy<'T> =
        typeinfo<'T> |> DataProxyMetadata.describeTablularProxy


    let routineproxies<'T> =
        typeinfo<'T> |> TypeElement |> DataProxyMetadata.describeRoutineProxies
            
    let fromProxyType<'T> =
        tabularproxy<'T> |> TabularProxy

           

    
               
           
        
                  



        