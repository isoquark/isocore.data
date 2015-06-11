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
    
    type DataType =
        | IntrinsicPrimitive of name : string 
        | CustomPrimitive of name : DataObjectName 
        | IntrinsicClrType of name : DataObjectName * clrType : Type
        | CustomClrType of name : DataObjectName * clrType : Type
        | CustomTableType of name : DataObjectName

    type DataTypeReference =
        | IntrinsicPrimitiveReference of dataType : DataType * length : int option * precision : uint8 option * scale : uint8 option
        | CustomPrimitive of dataType : DataType
        | IntrinsicClrType of dataType : DataType
        | CustomClrType of dataType : DataType
        | CustomTableType of dataType : DataType
    with 
        member this.DataType = 
            match this with 
                | IntrinsicPrimitiveReference(dataType=x) 
                | CustomPrimitive(dataType=x) 
                | IntrinsicClrType(dataType=x)
                | CustomClrType(dataType=x)
                | CustomTableType(dataType=x) -> x
        
        member this.Length = 
            match this with 
            | IntrinsicPrimitiveReference(length=x) -> x
            | _ -> None
    
        member this.Precision = 
            match this with 
            | IntrinsicPrimitiveReference(precision=x) -> x
            | _ -> None

        member this.Scale = 
            match this with 
            | IntrinsicPrimitiveReference(scale=x) -> x
            | _ -> None

    /// <summary>
    /// Describes a column in a table or view
    /// </summary>
    type ColumnDescription = {
        /// The column's name
        Name : string
        
        /// The column's position relative to the other columns
        Position : int

        /// The column's data type
        DataType : DataTypeReference option
        
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

    

