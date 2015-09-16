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

/// <summary>
/// Defines contract for unparameterized data stores that can be weakly queried
/// </summary>
type IQueryableObjectStore =
    abstract Select:obj->obj seq

    /// <summary>
    /// Gets the connection string used to connect to the store
    /// </summary>
    abstract ConnectionString : string

/// <summary>
/// Defines contract for unparameterized queryable data store
/// </summary>
type IQueryableDataStore =
    /// <summary>
    /// Retrieves a query-identified collection of data entities from the store
    /// </summary>
    abstract Select:'Q->'T seq
    
    /// <summary>
    /// Gets the connection string used to connect to the store
    /// </summary>
    abstract ConnectionString : string
    

/// <summary>
/// Defines contract for queryable data store parameterized by query type
/// </summary>
type IQueryableDataStore<'Q> =
    /// <summary>
    /// Retrieves a query-identified collection of data entities from the store
    /// </summary>
    abstract Select:'Q->'T seq
    
    /// <summary>
    /// Gets the connection string used to connect to the store
    /// </summary>
    abstract ConnectionString : string


/// <summary>
/// Defines contract for queryable data store parameterized by query and element type
/// </summary>
type IQueryableDataStore<'T,'Q> =
    /// <summary>
    /// Retrieves a query-identified collection of data entities from the store
    /// </summary>
    abstract Select:'Q->'T seq
    
    /// <summary>
    /// Gets the connection string used to connect to the store
    /// </summary>
    abstract ConnectionString : string

/// <summary>
/// Defines contract for weakly-typed data stores
/// </summary>
type IObjectStore =
    inherit IQueryableObjectStore
    /// <summary>
    /// Deletes a query-identified collection of data entities from the store
    /// </summary>
    abstract Delete:obj->unit

    /// <summary>
    /// Persists a collection of data entities to the store, inserting or updating as appropriate
    /// </summary>
    abstract Merge:obj seq->unit
    
    /// <summary>
    /// Inserts an arbitrary number of entities into the store, eliding existence checks
    /// </summary>
    abstract Insert:obj seq ->unit

/// <summary>
/// Defines contract for unparameterized mutable data store
/// </summary>
type IDataStore =
    inherit IQueryableDataStore
    /// <summary>
    /// Deletes a query-identified collection of data entities from the store
    /// </summary>
    abstract Delete:'Q->unit

    /// <summary>
    /// Persists a collection of data entities to the store, inserting or updating as appropriate
    /// </summary>
    abstract Merge:'T seq->unit
    
    /// <summary>
    /// Inserts an arbitrary number of entities into the store, eliding existence checks
    /// </summary>
    abstract Insert:'T seq ->unit

/// <summary>
/// Defines contract for mutable data store parametrized by query type
/// </summary>
type IDataStore<'Q> =
    inherit IQueryableDataStore<'Q>

    /// <summary>
    /// Deletes a query-identified collection of data entities from the store
    /// </summary>
    abstract Delete:'Q->unit

    /// <summary>
    /// Persists a collection of data entities to the store, inserting or updating as appropriate
    /// </summary>
    abstract Merge:'T seq->unit
    
    /// <summary>
    /// Inserts an arbitrary number of entities into the store, eliding existence checks
    /// </summary>
    abstract Insert:'T seq ->unit
    

/// <summary>
/// Defines contract for mutable data store parametrized by query and element type
/// </summary>
type IDataStore<'T,'Q> =
    inherit IQueryableDataStore<'T,'Q>
    
    /// <summary>
    /// Deletes a query-identified collection of data entities from the store
    /// </summary>
    abstract Delete:'Q->unit

    /// <summary>
    /// Persists a collection of data entities to the store, inserting or updating as appropriate
    /// </summary>
    abstract Merge:'T seq->unit
    
    /// <summary>
    /// Inserts an arbitrary number of entities into the store, eliding existence checks
    /// </summary>
    abstract Insert:'T seq ->unit


/// <summary>
/// Defines contract for a tabular data source
/// </summary>
type IDataTable =
    /// <summary>
    /// Describes the encapsulated data
    /// </summary>
    abstract Description : ITabularDescription
    /// <summary>
    /// The encapsulared data
    /// </summary>
    abstract RowValues : IReadOnlyList<obj[]>
    /// <summary>
    /// Gets the value at the intersection fo the specified (0-based) row and column
    /// </summary>
    abstract Item : row : int * col : int -> obj

type TabularData = TabularData of description : ITabularDescription * rowValues : rolist<obj[]>
with
    member this.RowValues = 
        match this with TabularData(rowValues=x) -> x
    
    member this.Description = 
        match this with TabularData(description=x) -> x
    
    interface IDataTable with
        member this.RowValues = this.RowValues
        member this.Description = this.Description
        member this.Item(row,col) = this.RowValues.[row].[col]
    
type ITabularStore =
    abstract Merge:IDataTable->unit
    abstract Delete:'Q->unit
    abstract Select: 'Q->IDataTable
    abstract Insert:IDataTable->unit


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
       
       
[<AutoOpen; Extension>]
module DataStoreExtensions =
    [<Extension>]
    type IQueryableDataStore<'T,'Q> 
    with
        member this.SelectOne(q) = q |> this.Select |> fun x -> x.First()
 

type RoutineQuery = RoutineQuery of routineName : string * parameters : QueryParameter list

/// <summary>
/// Represents the intent to retrieve data from a SQL data store
/// </summary>
type SqlDataStoreQuery =
    | DirectStoreQuery of string
    | DynamicStoreQuery of  DynamicQuery
    | TableFunctionQuery of  RoutineQuery
    | ProcedureQuery of RoutineQuery

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
/// Defines the contract for a SQL Server Data Store that can persist and hydrate data via proxy types
/// </summary>
type ISqlDataStore =
    /// <summary>
    /// Deletes and identified collection of data entities from the store
    /// </summary>
    abstract Delete:SqlDataStoreQuery -> unit
    /// <summary>
    /// Gets the connection string that identifies the represented store
    /// </summary>
    abstract ConnectionString :string
    /// <summary>
    /// Gets the store's metadata provider
    /// </summary>
    abstract MetadataProvider : ISqlMetadataProvider
    /// <summary>
    /// Obtains an identified contract from the store
    /// </summary>
    abstract GetContract: unit -> 'TContract when 'TContract : not struct
    /// <summary>
    /// Executes a supplied command against the store
    /// </summary>
    abstract ExecuteCommand:command : 'TCommand -> 'TResult
    /// <summary>
    /// Retrieves an identified collection of data entities from the store
    /// </summary>
    abstract Get:SqlDataStoreQuery -> 'T rolist
    /// <summary>
    /// Retrieves all entities of a given type from the store
    /// </summary>
    abstract Get:unit -> 'T rolist
    /// <summary>
    /// Retrieves an identified set of tabular data from the store
    /// </summary>
    abstract GetTable:SqlDataStoreQuery -> IDataTable
    /// <summary>
    /// Persists a collection of data entities to the store, inserting or updating as appropriate
    /// </summary>
    abstract Merge:'T seq -> unit
    /// <summary>
    /// Inserts an arbitrary number of entities into the store, eliding existence checks
    /// </summary>
    abstract Insert:'T seq ->unit
    /// <summary>
    /// Inserts specified tabular data into the store
    /// </summary>
    abstract InsertTable:IDataTable->unit
   
/// <summary>
/// Represents the intent to retrieve data from an Excel data store
/// </summary>
type ExcelDataStoreQuery =
    /// Retrieves the data contained within a named worksheet
    | FindWorksheetByName of worksheetName : string
    /// Retrieves data contained in an excel table identified by name
    | FindTableByName of tableName : string
    /// Retrieves all worksheets in the workbook
    | FindAllWorksheets


