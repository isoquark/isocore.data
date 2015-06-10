namespace IQ.Core.Data

open System
open System.Data


/// <summary>
/// Defines a metamodel that describes data models
/// </summary>
[<AutoOpen>]
module DataMetamodel = 
    /// <summary>
    /// Describes a data type
    /// </summary>
    type DataTypeDescription = {
        /// The name of the data type
        Name : DataObjectName
        
        /// Specifies whether the data type is user-defined
        IsUserDefined : bool

        /// The maximum length of a value
        MaxLength : int
    
        /// Specifies whether values of the type can be null
        IsNullable : bool
        
        /// The allowable precision of a value of the data type if applicable
        Precision : uint8

        /// The allowable scale of a value of the data type if applicable
        Scale : uint8
        
        /// If applicable, the intrinsic base type
        BaseType : DataTypeDescription option

        /// If applicable, the SqlDbType of the data type
        DbType : SqlDbType option
    }

    /// <summary>
    /// Represents a data type usage instance, such as when declaring a column to be of a given data type
    /// </summary>
    type DataTypeReference = {
        
        /// The data type being referenced
        DataType : DataTypeDescription
        
        /// If applicable, the maximum length of a value for variable-length data types or
        /// the absolute length of a value for fixed length data types
        MaxLength : int option

        /// If applicable, the precision of the data type reference
        Precision : uint8 option

        /// If applicable, the scale of the data type reference
        Scale : uint8 option        
    }
    
    /// <summary>
    /// Describes a column in a table or view
    /// </summary>
    type ColumnDescription = {
        /// The column's name
        Name : string
        
        /// The column's position relative to the other columns
        Position : int

        /// The column's data type
        DataType : DataTypeReference
        
        /// Specifies whether the column allows null
        Nullable : bool        
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
module DatMetamodelExtensions =
    /// <summary>
    /// Defines augmentations for the TableDescription type
    /// </summary>
    type TableDescription with
    member
        this.FindColumn(name) = this.Columns |> List.find(fun column -> column.Name = name)

    

