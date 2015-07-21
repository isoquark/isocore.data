// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Diagnostics

/// <summary>
/// Represents a query pameter
/// </summary>
type SqlQueryParameter = SqlQueryParameter of name : string * value : obj    

/// <summary>
/// Represents a query to the store
/// </summary>
type DataStoreQuery =
    | TabularQuery of tabularName : string * columnNames : string list
    | TableFunctionQuery of  functionName : string * parameters : SqlQueryParameter list
    | ProcedureQuery of procedureName : string * parameters : SqlQueryParameter list



/// <summary>
/// Classifies supported data object/element types
/// </summary>
type DataElementKind =
    | Table = 1uy
    | View = 2uy
    | Procedure = 3uy
    | TableFunction = 4uy
    | ScalarFunction = 6uy
    | Sequence = 5uy
    | Column = 1uy
    /// Identifies a top-level namescope within a data store. This has
    /// an obvious interpretation in the context of relational data stores.
    /// Otherwise, interpretation is dependent on the type of store
    | Schema = 1uy
    /// Identifies a custom primitive. In SQL server, for example, this
    /// is eqivalent to an object created via CREATE TYPE and based on
    /// an intrinsic type
    | CustomPrimitive = 1uy

/// <summary>
/// Represents a store command
/// </summary>
type SqlStoreCommand =
| TruncateTable of tableName : DataObjectName

/// <summary>
/// Responsible for identifying a Data Store, Network Address or other resource
/// </summary>
type ConnectionString = ConnectionString of string list
with
    /// <summary>
    /// The components of the connection string
    /// </summary>
    member this.Components = match this with ConnectionString(components) -> components
    
        
/// <summary>
/// Defines the contract for a SQL Server Data Store
/// </summary>
type ISqlDataStore =
    /// <summary>
    /// Retrieves an identified collection of data entities from the store
    /// </summary>
    abstract Get:DataStoreQuery -> 'T list

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
    abstract Del:DataStoreQuery -> unit

    /// <summary>
    /// Obtains an identified contract from the store
    /// </summary>
    abstract GetContract: unit -> 'TContract when 'TContract : not struct

    /// <summary>
    /// Inserts an arbitrary number of records into a table
    /// </summary>
    abstract BulkInsert:'T seq ->unit

    /// <summary>
    /// Executes a supplied command against the store
    /// </summary>
    abstract ExecuteCommand:command : SqlStoreCommand -> unit

    /// <summary>
    /// Gets the connection string that identifies the represented store
    /// </summary>
    abstract ConnectionString :string
   
    /// <summary>
    /// Gets the store's metadata provider
    /// </summary>
    abstract MetadataProvider : ISqlMetadataProvider
   