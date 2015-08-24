// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql

open System
open System.Data
open System.Linq
open System.Diagnostics

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data

module DataType = 
    let toSqlDbType (t : DataType) =
        match t with
        | BitDataType -> SqlDbType.Bit
        | UInt8DataType -> SqlDbType.TinyInt
        | UInt16DataType -> SqlDbType.Int
        | UInt32DataType -> SqlDbType.BigInt
        | UInt64DataType -> SqlDbType.VarBinary 
        | Int8DataType -> SqlDbType.SmallInt
        | Int16DataType -> SqlDbType.SmallInt
        | Int32DataType -> SqlDbType.Int
        | Int64DataType -> SqlDbType.BigInt
                        
        | BinaryFixedDataType(_) -> SqlDbType.Binary
        | BinaryVariableDataType(_) -> SqlDbType.VarBinary
        | BinaryMaxDataType -> SqlDbType.VarBinary
            
        | AnsiTextFixedDataType(length) -> SqlDbType.Char
        | AnsiTextVariableDataType(length) -> SqlDbType.VarChar
        | AnsiTextMaxDataType -> SqlDbType.VarChar
            
        | UnicodeTextFixedDataType(_) -> SqlDbType.NChar
        | UnicodeTextVariableDataType(_) -> SqlDbType.NVarChar
        | UnicodeTextMaxDataType -> SqlDbType.NVarChar
            
        | DateTimeDataType(_)-> SqlDbType.DateTime2
        | DateTimeOffsetDataType -> SqlDbType.DateTimeOffset
        | TimeOfDayDataType(_) -> SqlDbType.Time
        | DateDataType -> SqlDbType.Date
        | TimespanDataType -> SqlDbType.BigInt

        | Float32DataType -> SqlDbType.Real
        | Float64DataType -> SqlDbType.Float
        | DecimalDataType(precision,scale) -> SqlDbType.Decimal
        | MoneyDataType -> SqlDbType.Money
        | GuidDataType -> SqlDbType.UniqueIdentifier
        | XmlDataType(_) -> SqlDbType.Xml
        | JsonDataType -> SqlDbType.NVarChar
        | VariantDataType -> SqlDbType.Variant
        | CustomTableDataType(_) -> SqlDbType.Structured
        | CustomObjectDataType(_,_) -> SqlDbType.VarBinary 
        | CustomPrimitiveDataType(name) -> SqlDbType.Udt
        | TypedDocumentDataType(_) -> SqlDbType.NVarChar



