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
    /// Enumerates the available means that lead to a column being automatically populated
    /// with a valid value
    /// </summary>
    type AutoValueKind =
        /// Column is not automatically populated
        | None = 0
        /// Column is automatically populated with a default value
        | Default = 1
        /// Column is automatically populated with an identity value
        | Identity = 2
        /// Column is automatically populated with a computed value
        | Computed = 3
        /// Column is automatically populated with a value from a sequence
        | Sequence = 4
    
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
        StorageType : StorageType option
        
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
    

//    type StorageType 
//    with
//        static member Parse(text) = 
//            if text |> Txt.containsCharacter '(' then
                