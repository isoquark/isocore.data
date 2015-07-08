﻿namespace IQ.Core.Data

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
/// Responsible for identifying a data object within the scope of some container
/// </summary>
type DataObjectName = DataObjectName of schemaName : string * localName : string
with
    /// <summary>
    /// Specifies the name of the schema (or namescope such as a package or namespace)
    /// </summary>
    member this.SchemaName = match this with DataObjectName(schemaName=x) -> x
        
    /// <summary>
    /// Specifies the name of the object relative to the schema
    /// </summary>
    member this.LocalName = match this with DataObjectName(localName=x) -> x
            
    /// <summary>
    /// Renders a faithful representation of an instance as text
    /// </summary>
    member this.ToSemanticString() =
        match this with DataObjectName(s,l) -> sprintf "(%s,%s)" s l

    /// <summary>
    /// Renders a representation of an instance as text
    /// </summary>
    override this.ToString() = this.ToSemanticString()


/// <summary>
/// Represents a store command
/// </summary>
type SqlStoreCommand =
| TruncateTable of tableName : DataObjectName
    
        
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

    abstract BulkInsert:'T seq ->unit

    abstract ExecuteCommand:command : SqlStoreCommand -> unit
