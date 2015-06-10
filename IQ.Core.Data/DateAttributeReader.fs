namespace IQ.Core.Data

open System
open System.ComponentModel
open System.Reflection
open System.Data

open IQ.Core.Framework


module DataAttributeReader =
    
    let private getMemberDescription(m : MemberInfo) =
        if Attribute.IsDefined(m, typeof<DescriptionAttribute>) then
            (Attribute.GetCustomAttribute(m, typeof<DescriptionAttribute>) :?> DescriptionAttribute).Description |> Some
        else
            None

        
                                 
//    let describeColumn(proxy : RecordFieldDescription) =
//        match proxy.Property |> ClrProperty.getAttribute<ColumnAttribute> with
//        | Some(attrib) ->
//            {
//                ColumnDescription.Name = 
//                    match attrib.Name with 
//                    | Some(name) -> name
//                    | None -> proxy.Name
//                Position = proxy.Position |> defaultArg attrib.Position 
//                DataType = proxy.FieldType
//            }
//
//        | None ->
//            ()
    
    /// <summary>
    /// Infers a table description from a proxy
    /// </summary>
    /// <param name="proxyType">The type of proxy</param>
    let describeTable(proxy : Type) =
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
            match proxy |>  ClrType.getAttribute<TableAttribute> with
                | Some(a) -> 
                    let schemaName = 
                        match a.SchemaName with
                        | Some(schemaName) -> 
                            schemaName
                        | None ->
                            proxy.DeclaringType |> getSchemaName
                    let tableName = 
                        match a.Name with
                        | Some(tableName) -> 
                            tableName
                        | None ->
                            proxy.Name
                    DataObjectName(schemaName, tableName)
                    
                | None ->
                    let tableName = proxy.Name
                    let schemaName = proxy.DeclaringType |> getSchemaName
                    DataObjectName(schemaName, tableName)
        {
            TableDescription.Name = objectName
            Description = proxy |> getMemberDescription
            Columns = []
        }

[<AutoOpen>]
module DataProxyOperators =    
    let tableinfo<'T> =
        typeof<'T> |> DataAttributeReader.describeTable        
        