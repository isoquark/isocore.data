namespace IQ.Core.Data

open System
open System.ComponentModel
open System.Reflection
open System.Data

open FSharp.Data

open IQ.Core.Framework

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


    let private inferStorageType(field : RecordFieldDescription) =
        match field.Property |> ClrProperty.getAttribute<StorageTypeAttribute> with
        | Some(attrib) ->
            attrib |> DataStorageType.fromAttribute
        | None ->
            if defaultClrStorageTypeMap.ContainsKey(field.DataType) then
                defaultClrStorageTypeMap.[field.DataType]
            else
                NotSupportedException(sprintf "No default mapping exists for the %O type" field.DataType) |> raise
                                 
    /// <summary>
    /// Infers a Column Proxy Description
    /// </summary>
    /// <param name="field">The field correlated with the proxy</param>
    let private describeColumnProxy(field : RecordFieldDescription) =
        let column = 
            match field.Property |> ClrProperty.getAttribute<ColumnAttribute> with
            | Some(attrib) ->
                {
                    ColumnDescription.Name = 
                        match attrib.Name with 
                        | Some(name) -> name
                        | None -> field.Name
                    Position = field.Position |> defaultArg attrib.Position 
                    StorageType = field |> inferStorageType
                    Nullable = field.Property.PropertyType |> ClrType.isOptionType
                    AutoValue = None
                }

            | None ->
                {
                    ColumnDescription.Name = field.Name
                    Position = field.Position
                    StorageType = field |> inferStorageType 
                    Nullable = field.Property.PropertyType |> ClrType.isOptionType
                    AutoValue = None
                }
        ColumnProxyDescription(field, column)    
    
    /// <summary>
    /// Infers a Table Proxy Description
    /// </summary>
    /// <param name="proxyType">The type of proxy</param>
    let describeTable(proxyType : Type) =
        let getSchemaName(declaringType) =
            match declaringType |> ClrType.getAttribute<SchemaAttribute> with
            | Some(a) ->
                match a.Name with
                | Some(name) -> 
                    name
                | None -> 
                    declaringType.Name
            | None ->
                declaringType.Name
            
        let objectName = 
            match proxyType |>  ClrType.getAttribute<TableAttribute> with
                | Some(a) -> 
                    let schemaName = 
                        match a.SchemaName with
                        | Some(schemaName) -> 
                            schemaName
                        | None ->
                            proxyType.DeclaringType |> getSchemaName
                    let tableName = 
                        match a.Name with
                        | Some(tableName) -> 
                            tableName
                        | None ->
                            proxyType.Name
                    DataObjectName(schemaName, tableName)
                    
                | None ->
                    let tableName = proxyType.Name
                    let schemaName = proxyType.DeclaringType |> getSchemaName
                    DataObjectName(schemaName, tableName)
        
        let record = proxyType |> ClrRecord.describe
        let columnProxies = record.Fields |> List.map(fun field -> field |> describeColumnProxy)
        let table = {
            TableDescription.Name = objectName
            Description = proxyType |> getMemberDescription
            Columns = columnProxies |> List.map(fun p -> p.Column)
        }
        TableProxyDescription(record, table, columnProxies)

/// <summary>
/// Convenience methods/operators intended to minimize syntactic clutter
/// </summary>
[<AutoOpen>]
module DataProxyOperators =    
    let tableproxy<'T> =
        typeof<'T> |> DataProxyMetadataProvider.describeTable        
        