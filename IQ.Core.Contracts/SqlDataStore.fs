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
type ISqlDataStore =
    inherit IDataStore<SqlDataStoreQuery>        
            


