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
    /// Identifies a column and specifies selected storage characteristics
    /// </summary>
    type ColumnAttribute(name, position, autoValueKind) =
        inherit DataElementAttribute(name)

        new(name) =
            ColumnAttribute(name, UnspecifiedPosition, UnspecifiedAutoValue)        
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
            RoutineParameterAttribute(name, RoutineParameterDirection.Input, UnspecifiedPosition)

        new (name, position) =
            RoutineParameterAttribute(name, RoutineParameterDirection.Input, position)

        new (name, direction) =
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
            



    [<AbstractClass>]
    type ElementFacetAttribute<'T>(value : 'T) =
        inherit DataElementAttribute()

        /// <summary>
        /// Specifies the facet's value
        /// </summary>    
        member this.Value = value
            
    /// <summary>
    /// Specifies the nullability of the element to which it applies 
    /// </summary>
    type NullableAttribute(isNullable) =
        inherit ElementFacetAttribute<bool>(isNullable)

        new() =
            NullableAttribute(true)

    
    /// <summary>
    /// Specifies the relative position of the element to which it is applied
    /// </summary>
    type PositionAttribute(position) =
        inherit ElementFacetAttribute<int>(position)
        

    /// <summary>
    /// Specifies the (intrinsic) kind of data that is pxoxied by the element to which it is applied
    /// </summary>
    type DataKindAttribute(value) =
        inherit ElementFacetAttribute<DataKind>(value)
        
    

    /// <summary>
    /// Specifies the (custom) kind of data that is pxoxied by the element to which it is applied
    /// </summary>
    type CustomDataKindAttribute(kind, schemaName, typeName) =
        inherit DataKindAttribute(kind)

        member this.ObjectName = DataObjectName(schemaName, typeName) 
    
    /// <summary>
    /// Specifies the absolute length of the element to which it is applied
    /// </summary>
    type FixedLengthAttribute(len) =
        inherit ElementFacetAttribute<int>(len)
        

    /// <summary>
    /// Specifies the maximum length of the element to which it is applied
    /// </summary>
    type MaxLengthAttribute(len) =
        inherit ElementFacetAttribute<int>(len)
        
        /// <summary>
        /// The maximum length of the subject element
        /// </summary>
        member this.Value : int = len

    /// <summary>
    /// Specifies the minimum length of the element to which it is applied
    /// </summary>
    type MinLengthAttribute(len) =
        inherit ElementFacetAttribute<int>(len)
    

    /// <summary>
    /// Specifies the inclusive lower and upper bounds of the length of the element to which it applies
    /// </summary>
    /// <remarks>
    /// Logically equivalent to application of both <see cref="MinLengthAttribute"/> and <see cref="MaxLengthAttribute"/>
    /// </remarks>
    type LengthRangeAttribute(minLength, maxLength) =
        inherit ElementFacetAttribute<Range<int>>(Range(minLength,maxLength))

    
    /// <summary>
    /// Specifies the numeric precision of the element to which it is applied
    /// </summary>
    type PrecisionAttribute(value) =
        inherit ElementFacetAttribute<uint8>(value)
        

    /// <summary>
    /// Specifies the numeric scale of the element to which it is applied
    /// </summary>
    type ScaleAttribute(value) =
        inherit ElementFacetAttribute<uint8>(value)
        
    
    /// <summary>
    /// Specifies the minimum value of the element to which it is applied
    /// </summary>
    type MinScalarAttribute private (value) =
        inherit ElementFacetAttribute<NumericValue>(value)
        
        new (value : uint8) = MinScalarAttribute(UInt8Value(value))
        new (value : int8) = MinScalarAttribute(Int8Value(value))
        new (value : uint16) = MinScalarAttribute(UInt16Value(value))
        new (value : int16) = MinScalarAttribute(Int16Value(value))
        new (value : uint32) = MinScalarAttribute(UInt32Value(value))
        new (value : int32) = MinScalarAttribute(Int32Value(value))
        new (value : uint64) = MinScalarAttribute(UInt64Value(value))
        new (value : int64) = MinScalarAttribute(Int64Value(value))
        new (value : float32) = MinScalarAttribute(Float32Value(value))
        new (value : float) = MinScalarAttribute(Float64Value(value))        
        new (value : decimal) = MinScalarAttribute(DecimalValue(value))
        
        /// <summary>
        /// The minimum value of the subject element
        /// </summary>
        member this.Value = value


    /// <summary>
    /// Specifies the minimum value of the element to which it is applied
    /// </summary>
    type MaxScalarAttribute private (value) =
        inherit ElementFacetAttribute<NumericValue>(value)
        
        new (value : uint8) = MaxScalarAttribute(UInt8Value(value))
        new (value : int8) = MaxScalarAttribute(Int8Value(value))
        new (value : uint16) = MaxScalarAttribute(UInt16Value(value))
        new (value : int16) = MaxScalarAttribute(Int16Value(value))
        new (value : uint32) = MaxScalarAttribute(UInt32Value(value))
        new (value : int32) = MaxScalarAttribute(Int32Value(value))
        new (value : uint64) = MaxScalarAttribute(UInt64Value(value))
        new (value : int64) = MaxScalarAttribute(Int64Value(value))
        new (value : float32) = MaxScalarAttribute(Float32Value(value))
        new (value : float) = MaxScalarAttribute(Float64Value(value))
        new (value : decimal) = MaxScalarAttribute(DecimalValue(value))

        /// <summary>
        /// The maximum value of the subject element
        /// </summary>
        member this.Value = value

    /// <summary>
    /// Specifies the inclusive lower and upper bounds of the scalar value of the element to which it applies
    /// </summary>
    /// <remarks>
    /// Logically equivalent to application of both <see cref="MinScalarAttribute"/> and <see cref="MaxScalarAttribute"/>
    /// </remarks>
    type ScalarRangeAttribute(minValue, maxValue) =
        inherit ElementFacetAttribute<Range<NumericValue>>(Range(minValue,maxValue))
        
        new (minValue : uint8, maxValue : uint8) = ScalarRangeAttribute(UInt8Value(minValue), UInt8Value(maxValue))
        new (minValue : int8, maxValue : int8) = ScalarRangeAttribute(Int8Value(minValue), Int8Value(maxValue))
        new (minValue : uint16, maxValue : uint16) = ScalarRangeAttribute(UInt16Value(minValue), UInt16Value(maxValue))
        new (minValue : int16, maxValue : int16) = ScalarRangeAttribute(Int16Value(minValue), Int16Value(maxValue))
        new (minValue : uint32, maxValue : uint32) = ScalarRangeAttribute(UInt32Value(minValue), UInt32Value(maxValue))
        new (minValue : int32, maxValue : int32) = ScalarRangeAttribute(Int32Value(minValue), Int32Value(maxValue))
        new (minValue : uint64, maxValue : uint64) = ScalarRangeAttribute(UInt64Value(minValue), UInt64Value(maxValue))
        new (minValue : int64, maxValue : int64) = ScalarRangeAttribute(Int64Value(minValue), Int64Value(maxValue))
        new (minValue : float32, maxValue : float32) = ScalarRangeAttribute(Float32Value(minValue), Float32Value(maxValue))
        new (minValue : float, maxValue : float) = ScalarRangeAttribute(Float64Value(minValue), Float64Value(maxValue))
        new (minValue : decimal,maxValue : decimal) = ScalarRangeAttribute(DecimalValue(minValue), DecimalValue(maxValue))

    /// <summary>
    /// Specifies the minimum date value of the element to which it is applied
    /// </summary>
    type MinDateAttribute(value : string) =
        inherit ElementFacetAttribute<BclDateTime>(BclDateTime.Parse(value))

            
    /// <summary>
    /// Specifies the maximum date value of the element to which it is applied
    /// </summary>
    type MaxDateAttribute(value : string) =
        inherit ElementFacetAttribute<BclDateTime>(BclDateTime.Parse(value))

        /// <summary>
        /// The maximum value of the subject element
        /// </summary>
        member this.Value = DateTime.Parse(value)


    /// <summary>
    /// Specifies the inclusive lower and upper bounds of the date value of the element to which it applies
    /// </summary>
    type DateRangeAttribute(minValue : string, maxValue : string) =
        inherit ElementFacetAttribute<Range<BclDateTime>>(Range(BclDateTime.Parse(minValue), BclDateTime.Parse(maxValue)))

        

    /// <summary>
    /// Defines the supported data facet names
    /// </summary>
    module DataFacetNames =
        [<Literal>]
        let Nullable = "Nullable"
        [<Literal>]
        let Position = "Position"
        [<Literal>]
        let IntrinsicDataKind = "DataKind"
        [<Literal>]
        let CustomObjectName = "CustomDataKind"
        [<Literal>]
        let FixedLength = "FixedLength"
        [<Literal>]
        let MinLength = "MinLength"
        [<Literal>]
        let MaxLength = "MaxLength"
        [<Literal>]
        let Precision = "Precision"
        [<Literal>]
        let Scale = "Scale"
        [<Literal>]
        let MinScalar = "MinScalar"
        [<Literal>]
        let MaxScalar = "MaxScalar"
        [<Literal>]
        let MinDate = "MinDate"
        [<Literal>]
        let MaxDate = "MaxDate"
    

module DataFacetAttributeReader = 
    
    let private cast<'T>(x : obj) = x :?> 'T

    let private value<'T>(a : ClrAttribution) =
        a.AttributeInstance |> Option.get |> cast<ElementFacetAttribute<'T>> |> fun x -> x.Value
    
    let private attrib<'A,'T when 'A :> Attribute>(element : ClrElement) =
        match element.TryGetAttribute<'A>() with
            | Some(a) -> a |> value<'T> |> Some
            | None -> None

    let private getRangeMin(r : Range<'T>) =
        match r with |Range(MinValue=x) ->x

    let private getRangeMax(r : Range<'T>) =
        match r with |Range(MaxValue=x) ->x


    /// <summary>
    /// Retrieves the identified facet, if present
    /// </summary>
    /// <param name="facetName">The name of the facet</param>
    /// <param name="element">The element to which the facet may be attached/param>
    let tryGetFacet<'T> facetName (element : ClrElement) =
        match facetName with
        | DataFacetNames.Nullable -> 
            element |> attrib<NullableAttribute, 'T>
        
        | DataFacetNames.Position -> 
            element |> attrib<PositionAttribute, 'T>
        
        | DataFacetNames.IntrinsicDataKind -> 
            element |> attrib<DataKindAttribute, 'T>
        
        | DataFacetNames.CustomObjectName -> 
            element |> attrib<CustomDataKindAttribute, 'T>
        
        | DataFacetNames.FixedLength -> 
            element |> attrib<FixedLengthAttribute, 'T>            
        
        | DataFacetNames.MinLength -> 
            match element |> attrib<MinLengthAttribute, 'T> with
            | Some(x) -> Some(x)
            | None ->
                match element |> attrib<LengthRangeAttribute, Range<'T>> with
                | Some(x) -> x |> getRangeMin |> Some 
                | None -> None
        
        | DataFacetNames.MaxLength -> 
            match element |> attrib<MaxLengthAttribute, 'T> with
            | Some(x) -> Some(x)
            | None ->
                match element |> attrib<LengthRangeAttribute, Range<'T>> with
                | Some(x) -> x |> getRangeMin |> Some 
                | None -> None

        | DataFacetNames.Precision -> 
            element |> attrib<PrecisionAttribute, 'T>            
        
        | DataFacetNames.Scale -> 
            element |> attrib<ScaleAttribute, 'T>    
        
        | DataFacetNames.MinScalar -> 
            match element |> attrib<MinScalarAttribute, 'T> with
            | Some(x) -> Some(x)
            | None ->
                match element |> attrib<ScalarRangeAttribute, Range<'T>> with
                | Some(x) -> x |> getRangeMin |> Some 
                | None -> None
        
        | DataFacetNames.MaxScalar -> 
            match element |> attrib<MaxScalarAttribute, 'T> with
            | Some(x) -> Some(x)
            | None ->
                match element |> attrib<ScalarRangeAttribute, Range<'T>> with
                | Some(x) -> x |> getRangeMin |> Some 
                | None -> None
        
        | DataFacetNames.MinDate -> 
            match element |> attrib<MinDateAttribute, 'T> with
            | Some(x) -> Some(x)
            | None ->
                match element |> attrib<DateRangeAttribute, Range<'T>> with
                | Some(x) -> x |> getRangeMin |> Some 
                | None -> None
        
        | DataFacetNames.MaxDate -> 
            match element |> attrib<MaxDateAttribute, 'T> with
            | Some(x) -> Some(x)
            | None ->
                match element |> attrib<DateRangeAttribute, Range<'T>> with
                | Some(x) -> x |> getRangeMin |> Some 
                | None -> None

        | _ -> nosupport()    
    
