namespace IQ.Core.Data

open System
//open System.ComponentModel
open System.Reflection
open System.Data

open FSharp.Data

open IQ.Core.Framework

type DescriptionAttribute = System.ComponentModel.DescriptionAttribute

/// <summary>
/// Defines operations that populate a Data Proxy Metamodel
/// </summary>
module DataProxyMetadata =    
    [<Literal>]
    let private DefaultClrTypeMapResource = "Resources/DefaultClrStorageTypeMap.csv"
    
    type private DefaultClrTypeMap = CsvProvider<DefaultClrTypeMapResource, Separators="|">
    
    let private getDefaultClrStorageTypeMap() =
        let mapdata = DefaultClrTypeMap.Load(DefaultClrTypeMapResource)
        [for row in mapdata.Rows ->
            try
                Type.GetType(row.ClrTypeName, true), row.StorageType |> StorageType.parse |> Option.get
            with
                e ->
                    reraise()
        ] |> dict
                    
    let private defaultClrStorageTypeMap = getDefaultClrStorageTypeMap()

    let private getMemberDescription(m : MemberInfo) =
        if Attribute.IsDefined(m, typeof<DescriptionAttribute>) then
            (Attribute.GetCustomAttribute(m, typeof<DescriptionAttribute>) :?> DescriptionAttribute).Description |> Some
        else
            None        

    let private inferStorageTypeFromClrType (t : Type) =
        if defaultClrStorageTypeMap.ContainsKey(t.ItemValueType) then
            defaultClrStorageTypeMap.[t.ItemValueType]
        else
            NotSupportedException(sprintf "No default mapping exists for the %O type" t.ItemValueType) |> raise

    let private inferStorageTypeFromClrElement(element : ClrElementReference) =
        let valueType =
            match element with
            | PropertyElement(x) -> x.PropertyType
            | MethodParameterElement(x) -> x.ParameterType
            | _ ->
                NotSupportedException() |> raise
        valueType |> inferStorageTypeFromClrType
        
    let private inferStorageTypeFromAttribute (attrib : StorageTypeAttribute) =
        attrib |> StorageType.fromAttribute

    /// <summary>
    /// Infers a <see cref"StorageType"/> from a CLR element
    /// </summary>
    /// <param name="element"></param>
    let inferStorageType(element : ClrElementReference) =
        match element |> ClrElement.getAttribute<StorageTypeAttribute> with
        | Some(attrib) -> 
            attrib |> inferStorageTypeFromAttribute
        | None ->            
            element |> inferStorageTypeFromClrElement                                             

    /// <summary>
    /// Infers the name of the schema in which the element lives or represents
    /// </summary>
    /// <param name="clrElement">The element from which to infer the schema name</param>
    let inferSchemaName(clrElement : ClrElementReference) =              
        let inferFromDeclaringType() =
                match clrElement.DeclaringType with
                | Some(t) ->
                    match t |> ClrType.getAttribute<SchemaAttribute> with
                    | Some(a) ->
                        match a.Name with
                        | Some(n) ->
                            n
                        | None ->
                            t.Type.Name
                    | None ->
                        t.Type.Name
                | None ->
                    NotSupportedException() |> raise                    
        
        match clrElement |> ClrElement.getAttribute<SchemaAttribute> with        
        | Some(a) ->
            match a.Name with
            | Some(name) -> 
                name
            | None -> 
                inferFromDeclaringType()

        | None ->
            match clrElement |> ClrElement.getAttribute<DataObjectAttribute> with
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
    let inferDataObjectName(clrElement : ClrElementReference) =        
        
        let inferFromElementName() =
            match clrElement with
            | InterfaceElement(x) -> x.Type.Name
            | MethodElement(x) -> x.Method.Name
            | UnionElement(x) -> x.Type.Name
            | RecordElement(x) -> x.Type.Name
            | _ -> NotSupportedException(sprintf "I don't know how to infer the data object name for %O" clrElement) |> raise
        
        match clrElement |> ClrElement.getAttribute<DataObjectAttribute> with
        | Some(a) -> 
            let schemaName = clrElement |> inferSchemaName
            let localName = 
                match a.Name with
                | Some(x) -> 
                    x
                | None ->
                    inferFromElementName()
            DataObjectName(schemaName, localName)
                    
        | None ->
            let localName = inferFromElementName()
            let schemaName = clrElement |> inferSchemaName
            DataObjectName(schemaName, localName)

    
    /// <summary>
    /// Infers a <see cref"ColumnDescription"/>  from a CLR element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the column description will be inferred</param>
    let describeColumn(clrElement : ClrPropertyReference) =
        match clrElement.Property |> PropertyInfo.getAttribute<ColumnAttribute> with
        | Some(attrib) ->
            {
                ColumnDescription.Name = 
                    match attrib.Name with 
                    | Some(name) -> name
                    | None -> clrElement.Name.Text
                Position = clrElement.Position |> defaultArg attrib.Position 
                StorageType = clrElement |> PropertyElement |> inferStorageType
                Nullable = clrElement.Property.PropertyType |> Type.isOptionType
                AutoValue = None
            }

        | None ->
            {
                ColumnDescription.Name = clrElement.Name.Text
                Position = clrElement.Position
                StorageType = clrElement |> PropertyElement |> inferStorageType
                Nullable = clrElement.Property.PropertyType |> Type.isOptionType
                AutoValue = None
            }

    let private describeColumnProxy(clrElement : ClrPropertyReference) =
        let column = clrElement |> describeColumn
        ColumnProxyDescription(clrElement, column)

    /// <summary>
    /// Infers a collection of <see cref="ClrColumnProxyDescription"/> instances from a CLR type element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the column descriptions will be inferred</param>
    let private describeColumnProxies(clrType : ClrTypeReference) =
        let properties =
            match clrType with
            | RecordTypeReference(x) ->
                x.Fields 
            | InterfaceTypeReference(x) ->
                x.Members |> List.map(fun m -> 
                    match m with
                    | PropertyReference(p) -> p
                    | _ -> NotSupportedException() |> raise
                )
            | ClassTypeReference(x) ->
                x.Members |> List.map(fun m -> 
                    match m with
                    | PropertyReference(p) -> p
                    | _ -> NotSupportedException() |> raise
                )
            | UnionTypeReference(x) ->
                if x.Cases.Length = 1 then
                    //Assume that the columns are defined by the case fields
                    x.Cases.[0].Fields
                else
                    //Assume that the columns are defined by the case labels
                    [for case in x.Cases ->
                        if case.Fields.Length <> 1 then
                            NotSupportedException() |> raise
                        case.Fields.[0]
                    ]
        if properties.Length = 0 then
            NotSupportedException(sprintf "No columns were able to be inferred from the type %O" clrType) |> raise
        properties |> List.map describeColumnProxy

    /// <summary>
    /// Infers a collection of <see cref="ColumnDescription"/> instances from a CLR type element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the column descriptions will be inferred</param>
    let describeColumns (clrType : ClrTypeReference) =
        clrType |> describeColumnProxies |> List.map(fun x -> x.DataElement) 

    /// <summary>
    /// Infers a non-return RoutineParameterDescription from a CLR element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the parameter description will be inferred</param>
    let describeRoutineParameter(methparam  : ClrMethodParameterReference) =
        let name, direction, position = 
            match methparam.Subject.Element |> ParameterInfo.getAttribute<RoutineParameterAttribute> with
            | Some(attrib) ->
                let position =  match attrib.Position with |Some(p) -> p |None -> methparam.Position
                match attrib.Name with
                |Some(name) ->
                    name, attrib.Direction, position
                | None ->
                    methparam.Subject.Name.Text, attrib.Direction, position                
            | None ->
                (methparam.Subject.Name.Text, ParameterDirection.Input, methparam.Position)
        {
            RoutineParameterDescription.Name = name
            Position = position
            Direction = direction
            StorageType = methparam |> MethodParameterElement |> inferStorageType 
        }
    
    
    /// <summary>
    /// Infers a return RoutineParameterDescription from a CLR element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the parameter description will be inferred</param>
    let describeReturnParameter(methreturn : ClrMethodReturnReference) =
        let storageType = match methreturn.Method |> MethodInfo.getReturnAttribute<StorageTypeAttribute> with
                                | Some(attrib) -> 
                                    attrib |> inferStorageTypeFromAttribute
                                | None -> 
                                    methreturn.ReturnType |> Option.get |> inferStorageTypeFromClrType
        match methreturn.Method |> MethodInfo.getReturnAttribute<RoutineParameterAttribute> with            
        |Some(attrib) ->
            let position =  match attrib.Position with |Some(p) -> p |None -> -1
            {
                RoutineParameterDescription.Name = attrib.Name |> Option.get                   
                Direction = ParameterDirection.Output //Must always be output so value in attribute can be ignored
                StorageType = storageType
                Position = position
            }
        | None ->
            //No attribute, so assume stored procedure return value
            {
                RoutineParameterDescription.Name = "Return"
                Position = -1
                Direction = ParameterDirection.ReturnValue
                StorageType = storageType
            }
    

                                                 
    /// <summary>
    /// Constructs a <see cref="ParameterProxyDescription"/> based on the CLR element that will proxy it
    /// </summary>
    /// <param name="clrElement">The field correlated with the proxy</param>
    let private describeParameterProxy  (clrElement : MethodInputOutputReference) =
        let parameter =
            match clrElement with
            | MethodInputReference(methparam) ->
                methparam |> describeRoutineParameter
            | MethodOutputReference(methreturn) ->
                methreturn |> describeReturnParameter
        ParameterProxyDescription(clrElement, parameter)
                
    let describeProcedureCallProxy(proxy : ClrElementReference) =
        let objectName = proxy |> inferDataObjectName
        match proxy with
        | MethodElement(m) ->        
            let parameters = 
                [for p in m.Parameters do
                    yield p |> MethodInputReference
                 
                 if m.Return.ReturnType |> Option.isSome then
                    yield m.Return |> MethodOutputReference                     
                ]  |> List.map describeParameterProxy
            
            let procedure = {
                ProcedureDescription.Name = objectName
                Parameters = parameters |> List.map(fun p -> p.DataElement)
            }            
            ProcedureCallProxyDescription(m, procedure, parameters)

        | _ ->
            NotSupportedException() |> raise
                                                                                                        
    /// <summary>
    /// Infers a Table Proxy Description
    /// </summary>
    /// <param name="proxyType">The type of proxy</param>
    let describeTableProxy(clrType : ClrTypeReference) =
        let objectName = clrType |> ClrElement.fromTypeRef |> inferDataObjectName
        let columnProxies = clrType |> describeColumnProxies
        let table = {
            TableDescription.Name = objectName
            Description = clrType.Type |> getMemberDescription
            Columns = columnProxies |> List.map(fun p -> p.DataElement)
        }
        TableProxyDescription(clrType, table, columnProxies)
    
    let describeTableFunctionProxy(clrElement : ClrElementReference) =
        let objectName = clrElement |> inferDataObjectName
        let parameterProxies, columnProxies, clrMethod, returnType =
            match clrElement with
            | MethodElement(m) -> 
                m.Parameters |> List.map MethodInputReference |> List.map describeParameterProxy, 
                (match m.Return.ReturnType with
                | Some(t) ->
                    t |> ClrType.reference |> describeColumnProxies 
                | None ->
                    NotSupportedException() |> raise),
                m,
                m.Return.ReturnType |> Option.get |> ClrType.reference
            | _ ->
                NotSupportedException() |> raise
        let tableFunction = {
            TableFunctionDescription.Name = objectName   
            Parameters = parameterProxies|> List.map(fun p -> p.DataElement)
            Columns = columnProxies |> List.map(fun c -> c.DataElement)
        }
        let callProxy = TableFunctionCallProxyDescription(clrMethod, tableFunction, parameterProxies)
        let resultProxy = TabularResultProxyDescription(returnType, tableFunction |> TableFunction, columnProxies)
        TableFunctionProxy(callProxy, resultProxy)

/// <summary>
/// Convenience methods/operators intended to minimize syntactic clutter
/// </summary>
[<AutoOpen>]
module DataProxyOperators =    
    let tableproxy<'T> =
        typeref<'T> |> DataProxyMetadata.describeTableProxy
       
    
    let procproxies<'T> =        
        
        interfaceref<'T>.Members |> List.mapi (fun i m ->  
            match m with
            | MethodReference(m) ->
                 m |> MethodElement |> DataProxyMetadata.describeProcedureCallProxy
            | _ ->
                NotSupportedException() |> raise                
        )
        
        
                  



        