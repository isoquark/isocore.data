// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Contracts

open System
open System.Diagnostics
open System.Collections.Generic
open System.Runtime.CompilerServices

open IQ.Core.Framework

open IQ.Core.Framework.Contracts

/// <summary>
/// Responsible for identifying a data object within the scope of some container
/// </summary>
type DataObjectName = DataObjectName of SchemaName : string * LocalName : string            
with
    /// <summary>
    /// Renders a faithful representation of an instance as text
    /// </summary>
    member this.ToSemanticString() =
        match this with DataObjectName(s,l) -> sprintf "[%s].[%s]" s l

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
/// <remarks>
/// This is really a DataTypeReference or "instantiation"
/// </remarks>
type DataTypeReference =
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
    | DateTimeDataType of precision : uint8 * scale : uint8
    | DateTimeOffsetDataType
    | TimeOfDayDataType of precision : uint8 * scale : uint8
    | TimespanDataType 
    | RowversionDataType
    | DateDataType
    | Float32DataType
    | Float64DataType
    | DecimalDataType of precision : uint8 * scale : uint8
    | MoneyDataType of precision : uint8 * scale : uint8
    | GuidDataType
    | XmlDataType of schema : string
    | JsonDataType
    | VariantDataType
    | TableDataType of name : DataObjectName
    | ObjectDataType of name : DataObjectName * clrTypeName : string
    | CustomPrimitiveDataType of name : DataObjectName * baseType : DataTypeReference
    | TypedDocumentDataType of doctype : Type

/// <summary>
/// Represents a numeric value, duh.
/// </summary>
type NumericValue =
    | UInt8Value of uint8
    | UInt16Value of uint16
    | UInt32Value of uint32
    | UInt64Value of uint64
    | Int8Value of int8
    | Int16Value of int16
    | Int32Value of int32
    | Int64Value of int64
    | Float32Value of float32
    | Float64Value of float
    | DecimalValue of decimal
                   
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
/// Encapsulates the value of property within the context of a data element
/// </summary>
type DataElementProperty = | DataElementProperty of ProperyName : string * PropertyValue : obj
      
/// <summary>
/// Describes a data type definition
/// </summary>
type DataTypeDescription = {
    /// The name of the data type
    Name : DataObjectName
    /// The maximum length of the data type, if applicable
    MaxLength : int16
    /// The precision of the data type, if applicable 
    Precision : uint8
    /// The scale of the data type, if applicable 
    Scale : uint8
    /// Specifies whether the data type is allowed to be null
    IsNullable : bool
    /// Specifies whether the data type is a (user-defined) table data type
    IsTableType : bool
    /// Specifies whether the data type is a (user-defined) CLR object
    IsCustomObject : bool
    /// Specifies whether the data type is user-defined
    IsUserDefined : bool
    /// Specifies the name of the BCL type that the is used by ADO.Net
    DefaultBclTypeName : string
    /// If a user-defined primitive, the name of the intrinsic primitive base type
    BaseTypeName : DataObjectName option
    /// The attached properties
    Properties : DataElementProperty list
}
with
    override this.ToString() = this.Name.ToString()

/// <summary>
/// Describes a column in a table or view
/// </summary>
[<DebuggerDisplay("{Position} {Name,nq} {StorageType}")>]
type ColumnDescription = {
    /// The name of parent data object
    ParentName : DataObjectName
    /// The column's name
    Name : string        
    /// The column's position relative to the other columns
    Position : int
    /// The column's data type
    DataType : DataTypeReference    
    /// The column's documentation
    Documentation : string             
    /// Specifies whether the column allows null
    Nullable : bool           
    /// Specifies the means by which the column is automatically populated, if applicable 
    AutoValue : AutoValueKind     
    /// The attached properties
    Properties : DataElementProperty list
}

/// <summary>
/// Describes a primary key constraint
/// </summary>
type PrimaryKeyDescription = {
    /// The name of the primary key's parent, e.g, the table, table type, etc.
    ParentName : DataObjectName
    /// The name of the primary kay
    PrimaryKeyName : DataObjectName
    /// The columns included in the primary key
    Columns : ColumnDescription list
}

type ITabularDescription =
    abstract Name : DataObjectName
    abstract Documentation: string
    abstract Columns : ColumnDescription rolist

/// <summary>
/// Describes a table
/// </summary>
type TableDescription = {
    /// The name of the table
    Name : DataObjectName        
    /// The tabular's documentation
    Documentation : string 
    /// The columns in the table
    Columns : ColumnDescription list
    /// The attached properties
    Properties : DataElementProperty list
}
with
    interface ITabularDescription  with
        member this.Name = this.Name
        member this.Documentation = this.Documentation
        member this.Columns = this.Columns |> ReadOnlyList.Create

    /// <summary>
    /// Renders a textual representation of the object that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() =
      this.Name.ToString()

/// <summary>
/// Describes a table
/// </summary>
type ViewDescription = {
    /// The name of the table
    Name : DataObjectName        
    /// The tabular's documentation
    Documentation : string 
    /// The columns in the table
    Columns : ColumnDescription list
    /// The attached properties
    Properties : DataElementProperty list
}
with
    interface ITabularDescription  with
        member this.Name = this.Name
        member this.Documentation = this.Documentation
        member this.Columns = this.Columns |> ReadOnlyList.Create



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
    Documentation : string 
    /// The column's data type
    DataType : DataTypeReference
    /// The direction of the parameter
    Direction : RoutineParameterDirection
    /// The attached properties
    Properties : DataElementProperty list
}

/// <summary>
/// Describes a stored procedure
/// </summary>
type ProcedureDescription = {
    /// The name of the procedure
    Name : DataObjectName
    /// The parameters
    Parameters : RoutineParameterDescription rolist
    /// The procedures's documentation
    Documentation : string 
    /// The attached properties
    Properties : DataElementProperty list
}
   
/// <summary>
/// Describes a table-valued function
/// </summary>
type TableFunctionDescription = {
    /// The name of the procedure
    Name : DataObjectName    
    /// The parameters
    Parameters : RoutineParameterDescription rolist
    /// The function's documentation
    Documentation : string 
    /// The columns in the result set
    Columns : ColumnDescription rolist
    /// The attached properties
    Properties : DataElementProperty list
}

/// <summary>
/// Describes a sequence 
/// </summary>
type SequenceDescription = {
    /// The name of the sequence
    Name : DataObjectName 
    /// The start value fo the sequence
    StartValue : NumericValue
    /// The distance between each sequence element
    Increment : NumericValue 
    /// The inclusive lower bound of the sequence
    MinimumValue : NumericValue
    /// The inclusive upper bound of the sequence
    MaximumValue : NumericValue 
    DataType : DataTypeReference
    /// The current value of the sequence
    CurrentValue : NumericValue
    /// Idicates whether the sequence is allowed to cycle after reaching its maximum value
    IsCycling : bool
    /// Indicates whether the sequence can yield any more elements
    IsExhaused : bool
    IsCached : bool
    CacheSize : int
    /// The attached properties
    Properties : DataElementProperty list
}


/// <summary>
/// Unifies data object types
/// </summary>
type DataObjectDescription =
| TableFunctionDescription of TableFunctionDescription
| ProcedureDescription of ProcedureDescription
| TableDescription of TableDescription
| ViewDescription of ViewDescription
| SequenceDescription of SequenceDescription
| DataTypeDescription of DataTypeDescription

type SchemaDescription = {
    /// The name of the schema
    Name : string
    /// The objects defined in the schema
    Objects : DataObjectDescription list
    /// The schema's documentattion
    Documentation : string
    /// The attached properties
    Properties : DataElementProperty list
}


type SqlMetadataCatalog = {
    CatalogName : string
    Schemas : SchemaDescription rolist
}

/// <summary>
/// Represents routine parameter value
/// </summary>
type RoutineParameterValue = RoutineParameterValue of  name : string * position : int * value : obj
with
    member this.Position = match this with RoutineParameterValue(position=x) -> x
    member this.Name = match this with RoutineParameterValue(name=x) -> x
    member this.Value = match this with RoutineParameterValue(value=x) -> x


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

/// <summary>
/// Defines contract for discovering relational metadata
/// </summary>
type ISqlMetadataProvider =
    /// <summary>
    /// Describes the identified objects
    /// </summary>
    abstract Describe:SqlMetadataQuery->DataObjectDescription list
    
    /// <summary>
    /// Describes all schemas that are visible to the metadata provider
    /// </summary>
    abstract DescribeSchemas:unit-> SchemaDescription rolist
    
    /// <summary>
    /// Describes all tables that are visible to the metadata provider
    /// </summary>
    abstract DescribeTables:unit-> TableDescription rolist
    
    /// <summary>
    /// Describes an identified table
    /// </summary>
    abstract DescribeTable:DataObjectName ->TableDescription

    /// <summary>
    /// Describes the tables in an identified schema
    /// </summary>
    abstract DescribeTablesInSchema:string->TableDescription rolist

    /// <summary>
    /// Describes all views that are visible to the metadata provider
    /// </summary>
    abstract DescribeViews:unit -> ViewDescription rolist
    
    /// <summary>
    /// Describes the views in an identified schema
    /// </summary>
    abstract DescribeViewsInSchema:string->ViewDescription rolist
    
    /// <summary>
    /// Determines whether an identified object exists 
    /// </summary>
    abstract ObjectExists: DataObjectName -> bool
    abstract RefreshCache:unit->unit

