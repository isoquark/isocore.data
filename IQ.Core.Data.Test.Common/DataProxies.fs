namespace IQ.Core.Data.Test

open System

open IQ.Core.Framework
open IQ.Core.Data

module TestProxies =
    type Table04FunctionResult = {
        Id : int
        Code : string
        StartDate : BclDateTime
        EndDate : BclDateTime        
    }
        
    module SchemaNames =
        [<Literal>]
        let SqlTest = "SqlTest"

    module TableNames =
        [<Literal>]
        let Table01 = "Table01"
        [<Literal>]
        let Table02 = "Table01"
        [<Literal>]
        let Table03 = "Table01"

    [<Schema("SqlTest")>]
    type ISqlTestRoutines =
        /// <summary>
        /// Inserts a record into the [SqlTest].[Table02] table, assigning the Col01 
        /// column the next value from the [SqlTest].[Table02Sequence] and returning it as 
        /// an output parameter to the caller
        /// </summary>
        /// <param name="col02">The value of Col02</param>
        /// <param name="col03">The Value of Col03</param>
        [<Procedure>]
        abstract pTable02Insert:col02 : BclDateTime -> col03 : int64 -> [<return : RoutineParameter("col01", 0)>] int
        
        /// <summary>
        /// Inserts a record into the [SqlTest].[Table03] table and returns the integral
        /// result to the caller
        /// </summary>
        /// <param name="col01">The value of Col01</param>
        /// <param name="col02">The Value of Col02</param>
        /// <param name="col03">The value of Col03</param>
        /// <param name="col04">The Value of Col04</param>
        [<Procedure>]
        abstract pTable03Insert: Col01:uint8->Col02:int16->Col03:int32->Col04:int64->int

        [<Procedure>]
        abstract pTable04Truncate:unit->unit

        [<Procedure>]
        abstract pTable04Insert:code : string->startDate : BclDateTime -> endDate : BclDateTime -> int

        [<TableFunction>]
        abstract fTable04Before: startDate:BclDateTime-> Table04FunctionResult list

        [<TableFunction("SqlTest", "fTable04Before")>]
        abstract fTable04BeforeArray: startDate:BclDateTime-> Table04FunctionResult[]


    [<Description("SQL Test Table01")>]
    type Table01 = {
        [<Description("Col01 Description Text")>]
            
        Col01 : uint16
        [<Description("Col02 Description Text")>]
        Col02 : int64 option
        [<Description("Col03 Description Text")>]
        Col03 : string
        [<Description("Col04 Description Text")>]
        Col04 : string option
        [<Description("Col05 Description Text")>]
        Col05 : string
    }

    module Metadata =
        [<View("Metadata", "vDataType")>]
        type vDataTypeA = {
            DataTypeId : int
            DataTypeName : string
            SchemaId : int
            SchemaName : string   
            IsNullable : bool 
            IsUserDefined: bool
            BaseTypeId : uint8 option
        }

        [<View("Metadata", "vDataType")>]
        type vDataTypeB = {
            DataTypeId : int
            DataTypeName : string
            SchemaId : int
            SchemaName : string   
            MaxLength : decimal
            Precision : uint8
            Scale : uint8
            IsNullable : bool 
            IsUserDefined: bool
            BaseTypeId : uint8 option
        }

    [<AutoOpen>]
    module SqlTest =
        type Table05 = {
            Col01 : int32
            Col02 : uint8
            Col03 : int16
            Col04 : int64
        }
    
        type Table06 = {
            Col01 : int32
            Col02 : uint8 option
            Col03 : int16 option
            Col04 : int64
        }

        type Table07 = {
            [<Column(AutoValueKind.Identity)>]
            Col01 : int32 option
            Col02 : string
            Col03 : string
        }        

        //Inferred object name: [SqlTest].[Table10]
        type Table10() =
            //Inferred int not null
            member val Col01 = 0 with get, set
            
            //Inferred: bigint null
            member val Col02 = Nullable<int64>() with get, set
            
            //Inferred: binary(50) null
            [<Nullable; FixedLength(50)>]
            member val Col03 = Array.zeroCreate<uint8>(0) with get, set
            
            //Inferred: bit null
            member val Col04 = Nullable<bool>() with get, set
            
            //Inferred: char(50) null
            [<DataKind(DataKind.AnsiTextFixed); Nullable; FixedLength(50)>]
            member val Col05 = String.Empty with get, set
            
            //Inferred: date null
            [<DataKind(DataKind.Date)>]
            member val Col06 = Nullable<BclDateTime>() with get, set
            
            //Inferred: datetime null
            [<DataKind(DataKind.LegacyDateTime)>]
            member val Col07 = Nullable<BclDateTime>() with get, set
            
            //Inferred: datetime2(7) null
            member val Col08 = Nullable<BclDateTime>() with get, set
            
            //Inferred: decimal(18,12) null
            [<Precision(18uy); Scale(12uy)>]
            member val Col09 = Nullable<decimal>() with get, set
            
            //Inferred: float null
            member val Col10 = Nullable<float>() with get, set
            
            //Inferred: money null
            [<DataKind(DataKind.Money)>]
            member val Col11 = Nullable<decimal>() with get,set
            
            //Inferred varchar(100) null
            [<Nullable; FixedLength(100)>]            
            member val Col12 = String.Empty with get, set
            
            //Inferred: decimal(15, 5) null
            [<Precision(15uy); Scale(5uy)>]
            member val Col13 = Nullable<decimal>() with get,set
            
            //Inferred: nvarchar(73) null
            [<Nullable; MaxLength(73)>]            
            member val Col14 = String.Empty with get, set
            
            //Inferred: real null
            member val Col15 = Nullable<float32>() with get, set
            
            //Inferred: smalldatetime null
            [<DataKind(DataKind.LegacySmallDateTime)>]
            member val Col16 = Nullable<BclDateTime>() with get, set
            
            //Inferred: smallint null
            member val Col17 = Nullable<int16>() with get,set
            
            //Inferred: smallmoney null
            [<DataKind(DataKind.SmallMoney)>]
            member val Col18 = Nullable<decimal>() with get,set
            
            //Inferred: sql_variant null
            [<Nullable>]
            member val Col19 = Unchecked.defaultof<obj> with get, set
            
            //Inferred: tinyint null
            member val Col20 = Nullable<uint8>() with get,set
            
            //Inferred: uniqueidentifier null
            member val Col21 = Nullable<Guid>() with get,set
            
            //Inferred: varbinary(223)
            [<Nullable; MaxLength(223)>]
            member val Col22 = Array.zeroCreate<uint8>(0) with get, set
            
            //Inferred: varchar(121)
            [<Nullable; MaxLength(121)>]
            member val Col23 = String.Empty with get, set
            
            //Inferred varbinary(MAX) null
            [<Nullable>]
            member val Col24 = Array.zeroCreate<uint8>(0) with get, set
            
            //Inferred varchar(MAX)
            [<DataKind(DataKind.AnsiTextMax); Nullable>]
            member val Col25 = String.Empty with get, set
            
            //Inferred nvarchar(MAX)
            [<Nullable>]            
            member val Col26 = String.Empty with get, set
