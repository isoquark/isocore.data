// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Contracts

open System
open System.Diagnostics
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Linq

open IQ.Core.Framework

open IQ.Core.Framework.Contracts

type IDataStoreCommon =
    abstract ConnectionString:string

/// <summary>
/// Defines contract for unparameterized queryable data store
/// </summary>
type IQueryableDataStore =
    inherit IDataStoreCommon
    /// <summary>
    /// Retrieves a query-identified collection of data entities from the store
    /// </summary>
    abstract Select:'Q->'T seq

    /// <summary>
    /// Retrieves all the items of a specified type
    /// </summary>
    abstract SelectAll:unit ->'T seq

    /// <summary>
    /// Retrieves an identified matrix
    /// </summary>
    abstract SelectMatrix: 'Q -> IDataMatrix

    /// <summary>
    /// Retrieves implementation of the identified command contract from the store
    /// </summary>
    abstract GetQueryContract: unit -> 'TContract when 'TContract : not struct    
        
/// <summary>
/// Defines contract for queryable data store parameterized by query type
/// </summary>
type IQueryableDataStore<'Q> =
    inherit IDataStoreCommon
    /// <summary>
    /// Retrieves a query-identified collection of data entities from the store
    /// </summary>
    abstract Select:'Q->'T seq
    
    /// <summary>
    /// Retrieves all the items of a specified type
    /// </summary>
    abstract SelectAll:unit ->'T seq

    /// <summary>
    /// Retrieves an identified matrix
    /// </summary>
    abstract SelectMatrix: 'Q -> IDataMatrix
    
    /// <summary>
    /// Retrieves implementation of the identified command contract from the store
    /// </summary>
    abstract GetQueryContract: unit -> 'TContract when 'TContract : not struct    


/// <summary>
/// Defines contract for unparameterized mutable data store
/// </summary>
type IMutableDataStore =
    inherit IDataStoreCommon
    /// <summary>
    /// Persists a collection of data entities to the store, inserting or updating as appropriate
    /// </summary>
    abstract Merge:'T seq->unit

    /// <summary>
    /// Merges the matrix into the store
    /// </summary>
    abstract MergeMatrix:IDataMatrix->unit
    
    /// <summary>
    /// Inserts an arbitrary number of entities into the store, eliding existence checks
    /// </summary>
    abstract Insert:'T seq ->unit

    /// <summary>
    /// Inserts the matrix into the store
    /// </summary>
    abstract InsertMatrix:IDataMatrix -> unit

    /// <summary>
    /// Retrieves implementation of the identified command contract from the store
    /// </summary>
    abstract GetCommandContract: unit -> 'TContract when 'TContract : not struct    

    /// <summary>
    /// Executes a supplied command against the store
    /// </summary>
    abstract ExecuteCommand:command : 'TCommand -> 'TResult

/// <summary>
/// Defines contract for mutable data store parametrized by query type
/// </summary>
type IMutableDataStore<'Q> =
    inherit IDataStoreCommon
    /// <summary>
    /// Persists a collection of data entities to the store, inserting or updating as appropriate
    /// </summary>
    abstract Merge:'T seq->unit
    
    /// <summary>
    /// Inserts an arbitrary number of entities into the store, eliding existence checks
    /// </summary>
    abstract Insert:'T seq ->unit

    /// <summary>
    /// Inserts the matrix into the store
    /// </summary>
    abstract InsertMatrix:IDataMatrix -> unit

    /// <summary>
    /// Merges the matrix into the store
    /// </summary>
    abstract MergeMatrix:IDataMatrix->unit

    /// <summary>
    /// Retrieves implementation of the identified command contract from the store
    /// </summary>
    abstract GetCommandContract: unit -> 'TContract when 'TContract : not struct    

    /// <summary>
    /// Executes a supplied command against the store
    /// </summary>
    abstract ExecuteCommand:command : 'TCommand -> 'TResult



/// <summary>
/// Defines contract for fully-capable unparameterized data store
/// </summary>
type IDataStore =
    inherit IQueryableDataStore
    inherit IMutableDataStore

/// <summary>
/// Defines contract for a fully-capable data store parameterized by query type
/// </summary>
type IDataStore<'Q> =
    inherit IMutableDataStore<'Q>
    inherit IQueryableDataStore<'Q>
    
type IDataStoreProvider =
    abstract GetDataStore:string-> IDataStore
    abstract GetSpecificStore: string -> 'T

[<AbstractClass>]
type DataStoreProvider<'Q>( factory: string -> IDataStore<'Q>) =
                
    let adapt (store : IDataStore<'Q>) =
        {
            new IDataStore with
                member this.Insert items =
                    items |> store.Insert

                member this.InsertMatrix m =
                    m |> store.InsertMatrix
                        
                member this.Merge items =
                    items |> store.Merge
        
                member this.MergeMatrix m =
                    m |> store.MergeMatrix

                member this.SelectAll() =
                    store.SelectAll()
        
                member this.SelectMatrix(q) =
                    store.SelectMatrix(q :> obj :?> 'Q)

                member this.Select(q) = 
                    store.Select(q :> obj :?> 'Q)

                member this.GetCommandContract() : 'TContract =
                    store.GetCommandContract<'TContract>()

                member this.GetQueryContract() =
                    NotImplementedException() |> raise

                member this.ExecuteCommand(c) =
                    store.ExecuteCommand(c)
                member this.ConnectionString =
                    store.ConnectionString
          }
           
    interface IDataStoreProvider with
        member this.GetDataStore cs = 
            cs |> factory |> adapt

        member this.GetSpecificStore cs =
            cs |> factory :> obj :?> 'T    
        


        
     
type ColumnFilter = 
    | Equal of columnName : string * value : obj
    | NotEqual of columnName : string * value : obj
    | GreaterThan of columnName : string * value : obj
    | GreaterThanOrEqual of columnName : string * value : obj
    | LessThan of columnName : string * value : obj
    | LessThanOrEqual of columnName : string * value : obj
    | StartsWith of columnName : string * value : obj
    | EndsWith of columnName : string * value : obj
    | Contains of columnName : string * value : obj

type ColumnSortCriterion =
    | AscendingSort of columnName : string
    | DescendingSort of columnName : string

type TabularPageInfo = TabularPageInfo of pageNumber : int option * pageSize : int option

type ColumnFilterCriterion =
    | AndFilter of ColumnFilter
    | OrFilter of ColumnFilter
with
    member this.Filter = match this with |AndFilter(x)|OrFilter(x) -> x

/// <summary>
/// Represents a parameter supplied to a query
/// </summary>
type QueryParameter = QueryParameter of Name : string * Value : obj

/// <summary>
/// Represents a runtime-configurable query that supports pagination along with 
//  adjustable column selection, sorting criteria and fiter criteria
/// </summary>
type DynamicQuery = 
        DynamicQuery of 
            TabularName : DataObjectName * 
            ColumnNames : string list *
            Filters : ColumnFilterCriterion list*
            SortCriteria : ColumnSortCriterion list*
            Parameters : QueryParameter list *
            PageNumber : int option *
            PageSize : int option
       
       
/// <summary>
/// Specifies a query that is fulfilled by a routine
/// </summary>
type RoutineQuery = RoutineQuery of RoutineName : string * Parameters : QueryParameter list

        
           
/// <summary>
/// Represents the intent to retrieve data from an Excel workbook
/// </summary>
type ExcelDataStoreQuery =
    /// Retrieves the data contained within a named worksheet
    | FindWorksheetByName of worksheetName : string
    /// Retrieves data contained in an excel table identified by name
    | FindTableByName of tableName : string
    /// Retrieves all worksheets in the workbook
    | FindAllWorksheets

/// <summary>
/// Defines the contract for a data store that can read/write Excel workbooks
/// </summary>
type IExcelDataStore =
    inherit IDataStore<ExcelDataStoreQuery>

/// <summary>
/// Represents the intent to retrieve data from a CSV file
/// </summary>
type CsvDataStoreQuery =
    /// Retrieves the data contained in the identified CSV file 
    | FindCsvByFilename of filename : string

/// <summary>
/// Defines the contract for a data store that can read/write CSV files
/// </summary>
type ICsvDataStore =
    inherit IDataStore<CsvDataStoreQuery>
