// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Behavior

open System
//open System.Data
open System.Collections.Generic

open FSharp.Data

open IQ.Core.Framework
open IQ.Core.Data.Contracts

/// <summary>
/// Defines attributes that are intended to be applied to proxy elements to specify 
/// data source characteristics that cannot otherwise be inferred
/// </summary>
[<AutoOpen>]
module DataAttributes =
    
    [<Literal>]
    let private UnspecifiedName = ""
    [<Literal>]
    let private UnspecifiedPrecision = 201uy
    [<Literal>]
    let private UnspecifiedScale = 201uy
    [<Literal>]
    let private UnspecifiedLength = -1
    [<Literal>]
    let private UnspecifiedStorage = DataKind.Unspecified
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

        new () =
            DataElementAttribute(UnspecifiedName)
        
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
    type DataTypeAttribute(dataKind, length, precision, scale, clrType, customTypeSchemaName, customTypeName) =
        inherit DataAttribute()

        //For any kind of data that doesn't require additional information to instantiate a value, e.g., Int32, Bit and so forth
        new (dataKind) =
            DataTypeAttribute(dataKind, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale, UnspecifiedType, UnspecifiedName, UnspecifiedName)
        //For variable-length or fixed-length text and binary data types that whose length has a specified upper bound
        new (dataKind, length) =
            DataTypeAttribute(dataKind, length, UnspecifiedPrecision, UnspecifiedScale, UnspecifiedType, UnspecifiedName, UnspecifiedName)                        
        //For data types that have a specifiable precision
        new (dataKind, precision) =
            DataTypeAttribute(dataKind, UnspecifiedLength, precision, UnspecifiedScale, UnspecifiedType, UnspecifiedName, UnspecifiedName)                
        //For data types that have both specifiable precision and scale
        new (dataKind, precision, scale) =
            DataTypeAttribute(dataKind, UnspecifiedLength, precision, scale, UnspecifiedType, UnspecifiedName, UnspecifiedName)
        //For Geography, Geometry and Hierarchy types
        new (dataKind, clrType) =
            DataTypeAttribute(dataKind, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale, clrType, UnspecifiedName, UnspecifiedName)
        //For CustomObject
        new (dataKind, clrType, customTypeSchemaName, customTypeName) =
            DataTypeAttribute(dataKind, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale,  clrType, customTypeSchemaName, customTypeName)
        //For CustomTable | CustomPrimitive
        new (dataKind, customTypeSchemaName, customTypeName) =
            DataTypeAttribute(dataKind, UnspecifiedLength, UnspecifiedPrecision, UnspecifiedScale,  UnspecifiedType, customTypeSchemaName, customTypeName)
        
        /// Indicates the kind of storage
        member this.DataKind : DataKind = dataKind
        
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
    /// Applied to an element, such as a column or parameter proxy, to indicate that null values are allowed
    /// </summary>
    type NullableAttribute() =
        inherit DataElementAttribute()
    
    /// <summary>
    /// Applied to an element, such as a column or parameter proxy, to indicate its position relative to other elements
    /// </summary>
    type PositionAttribute(position) =
        inherit DataElementAttribute()
        
        member this.Position : int = position

    type DataKindAttribute(kind, customTypeSchema, customTypeName) =
        inherit DataElementAttribute()
        
        new(kind) =
            DataKindAttribute(kind, UnspecifiedName, UnspecifiedName)

        member this.Kind : DataKind = kind

        member this.CustomTypeSchema = customTypeSchema

        member this.CustomTypeName = customTypeName
    
    type LengthAttribute(len) =
        inherit DataElementAttribute()
        
        member this.Length : int = len

    type MaxLengthAttribute(len) =
        inherit DataElementAttribute()
        
        member this.Length : int = len
    
    
    type PrecisionAttribute(precision) =
        inherit DataElementAttribute()
        
        member this.Precision : uint8 = precision

    type ScaleAttribute(scale) =
        inherit DataElementAttribute()
        
        member this.Scale : uint8 = scale

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
            RoutineParameterAttribute(name, RoutineParameterDirection.Input, UnspecifiedPosition)

        new (name, position) =
            RoutineParameterAttribute(name, RoutineParameterDirection.Input, position)

        new (direction) =
            RoutineParameterAttribute(UnspecifiedName, direction, UnspecifiedPosition)

        new (direction, position) =
            RoutineParameterAttribute(UnspecifiedName, direction, position)

        new (position) =
            RoutineParameterAttribute(UnspecifiedName, RoutineParameterDirection.Input, position)

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
            

     
