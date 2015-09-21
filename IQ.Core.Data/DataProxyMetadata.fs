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
    
    /// <summary>
    /// Infers the kind of data type from a reference
    /// </summary>
    let getKind (t : DataTypeReference) =
        match t with
        | BitDataType -> DataKind.Bit
        | UInt8DataType -> DataKind.UInt8
        | UInt16DataType -> DataKind.UInt64
        | UInt32DataType -> DataKind.UInt32
        | UInt64DataType -> DataKind.UInt64
        | Int8DataType -> DataKind.Int8
        | Int16DataType -> DataKind.Int16
        | Int32DataType -> DataKind.Int32
        | Int64DataType -> DataKind.Int64  
        | RowversionDataType -> DataKind.BinaryFixed                     
        | BinaryFixedDataType(_) -> DataKind.BinaryFixed
        | BinaryVariableDataType(_) -> DataKind.BinaryVariable
        | BinaryMaxDataType -> DataKind.BinaryMax            
        | AnsiTextFixedDataType(length) -> DataKind.AnsiTextFixed
        | AnsiTextVariableDataType(length) -> DataKind.AnsiTextVariable
        | AnsiTextMaxDataType -> DataKind.AnsiTextMax            
        | UnicodeTextFixedDataType(_) -> DataKind.UnicodeTextFixed
        | UnicodeTextVariableDataType(_) -> DataKind.UnicodeTextVariable
        | UnicodeTextMaxDataType -> DataKind.UnicodeTextMax            
        | DateTimeDataType(_,_)-> DataKind.DateTime
        | DateTimeOffsetDataType -> DataKind.DateTimeOffset
        | TimeOfDayDataType(_) -> DataKind.TimeOfDay
        | DateDataType -> DataKind.Date
        | DurationDataType -> DataKind.Duration            
        | Float32DataType -> DataKind.Float32
        | Float64DataType -> DataKind.Float64
        | DecimalDataType(precision,scale) -> DataKind.Decimal
        | MoneyDataType(_,_) -> DataKind.Money
        | GuidDataType -> DataKind.Guid
        | XmlDataType(_) -> DataKind.Xml
        | JsonDataType -> DataKind.Json
        | VariantDataType -> DataKind.Variant
        | TableDataType(_) -> DataKind.CustomTable
        | ObjectDataType(_) -> DataKind.CustomObject
        | CustomPrimitiveDataType(_) -> DataKind.CustomPrimitive
        | TypedDocumentDataType(_) -> DataKind.TypedDocument

    let private kindMap = 
        [
            Type.GetType("System.Boolean"), DataKind.Bit
            Type.GetType("System.Byte"), DataKind.UInt8
            Type.GetType("System.Byte[]"), DataKind.BinaryMax
            Type.GetType("System.DateTime"), DataKind.DateTime
            Type.GetType("System.DateTimeOffset"), DataKind.DateTimeOffset
            Type.GetType("System.Decimal"), DataKind.Decimal
            Type.GetType("System.Double"), DataKind.Float64
            Type.GetType("System.Guid"), DataKind.Guid
            Type.GetType("System.Int16"), DataKind.Int16
            Type.GetType("System.Int32"), DataKind.Int32
            Type.GetType("System.Int64"), DataKind.Int64
            Type.GetType("System.Object"), DataKind.Variant
            Type.GetType("System.Single"), DataKind.Float32
            Type.GetType("System.Char"), DataKind.UnicodeTextFixed
            Type.GetType("System.String"), DataKind.UnicodeTextVariable
            Type.GetType("System.TimeSpan"), DataKind.Int64 
        ] |> dict
               

    let getDefaultTypeFromKind kind =
        match kind with
        | DataKind.Bit -> 
            BitDataType
        | DataKind.UInt8 -> 
            UInt8DataType
        | DataKind.UInt16 -> 
            UInt16DataType 
        | DataKind.UInt32 ->
            UInt32DataType
        | DataKind.UInt64 ->
            UInt64DataType
        | DataKind.Int8 ->
            Int8DataType
        | DataKind.Int16 -> 
            Int16DataType
        | DataKind.Int32 -> 
            Int32DataType
        | DataKind.Int64 -> 
            Int64DataType
        | DataKind.BinaryFixed -> 
            50 |> BinaryFixedDataType
        | DataKind.BinaryVariable -> 
            50 |> BinaryVariableDataType
        | DataKind.BinaryMax -> 
            BinaryMaxDataType      
        | DataKind.AnsiTextFixed -> 
            50 |> AnsiTextFixedDataType
        | DataKind.AnsiTextVariable -> 
            50 |> AnsiTextVariableDataType
        | DataKind.AnsiTextMax -> 
            AnsiTextMaxDataType
        | DataKind.UnicodeTextFixed -> 
            50 |> UnicodeTextFixedDataType
        | DataKind.UnicodeTextVariable -> 
            50 |> UnicodeTextVariableDataType
        | DataKind.UnicodeTextMax -> 
            UnicodeTextMaxDataType
        | DataKind.DateTime -> 
            DateTimeDataType(27uy, 7uy)
        | DataKind.DateTimeOffset -> 
            DateTimeOffsetDataType
        | DataKind.TimeOfDay -> 
            TimeOfDayDataType(16uy, 7uy)
        | DataKind.Date -> 
            DateDataType
        | DataKind.Duration -> 
            DurationDataType
        | DataKind.LegacyDateTime -> 
            DateTimeDataType(23uy, 3uy)
        | DataKind.LegacySmallDateTime -> 
            DateTimeDataType(16uy, 0uy)
        | DataKind.Float32 -> 
            Float32DataType
        | DataKind.Float64 -> 
            Float64DataType
        | DataKind.Decimal -> 
            DecimalDataType(19uy, 4uy)
        | DataKind.Money -> 
            MoneyDataType(19uy,4uy)
        | DataKind.SmallMoney -> 
            MoneyDataType(10uy, 4uy)
        | DataKind.Guid -> 
            GuidDataType
        | DataKind.Xml -> 
            String.Empty |> XmlDataType
        | DataKind.Json -> 
            UnicodeTextMaxDataType
        | DataKind.Variant -> 
            VariantDataType                      
        | DataKind.Geography ->
            ObjectDataType(DataObjectName("sys", "geography"), "Microsoft.SqlServer.Types.Geography")
        | DataKind.Geometry -> 
            ObjectDataType(DataObjectName("sys", "geometry"), "Microsoft.SqlServer.Types.SqlGeometry")
        | DataKind.Hierarchy -> 
            ObjectDataType(DataObjectName("sys", "hierarchyid"), "Microsoft.SqlServer.Types.SqlHierarchyId")
        | DataKind.TypedDocument -> 
            typeof<obj> |> TypedDocumentDataType
        | DataKind.CustomTable -> 
            nosupport()
        | DataKind.CustomObject -> 
            nosupport()
        | DataKind.CustomPrimitive -> 
            nosupport()
        | _ ->
            nosupport()


    let private getClrType (element : ClrElement) =
        match element with
        | MemberElement(m) -> 
            match m with
            | PropertyMember(p) ->
                p.ReflectedElement.Value.PropertyType
            | FieldMember(f) ->
                f.ReflectedElement.Value.FieldType
            | MethodMember(m) ->
                m.ReflectedElement.Value.ReturnType
            | ConstructorMember(c) ->
                nosupport()
            | EventMember(e) ->
                nosupport()
        | TypeElement(t) ->
            t.ReflectedElement.Value
        | AssemblyElement(a) ->
            nosupport()
        | ParameterElement(p) ->
            p.ReflectedElement.Value.ParameterType
        | UnionCaseElement(c) ->
            nosupport()


    let facet<'T> name element =
        element |> DataFacet.tryGetFacetValue<'T> name

    let hasFacet<'T> name element =
        element |> DataFacet.hasFacet<'T> name

    let inferKindTypefromClrType(t : Type) =
        if kindMap.ContainsKey(t) then
            kindMap.[t]
        else
            ArgumentException(sprintf "No default mapping for %s exists" t.FullName) |> raise
       
            
    
    let rec private inferStoreDataType(element : ClrElement) =
        
        let clrType = element |> getClrType
        let clrItemValueType = clrType.ItemValueType
                
        let getLen defaultValue =
            match element |> facet<int>(DataFacetNames.FixedLength) with 
            | Some(x) -> x |None -> defaultValue
        
        let getMaxLen defaultValue = 
            match element |> facet<int>(DataFacetNames.MaxLength) with 
            | Some(x) -> x |None -> defaultValue
        
        let getPrecision defaultValue =
            match element |> facet<uint8> DataFacetNames.Precision with
            | Some x -> x | None -> defaultValue

        let getScale defaultValue =
            match element |> facet<uint8> DataFacetNames.Scale with
            | Some x -> x | None -> defaultValue

        let getXmlSchema defaultValue =
            match element |> facet<string> DataFacetNames.XmlSchema with
            | Some x -> x | None -> defaultValue

        let getRepresentationType defaultValue =
            match element |> facet<Type> DataFacetNames.RepresentationType with
            | Some x -> x | None -> defaultValue

        let tryGetDataObjectName() =
            element |> facet<DataObjectName> DataFacetNames.DataObjectName  

        let kind = 
            match element |> facet<DataKind>(DataFacetNames.DataKind) with
            | Some(x) -> x
            | None ->
                if clrType = typeof<Byte[]> then
                    if element |> hasFacet<int> DataFacetNames.FixedLength then
                        DataKind.BinaryFixed
                    else if element |> hasFacet<int> DataFacetNames.MaxLength then
                        DataKind.BinaryVariable
                    else
                        DataKind.BinaryMax
                else if clrItemValueType = typeof<string> then
                    if element |> hasFacet<int> DataFacetNames.FixedLength then
                        DataKind.UnicodeTextFixed
                    else
                        DataKind.UnicodeTextVariable                    
                else if kindMap.ContainsKey(clrItemValueType) then
                        kindMap.[clrItemValueType]
                else
                     ArgumentException(sprintf "No default mapping for %s exists" clrType.FullName) |> raise 

        match kind with
        | DataKind.Bit -> 
            BitDataType
        | DataKind.UInt8 -> 
            UInt8DataType
        | DataKind.UInt16 -> 
            UInt16DataType 
        | DataKind.UInt32 ->
            UInt32DataType
        | DataKind.UInt64 ->
            UInt64DataType
        | DataKind.Int8 ->
            Int8DataType
        | DataKind.Int16 -> 
            Int16DataType
        | DataKind.Int32 -> 
            Int32DataType
        | DataKind.Int64 -> 
            Int64DataType
        | DataKind.BinaryFixed -> 
            50 |> getLen |> BinaryFixedDataType
        | DataKind.BinaryVariable -> 
            50 |> getMaxLen |> BinaryVariableDataType
        | DataKind.BinaryMax -> 
            BinaryMaxDataType      
        | DataKind.AnsiTextFixed -> 
            50 |> getLen |> AnsiTextFixedDataType
        | DataKind.AnsiTextVariable -> 
            50 |> getMaxLen |> AnsiTextVariableDataType
        | DataKind.AnsiTextMax -> 
            AnsiTextMaxDataType
        | DataKind.UnicodeTextFixed -> 
            50 |> getLen |> UnicodeTextFixedDataType
        | DataKind.UnicodeTextVariable -> 
            50 |> getMaxLen |> UnicodeTextVariableDataType
        | DataKind.UnicodeTextMax -> 
            UnicodeTextMaxDataType
        | DataKind.DateTime -> 
            DateTimeDataType(27uy, getScale 7uy)
        | DataKind.DateTimeOffset -> 
            DateTimeOffsetDataType
        | DataKind.TimeOfDay -> 
            TimeOfDayDataType(getPrecision 16uy, getScale 7uy)
        | DataKind.Date -> 
            DateDataType
        | DataKind.Duration -> 
            DurationDataType
        | DataKind.LegacyDateTime -> 
            DateTimeDataType(23uy, 3uy)
        | DataKind.LegacySmallDateTime -> 
            DateTimeDataType(16uy, 0uy)
        | DataKind.Float32 -> 
            Float32DataType
        | DataKind.Float64 -> 
            Float64DataType
        | DataKind.Decimal -> 
            DecimalDataType(getPrecision 19uy, getScale 4uy)
        | DataKind.Money -> 
            MoneyDataType(19uy,4uy)
        | DataKind.SmallMoney -> 
            MoneyDataType(10uy, 4uy)
        | DataKind.Guid -> 
            GuidDataType
        | DataKind.Xml -> 
            String.Empty |> getXmlSchema |> XmlDataType
        | DataKind.Json -> 
            UnicodeTextMaxDataType
        | DataKind.Variant -> 
            VariantDataType                      
        | DataKind.Geography ->
            ObjectDataType(DataObjectName("sys", "geography"), "Microsoft.SqlServer.Types.Geography")
        | DataKind.Geometry -> 
            ObjectDataType(DataObjectName("sys", "geometry"), "Microsoft.SqlServer.Types.SqlGeometry")
        | DataKind.Hierarchy -> 
            ObjectDataType(DataObjectName("sys", "hierarchyid"), "Microsoft.SqlServer.Types.SqlHierarchyId")
        | DataKind.TypedDocument -> 
            typeof<obj> |> getRepresentationType |> TypedDocumentDataType
        | DataKind.CustomTable -> 
            tryGetDataObjectName() |> Option.get |> TableDataType
        | DataKind.CustomObject -> 
            let objectName = element |> facet<DataObjectName>(DataFacetNames.CustomObjectName) |> Option.get
            ObjectDataType(objectName, clrItemValueType.Name)     
        | DataKind.CustomPrimitive -> 
            //Obviously, this needs work
            CustomPrimitiveDataType(DataObjectName("",""), Int32DataType)
        | _ ->
            nosupport()
        
    let clrMDP = ClrMetadataProvider.getDefault()

       
        
         
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
                    let description = t |> clrMDP.FindType |> TypeElement
                   
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
        let dataType = gDescription |> inferStoreDataType
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
                DataKind= dataType |> getKind
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
                DataKind = dataType |> getKind
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
            DataType = description |> ParameterElement|> inferStoreDataType 
            Properties = []
        }
    
    
    /// <summary>
    /// Infers a return RoutineParameterDescription from a CLR element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the parameter description will be inferred</param>
    let describeReturnParameter(description : ClrMethod) =
        let eDescription   = description |> MethodMember |> MemberElement 
        let storageType = description.ReturnType  |> Option.get |> clrMDP.FindType |> TypeElement |>   inferStoreDataType
        
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
        let returnsRows = 
            if element.HasAttribute<ProcedureAttribute>() then
                element.GetAttibuteInstance<ProcedureAttribute>().ProvidesDataSet
            else
                false
        match element with
        | MemberElement(m) -> 
            match m with
            | MethodMember(m) ->
                let parameters = 
                    if returnsRows then
                        m.Parameters |> List.filter(fun x -> x.IsReturn |> not) 
                    else
                        m.Parameters 
                    |> List.map(fun x -> x |> describeParameterProxy m) 

                let columns = 
                    if returnsRows then                
                        m.ReflectedElement.Value.ReturnType.ItemValueType.TypeName  
                        |> clrMDP.FindType 
                        |> describeColumnProxies objectName                          
                    else
                        []                                   
                                
                let procedure = {
                    RoutineDescription.Name = objectName
                    Parameters = parameters |> List.map(fun p -> p.DataElement) 
                    Columns = columns |> List.map(fun c -> c.DataElement)
                    Documentation = String.Empty
                    RoutineKind = DataElementKind.Procedure
                    Properties = []
                }            
                let call = RoutineCallProxyDescription(m, procedure, parameters)
                let result = if returnsRows then
                                let returnProxy = m.ReflectedElement.Value.ReturnType.TypeName |> clrMDP.FindType
                                RoutineResultProxyDescription(returnProxy, procedure, columns) |> Some
                             else
                                None
                RoutineProxyDescription(call, result) |> ProcedureProxy
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
                    let itemType = m.ReflectedElement.Value.ReturnType.ItemValueType.TypeName  |> clrMDP.FindType
                    let itemTypeProxies = itemType |> describeColumnProxies objectName                                       
                    m.InputParameters |> List.mapi (fun i x ->  x |> describeParameterProxy m ), 
                    itemTypeProxies,
                    m,
                    m.ReturnType |> Option.get |> clrMDP.FindType
                | _ ->
                    nosupport()
            | _ ->
                nosupport()
        let tableFunction = {
            RoutineDescription.Name = objectName   
            Parameters = parameterProxies|> List.map(fun p -> p.DataElement) 
            Columns = columnProxies |> List.map(fun c -> c.DataElement) 
            Documentation = String.Empty
            RoutineKind = DataElementKind.TableFunction
            Properties = []
        }
        let callProxy = RoutineCallProxyDescription(clrMethod, tableFunction, parameterProxies)
        let resultProxy = RoutineResultProxyDescription(returnType, tableFunction, columnProxies) |> Some
        RoutineProxyDescription(callProxy, resultProxy) |> TableFunctionProxy

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
            

           

    
               
           
        
                  



        