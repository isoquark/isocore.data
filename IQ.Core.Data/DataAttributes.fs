namespace IQ.Core.Data

open System
open System.Data

/// <summary>
/// Defines attributes that are intended to be applied to proxy elements to specify 
/// data source characteristics that cannot otherwise be inferred
/// </summary>
[<AutoOpen>]
module DataAttributes =
    
    

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

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="schemaName">The name of the schema in which the object is defined</param>
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

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="schemaName">The name of the schema in which the view is defined</param>
        new (schemaName) =
            ViewAttribute(schemaName, String.Empty)


    [<Literal>]
    let private UnspecifiedPrecision = -1y

    [<Literal>]
    let private UnspecifiedScale = -1y

    [<Literal>]
    let private UnspecifiedLength = -1
       
    [<Literal>]
    let private UnspecifiedPosition = -1

    [<Literal>]
    let private UnspecifiedName = ""

    [<Literal>]
    let private UnspecifiedStorage = StorageKind.Unspecified

    [<Literal>]
    let private UnspecifiedAutoValue = AutoValueKind.None


    /// <summary>
    /// Identifies a column and specifies selected storage characteristics
    /// </summary>
    type ColumnAttribute(name, position, autoValueKind, sequenceName, storageKind, length, precision, scale) =
        inherit DataElementAttribute(name)

        //Lots of constructors for convenient usage
        new (storageKind) =
            ColumnAttribute(UnspecifiedName, UnspecifiedPosition, UnspecifiedAutoValue, UnspecifiedName, storageKind, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale)                
        new (storageKind, length) =
            ColumnAttribute(UnspecifiedName, UnspecifiedPosition, UnspecifiedAutoValue, UnspecifiedName, storageKind, length, UnspecifiedPrecision, UnspecifiedScale)            
        new (storageKind, precision) =
            ColumnAttribute(UnspecifiedName, UnspecifiedPosition, UnspecifiedAutoValue, UnspecifiedName, storageKind, UnspecifiedLength, precision, UnspecifiedScale)
        new (storageKind, precision, scale) =
            ColumnAttribute(UnspecifiedName, UnspecifiedPosition, UnspecifiedAutoValue, UnspecifiedName, storageKind, UnspecifiedLength, precision, scale)

        new(name) =
            ColumnAttribute(name, UnspecifiedPosition, UnspecifiedAutoValue, UnspecifiedName, UnspecifiedStorage, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale)        
        new (name, storageKind) =
            ColumnAttribute(name, UnspecifiedPosition, UnspecifiedAutoValue, UnspecifiedName, storageKind, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale)
        new (name, storageKind, length) =
            ColumnAttribute(name, UnspecifiedPosition, UnspecifiedAutoValue, UnspecifiedName, storageKind, length, UnspecifiedPrecision, UnspecifiedScale)            
        new (name, storageKind, precision) =
            ColumnAttribute(name, UnspecifiedPosition, UnspecifiedAutoValue, UnspecifiedName, storageKind, UnspecifiedLength, precision, UnspecifiedScale)            
        new (name, storageKind, precision, scale) =
            ColumnAttribute(name, UnspecifiedPosition, UnspecifiedAutoValue, UnspecifiedName, storageKind, UnspecifiedLength, precision, scale)                    
        
        new (name, sequenceName) =
            ColumnAttribute(name, UnspecifiedPosition, UnspecifiedAutoValue, sequenceName, UnspecifiedStorage, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale)                
        new (name, position, autoValueKind) =
            ColumnAttribute(name, position, autoValueKind, UnspecifiedName, UnspecifiedStorage, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale)         
        new (name, position) =
            ColumnAttribute(name, position, UnspecifiedAutoValue, UnspecifiedName, UnspecifiedStorage, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale)
        
        new() =
            ColumnAttribute(UnspecifiedName, UnspecifiedPosition, UnspecifiedAutoValue, UnspecifiedName, UnspecifiedStorage, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale)

        /// Indicates the name of the represented column if specified
        member this.Name = if String.IsNullOrWhiteSpace(name) then None else Some(name)
        
        /// Indicates the position of the represented column if specified
        member this.Position = if position = UnspecifiedPosition then None else Some(position)

        /// Indicates the means by which the column is automatically populated if specified
        member this.AutoValue = if autoValueKind = UnspecifiedAutoValue then None else Some(autoValueKind)
                
        /// Indicates the sequence from which the column obtains its value if specified
        member this.SequenceName = if String.IsNullOrWhiteSpace(sequenceName) then None else Some(sequenceName)
        
        /// Indicates the kind of storage needed by the column if specified
        member this.StorageKind = if storageKind = UnspecifiedStorage then None else Some(storageKind)
        
        /// Indicates the length of the data type if specified
        member this.Length = if length = UnspecifiedLength then None else Some(length)

        /// Indicates the precision of the data type if specified
        member this.Precision = if precision = UnspecifiedPrecision then None else Some(precision)

        /// Indicates the scale of the data type if specified
        member this.Scale = if precision = UnspecifiedScale then None else Some(scale)
            

     


