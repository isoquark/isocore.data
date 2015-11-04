// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Contracts

open System
open System.Diagnostics
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Linq

open IQ.Core.Contracts

type IDataStoreCommon =
    abstract ConnectionString:string
        
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
    /// Executes a supplied command against the store that returns no result
    /// </summary>
    abstract ExecutePureCommand: command : 'TCommand -> unit


/// <summary>
/// Defines contract for a fully-capable data store parameterized by query type
/// </summary>
type IDataStore<'Q> =
    inherit IMutableDataStore<'Q>
    inherit IQueryableDataStore<'Q>

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
    /// Executes a supplied command against the store that returns no result
    /// </summary>
    abstract ExecutePureCommand: command : 'TCommand -> unit

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
/// Defines contract for fully-capable unparameterized data store
/// </summary>
type IDataStore =
    inherit IQueryableDataStore
    inherit IMutableDataStore
    

/// <summary>
/// Lists the supported kinds of data stores
/// </summary>
type DataStoreKind =
    /// Identifies a SQL Server data store
    | Sql = 1uy
    /// Identifies a CSV data store
    | Csv = 2uy
    /// Identifies an Excel data store
    | Xls = 3uy
    /// Identifies a memory store
    | Mem = 4uy

type IDataStoreProvider =
    abstract GetDataStore: string-> IDataStore
    abstract GetDataStore: string -> 'T
    
    /// <summary>
    ///  Specifies the kinds of data stores the provider can yield
    /// </summary>
    abstract SupportedStores : DataStoreKind list


[<AttributeUsage(AttributeTargets.Interface)>]
type DataStoreContractAttribute(storeKind) =
    inherit Attribute()
    
    member this.StoreKind : DataStoreKind = storeKind
    
/// <summary>
/// Identifies a function that produces a data store provider
/// </summary>
[<AttributeUsage(AttributeTargets.Method)>]
type DataStoreProviderFactoryAttribute(storeKind) =
    inherit Attribute()
    
    member this.StoreKind : DataStoreKind = storeKind
    
            
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
    /// Retrieves the data contained within a worksheet at a specified (0-based) index
    | FindWorksheetByIndex of index : uint16
    /// Retrieves data contained in an excel table identified by name
    | FindTableByName of tableName : string
    /// Retrieves all worksheets in the workbook
    | FindAllWorksheets

/// <summary>
/// Defines the contract for a data store that can read/write Excel workbooks
/// </summary>
[<DataStoreContract(DataStoreKind.Xls)>]
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
[<DataStoreContract(DataStoreKind.Csv)>]
type ICsvDataStore =
    inherit IDataStore<CsvDataStoreQuery>

/// <summary>
/// Classifies supported data object/element types
/// </summary>
type SqlElementKind =
    /// Identifies a table
    | Table = 1uy
    /// Identifies a view
    | View = 2uy
    /// Identifies a stored procedure
    | Procedure = 3uy
    /// Identifies a table-valued function
    | TableFunction = 4uy
    /// Identifies a scalar function
    | ScalarFunction = 6uy
    /// Identifies a sequence object
    | Sequence = 5uy
    /// Identifies a column
    | Column = 1uy
    /// Identifies a schema
    | Schema = 1uy
    /// Identifies a custom primitive, e.g., an object created by issuing
    /// a CREATE TYPE command that is based on an intrinsic type
    | CustomPrimitive = 1uy

/// <summary>
/// Identifies a function that handles a SQL command
/// </summary>
[<AttributeUsage(AttributeTargets.Method)>]
type SqlCommandHandlerAttribute() =
    inherit Attribute()

/// <summary>
/// Represents the intent to truncate a table
/// </summary>
type TruncateTable = TruncateTable of TableName : DataObjectName

/// <summary>
/// Encapsulates the result of executing the TruncateTable command
/// </summary>
type TruncateTableResult = TruncateTableResult of RowCount : int
    
/// <summary>
/// Represents the intent to create a table
/// </summary>
type CreateTable = CreateTable of Description : TableDescription

/// <summary>
/// Represents the intent to drop a table
/// </summary>
type DropTable = DropTable of TableName : DataObjectName
    
/// <summary>
/// Represents the intent to allocate a range of sequence values
/// </summary>
type AllocateSequenceRange = AllocateSequenceRange of SequenceName : DataObjectName * Count : int

/// <summary>
/// Encapsulates the result of executing the AllocateSequenceRange command
/// </summary>
type AllocateSequenceRangeResult = AllocateSequenceRangeResult of FirstNumber : obj

/// <summary>
/// Represents the intent to retrieve the file table root of the current database
/// </summary>
type GetFileTableRoot() = class end

/// <summary>
/// Encapsulates the result of executing the FILETABLEROOTPATH() command
/// </summary>
type GetFileTableRootResult = GetFileTableRootResult of string

/// <summary>
/// Represents the intent to retrieve data from a SQL data store
/// </summary>
type SqlDataStoreQuery =
    | DirectStoreQuery of string
    | DynamicStoreQuery of  DynamicQuery
    | TableFunctionQuery of  RoutineQuery
    | ProcedureQuery of RoutineQuery
       
/// <summary>
/// Defines the contract for a SQL Server Data Store that can persist and hydrate data via proxy types
/// </summary>
[<DataStoreContract(DataStoreKind.Sql)>]
type ISqlDataStore =
    inherit IDataStore<SqlDataStoreQuery>
    /// <summary>
    /// Provides access to the store's underlying metadata provider
    /// </summary>
    abstract MetadataProvider : ISqlMetadataProvider
    
    /// <summary>
    /// Executes the (command) sql without interpetation
    /// </summary>
    abstract ExecuteSql:string->unit

            


