namespace IQ.Core.Data.Sql

open System

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

    
    type DataTypeDescription = {
        Name : string
        Id : int
        UserDefined : bool
    }

    type DataTypeReference = {
        DataType : DataTypeDescription
        Length : int option
        Precision : uint8 option
        Scale : uint8 option        
    }
    

    type ColumnDescription = {
        /// The column's name
        Name : string
        
        /// The column's data type
        DataType : DataTypeReference
        
        /// Specifies whether the column allows null
        Nullable : bool        
    }
