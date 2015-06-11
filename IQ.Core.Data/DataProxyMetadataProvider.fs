namespace IQ.Core.Data

open System
open System.ComponentModel
open System.Reflection
open System.Data

open IQ.Core.Framework


/// <summary>
/// Defines operations that populate a Data Proxy Metamodel
/// </summary>
module DataProxyMetadataProvider =    
    let private getMemberDescription(m : MemberInfo) =
        if Attribute.IsDefined(m, typeof<DescriptionAttribute>) then
            (Attribute.GetCustomAttribute(m, typeof<DescriptionAttribute>) :?> DescriptionAttribute).Description |> Some
        else
            None        
                                 
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
                    StorageType = None
                    Nullable = field.Property.PropertyType |> ClrType.isOptionType
                    AutoValue = None
                }

            | None ->
                {
                    ColumnDescription.Name = field.Name
                    Position = field.Position
                    StorageType = None
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
        