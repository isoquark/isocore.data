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
    type RoutineParameter = {
        /// The parameter's name
        Name : string
    
        /// The column's data type
        StorageType : StorageType

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
        Parameters : RoutineParameter list
    }
   
    /// <summary>
    /// Describes a table-valued function
    /// </summary>
    type TableFunctionDescription = {
        /// The name of the procedure
        Name : DataObjectName
    
        /// The parameters
        Parameters : RoutineParameter list

        /// The columns in the result set
        Columns : ColumnDescription list

    }

module ProcedureDescription =
    /// <summary>
    /// Finds a procedure's named parameter
    /// </summary>
    /// <param name="name">The name of the parameter</param>
    /// <param name="proc">The procedure</param>
    let findParameter name (proc : ProcedureDescription) =
        proc.Parameters |> List.find(fun p -> p.Name = name)

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
    type ProcedureDescription
    with
        member this.FindParameter(name) =
            this |> ProcedureDescription.findParameter name
                