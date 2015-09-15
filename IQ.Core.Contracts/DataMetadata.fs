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
    /// Provides a textual representation of the element
    /// </summary>
    member this.Text = 
        match this with DataObjectName(s,l) -> sprintf "[%s].[%s]" s l
    
    /// <summary>
    /// Provides a textual representation of the element suitable for diagnostic purposes
    /// </summary>
    override  this.ToString() = this.Text


/// <summary>
/// Specifies the available DataType classifications
/// </summary>
/// <remarks>
/// Note that the DataType class is not sufficient to characterize the DataType type and
/// additional information, such as length or data object name is needed to store/instantiate
/// a corresponding value
/// </remarks>
type DataKind =
    /// Data type is unknown
    | Unspecified = 0uy        
    ///Identifies a bit type (map to bit in SQL Server)
    | Bit = 10uy     
    ///Identifies an unsigned 8-bit integer type (map to tinyint in SQL Server)
    | UInt8 = 20uy     
    ///Identifies an unsigned 16-bit integer type (map to int in SQL Server)
    | UInt16 = 21uy     
    ///Identifies an unsigned 32-bit integer type (map to bigint in SQL Server)
    | UInt32 = 22uy     
    ///Identifies an unsigned 64-bit integer type (map to varbinary(8) in SQL Server)
    | UInt64 = 23uy     
    ///Identifies a signed 8-bit integer type (map to smallint in SQL Server)
    | Int8 = 30uy     
    ///Identifies a signed 16-bit integer type (map to smallint in SQL Server)
    | Int16 = 31uy     
    ///Identifies a signed 32-bit integer type (map to int in SQL Server)
    | Int32 = 32uy //int    
    ///Identifies a signed 64-bit integer type (map to bigint in SQL Server)
    | Int64 = 33uy         
    
    ///Identifies a fixed-length array of bytes (map to binary(n) in SQL Server)
    | BinaryFixed = 40uy 
    /// Identifies a variable-length array of bytes with an explicit value given for 
    /// the upper bound of the array's length (map to varbinary(n) in SQL Server)
    | BinaryVariable = 41uy 
    /// Identifies a variable-length array of bytes with no explicit value given for 
    /// the upper bound of the array's length (map to varbinary(MAX) in SQL Server)
    | BinaryMax = 42uy        
   
    ///Identifies a fixed-length array of ansi characters (map to char(n) in SQL Server)
    | AnsiTextFixed = 50uy
    /// Identifies a variable-length array of ansi characters with an explicit value give for
    /// the upper bound of the array's length (map to varchar(n) in SQL Server)
    | AnsiTextVariable = 51uy 
    /// Identifies a variable-length array of ansi characters with no explicit value given for 
    /// the upper bound of the array's length (map to varchar(MAX) in SQL Server)
    | AnsiTextMax = 52uy        
    ///Identifies a fixed-length array of unicode characters (map to nchar(n) in SQL Server)
    
    | UnicodeTextFixed = 53uy 
    /// Identifies a variable-length array of unicode characters with an explicit value give for
    /// the upper bound of the array's length (map to nvarchar(n) in SQL Server)
    | UnicodeTextVariable = 54uy 
    /// Identifies a variable-length array of unicode characters with no explicit value given for 
    /// the upper bound of the array's length (map to nvarchar(MAX) in SQL Server)
    | UnicodeTextMax = 55uy
    
    /// Identifies a type representing a date and time (map to datetime2(7) by default in sql server)
    | DateTime = 62uy 
            
    | DateTimeOffset = 63uy
    | TimeOfDay = 64uy //corresponds to time
    | Date = 65uy //corresponds to date        
    | Duration = 66uy //no direct map, use bigint to store number of ticks
    
    /// Identifies a type representing a date and time (map to datetime in sql server)
    /// DataTypeReference DateTime(23,3)
    | LegacyDateTime = 67uy
    /// This should never be used in new tables, its just here for the sake of compatibilty
    /// DataTypeReference DateTime(16,0)
    | LegacySmallDateTime = 68uy

        

    | Float32 = 70uy //corresponds to real
    | Float64 = 71uy //corresponds to float
        
    | Decimal = 80uy
    /// DataTypeReference Money(10,4)
    | Money = 81uy
    /// DataTypeReference Money(19,4)
    | SmallMoney = 82uy
        
    | Guid = 90uy //corresponds to uniqueidentifier
    | Xml = 100uy
    | Json = 101uy
    | Flexible = 110uy //corresponds to sql_variant
                      
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
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() =
        match this with
        | BitDataType -> "bit"
        | UInt8DataType -> "uint8"
        | UInt16DataType -> "uint16"
        | UInt32DataType -> "uint32"
        | UInt64DataType -> "uint64"
        | Int8DataType -> "int8"
        | Int16DataType -> "int16"
        | Int32DataType -> "int32"
        | Int64DataType -> "int64"
        | BinaryFixedDataType(len) -> sprintf "byte[%i]" len
        | BinaryVariableDataType(len) -> sprintf "byte[1..%i]" len
        | BinaryMaxDataType -> "byte[*]" 
        | AnsiTextFixedDataType(len) -> sprintf "text[%i]" len
        | AnsiTextVariableDataType(len) -> sprintf "text[1..%i]" len
        | AnsiTextMaxDataType -> "text[*]"
        | UnicodeTextFixedDataType(len) -> sprintf "ntext[%i]" len
        | UnicodeTextVariableDataType(len) -> sprintf "ntext[1..%i]" len
        | UnicodeTextMaxDataType -> "ntext[*]"
        | DateTimeDataType(p,s) -> sprintf "datetime(%i,%i)" p s
        | DateTimeOffsetDataType -> sprintf "datetimeoffset"
        | TimeOfDayDataType(p,s) -> sprintf "timeofday(%i,%i)" p s
        | TimespanDataType -> sprintf "timespan"
        | RowversionDataType -> sprintf "rowversion"
        | DateDataType -> "date"
        | Float32DataType -> "float32"
        | Float64DataType -> "float64"
        | DecimalDataType(p,s) -> sprintf "decimal(%i,%i)" p s
        | MoneyDataType(p,s) -> sprintf "money(%i,%i)" p s
        | GuidDataType -> "guid"
        | XmlDataType(schema) -> sprintf "xml(%s)" schema
        | JsonDataType -> "json"
        | VariantDataType -> "variant"
        | TableDataType(name) -> sprintf "table : %O" name
        | ObjectDataType(name,clrTypeName) -> sprintf "%O : %s" name clrTypeName
        | CustomPrimitiveDataType(name, baseType) -> sprintf "%O : %O" name baseType
        | TypedDocumentDataType(doctype) -> sprintf "document : %s" doctype.Name



/// <summary>
/// Represents a numeric value, duh.
/// </summary>
/// <remarks>
/// The intent is not to use this for calculations and such as it would be very slow; its
/// intended use case is rather clarity of expression and/or type-safety when.
/// </remarks>
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
/// Enumerates recognized data element classifications
/// </summary>
type DataElementKind =
    /// Classifies an element as a table
    | Table = 1uy    
    /// Classifies an element as a view
    | View = 2uy    
    /// Classifies an element as a column
    | Column = 3uy    
    /// Classifies an element as a schema
    | Schema = 4uy    
    /// Classifies an element as a sequence
    | Sequence = 5uy    
    /// Classifies an element as a table-valued function
    | TableFunction = 6uy    
    /// Classifies an element as a stored procedure
    | Procedure = 7uy    
    /// Classifies an element as a data type (definition)
    | DataType = 8uy
    /// Classifies an element as primary key
    | PrimaryKey = 9uy
    /// Classifies an element as a routine parameter
    | Parameter = 10uy

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
/// Defines common contract that all description elements are required to support
/// </summary>
/// <remarks>
/// The use of the word "Element" here is deliberate to explicitly distinguish a database
/// element from a database object. All database objects are considered elements but
/// not all elements, e.g., columns, are considered objects. This is motivated, more-or-less,
/// by the distinction made in SQL Sever between database objects (which appear in the
/// sys.objects table) and those elements that do not (such as columns)
/// </remarks>
type IDataElementDescription =
    /// <summary>
    /// Gets the kind of element being described
    /// </summary>
    abstract ElementKind : DataElementKind
    /// <summary>
    /// Gets the textual representation of an element's name
    /// </summary>
    abstract Name : string
    /// <summary>
    /// Gets the element's documentation
    /// </summary>
    abstract Documentation : string

/// <summary>
/// Defines common contract for data object descriptions
/// </summary>
type IDataObjectDescription =
    inherit IDataElementDescription
    abstract ObjectName : DataObjectName


/// <summary>
/// Describes a data type definition
/// </summary>
type DataTypeDescription = {
    /// The name of the data type
    Name : DataObjectName
    /// The data type's documentation
    Documentation : string
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
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = this.Name.Text

    interface IDataObjectDescription with
        member this.ElementKind = DataElementKind.DataType
        member this.ObjectName = this.Name
        member this.Name = this.Name.Text
        member this.Documentation = this.Documentation

/// <summary>
/// Describes a column in a table or view
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
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
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = sprintf "%s : %O"  this.Name this.DataType

    interface IDataElementDescription with
        member this.ElementKind = DataElementKind.Column
        member this.Name = this.Name
        member this.Documentation = this.Documentation



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
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        sprintf "%O : %O" this.Name this.DataType

    interface IDataElementDescription with
        member this.ElementKind = DataElementKind.Parameter
        member this.Name = this.Name
        member this.Documentation = this.Documentation

/// <summary>
/// Defines common contract for data elements that own/parent a collection of columns
/// </summary>
type ITabularDescription =
    inherit IDataObjectDescription
    abstract Columns : ColumnDescription rolist


/// <summary>
/// Describes a primary key constraint
/// </summary>
type PrimaryKeyDescription = {
    /// The name of the primary key's parent, e.g, the table, table type, etc.
    ParentName : DataObjectName
    /// The name of the primary kay
    PrimaryKeyName : DataObjectName
    /// The primary key's documentation
    Documentation : string
    /// The columns included in the primary key
    Columns : ColumnDescription list
}
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = this.PrimaryKeyName.Text

    interface IDataObjectDescription with
        member this.ElementKind = DataElementKind.PrimaryKey
        member this.Name = this.PrimaryKeyName.Text
        member this.ObjectName = this.PrimaryKeyName
        member this.Documentation = this.Documentation



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
        member this.Name = this.Name.Text
        member this.ElementKind = DataElementKind.Table
        member this.ObjectName = this.Name
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
        member this.Name = this.Name.Text
        member this.ElementKind = DataElementKind.View
        member this.ObjectName = this.Name
        member this.Documentation = this.Documentation
        member this.Columns = this.Columns |> ReadOnlyList.Create





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
with 
    interface IDataObjectDescription with
        member this.Name = this.Name.Text
        member this.ElementKind = DataElementKind.Procedure
        member this.ObjectName = this.Name
        member this.Documentation = this.Documentation
        
   
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
with 
    interface IDataObjectDescription with
        member this.Name = this.Name.Text
        member this.ElementKind = DataElementKind.TableFunction
        member this.ObjectName = this.Name
        member this.Documentation = this.Documentation

/// <summary>
/// Describes a sequence 
/// </summary>
type SequenceDescription = {
    /// The name of the sequence
    Name : DataObjectName 
    /// The documentation for the sequence
    Documentation : string 
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
    /// Indicates whether caching is enabled for the sequence
    IsCached : bool
    /// If caching is enabled for the sequence, indicates the size of the cache
    CacheSize : int
    /// The attached properties
    Properties : DataElementProperty list
}
with 
    interface IDataObjectDescription with
        member this.Name = this.Name.Text
        member this.ElementKind = DataElementKind.Sequence
        member this.ObjectName = this.Name
        member this.Documentation = this.Documentation

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
with         
    interface IDataObjectDescription with
        member this.ObjectName =
            match this with
                | TableFunctionDescription x -> x.Name
                | ProcedureDescription x -> x.Name
                | TableDescription x -> x.Name
                | ViewDescription x -> x.Name
                | SequenceDescription x -> x.Name
                | DataTypeDescription x -> x.Name
        
        member this.ElementKind =
            match this with
                | TableFunctionDescription(_) -> DataElementKind.TableFunction
                | ProcedureDescription(_) -> DataElementKind.Procedure
                | TableDescription(_) -> DataElementKind.Table
                | ViewDescription(_) -> DataElementKind.View
                | SequenceDescription(_) -> DataElementKind.Sequence
                | DataTypeDescription(_) -> DataElementKind.DataType
        
        member this.Documentation =     
                match this with
                | TableFunctionDescription x -> x.Documentation
                | ProcedureDescription x -> x.Documentation
                | TableDescription x -> x.Documentation
                | ViewDescription x -> x.Documentation
                | SequenceDescription x -> x.Documentation
                | DataTypeDescription x -> x.Documentation

        member this.Name =
            match this with
                | TableFunctionDescription x -> x.Name.Text
                | ProcedureDescription x -> x.Name.Text
                | TableDescription x -> x.Name.Text
                | ViewDescription x -> x.Name.Text
                | SequenceDescription x -> x.Name.Text
                | DataTypeDescription x -> x.Name.Text

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
with
    interface IDataElementDescription with
        member this.ElementKind = DataElementKind.Schema
        member this.Name = this.Name
        member this.Documentation = this.Documentation


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
    /// Describes an identified view
    /// </summary>
    abstract DescribeView:DataObjectName ->ViewDescription

    /// <summary>
    /// Describes the views in an identified schema
    /// </summary>
    abstract DescribeViewsInSchema:string->ViewDescription rolist
    
    /// <summary>
    /// Determines whether an identified object exists 
    /// </summary>
    abstract ObjectExists: DataObjectName -> bool
    
    /// <summary>
    /// Determines the kind of identified data object if one can be found
    /// </summary>
    abstract GetObjectKind: DataObjectName -> Nullable<DataElementKind>
    
    /// <summary>
    /// Forces the metadata provider to refetch metadta from the server
    /// </summary>
    abstract RefreshCache:unit->unit

