namespace IQ.Core.Data.Test

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Framework

module ProxyTestCases =
    
    type Table04FunctionResult = {
        Id : int
        Code : string
        StartDate : DateTime
        EndDate : DateTime        
    }

    /// <summary>
    /// Defines stored procedure proxy contracts for testing purposes
    /// </summary>
    [<Schema("SqlTest")>]
    type ISqlTestProcs =
        /// <summary>
        /// Inserts a record into the [SqlTest].[Table02] table, assigning the Col01 
        /// column the next value from the [SqlTest].[Table02Sequence] and returning it as 
        /// an output parameter to the caller
        /// </summary>
        /// <param name="col02">The value of Col02</param>
        /// <param name="col03">The Value of Col03</param>
        [<Procedure>]
        abstract pTable02Insert:col02 : DateTime -> col03 : int64 -> [<return : RoutineParameter("col01", 0)>] int
        
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

    [<Schema("SqlTest")>]
    type ISqlTestFunctions =
        [<TableFunction>]
        abstract fTable04Before: startDate:DateTime-> Table04FunctionResult list


    type ISqlTestRoutines =
        inherit ISqlTestProcs
        inherit ISqlTestFunctions

    [<Description("SQL Test Table01")>]
    type Table01 = {
        [<Description("Col01 Description Text")>]
            
        [<StorageType(StorageKind.Int32)>]
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

    module ProxyTestCaseMethod =
        let getDbObjectName (testMethod : MethodBase) =
            let values = testMethod.Name 
                       |> Txt.betweenMarkers "[" "]" true 
                       |> Txt.matchRegexGroups ["Schema"; "Name"] Txt.StockRegularExpressions.QualifiedDataObjectName
            DataObjectName(values.["Schema"], values.["Name"])