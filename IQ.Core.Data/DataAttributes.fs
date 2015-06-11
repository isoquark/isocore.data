namespace IQ.Core.Data

open System
open System.Data

/// <summary>
/// Defines attributes that are intended to be applied to proxy elements to specify 
/// data source characteristics that cannot otherwise be inferred
/// </summary>
[<AutoOpen>]
module DataAttributes =
    
    [<Literal>]
    let private UnspecifiedName = ""
    [<Literal>]
    let private UnspecifiedPrecision = -1y
    [<Literal>]
    let private UnspecifiedScale = -1y
    [<Literal>]
    let private UnspecifiedLength = -1
    [<Literal>]
    let private UnspecifiedStorage = StorageKind.Unspecified
    let private UnspecifiedType = Unchecked.defaultof<Type>
    [<Literal>]
    let private UnspecifiedPosition = -1
    [<Literal>]
    let private UnspecifiedAutoValue = AutoValueKind.None
    
    /// <summary>
    /// Base type for attributes that participate in the data attribution system
    /// </summary>
    [<AbstractClass>]
    type DataAttribute() =
        inherit Attribute()

    /// <summary>
    /// Base type for attributes that identify data elements
    /// </summary>
    [<AbstractClass>]
    type DataElementAttribute(name) =
        inherit DataAttribute()

        /// <summary>
        /// Gets the local name of the element, if specified
        /// </summary>
        member this.Name = 
            if name = UnspecifiedName then None else Some(name)
    
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
            DataObjectAttribute(schemaName, UnspecifiedName)

        /// <summary>
        /// Gets the name of the schema in which the object resides, if specified
        /// </summary>
        member this.SchemaName = 
            if schemaName = UnspecifiedName then None else Some(schemaName)
        

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
            TableAttribute(schemaName, UnspecifiedName)

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
            ViewAttribute(schemaName, UnspecifiedName)
                   
    /// <summary>
    /// Specifies storage characteristics
    /// </summary>
    type StorageTypeAttribute(storageKind, length, precision, scale, clrType, customTypeSchemaName, customTypeName) =
        inherit DataAttribute()

        //For any kind of data that doesn't require additional information to instantiate a value, e.g., Int32, Bit and so forth
        new (storageKind) =
            StorageTypeAttribute(storageKind, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale, UnspecifiedType, UnspecifiedName, UnspecifiedName)
        //For variable-length or fixed-length text and binary data types that whose length has a specified upper bound
        new (storageKind, length) =
            StorageTypeAttribute(storageKind, length, UnspecifiedPrecision, UnspecifiedScale, UnspecifiedType, UnspecifiedName, UnspecifiedName)                        
        //For data types that have a specifiable precision
        new (storageKind, precision) =
            StorageTypeAttribute(storageKind, UnspecifiedLength, precision, UnspecifiedScale, UnspecifiedType, UnspecifiedName, UnspecifiedName)                
        //For data types that have both specifiable precision and scale
        new (storageKind, precision, scale) =
            StorageTypeAttribute(storageKind, UnspecifiedLength, precision, scale, UnspecifiedType, UnspecifiedName, UnspecifiedName)
        //For Geography, Geometry and Hierarchy types
        new (storageKind, clrType) =
            StorageTypeAttribute(storageKind, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale, clrType, UnspecifiedName, UnspecifiedName)
        //For CustomObject
        new (storageKind, clrType, customTypeSchemaName, customTypeName) =
            StorageTypeAttribute(storageKind, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale,  clrType, customTypeSchemaName, customTypeName)
        //For CustomTable | CustomPrimitive
        new (storageKind, customTypeSchemaName, customTypeName) =
            StorageTypeAttribute(storageKind, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale,  UnspecifiedType, customTypeSchemaName, customTypeName)

        /// Indicates the kind of storage
        member this.StorageKind = if storageKind = UnspecifiedStorage then None else Some(storageKind)
        
        /// Indicates the length of the data type if specified
        member this.Length = if length = UnspecifiedLength then None else Some(length)

        /// Indicates the precision of the data type if specified
        member this.Precision = if precision = UnspecifiedPrecision then None else Some(precision)

        /// Indicates the scale of the data type if specified
        member this.Scale = if precision = UnspecifiedScale then None else Some(scale)

        /// Indicates the CLR data type, if specified
        member this.ClrType = if clrType = UnspecifiedType then None else Some(clrType)

        /// Indicates the name of a custom type, if specified
        member this.CustomTypeName = 
            if (customTypeSchemaName = UnspecifiedName || customTypeName = UnspecifiedName) then 
                None 
            else
                DataObjectName(customTypeSchemaName, customTypeName) |> Some

    /// <summary>
    /// Identifies a column and specifies selected storage characteristics
    /// </summary>
    type ColumnAttribute(name, position, autoValueKind) =
        inherit DataElementAttribute(name)

        new(name) =
            ColumnAttribute(name, UnspecifiedPosition, UnspecifiedAutoValue)        
        new (name, position) =
            ColumnAttribute(name, position, UnspecifiedAutoValue)                
        new() =
            ColumnAttribute(UnspecifiedName, UnspecifiedPosition, UnspecifiedAutoValue)

        /// Indicates the name of the represented column if specified
        member this.Name = if String.IsNullOrWhiteSpace(name) then None else Some(name)
        
        /// Indicates the position of the represented column if specified
        member this.Position = if position = UnspecifiedPosition then None else Some(position)

        /// Indicates the means by which the column is automatically populated if specified
        member this.AutoValue = if autoValueKind = UnspecifiedAutoValue then None else Some(autoValueKind)
                
    /// <summary>
    /// Identifies a sequence that will yield a value for the element to which
    /// the attribute is attached
    /// </summary>
    type SourceSequenceAttribute(schemaName, localName) =
        inherit DataAttribute()

        /// <summary>
        /// Gets the name of the schema in which the sequence resides
        /// </summary>
        member this.SchemaName = 
            if schemaName = UnspecifiedName then None else Some(schemaName)

        /// <summary>
        /// Gets the local name of the element
        /// </summary>
        member this.Name = 
            if localName = UnspecifiedName then None else Some(localName)
            

     


