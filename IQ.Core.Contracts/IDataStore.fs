// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System

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
/// Defines the contract to which all data stores must adhere
/// </summary>
type IDataStore<'Q> =
    /// <summary>
    /// Retrieves a query-identified collection of data entities from the store
    /// </summary>
    abstract Get:'Q->'T list

    /// <summary>
    /// Deletes a query-identified collection of data entities from the store
    /// </summary>
    abstract Del:'Q->unit

    /// <summary>
    /// Persists a collection of data entities to the store, inserting or updating as appropriate
    /// </summary>
    abstract Put:'T seq->unit
    
    /// <summary>
    /// Inserts an arbitrary number of entities into the store, eliding existence checks
    /// </summary>
    abstract Insert:'T seq ->unit
    
    /// <summary>
    /// Gets the connection string that identifies the represented store
    /// </summary>
    abstract ConnectionString :string



