// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Contracts

open System
open System.Diagnostics
open System.Collections.Generic

/// <summary>
/// Represents a query pameter
/// </summary>
type SqlQueryParameter = SqlQueryParameter of name : string * value : obj    

/// <summary>
/// Represents the intent to retrieve data from a SQL data store
/// </summary>
type SqlDataStoreQuery =
    | TabularQuery of tabularName : string * columnNames : string list
    | TableFunctionQuery of  functionName : string * parameters : SqlQueryParameter list
    | ProcedureQuery of procedureName : string * parameters : SqlQueryParameter list

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
/// Represents a store command
/// </summary>
type SqlStoreCommand =
    /// Represents the intent to truncate a table
    | TruncateTable of tableName : DataObjectName
    /// Represents the intent to allocate a range of sequence values
    | AllocateSequenceRange of sequenceName : DataObjectName * count : int

/// <summary>
/// Represents a store command result
/// </summary>
type SqlStoreCommandResult =
    | TruncateTableResult of rows : int
    | AllocateSequenceRangeResult of first : obj

/// <summary>
/// Defines the contract for a SQL Server Data Store
/// </summary>
type ISqlDataStore =
    /// <summary>
    /// Retrieves an identified collection of data entities from the store
    /// </summary>
    abstract Get:SqlDataStoreQuery -> 'T list

    /// <summary>
    /// Retrieves all entities of a given type from the store
    /// </summary>
    abstract Get:unit -> 'T list

    /// <summary>
    /// Persists a collection of data entities to the store, inserting or updating as appropriate
    /// </summary>
    abstract Put:'T seq -> unit

    /// <summary>
    /// Deletes and identified collection of data entities from the store
    /// </summary>
    abstract Del:SqlDataStoreQuery -> unit

    /// <summary>
    /// Obtains an identified contract from the store
    /// </summary>
    abstract GetContract: unit -> 'TContract when 'TContract : not struct

    /// <summary>
    /// Inserts an arbitrary number of entities into the store, eliding existence checks
    /// </summary>
    abstract Insert:'T seq ->unit

    /// <summary>
    /// Executes a supplied command against the store
    /// </summary>
    abstract ExecuteCommand:command : SqlStoreCommand -> SqlStoreCommandResult

    /// <summary>
    /// Gets the connection string that identifies the represented store
    /// </summary>
    abstract ConnectionString :string
   
    /// <summary>
    /// Gets the store's metadata provider
    /// </summary>
    abstract MetadataProvider : ISqlMetadataProvider
   