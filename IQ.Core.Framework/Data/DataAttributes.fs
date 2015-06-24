namespace IQ.Core.Data

open System
open System.Data
open System.Collections.Generic

open FSharp.Data

open IQ.Core.Framework

/// <summary>
/// Defines attributes that are intended to be applied to proxy elements to specify 
/// data source characteristics that cannot otherwise be inferred
/// </summary>
[<AutoOpen>]
module DataAttributes =

    /// <summary>
    /// Specifies the available storage classes
    /// </summary>
    /// <remarks>
    /// Note that the storage class is not sufficient to characterize the storage type and
    /// additional information, such as length or data object name is needed to store/instantiate
    /// a corresponding value
    /// </remarks>
    type StorageKind =
        | Unspecified = 0uy
        | Bit = 10uy //bit
        | UInt8 = 20uy //tinyint
        | UInt16 = 21uy //no direct map, use int
        | UInt32 = 22uy // no direct map, use bigint
        | UInt64 = 23uy // no direct map, use varbinary(8)
        | Int8 = 30uy //no direct map, use smallint
        | Int16 = 31uy //smallint
        | Int32 = 32uy //int
        | Int64 = 33uy //bigint
        | BinaryFixed = 40uy //binary 
        | BinaryVariable = 41uy //varbinary
        | BinaryMax = 42uy
        | AnsiTextFixed = 50uy //char
        | AnsiTextVariable = 51uy //varchar
        | AnsiTextMax = 52uy
        | UnicodeTextFixed = 53uy //nchar
        | UnicodeTextVariable = 54uy //nvarchar
        | UnicodeTextMax = 55uy
        | DateTime32 = 60uy //corresponds to smalldatetime
        | DateTime64 = 61uy //corresponds to datetime
        | DateTime = 62uy //corresponds to datetime2
        | DateTimeOffset = 63uy
        | TimeOfDay = 64uy //corresponds to time
        | Date = 65uy //corresponds to date
        | Float32 = 70uy //corresponds to real
        | Float64 = 71uy //corresponds to float
        | Decimal = 80uy
        | Money = 81uy
        | Guid = 90uy //corresponds to uniqueidentifier
        | Xml = 100uy
        | Variant = 110uy //corresponds to sql_variant
        | CustomTable = 150uy //a non-intrinsic table data type
        | CustomObject = 151uy //a non-intrinsic CLR type
        | CustomPrimitive = 152uy //a non-intrinsic primitive based on an intrinsic primitive
        | Geography = 160uy
        | Geometry = 161uy
        | Hierarchy = 162uy

    /// <summary>
    /// Enumerates the available means that lead to a column being automatically populated
    /// with a valid value
    /// </summary>
    type AutoValueKind =
        /// Column is not automatically populated
        | None = 0
        /// Column is automatically populated with a default value
        | Default = 1
        /// Column is automatically populated with an identity value
        | Identity = 2
        /// Column is automatically populated with a computed value
        | Computed = 3
        /// Column is automatically populated with a value from a sequence
        | Sequence = 4
    
    [<Literal>]
    let private UnspecifiedName = ""
    [<Literal>]
    let private UnspecifiedPrecision = 201uy
    [<Literal>]
    let private UnspecifiedScale = 201uy
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
    /// Identifies a schema
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
    /// Identifies a view
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
        member this.StorageKind : StorageKind = storageKind
        
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
        new(autoValueKind) =
            ColumnAttribute(UnspecifiedName, UnspecifiedPosition, autoValueKind)

        /// Indicates the name of the represented column if specified
        member this.Name = if String.IsNullOrWhiteSpace(name) then None else Some(name)
        
        /// Indicates the position of the represented column if specified
        member this.Position = if position = UnspecifiedPosition then None else Some(position)

        /// Indicates the means by which the column is automatically populated if specified
        member this.AutoValue = if autoValueKind = UnspecifiedAutoValue then None else Some(autoValueKind)
                
    
    /// <summary>
    /// Identifies a stored procedure
    /// </summary>
    type ProcedureAttribute(schemaName, localName) =
        inherit DataObjectAttribute(schemaName, localName)

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="schemaName">The name of the schema in which the procedure is defined</param>
        new(schemaName) =
            ProcedureAttribute(schemaName, UnspecifiedName)

        new() =
            ProcedureAttribute(UnspecifiedName, UnspecifiedName)

    /// <summary>
    /// Identifies a table-valued function
    /// </summary>
    type TableFunctionAttribute(schemaName, localName) = 
        inherit DataObjectAttribute(schemaName, localName)

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="schemaName">The name of the schema in which the procedure is defined</param>
        new(schemaName) =
            TableFunctionAttribute(schemaName, UnspecifiedName)

        new() =
            TableFunctionAttribute(UnspecifiedName, UnspecifiedName)


    /// <summary>
    /// Identifies a routine parameter
    /// </summary>
    type RoutineParameterAttribute(name, direction, position) =
        inherit DataElementAttribute(name)

        new (name) =
            RoutineParameterAttribute(name, ParameterDirection.Input, UnspecifiedPosition)

        new (name, position) =
            RoutineParameterAttribute(name, ParameterDirection.Input, position)

        new (direction) =
            RoutineParameterAttribute(UnspecifiedName, direction, UnspecifiedPosition)

        new (direction, position) =
            RoutineParameterAttribute(UnspecifiedName, direction, position)

        new (position) =
            RoutineParameterAttribute(UnspecifiedName, ParameterDirection.Input, position)

        new (name,direction) =
            RoutineParameterAttribute(name, direction, UnspecifiedPosition)

        /// <summary>
        /// Gets the direction of the parameter
        /// </summary>
        member this.Direction = direction

        /// <summary>
        /// Gets the parameter's ordinal position
        /// </summary>
        member this.Position = if position = UnspecifiedPosition then None else position |> Some
    
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
            

     
module StorageKind =
        [<Literal>]
        let private DefaultStorageKindAspectsResource = "Resources/DefaultStorageKindAspects.csv"                
        type private DefaultStorageKindAspects = CsvProvider<DefaultStorageKindAspectsResource, Separators="|", PreferOptionals=true>
        
        type private StorageKindAspects = | StorageKindAspects of length : int option * precision : uint8 option * scale : uint8 option
        with
            member this.Length = match this with StorageKindAspects(length=x) -> x |> Option.get
            member this.Precision = match this with StorageKindAspects(precision=x) -> x |> Option.get
            member this.Scale = match this with StorageKindAspects(scale=x) -> x |> Option.get

        let private defaults : IDictionary<StorageKind, StorageKindAspects> = 
            [for row in (DefaultStorageKindAspectsResource |> DefaultStorageKindAspects.Load).Cache().Rows ->
                (StorageKind.Parse row.StorageKindName, StorageKindAspects(row.Length, row.Precision |> Convert.ToUInt8Option , row.Scale |> Convert.ToUInt8Option))
            ] |> dict        
        
        /// <summary>
        /// Gets the storage kind's default length
        /// </summary>
        /// <param name="kind">The kind of storage</param>
        let getDefaultLength kind =
            defaults.[kind].Length 

        /// <summary>
        /// Gets the storage kind's default precision
        /// </summary>
        /// <param name="kind">The kind of storage</param>
        let getDefaultPrecision kind =
            defaults.[kind].Precision

        /// <summary>
        /// Gets the storage kind's default scale
        /// </summary>
        /// <param name="kind">The kind of storage</param>
        let getDefaultScale kind =
            defaults.[kind].Scale

[<AutoOpen>]
module StorageKindExtensions =
    type StorageKind
    with
        member this.DefaultLength = this |> StorageKind.getDefaultLength
        member this.DefaultPrecision = this |> StorageKind.getDefaultPrecision
        member this.DefaultScale = this |> StorageKind.getDefaultScale