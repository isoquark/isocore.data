// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open IQ.Core.Framework


[<AutoOpen>]
module DataValueVocabulary =
    type DataValue =
        | BitValue of bool
        | UInt8Value of uint8
        | UInt16Value of uint16
        | UInt32Value of uint32
        | UInt64Value of uint64
        | Int8Value of int8
        | Int16Value of int16
        | Int32Value of int32
        | Int64Value of int64
        | BinaryFixedValue of uint8[] 
        | BinaryVariableValue of uint8[]
        | BinaryMaxValue of uint8
        | AnsiTextFixedValue of string
        | AnsiTextVariableValue of string
        | AnsiTextMaxValue of string
        | UnicodeTextFixedValue of string
        | UnicodeTextVariableValue of string
        | UnicodeTextMaxValue of string
        | DateTimeValue of DateTime
        | DateTimeOffsetValue of DateTimeOffset
        | TimeOfDayValue of BclDateTime
        | DateValue of BclDateTime
        | TimespanValue of Period
        | Float32DValue of float32
        | Float64Value of float
        | DecimalValue of decimal
        | MoneyValue of decimal
        | GuidValue of Guid
        | XmlValue of string
        | VariantValue of obj
        | CustomTableValue of obj
        | CustomObjectValue of obj
        | CustomPrimitiveValue of obj
        | TypedDocumentValue of string

    type DataPoint = | DataPoint of v : DataValue * t : DataType 


/// <summary>
/// Defines  operations for manipulating Data Values
/// </summary>
module DataValue =
    let inline private cast<'T>(o : obj) = o :?> 'T
    
    /// <summary>
    /// Extracts the value payload
    /// </summary>
    /// <param name="v">The value whose payload is to be extracted</param>
    let unwrap<'T>(v : DataValue) =
        match v with
        | BitValue(x) -> x |> cast<'T>
        | UInt8Value(x) -> x |> cast<'T> 
        | UInt16Value(x) -> x |> cast<'T>
        | UInt32Value(x) -> x |> cast<'T>
        | UInt64Value(x) -> x |> cast<'T>
        | Int8Value(x) -> x |> cast<'T>
        | Int16Value(x) -> x |> cast<'T>
        | Int32Value(x) -> x |> cast<'T>
        | Int64Value(x) -> x |> cast<'T>
        | BinaryFixedValue(x) -> x |> cast<'T>
        | BinaryVariableValue(x) -> x |> cast<'T>
        | BinaryMaxValue(x) -> x |> cast<'T>
        | AnsiTextFixedValue(x) -> x |> cast<'T>
        | AnsiTextVariableValue(x) -> x |> cast<'T>
        | AnsiTextMaxValue(x) -> x |> cast<'T>
        | UnicodeTextFixedValue(x) -> x |> cast<'T>
        | UnicodeTextVariableValue(x) -> x |> cast<'T>
        | UnicodeTextMaxValue(x) -> x |> cast<'T>
        | DateTimeValue(x) -> x |> cast<'T>
        | DateTimeOffsetValue(x) -> x |> cast<'T>
        | TimeOfDayValue(x) -> x |> cast<'T>
        | DateValue(x) -> x |> cast<'T>
        | TimespanValue(x) -> x |> cast<'T>
        | Float32DValue(x) -> x |> cast<'T>
        | Float64Value(x) -> x |> cast<'T>
        | DecimalValue(x) -> x |> cast<'T>
        | MoneyValue(x) -> x |> cast<'T>
        | GuidValue(x) -> x |> cast<'T>
        | XmlValue(x) -> x |> cast<'T>
        | VariantValue(x) -> x |> cast<'T>
        | CustomTableValue(x) -> x |> cast<'T>
        | CustomObjectValue(x) -> x |> cast<'T>
        | CustomPrimitiveValue(x) -> x |> cast<'T>
        | TypedDocumentValue(x) -> x |> cast<'T>

//    let wrap (kind : DataKind) x =
//        match kind with
//        | DataKind.Bit -> ()
//        | DataKind.UInt8 = 20uy //tinyint
//        | DataKind.UInt16 = 21uy //no direct map, use int
//        | DataKind.UInt32 = 22uy // no direct map, use bigint
//        | DataKind.UInt64 = 23uy // no direct map, use varbinary(8)
//        | DataKind.Int8 = 30uy //no direct map, use smallint
//        | DataKind.Int16 = 31uy //smallint
//        | DataKind.Int32 = 32uy //int
//        | DataKind.Int64 = 33uy //bigint
//        | DataKind.BinaryFixed = 40uy //binary 
//        | DataKind.BinaryVariable = 41uy //varbinary
//        | DataKind.BinaryMax = 42uy
//        | DataKind.AnsiTextFixed = 50uy //char
//        | DataKind.AnsiTextVariable = 51uy //varchar
//        | DataKind.AnsiTextMax = 52uy
//        | DataKind.UnicodeTextFixed = 53uy //nchar
//        | DataKind.UnicodeTextVariable = 54uy //nvarchar
//        | DataKind.UnicodeTextMax = 55uy
//        | DataKind.DateTime = 62uy //corresponds to datetime2
//        | DataKind.DateTimeOffset = 63uy
//        | DataKind.TimeOfDay = 64uy //corresponds to time
//        | DataKind.Date = 65uy //corresponds to date
//        | DataKind.Timespan = 66uy //no direct map, use bigint to store number of ticks
//        | DataKind.Float32 = 70uy //corresponds to real
//        | DataKind.Float64 = 71uy //corresponds to float
//        | DataKind.Decimal = 80uy
//        | DataKind.Money = 81uy
//        | DataKind.Guid = 90uy //corresponds to uniqueidentifier
//        | DataKind.Xml = 100uy
//        | DataKind.Variant = 110uy //corresponds to sql_variant
//        | DataKind.CustomTable = 150uy //a non-intrinsic table data type
//        | DataKind.CustomObject = 151uy //a non-intrinsic CLR type
//        | DataKind.CustomPrimitive = 152uy //a non-intrinsic primitive based on an intrinsic primitive
//        | DataKind.Geography = 160uy
//        | DataKind.Geometry = 161uy
//        | DataKind.Hierarchy = 162uy
//        | DataKind.TypedDocument = 180uy

    let inline bit(x) = BitValue(x)
    let inline uint8(x) = UInt8Value(x)
    let inline uint16(x) = UInt16Value(x)
    let inline uint32(x) = UInt32Value(x)
    let inline uint64(x) = UInt64Value(x)
    let inline int8(x) = Int8Value(x)
    let inline int16(x) = Int16Value(x)
    let inline int32(x) = Int32Value(x)
    let inline int64(x) = Int64Value(x)
    let inline binf(x) = BinaryFixedValue(x)
    let inline binv(x) = BinaryVariableValue(x)
    let inline binm(x) = BinaryMaxValue(x)
    let inline atextf(x) = AnsiTextFixedValue(x)
    let inline atextv(x) = AnsiTextVariableValue(x)
    let inline atextm(x) = AnsiTextMaxValue(x)
    let inline utextf(x) = UnicodeTextFixedValue(x)
    let inline utextv(x) = UnicodeTextVariableValue(x)
    let inline utextm(x) = UnicodeTextMaxValue(x)
    let inline datetime(x) = DateTimeValue(x)


[<AutoOpen>]
module DataValueExtensions =
    type DataValue
    with
        member this.Unwrap<'T>() = this |> DataValue.unwrap<'T>