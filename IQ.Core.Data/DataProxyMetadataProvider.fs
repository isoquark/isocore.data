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
module DataProxyMetadataProvider =    
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
        if defaultClrStorageTypeMap.ContainsKey(t.ValueType) then
            defaultClrStorageTypeMap.[t.ValueType]
        else
            NotSupportedException(sprintf "No default mapping exists for the %O type" t.ValueType) |> raise

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

    let private inferStorageType(element : ClrElementReference) =
        match element |> ClrElement.getAttribute<StorageTypeAttribute> with
        | Some(attrib) -> 
            attrib |> inferStorageTypeFromAttribute
        | None ->            
            element |> inferStorageTypeFromClrElement                                             
    /// <summary>
    /// Infers a Column Proxy Description
    /// </summary>
    /// <param name="field">The field correlated with the proxy</param>
    let private describeColumnProxy(field : ClrPropertyReference) =
        let column = 
            match field.Property |> PropertyInfo.getAttribute<ColumnAttribute> with
            | Some(attrib) ->
                {
                    ColumnDescription.Name = 
                        match attrib.Name with 
                        | Some(name) -> name
                        | None -> field.Name.Text
                    Position = field.Position |> defaultArg attrib.Position 
                    StorageType = field |> PropertyElement |> inferStorageType
                    Nullable = field.Property.PropertyType |> Type.isOptionType
                    AutoValue = None
                }

            | None ->
                {
                    ColumnDescription.Name = field.Name.Text
                    Position = field.Position
                    StorageType = field |> PropertyElement |> inferStorageType
                    Nullable = field.Property.PropertyType |> Type.isOptionType
                    AutoValue = None
                }
        ColumnProxyDescription(field, column)    

            

    let private describeIOProxy i (proxy : MethodInputOutputReference) =
        match proxy with
        | MethodInputReference(inputref) ->
            let name, direction, position = 
                match inputref.Subject.Element |> ParameterInfo.getAttribute<RoutineParameterAttribute> with
                | Some(attrib) ->
                    let position =  match attrib.Position with |Some(p) -> p |None -> i
                    match attrib.Name with
                    |Some(name) ->
                        name, attrib.Direction, position
                    | None ->
                        inputref.Subject.Name.Text, attrib.Direction, position                
                | None ->
                    (inputref.Subject.Name.Text, ParameterDirection.Input, i)
            let parameter = {
                RoutineParameter.Name = name
                Position = position
                Direction = direction
                StorageType = inputref |> MethodParameterElement |> inferStorageType 
            }
            ParameterProxyDescription(proxy, parameter)
        | MethodOutputReference(outputref) ->
            let storageType = match outputref.Method |> MethodInfo.getReturnAttribute<StorageTypeAttribute> with
                                  | Some(attrib) -> 
                                        attrib |> inferStorageTypeFromAttribute
                                  | None -> 
                                        outputref.ReturnType |> Option.get |> inferStorageTypeFromClrType
            match outputref.Method |> MethodInfo.getReturnAttribute<RoutineParameterAttribute> with            
            |Some(attrib) ->
                let position =  match attrib.Position with |Some(p) -> p |None -> i
                let parameter = {
                    RoutineParameter.Name = attrib.Name |> Option.get                   
                    Direction = ParameterDirection.Output //Must always be output so value in attribute can be ignored
                    StorageType = storageType
                    Position = position
                }
                ParameterProxyDescription(proxy, parameter)
            | None ->
                //No attribute, so assume stored procedure return value
                let parameter = {
                    RoutineParameter.Name = "Return"
                    Position = i
                    Direction = ParameterDirection.ReturnValue
                    StorageType = storageType
                }
                ParameterProxyDescription(proxy, parameter)
                


    let private getSchemaName(proxy : ClrElementReference) =
        match proxy |> ClrElement.getAttribute<SchemaAttribute> with
        | Some(a) ->
            match a.Name with
            | Some(name) -> 
                name
            | None -> 
                proxy.Name.Text
        | None ->
            proxy.Name.Text
            

    let private getObjectName(proxy : ClrElementReference) =
        match proxy |> ClrElement.getAttribute<DataObjectAttribute> with
                | Some(a) -> 
                    let schemaName = 
                        match a.SchemaName with
                        | Some(x) -> 
                            x
                        | None ->
                            proxy |> ClrElement.getDeclaringElement |> Option.get |> getSchemaName
                    let localName = 
                        match a.Name with
                        | Some(x) -> 
                            x
                        | None ->
                            proxy.Name.Text
                    DataObjectName(schemaName, localName)
                    
                | None ->
                    let localName = proxy.Name.Text
                    let schemaName = proxy |> ClrElement.getDeclaringElement |> Option.get |> getSchemaName
                    DataObjectName(schemaName, localName)

    let describeProcedureProxy(proxy : ClrElementReference) =
        let objectName = proxy |> getObjectName
        match proxy with
        | MethodElement(m) ->        
            let parameters = 
                [for p in m.Parameters do
                    yield p |> MethodInputReference
                 
                 if m.Return.ReturnType |> Option.isSome then
                    yield m.Return |> MethodOutputReference                     
                ]  |> List.mapi describeIOProxy
            
            let procedure = {
                ProcedureDescription.Name = objectName
                Parameters = parameters |> List.map(fun p -> p.DataElement)
            }            
            ProcedureProxyDescription(m, procedure, parameters)

        | _ ->
            NotSupportedException() |> raise
                    

    
    /// <summary>
    /// Infers a Table Proxy Description
    /// </summary>
    /// <param name="proxyType">The type of proxy</param>
    let describeTable(proxy : Type) =
        let getSchemaName2(t : Type) =
            match t |> Type.getAttribute<SchemaAttribute> with
            | Some(a) ->
                match a.Name with
                | Some(name) -> 
                    name
                | None -> 
                    t.Name
            | None ->
                t.Name
            
        let objectName = 
            match proxy |>  Type.getAttribute<DataObjectAttribute> with
                | Some(a) -> 
                    let schemaName = 
                        match a.SchemaName with
                        | Some(x) -> 
                            x
                        | None ->
                            proxy.DeclaringType |> getSchemaName2
                    let localName = 
                        match a.Name with
                        | Some(x) -> 
                            x
                        | None ->
                            proxy.Name
                    DataObjectName(schemaName, localName)
                    
                | None ->
                    let localName = proxy.Name
                    let schemaName = proxy.DeclaringType |> getSchemaName2
                    DataObjectName(schemaName, localName)
        
        let record = proxy |> ClrRecord.reference
        let columnProxies = record.Fields |> List.map(fun field -> field |> describeColumnProxy)
        let table = {
            TableDescription.Name = objectName
            Description = proxy |> getMemberDescription
            Columns = columnProxies |> List.map(fun p -> p.DataElement)
        }
        TableProxyDescription(record, table, columnProxies)
    

/// <summary>
/// Convenience methods/operators intended to minimize syntactic clutter
/// </summary>
[<AutoOpen>]
module DataProxyOperators =    
    let tableproxy<'T> =
        typeof<'T> |> DataProxyMetadataProvider.describeTable
       
    
    let procproxies<'T> =        
        
        interfaceref<'T>.Members |> List.mapi (fun i m ->  
            match m with
            | InterfaceMethodReference(m) ->
                 m |> MethodElement |> DataProxyMetadataProvider.describeProcedureProxy
            | _ ->
                NotSupportedException() |> raise                
        )
        
        
                  



        