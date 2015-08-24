// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Contracts

open System
open System.Diagnostics



/// <summary>
/// Responsible for identifying a data object within the scope of some container
/// </summary>
type DataObjectName = DataObjectName of schemaName : string * localName : string
with
    /// <summary>
    /// Specifies the name of the schema (or namescope such as a package or namespace)
    /// </summary>
    member this.SchemaName = match this with DataObjectName(schemaName=x) -> x
        
    /// <summary>
    /// Specifies the name of the object relative to the schema
    /// </summary>
    member this.LocalName = match this with DataObjectName(localName=x) -> x
            
    /// <summary>
    /// Renders a faithful representation of an instance as text
    /// </summary>
    member this.ToSemanticString() =
        match this with DataObjectName(s,l) -> sprintf "(%s,%s)" s l

    /// <summary>
    /// Renders a representation of an instance as text
    /// </summary>
    override this.ToString() = this.ToSemanticString()

/// <summary>
/// Specifies the available DataType classifications
/// </summary>
/// <remarks>
/// Note that the DataType class is not sufficient to characterize the DataType type and
/// additional information, such as length or data object name is needed to store/instantiate
/// a corresponding value
/// </remarks>
type DataKind =
    | Unspecified = 0uy
        
    //Integer types
    //-------------------------------------------------------------------------
    | Bit = 10uy //bit
    | UInt8 = 20uy //tinyint
    | UInt16 = 21uy //no direct map, use int
    | UInt32 = 22uy // no direct map, use bigint
    | UInt64 = 23uy // no direct map, use varbinary(8)
    | Int8 = 30uy //no direct map, use smallint
    | Int16 = 31uy //smallint
    | Int32 = 32uy //int
    | Int64 = 33uy //bigint
        
    //Binary types
    //-------------------------------------------------------------------------
    | BinaryFixed = 40uy //binary 
    | BinaryVariable = 41uy //varbinary
    | BinaryMax = 42uy
        
    //ANSI text types
    //-------------------------------------------------------------------------
    | AnsiTextFixed = 50uy //char
    | AnsiTextVariable = 51uy //varchar
    | AnsiTextMax = 52uy
        
    ///Unicode text types
    //-------------------------------------------------------------------------
    | UnicodeTextFixed = 53uy //nchar
    | UnicodeTextVariable = 54uy //nvarchar
    | UnicodeTextMax = 55uy
        
    ///Time-related types
    //-------------------------------------------------------------------------
    | DateTime = 62uy //corresponds to datetime2
    | DateTimeOffset = 63uy
    | TimeOfDay = 64uy //corresponds to time
    | Date = 65uy //corresponds to date        
    | Duration = 66uy //no direct map, use bigint to store number of ticks
        
    //Approximate real types
    //-------------------------------------------------------------------------
    | Float32 = 70uy //corresponds to real
    | Float64 = 71uy //corresponds to float
        
    //Exact real types
    //-------------------------------------------------------------------------
    | Decimal = 80uy
    | Money = 81uy
        
    //Other types
    //-------------------------------------------------------------------------
    | Guid = 90uy //corresponds to uniqueidentifier
    | Xml = 100uy
    | Json = 101uy
    | Flexible = 110uy //corresponds to sql_variant
                      
    ///Intrinsic SQL CLR types
    //-------------------------------------------------------------------------
    | Geography = 150uy
    | Geometry = 151uy
    | Hierarchy = 152uy
        
    /// A structured document of some sort; specification of an instance
    /// requires a DOM in code that represents the type (may be a simple record
    /// or something as involved as the HTML DOM) and a reader/writer type identifier
    /// than can be used to serialize/reconstitute document instances from ther
    /// storage format.
    | TypedDocument = 160uy 

    /// A non-intrinsic table data type in sql; probably data DataTable or similar in CLR
    | CustomTable = 170uy 
    /// A non-intrinsic CLR type
    | CustomObject = 171uy 
    /// A non-intrinsic primitive based on an intrinsic primitive; a custom type in code,
    /// e.g. a specialized struct, DU, etc.
    | CustomPrimitive = 172uy 

/// <summary>
/// Specifies a DataType class together with the information that is required to
/// instantiate and store values corresponding to that class
/// </summary>
type DataType =
    | BitDataType
    | UInt8DataType
    | UInt16DataType
    | UInt32DataType
    | UInt64DataType
    | Int8DataType
    | Int16DataType
    | Int32DataType
    | Int64DataType
    | BinaryFixedDataType of len : int
    | BinaryVariableDataType of maxlen : int
    | BinaryMaxDataType
    | AnsiTextFixedDataType of len : int
    | AnsiTextVariableDataType of maxlen : int
    | AnsiTextMaxDataType
    | UnicodeTextFixedDataType of len : int
    | UnicodeTextVariableDataType of maxlen : int
    | UnicodeTextMaxDataType
    | DateTimeDataType of precision : uint8
    | DateTimeOffsetDataType
    | TimeOfDayDataType of precision : uint8
    | TimespanDataType 
    | DateDataType
    | Float32DataType
    | Float64DataType
    | DecimalDataType of precision : uint8 * scale : uint8
    | MoneyDataType
    | GuidDataType
    | XmlDataType of schema : string
    | JsonDataType
    | VariantDataType
    | CustomTableDataType of name : DataObjectName
    | CustomObjectDataType of name : DataObjectName * clrType : Type
    | CustomPrimitiveDataType of name : DataObjectName
    | TypedDocumentDataType of doctype : Type
               
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
    StorageType : DataType    
    /// The column's documentation
    Documentation : string option            
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
    /// The tabular's documentation
    Documentation : string option
    /// The columns in the table
    Columns : ColumnDescription list
}

/// <summary>
/// Specifies the direction of a routine parameter
/// </summary>
type RoutineParameterDirection = 
    | Input       = 1 
    | Output      = 2 
    | InputOutput = 3 
    | ReturnValue = 6

/// <summary>
/// Describes a routine in a function or procedure
/// </summary>
type RoutineParameterDescription = {
    /// The parameter's name
    Name : string
    /// The parameter's position relative to the other columns
    Position : int
    /// The parameter's documentation
    Documentation : string option
    /// The column's data type
    StorageType : DataType
    /// The direction of the parameter
    Direction : RoutineParameterDirection
}

/// <summary>
/// Describes a stored procedure
/// </summary>
type ProcedureDescription = {
    /// The name of the procedure
    Name : DataObjectName
    /// The parameters
    Parameters : RoutineParameterDescription list
    /// The procedures's documentation
    Documentation : string option
}
   
/// <summary>
/// Describes a table-valued function
/// </summary>
type TableFunctionDescription = {
    /// The name of the procedure
    Name : DataObjectName    
    /// The parameters
    Parameters : RoutineParameterDescription list
    /// The function's documentation
    Documentation : string option
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

type SchemaMetadataQuery =
    | FindAllSchemas

type ViewMetadataQuery =
    | FindAllViews
    | FindUserViews
    | FindViewsBySchema of schemaName : string

type TableMetadataQuery =
    | FindAllTables
    | FindUserTables
    | FindTablesBySchema of schemaName : string

type TableFunctionMetadataQuery =
    | FindAllTableFunctions
    | FindTableFunctionsBySchema of schemaName : string

type ProcedureMetadataQuery =
    | FindAllProcedures
    | FindProceduresBySchema of schemaName : string


type SequenceMetadataQuery = 
    | FindAllSequences
    | FindSequencesBySchema of schemaName : string

type SqlMetadataQuery =
    | FindSchemas of SchemaMetadataQuery
    | FindViews of ViewMetadataQuery
    | FindTables of TableMetadataQuery
    | FindProcedures of ProcedureMetadataQuery
    | FindSequences of SequenceMetadataQuery

type ISqlMetadataProvider =
    abstract Describe:SqlMetadataQuery->DataObjectDescription list