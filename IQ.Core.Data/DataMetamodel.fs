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
        StorageType : StorageType
                
        /// Specifies whether the column allows null
        Nullable : bool   
        
        /// Specifies the means by which the column is automatically populated, if applicable 
        AutoValue : AutoValueKind option    
    }

    /// <summary>
    /// Describes a table
    /// </summary>
    type TableDescription = {
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
        StorageType : StorageType

        /// The direction of the parameter
        Direction : ParameterDirection

    }

    /// <summary>
    /// Represents the value passed to a routine parameters
    /// </summary>
    type RoutineParameterValue = RoutineParameterValue of  name : string * position : int * value : obj
    with
        member this.Position = match this with RoutineParameterValue(position=x) -> x
        member this.Name = match this with RoutineParameterValue(name=x) -> x
        member this.Value = match this with RoutineParameterValue(value=x) -> x

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


    type DataObjectDescription =
    | TableFunctionObject of TableFunctionDescription
    | ProcedureObject of ProcedureDescription
    | TableObject of TableDescription


module DataObjectDescription =
    let getName (subject : DataObjectDescription) =
        match subject with
        | TableFunctionObject(x) -> 
            x.Name
        | ProcedureObject(x) -> 
            x.Name
        | TableObject(x) ->
            x.Name

    let getParameters (subject : DataObjectDescription) =
        match subject with
        | TableFunctionObject(x) -> 
            x.Parameters
        | ProcedureObject(x) -> 
            x.Parameters
        | TableObject(x) ->
            []
    
    let tryFindParameter name (subject : DataObjectDescription) =
        match subject with
        | TableFunctionObject(x) -> 
            x.Parameters |> List.tryFind(fun p -> p.Name = name)
        | ProcedureObject(x) -> 
            x.Parameters |> List.tryFind(fun p -> p.Name = name)
        | TableObject(x) ->
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

    let unwrapTable (subject : DataObjectDescription) =
        match subject with
        | TableObject(x) -> x
        | _ -> ArgumentException() |> raise

[<AutoOpen>]
module DataMetamodelExtensions =
    
    /// <summary>
    /// Defines augmentations for the TableDescription type
    /// </summary>
    type TableDescription 
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



                        