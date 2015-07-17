// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Data
open System.Diagnostics
open System.Text


open IQ.Core.Framework


/// <summary>
/// Defines a metamodel that describes data models
/// </summary>
[<AutoOpen>]
module DataMetamodel = 
                    
    /// <summary>
    /// Describes a column in a table or view
    /// </summary>
    [<DebuggerDisplay("{Position} {Name,nq} {StorageType}")>]
    type ColumnDescription = {
        /// The column's name
        Name : string        
        /// The column's position relative to the other columns
        Position : int
        /// The column's data type
        StorageType : DataType                
        /// Specifies whether the column allows null
        Nullable : bool           
        /// Specifies the means by which the column is automatically populated, if applicable 
        AutoValue : AutoValueKind option    
    }

    /// <summary>
    /// Describes a table or view
    /// </summary>
    type TabularDescription = {
        /// The name of the table
        Name : DataObjectName        
        /// Specifies the  purpose of the table
        Description : string option
        /// The columns in the table
        Columns : ColumnDescription list
    }

    /// <summary>
    /// Describes a routine in a function or procedure
    /// </summary>
    type RoutineParameterDescription = {
        /// The parameter's name
        Name : string
        /// The parameter's position relative to the other columns
        Position : int
        /// The column's data type
        StorageType : DataType
        /// The direction of the parameter
        Direction : ParameterDirection
    }

    /// <summary>
    /// Describes a stored procedure
    /// </summary>
    type ProcedureDescription = {
        /// The name of the procedure
        Name : DataObjectName
        /// The parameters
        Parameters : RoutineParameterDescription list
    }
   
    /// <summary>
    /// Describes a table-valued function
    /// </summary>
    type TableFunctionDescription = {
        /// The name of the procedure
        Name : DataObjectName    
        /// The parameters
        Parameters : RoutineParameterDescription list
        /// The columns in the result set
        Columns : ColumnDescription list
    }


    /// <summary>
    /// Unifies data object types
    /// </summary>
    type DataObjectDescription =
    | TableFunctionObject of TableFunctionDescription
    | ProcedureObject of ProcedureDescription
    | TabularObject of TabularDescription


    /// <summary>
    /// Represents a data parameter value
    /// </summary>
    type DataParameterValue = DataParameterValue of  name : string * position : int * value : obj
    with
        member this.Position = match this with DataParameterValue(position=x) -> x
        member this.Name = match this with DataParameterValue(name=x) -> x
        member this.Value = match this with DataParameterValue(value=x) -> x


module DataObjectDescription =
    let getName (subject : DataObjectDescription) =
        match subject with
        | TableFunctionObject(x) -> 
            x.Name
        | ProcedureObject(x) -> 
            x.Name
        | TabularObject(x) ->
            x.Name

    let getParameters (subject : DataObjectDescription) =
        match subject with
        | TableFunctionObject(x) -> 
            x.Parameters
        | ProcedureObject(x) -> 
            x.Parameters
        | TabularObject(x) ->
            []
    
    let tryFindParameter name (subject : DataObjectDescription) =
        match subject with
        | TableFunctionObject(x) -> 
            x.Parameters |> List.tryFind(fun p -> p.Name = name)
        | ProcedureObject(x) -> 
            x.Parameters |> List.tryFind(fun p -> p.Name = name)
        | TabularObject(x) ->
            None

    let findParameter name (subject : DataObjectDescription) =
        subject |> tryFindParameter name |> Option.get

    let unwrapTableFunction (subject : DataObjectDescription) =
        match subject with
        | TableFunctionObject(x) -> x
        | _ -> ArgumentException() |> raise
        
    let unwrapProcedure (subject : DataObjectDescription) =
        match subject with
        | ProcedureObject(x) -> x
        | _ -> ArgumentException() |> raise

    let unwrapTabular (subject : DataObjectDescription) =
        match subject with
        | TabularObject(x) -> x
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



                        