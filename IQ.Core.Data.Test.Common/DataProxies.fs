﻿namespace IQ.Core.Data.Test

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
            
        [<DataTypeAttribute(DataKind.Int32)>]
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
            [<DataTypeAttribute(DataKind.Int16)>]
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

