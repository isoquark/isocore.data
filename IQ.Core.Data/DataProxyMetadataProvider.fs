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
                Type.GetType(row.ClrTypeName, true), row.StorageType |> DataStorageType.parse |> Option.get
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

    let private inferStorageType(element : ClrElement) =
        match element |> ClrElement.getAttribute<StorageTypeAttribute> with
        | Some(attrib) -> 
            attrib |> DataStorageType.fromAttribute
        | None ->            
            let valueType =
                match element with
                | PropertyElement(x) -> x.PropertyType.ValueType
                | MethodParameterElement(x) -> x.ParameterType.ValueType
                | PropertyFieldElement(x) -> x.ValueType
                | _ ->
                    NotSupportedException() |> raise
            if defaultClrStorageTypeMap.ContainsKey(valueType) then
                defaultClrStorageTypeMap.[valueType]
            else
                NotSupportedException(sprintf "No default mapping exists for the %O type" valueType) |> raise
                                             
    /// <summary>
    /// Infers a Column Proxy Description
    /// </summary>
    /// <param name="field">The field correlated with the proxy</param>
    let private describeColumnProxy(field : PropertyFieldDescription) =
        let column = 
            match field.Property |> PropertyInfo.getAttribute<ColumnAttribute> with
            | Some(attrib) ->
                {
                    ColumnDescription.Name = 
                        match attrib.Name with 
                        | Some(name) -> name
                        | None -> field.Name
                    Position = field.Position |> defaultArg attrib.Position 
                    StorageType = field |> PropertyFieldElement |> inferStorageType
                    Nullable = field.Property.PropertyType |> Type.isOptionType
                    AutoValue = None
                }

            | None ->
                {
                    ColumnDescription.Name = field.Name
                    Position = field.Position
                    StorageType = field |> PropertyFieldElement |> inferStorageType
                    Nullable = field.Property.PropertyType |> Type.isOptionType
                    AutoValue = None
                }
        ColumnProxyDescription(field, column)    

    

    let private describeParameterProxy i (proxy : MethodParameterDescription) =
        let name, direction, position = 
            match proxy.Parameter |> ParameterInfo.getAttribute<RoutineParameterAttribute> with
            | Some(attrib) ->
                let position =  match attrib.Position with |Some(p) -> p |None -> i
                match attrib.Name with
                |Some(name) ->
                    name, attrib.Direction, position
                | None ->
                    proxy.Name, attrib.Direction, position                
            | None ->
                (proxy.Name, ParameterDirection.Input, i)
        let parameter = {
            RoutineParameter.Name = name
            Direction = direction
            Position = position
            StorageType = proxy |> MethodParameterElement |> inferStorageType 
        }
        ParameterProxyDescription(proxy, parameter)
            

    let private getSchemaName(proxy : ClrElement) =
        match proxy |> ClrElement.getAttribute<SchemaAttribute> with
        | Some(a) ->
            match a.Name with
            | Some(name) -> 
                name
            | None -> 
                proxy.Name
        | None ->
            proxy.Name
            

    let private getObjectName(proxy : ClrElement) =
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
                            proxy.Name
                    DataObjectName(schemaName, localName)
                    
                | None ->
                    let localName = proxy.Name
                    let schemaName = proxy |> ClrElement.getDeclaringElement |> Option.get |> getSchemaName
                    DataObjectName(schemaName, localName)

    let describeProcedureProxy(proxy : ClrElement) =
        let objectName = proxy |> getObjectName
        match proxy with
        | MethodElement(m) ->        
            let parameters = m.Parameters |> List.mapi describeParameterProxy
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
        
        let record = proxy |> ClrRecord.describe
        let columnProxies = record.Fields |> List.map(fun field -> field |> describeColumnProxy)
        let table = {
            TableDescription.Name = objectName
            Description = proxy |> getMemberDescription
            Columns = columnProxies |> List.map(fun p -> p.DataElement)
        }
        TableProxyDescription(record, table, columnProxies)
    

//    let rec private getSchemaAttribute(t : Type) =
//        match t |> Type.getAttribute<SchemaAttribute> with
//        | Some(attrib) -> attrib |> Some
//        | None ->
//            if t.DeclaringType <> null then
//                t.DeclaringType |> getSchemaAttribute
//            else
//                None

    //let getSchemaName(proxy : MethodDescription) =
        
            

//    let describeProcedure(proxy : MethodDescription) =
//        match proxy.Method |> MethodInfo.getAttribute<ProcedureAttribute> with
//        | Some(procAttrib) ->
//           
//        | None ->
//            ()

/// <summary>
/// Convenience methods/operators intended to minimize syntactic clutter
/// </summary>
[<AutoOpen>]
module DataProxyOperators =    
    let tableproxy<'T> =
        typeof<'T> |> DataProxyMetadataProvider.describeTable
       
    let procproxy(m : MethodInfo) =
        m |> ClrMethod.describe |> MethodElement |> DataProxyMetadataProvider.describeProcedureProxy
    
    let procproxies<'T> =
        [for m in  (typeof<'T> |> ClrInterface.describe |> fun x -> x.Members) do
            match m with
            | InterfaceMethod(m) ->
                yield m.Method |> procproxy
            | _ ->
                ()
        ]
           



        