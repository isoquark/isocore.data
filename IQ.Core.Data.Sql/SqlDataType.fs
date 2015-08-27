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
        | MoneyDataType(_,_) -> SqlDbType.Money
        | GuidDataType -> SqlDbType.UniqueIdentifier
        | XmlDataType(_) -> SqlDbType.Xml
        | JsonDataType -> SqlDbType.NVarChar
        | VariantDataType -> SqlDbType.Variant
        | TableDataType(_) -> SqlDbType.Structured
        | ObjectDataType(_,_) -> SqlDbType.VarBinary 
        | CustomPrimitiveDataType(name) -> SqlDbType.Udt
        | TypedDocumentDataType(_) -> SqlDbType.NVarChar
        | RowversionDataType -> SqlDbType.Timestamp


(**
select 
	'[<Literal>] let ' + DataTypeName + ' ="' + DataTypeName + '"'
from 
	Metadata.vDataType 
where 
	IsUserDefined = 0

*)
module internal SqlDataTypeNames = 
    [<Literal>]  
    let bigint ="bigint"
    [<Literal>]  
    let binary ="binary"
    [<Literal>]  
    let bit ="bit"
    [<Literal>]  
    let char ="char"
    [<Literal>]  
    let date ="date"
    [<Literal>]  
    let datetime ="datetime"
    [<Literal>]  
    let datetime2 ="datetime2"
    [<Literal>]  
    let datetimeoffset ="datetimeoffset"
    [<Literal>]  
    let decimal ="decimal"
    [<Literal>]  
    let float ="float"
    [<Literal>]  
    let geography ="geography"
    [<Literal>]  
    let geometry ="geometry"
    [<Literal>]  
    let hierarchyid ="hierarchyid"
    [<Literal>]  
    let image ="image"
    [<Literal>]  
    let int ="int"
    [<Literal>]  
    let money ="money"
    [<Literal>]  
    let nchar ="nchar"
    [<Literal>]  
    let ntext ="ntext"
    [<Literal>]  
    let numeric ="numeric"
    [<Literal>]  
    let nvarchar ="nvarchar"
    [<Literal>]  
    let real ="real"
    [<Literal>]  
    let smalldatetime ="smalldatetime"
    [<Literal>]  
    let smallint ="smallint"
    [<Literal>]  
    let smallmoney ="smallmoney"
    [<Literal>]  
    let sql_variant ="sql_variant"
    [<Literal>]  
    let sysname ="sysname"
    [<Literal>]  
    let text ="text"
    [<Literal>]  
    let time ="time"
    [<Literal>]  
    let timestamp ="timestamp"
    [<Literal>]  
    let rowversion ="rowversion"
    [<Literal>] 
    let tinyint ="tinyint"
    [<Literal>]  
    let uniqueidentifier ="uniqueidentifier"
    [<Literal>]  
    let varbinary ="varbinary"
    [<Literal>]  
    let varchar ="varchar"
    [<Literal>]  let xml ="xml"

    
