namespace IQ.Core.Data

open System
open System.Data

[<AutoOpen>]
module DataAttributes =
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
            

     


