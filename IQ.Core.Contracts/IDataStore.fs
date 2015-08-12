// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Contracts

open System
open System.Collections.Generic
open System.Runtime.CompilerServices

/// <summary>
/// Responsible for identifying a Data Store, Network Address or other resource
/// </summary>
type ConnectionString = ConnectionString of string list
with
    /// <summary>
    /// The components of the connection string
    /// </summary>
    member this.Components = match this with ConnectionString(components) -> components
    

type IQueryableObjectStore =
    abstract Select:obj->obj IReadOnlyList

    /// <summary>
    /// Gets the connection string used to connect to the store
    /// </summary>
    abstract ConnectionString : ConnectionString

/// <summary>
/// Defines contract for unparameterized queryable data store
/// </summary>
type IQueryableDataStore =
    /// <summary>
    /// Retrieves a query-identified collection of data entities from the store
    /// </summary>
    abstract Select:'Q->'T IReadOnlyList
    
    /// <summary>
    /// Gets the connection string used to connect to the store
    /// </summary>
    abstract ConnectionString : ConnectionString
    

/// <summary>
/// Defines contract for queryable data store parameterized by query type
/// </summary>
type IQueryableDataStore<'Q> =
    /// <summary>
    /// Retrieves a query-identified collection of data entities from the store
    /// </summary>
    abstract Select:'Q->'T IReadOnlyList
    
    /// <summary>
    /// Gets the connection string used to connect to the store
    /// </summary>
    abstract ConnectionString : ConnectionString


/// <summary>
/// Defines contract for queryable data store parameterized by query and element type
/// </summary>
type IQueryableDataStore<'T,'Q> =
    /// <summary>
    /// Retrieves a query-identified collection of data entities from the store
    /// </summary>
    abstract Select:'Q->'T IReadOnlyList
    
    /// <summary>
    /// Gets the connection string used to connect to the store
    /// </summary>
    abstract ConnectionString : ConnectionString

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

[<AutoOpen; Extension>]
module DataStoreExtensions =
    [<Extension>]
    type IQueryableDataStore<'T,'Q> 
    with
        member this.SelectOne(q) = q |> this.Select |> fun x -> x.[0]
    