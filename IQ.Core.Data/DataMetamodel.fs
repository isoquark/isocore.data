// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Data
open System.Diagnostics
open System.Text


open IQ.Core.Framework




module DataObjectDescription =
    let getName (subject : DataObjectDescription) =
        match subject with
        | TableFunctionDescription(x) -> 
            x.Name
        | ProcedureDescription(x) -> 
            x.Name
        | TableDescription(x) ->
            x.Name
        | ViewDescription(x) ->
            x.Name
        | SequenceDescription(x) ->
            x.Name
        | DataTypeDescription(x) ->
            x.Name

    let getParameters (subject : DataObjectDescription) =
        match subject with
        | TableFunctionDescription(x) -> 
            x.Parameters
        | ProcedureDescription(x) -> 
            x.Parameters
        | TableDescription(x) ->
            []
        | ViewDescription(x) ->
            []
        | SequenceDescription(x) ->
            []
        | DataTypeDescription(x) ->
            []
    
    let tryFindParameter name (subject : DataObjectDescription) =
        match subject with
        | TableFunctionDescription(x) -> 
            x.Parameters |> List.tryFind(fun p -> p.Name = name)
        | ProcedureDescription(x) -> 
            x.Parameters |> List.tryFind(fun p -> p.Name = name)
        | TableDescription(x) ->
            None
        | ViewDescription(x) ->
            None
        | SequenceDescription(x) ->
            None
        | DataTypeDescription(x) ->
            None

    let findParameter name (subject : DataObjectDescription) =
        subject |> tryFindParameter name |> Option.get

    let unwrapTableFunction (subject : DataObjectDescription) =
        match subject with
        | TableFunctionDescription(x) -> x
        | _ -> ArgumentException() |> raise
        
    let unwrapProcedure (subject : DataObjectDescription) =
        match subject with
        | ProcedureDescription(x) -> x
        | _ -> ArgumentException() |> raise

    let unwrapTabular (subject : DataObjectDescription) =
        match subject with
        | TableDescription(x) -> x
        | ViewDescription(x) -> x
        | _ -> ArgumentException() |> raise

[<AutoOpen>]
module DataMetamodelExtensions =
    
    /// <summary>
    /// Defines augmentations for the TableDescription type
    /// </summary>
    type TabularDescription 
    with
        /// <summary>
        /// Finds a column identified by its name
        /// </summary>
        /// <param name="name">The name of the column</param>
        member this.Item(name) = 
            this.Columns |> List.find(fun column -> column.Name = name)

        /// <summary>
        /// Finds a column identified by its position
        /// </summary>
        /// <param name="position">The position of the column</param>
        member this.Item(position) = 
            this.Columns |> List.find(fun column -> column.Position = position)
    

    /// <summary>
    /// Defines augmentations for the ProcedureDescription type
    /// </summary>
    type DataObjectDescription
    with
        member this.FindParameter(name) =
            this |> DataObjectDescription.findParameter name

        member this.Parameters = 
            this |> DataObjectDescription.getParameters
        
        member this.Name =
            this |> DataObjectDescription.getName



                        