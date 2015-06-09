namespace IQ.Core.Data.Sql

open System
open System.ComponentModel
open System.Reflection
open System.Data

open IQ.Core.Framework

[<AutoOpen>]
module SqlMetamodelVocabulary =
    
    type DataObjectName = DataObjectName of schemaName : string * localName : string

    /// <summary>
    /// Base type for attributes that identify data elements
    /// </summary>
    [<AbstractClass>]
    type DataElementAttribute(name) =
        inherit Attribute()

        /// <summary>
        /// Gets the local name of the element, if specified
        /// </summary>
        member this.Name = 
            if name <> String.Empty then Some(name) else None
    
    /// <summary>
    /// Base type for attributes that identify data objects
    /// </summary>
    [<AbstractClass>]
    type DataObjectAttribute(schemaName, localName) =
        inherit DataElementAttribute(localName)

        new(schemaName) =
            DataObjectAttribute(schemaName, String.Empty)

        /// <summary>
        /// Gets the name of the schema in which the object resides, if specified
        /// </summary>
        member this.SchemaName = 
            if schemaName <> String.Empty then Some(schemaName) else None
        

    /// <summary>
    /// Identifies a schema when applied
    /// </summary>
    type SchemaAttribute(schemaName) =
        inherit DataElementAttribute(schemaName)        

    
    /// <summary>
    /// Identifies a table when applied
    /// </summary>
    type TableAttribute(schemaName, localName) =
        inherit DataObjectAttribute(schemaName,localName)

        new (schemaName) =
            TableAttribute(schemaName, String.Empty)

    /// <summary>
    /// Identifies a view when applied
    /// </summary>
    type ViewAttribute(schemaName, localName) =
        inherit DataObjectAttribute(schemaName,localName)

        new (schemaName) =
            ViewAttribute(schemaName, String.Empty)

    /// <summary>
    /// Identifies a column when applied
    /// </summary>
    type ColumnAttribute(name, position, isComputed, isIdentity, hasDefault,sequenceName,dbType) =
        inherit DataElementAttribute(name)

        new (dbType) =
            ColumnAttribute(String.Empty, -1, false, false, false, String.Empty, dbType)
        
        new (name, position, dbType) =
            ColumnAttribute(name, position, false, false, false, String.Empty, enum<SqlDbType>(-1))        
        
        new (name, sequenceName) =
            ColumnAttribute(name, -1, false, false, false, sequenceName, enum<SqlDbType>(-1))
                
        new (name, position, isComputed, isIdentity, hasDefault) =
            ColumnAttribute(name, position, isComputed, isIdentity, hasDefault, String.Empty, enum<SqlDbType>(-1)) 
        
        new (name, position) =
            ColumnAttribute(name, position, false, false, false, String.Empty, enum<SqlDbType>(-1))        

        new(name) =
            ColumnAttribute(name, -1, false, false, false, String.Empty, enum<SqlDbType>(-1))
        
        new() =
            ColumnAttribute(String.Empty, -1, false, false, false, String.Empty, enum<SqlDbType>(-1))

        /// The name of the represented column if specified
        member this.Name = if String.IsNullOrWhiteSpace(name) then None else Some(name)
        
        /// The position of the represented column if specified
        member this.Position = if position = -1 then None else Some(position)

        /// Specifies whether column is computed
        member this.IsComputed = isComputed

        /// Specifies whether the column is an identity column
        member this.IsIdentity = isIdentity
        
        /// Specifies whether the column has a default value 
        member this.HasDefault = hasDefault
        
        /// Specifies the sequence from which the column obtains its value if specified
        member this.SequenceName = if String.IsNullOrWhiteSpace(sequenceName) then None else Some(sequenceName)
        
        /// Specifies the database engine type of the represented column
        member this.DbType = if dbType = enum<SqlDbType>(-1) then None else Some(dbType)
            

        


    /// <summary>
    /// Describes a data type
    /// </summary>
    type DataTypeDescription = {
        
        /// The data type's identifier
        Id : int
        
        /// The name of the data type
        Name : DataObjectName
        
        /// Specifies whether the data type is user-defined
        IsUserDefined : bool

        /// The maximum length of a value
        MaxLength : int
    
        /// Specifies whether values of the type can be null
        IsNullable : bool
        
        /// The allowable precision of a value of the data type if applicable
        Precision : uint8

        /// The allowable scale of a value of the data type if applicable
        Scale : uint8
        
        /// If applicable, the intrinsic base type
        BaseType : DataTypeDescription option

        /// If applicable, the SqlDbType of the data type
        DbType : SqlDbType option
    }

    /// <summary>
    /// Represents a data type usage instance, such as when declaring a column to be of a given data type
    /// </summary>
    type DataTypeReference = {
        
        /// The data type being referenced
        DataType : DataTypeDescription
        
        /// If applicable, the maximum length of a value for variable-length data types or
        /// the absolute length of a value for fixed length data types
        MaxLength : int option

        /// If applicable, the precision of the data type reference
        Precision : uint8 option

        /// If applicable, the scale of the data type reference
        Scale : uint8 option        
    }
    
    /// <summary>
    /// Describes a column in a table or view
    /// </summary>
    type ColumnDescription = {
        /// The column's name
        Name : string
        
        /// The column's position relative to the other columns
        Position : int

        /// The column's data type
        DataType : DataTypeReference
        
        /// Specifies whether the column allows null
        Nullable : bool        
    }

    /// <summary>
    /// Describes a table
    /// </summary>
    type TableDescription = {
        /// The name of the table
        Name : DataObjectName
        
        /// Specifies the  purpose of the table
        Description : string option

        /// The columns in the table
        Columns : ColumnDescription list
    }

module SqlProxyMetadataProvider =
    
    let private getMemberDescription(m : MemberInfo) =
        if Attribute.IsDefined(m, typeof<DescriptionAttribute>) then
            (Attribute.GetCustomAttribute(m, typeof<DescriptionAttribute>) :?> DescriptionAttribute).Description |> Some
        else
            None
    
    
//    let describeColumn(proxy : PropertyInfo) =
//        match proxy |> ClrProperty.getAttribute<ColumnAttribute> with
//        |Some(attrib) ->
            
            


    
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
            Name = objectName
            Description = proxy |> getMemberDescription
            Columns = []
        }

[<AutoOpen>]
module SqlProxyMetadataOperators =    
    let tableinfo<'T> =
        typeof<'T> |> SqlProxyMetadataProvider.describeTable        
        