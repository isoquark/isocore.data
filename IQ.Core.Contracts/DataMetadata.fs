// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Contracts

open System
open System.Diagnostics
open System.Collections.Generic
open System.Runtime.CompilerServices

open IQ.Core.Framework

open IQ.Core.Framework.Contracts
open IQ.Core.Math.Contracts

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

type DataObjectNameAttribute(schemaName, localName) =
    inherit Attribute()
    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="schemaName">The name of the schema in which the object is defined</param>
    new(schemaName) =
        DataObjectNameAttribute(schemaName, String.Empty)

    /// <summary>
    /// Gets the name of the schema in which the object resides, if specified
    /// </summary>
    member this.SchemaName = 
        if schemaName = String.Empty then None else Some(schemaName)

    /// <summary>
    /// Provides a textual representation of the element
    /// </summary>
    member this.Text = 
        sprintf "[%s].[%s]" schemaName localName


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
    | Variant = 110uy //corresponds to sql_variant
                      
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
    | DurationDataType 
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
        | DurationDataType -> sprintf "timespan"
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
/// intended use case is rather clarity of expression and/or type-safety when useful.
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
    /// Unknown if the column has an automatically popuated value
    | Unknown = 0
    /// Column is not automatically populated
    | None = 0
    /// Column is automatically populated with a default value
    | Default = 1
    /// Column is automatically incremented (either via a sequence for legacy identity column)
    | AutoIncrement = 2
    /// Column is automatically populated with a computed value
    | Computed = 3

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
    /// The column's data kind
    DataKind : DataKind
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
    /// If table data type, the columns defined by the type
    Columns : ColumnDescription list
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
/// Describes a data matrix
/// </summary>
type DataMatrixDescription = DataMatrixDescription of Name : DataObjectName * Columns : ColumnDescription list


/// <summary>
/// Defines contract for a tabular data source
/// </summary>
type IDataMatrix =
    /// <summary>
    /// Describes the encapsulated data
    /// </summary>
    abstract Description : DataMatrixDescription
    /// <summary>
    /// The encapsulared data
    /// </summary>
    abstract Rows : IReadOnlyList<obj[]>
    /// <summary>
    /// Gets the value at the intersection fo the specified (0-based) row and column
    /// </summary>
    abstract Item : row : int * col : int -> obj


type DataMatrix =  DataMatrix of Description : DataMatrixDescription * Rows : obj[] IReadOnlyList
with
    interface IDataMatrix with
        member this.Rows = match this with DataMatrix(Rows=x) -> x
        member this.Description = match this with DataMatrix(Description=x) -> x
        member this.Item(row,col) = match this with DataMatrix(Rows=x) -> x.[row].[col]


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
    abstract Columns : ColumnDescription list


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
    /// Specifies whether the table is a file table
    IsFileTable : bool
}
with
    interface ITabularDescription  with
        member this.Name = this.Name.Text
        member this.ElementKind = DataElementKind.Table
        member this.ObjectName = this.Name
        member this.Documentation = this.Documentation
        member this.Columns = this.Columns

    /// <summary>
    /// Renders a textual representation of the object that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = this.Name.ToString()

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
        member this.Columns = this.Columns 

    /// <summary>
    /// Renders a textual representation of the object that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = this.Name.ToString()



/// <summary>
/// Describes a routine of some sort
/// </summary>
type RoutineDescription = {
    /// The name of the procedure
    Name : DataObjectName
    /// The parameters
    Parameters : RoutineParameterDescription list
    /// The procedures's documentation
    Documentation : string 
    /// The columns in the (first) result set if applicable
    Columns : ColumnDescription list
    /// The attached properties
    Properties : DataElementProperty list
    /// The kind of routine being described (stored procedure, TVF, etc)
    RoutineKind : DataElementKind
}
with 
    member this.Item(name) =
        this.Parameters |> List.find(fun x -> x.Name = name)
    
    interface IDataObjectDescription with
        member this.Name = this.Name.Text
        member this.ElementKind = this.RoutineKind
        member this.ObjectName = this.Name
        member this.Documentation = this.Documentation
        
    /// <summary>
    /// Renders a textual representation of the object that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = this.Name.ToString()
   
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
    /// Renders a textual representation of the object that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = this.Name.ToString()

/// <summary>
/// Unifies data object types
/// </summary>
type DataObjectDescription =
| RoutineDescription of RoutineDescription
| TableDescription of TableDescription
| ViewDescription of ViewDescription
| SequenceDescription of SequenceDescription
| DataTypeDescription of DataTypeDescription


with         
    member this.Name = 
        match this with
            | RoutineDescription x -> x.Name.Text
            | TableDescription x -> x.Name.Text
            | ViewDescription x -> x.Name.Text
            | SequenceDescription x -> x.Name.Text
            | DataTypeDescription x -> x.Name.Text
    
    member this.Documentation =     
            match this with
            | RoutineDescription x -> x.Documentation
            | TableDescription x -> x.Documentation
            | ViewDescription x -> x.Documentation
            | SequenceDescription x -> x.Documentation
            | DataTypeDescription x -> x.Documentation
    
    member this.ElementKind = 
        match this with
            | RoutineDescription(_) -> DataElementKind.TableFunction
            | TableDescription(_) -> DataElementKind.Table
            | ViewDescription(_) -> DataElementKind.View
            | SequenceDescription(_) -> DataElementKind.Sequence
            | DataTypeDescription(_) -> DataElementKind.DataType

        member this.ObjectName =
            match this with
                | RoutineDescription x -> x.Name
                | TableDescription x -> x.Name
                | ViewDescription x -> x.Name
                | SequenceDescription x -> x.Name
                | DataTypeDescription x -> x.Name

    interface IDataObjectDescription with
        member this.ObjectName = this.ObjectName        
        member this.ElementKind = this.ElementKind        
        member this.Documentation = this.Documentation                
        member this.Name = this.Name

    /// <summary>
    /// Renders a textual representation of the object that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = this.Name

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
    /// <summary>
    /// Renders a textual representation of the object that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = this.Name


type SqlMetadataCatalog = {
    CatalogName : string
    Schemas : SchemaDescription list
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
    abstract DescribeSchemas:unit-> SchemaDescription list
    
    /// <summary>
    /// Describes all tables that are visible to the metadata provider
    /// </summary>
    abstract DescribeTables:unit-> TableDescription list
    
    /// <summary>
    /// Describes an identified table
    /// </summary>
    abstract DescribeTable:DataObjectName ->TableDescription

    /// <summary>
    /// Describes the tables in an identified schema
    /// </summary>
    abstract DescribeTablesInSchema:string->TableDescription list

    /// <summary>
    /// Describes all views that are visible to the metadata provider
    /// </summary>
    abstract DescribeViews:unit -> ViewDescription list
    
    /// <summary>
    /// Describes an identified view
    /// </summary>
    abstract DescribeView:DataObjectName ->ViewDescription

    /// <summary>
    /// Describes the views in an identified schema
    /// </summary>
    abstract DescribeViewsInSchema:string->ViewDescription list
    
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

    /// <summary>
    /// Describes a tabular object
    /// </summary>
    abstract DescribeDataMatrix:DataObjectName -> DataMatrixDescription

/// <summary>
/// Defines attributes that are intended to be applied to proxy elements to specify 
/// data source characteristics that cannot otherwise be inferred
/// </summary>
[<AutoOpen>]
module private DataAttributeConstants =    
    [<Literal>]
    let UnspecifiedName = ""
    [<Literal>]
    let UnspecifiedPrecision = 201uy
    [<Literal>]
    let UnspecifiedScale = 201uy
    [<Literal>]
    let UnspecifiedLength = -1
    [<Literal>]
    let UnspecifiedStorage = DataKind.Unspecified
    let UnspecifiedType = Unchecked.defaultof<Type>
    [<Literal>]
    let UnspecifiedPosition = -1
    [<Literal>]
    let UnspecifiedAutoValue = AutoValueKind.None
    
        
/// <summary>
/// Base type for attributes that participate in the data attribution system
/// </summary>
[<AbstractClass>]
type DataAttribute() =
    inherit Attribute()

/// <summary>
/// Base type for attributes that identify data elements
/// </summary>
[<AbstractClass>]
type DataElementAttribute(name) =
    inherit DataAttribute()

    new () =
        DataElementAttribute(String.Empty)
        
    /// <summary>
    /// Gets the local name of the element, if specified
    /// </summary>
    member this.Name = 
        if name = String.Empty then None else Some(name)
    
/// <summary>
/// Base type for attributes that identify data objects
/// </summary>
[<AbstractClass>]
type DataObjectAttribute(schemaName, localName) =
    inherit DataElementAttribute(localName)

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="schemaName">The name of the schema in which the object is defined</param>
    new(schemaName) =
        DataObjectAttribute(schemaName, String.Empty)

    /// <summary>
    /// Gets the name of the schema in which the object resides, if specified
    /// </summary>
    member this.SchemaName = 
        if schemaName = String.Empty then None else Some(schemaName)

/// <summary>
/// Identifies a schema
/// </summary>
type SchemaAttribute(schemaName) =
    inherit DataElementAttribute(schemaName)        

    
/// <summary>
/// Identifies a table when applied
/// </summary>
type TableAttribute(schemaName, localName) =
    inherit DataObjectAttribute(schemaName,localName)

    new (schemaName) =
        TableAttribute(schemaName, UnspecifiedName)

/// <summary>
/// Identifies a sequence when applied
/// </summary>
type SequenceAttribute(schemaName, localName) =
    inherit DataObjectAttribute(schemaName,localName)

    new (schemaName) =
        SequenceAttribute(schemaName, UnspecifiedName)

/// <summary>
/// Identifies a view
/// </summary>
type ViewAttribute(schemaName, localName) =
    inherit DataObjectAttribute(schemaName,localName)

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="schemaName">The name of the schema in which the view is defined</param>
    new (schemaName) =
        ViewAttribute(schemaName, UnspecifiedName)                      
    
           
/// <summary>
/// Identifies a column and specifies selected storage characteristics
/// </summary>
type ColumnAttribute(name, position, autoValueKind) =
    inherit DataElementAttribute(name)

    new(name) =
        ColumnAttribute(name, UnspecifiedPosition, UnspecifiedAutoValue)        
    new() =
        ColumnAttribute(UnspecifiedName, UnspecifiedPosition, UnspecifiedAutoValue)
    new(autoValueKind) =
        ColumnAttribute(UnspecifiedName, UnspecifiedPosition, autoValueKind)

    /// Indicates the name of the represented column if specified
    member this.Name = if String.IsNullOrWhiteSpace(name) then None else Some(name)
        
    /// Indicates the position of the represented column if specified
    member this.Position = if position = UnspecifiedPosition then None else Some(position)

    /// Indicates the means by which the column is automatically populated if specified
    member this.AutoValue = if autoValueKind = UnspecifiedAutoValue then None else Some(autoValueKind)
               
    

/// <summary>
/// Identifies a stored procedure
/// </summary>
type ProcedureAttribute(schemaName, localName, providesDataSet) =
    inherit DataObjectAttribute(schemaName, localName)

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="schemaName">The name of the schema in which the procedure is defined</param>
    new(schemaName) =
        ProcedureAttribute(schemaName, UnspecifiedName, false)

    new() =
        ProcedureAttribute(UnspecifiedName, UnspecifiedName, false)

    new(providesDataSet) =
        ProcedureAttribute(UnspecifiedName, UnspecifiedName, providesDataSet)

    member this.ProvidesDataSet = providesDataSet

/// <summary>
/// Identifies a user-defined table type 
/// </summary>
type TableTypeAttribute(schemaName, localName) =
    inherit DataObjectAttribute(schemaName, localName)

    new(schemaName) =
        TableTypeAttribute(schemaName, UnspecifiedName)

    new() =
        TableTypeAttribute(UnspecifiedName, UnspecifiedName)

/// <summary>
/// Identifies a table-valued function
/// </summary>
type TableFunctionAttribute(schemaName, localName) = 
    inherit DataObjectAttribute(schemaName, localName)

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="schemaName">The name of the schema in which the procedure is defined</param>
    new(schemaName) =
        TableFunctionAttribute(schemaName, UnspecifiedName)

    new() =
        TableFunctionAttribute(UnspecifiedName, UnspecifiedName)


/// <summary>
/// Identifies a routine parameter
/// </summary>
type RoutineParameterAttribute(name, direction, position) =
    inherit DataElementAttribute(name)

    new (name) =
        RoutineParameterAttribute(name, RoutineParameterDirection.Input, UnspecifiedPosition)

    new (name, position) =
        RoutineParameterAttribute(name, RoutineParameterDirection.Input, position)

    new (name, direction) =
        RoutineParameterAttribute(name, direction, UnspecifiedPosition)

    /// <summary>
    /// Gets the direction of the parameter
    /// </summary>
    member this.Direction = direction

    /// <summary>
    /// Gets the parameter's ordinal position
    /// </summary>
    member this.Position = if position = UnspecifiedPosition then None else position |> Some
    
/// <summary>
/// Identifies a sequence that will yield a value for the element to which
/// the attribute is attached
/// </summary>
type SourceSequenceAttribute(schemaName, localName) =
    inherit DataAttribute()

    /// <summary>
    /// Gets the name of the schema in which the sequence resides
    /// </summary>
    member this.SchemaName = 
        if schemaName = UnspecifiedName then None else Some(schemaName)

    /// <summary>
    /// Gets the local name of the element
    /// </summary>
    member this.Name = 
        if localName = UnspecifiedName then None else Some(localName)
            
[<AbstractClass>]
type ElementFacetAttribute<'T>(value : 'T) =
    inherit DataElementAttribute()

    /// <summary>
    /// Specifies the facet's value
    /// </summary>    
    member this.Value = value
            
/// <summary>
/// Specifies the nullability of the element to which it applies 
/// </summary>
type NullableAttribute(isNullable) =
    inherit ElementFacetAttribute<bool>(isNullable)

    new() =
        NullableAttribute(true)
    
/// <summary>
/// Specifies the relative position of the element to which it is applied
/// </summary>
type PositionAttribute(position) =
    inherit ElementFacetAttribute<int>(position)        

/// <summary>
/// Specifies the (intrinsic) kind of data that is pxoxied by the element to which it is applied
/// </summary>
type DataKindAttribute(value) =
    inherit ElementFacetAttribute<DataKind>(value)            

/// <summary>
/// Specifies the (custom) kind of data that is pxoxied by the element to which it is applied
/// </summary>
type CustomDataKindAttribute(kind, schemaName, typeName) =
    inherit DataKindAttribute(kind)

    member this.ObjectName = DataObjectName(schemaName, typeName) 
    
/// <summary>
/// Specifies the absolute length of the element to which it is applied
/// </summary>
type FixedLengthAttribute(len) =
    inherit ElementFacetAttribute<int>(len)
        
/// <summary>
/// Specifies the maximum length of the element to which it is applied
/// </summary>
type MaxLengthAttribute(len) =
    inherit ElementFacetAttribute<int>(len)
        
/// <summary>
/// Specifies the minimum length of the element to which it is applied
/// </summary>
type MinLengthAttribute(len) =
    inherit ElementFacetAttribute<int>(len)
    
/// <summary>
/// Specifies the inclusive lower and upper bounds of the length of the element to which it applies
/// </summary>
/// <remarks>
/// Logically equivalent to application of both <see cref="MinLengthAttribute"/> and <see cref="MaxLengthAttribute"/>
/// </remarks>
type LengthRangeAttribute(minLength, maxLength) =
    inherit ElementFacetAttribute<Range<int>>(Range(minLength,maxLength))
    
/// <summary>
/// Specifies the numeric precision of the element to which it is applied
/// </summary>
type PrecisionAttribute(value) =
    inherit ElementFacetAttribute<uint8>(value)
        
/// <summary>
/// Specifies the numeric scale of the element to which it is applied
/// </summary>
type ScaleAttribute(value) =
    inherit ElementFacetAttribute<uint8>(value)
            
/// <summary>
/// Specifies the minimum value of the element to which it is applied
/// </summary>
type MinScalarAttribute private (value) =
    inherit ElementFacetAttribute<NumericValue>(value)
        
    new (value : uint8) = MinScalarAttribute(UInt8Value(value))
    new (value : int8) = MinScalarAttribute(Int8Value(value))
    new (value : uint16) = MinScalarAttribute(UInt16Value(value))
    new (value : int16) = MinScalarAttribute(Int16Value(value))
    new (value : uint32) = MinScalarAttribute(UInt32Value(value))
    new (value : int32) = MinScalarAttribute(Int32Value(value))
    new (value : uint64) = MinScalarAttribute(UInt64Value(value))
    new (value : int64) = MinScalarAttribute(Int64Value(value))
    new (value : float32) = MinScalarAttribute(Float32Value(value))
    new (value : float) = MinScalarAttribute(Float64Value(value))        
    new (value : decimal) = MinScalarAttribute(DecimalValue(value))
        


/// <summary>
/// Specifies the minimum value of the element to which it is applied
/// </summary>
type MaxScalarAttribute private (value) =
    inherit ElementFacetAttribute<NumericValue>(value)
        
    new (value : uint8) = MaxScalarAttribute(UInt8Value(value))
    new (value : int8) = MaxScalarAttribute(Int8Value(value))
    new (value : uint16) = MaxScalarAttribute(UInt16Value(value))
    new (value : int16) = MaxScalarAttribute(Int16Value(value))
    new (value : uint32) = MaxScalarAttribute(UInt32Value(value))
    new (value : int32) = MaxScalarAttribute(Int32Value(value))
    new (value : uint64) = MaxScalarAttribute(UInt64Value(value))
    new (value : int64) = MaxScalarAttribute(Int64Value(value))
    new (value : float32) = MaxScalarAttribute(Float32Value(value))
    new (value : float) = MaxScalarAttribute(Float64Value(value))
    new (value : decimal) = MaxScalarAttribute(DecimalValue(value))


/// <summary>
/// Specifies the inclusive lower and upper bounds of the scalar value of the element to which it applies
/// </summary>
/// <remarks>
/// Logically equivalent to application of both <see cref="MinScalarAttribute"/> and <see cref="MaxScalarAttribute"/>
/// </remarks>
type ScalarRangeAttribute(minValue, maxValue) =
    inherit ElementFacetAttribute<Range<NumericValue>>(Range(minValue,maxValue))
        
    new (minValue : uint8, maxValue : uint8) = ScalarRangeAttribute(UInt8Value(minValue), UInt8Value(maxValue))
    new (minValue : int8, maxValue : int8) = ScalarRangeAttribute(Int8Value(minValue), Int8Value(maxValue))
    new (minValue : uint16, maxValue : uint16) = ScalarRangeAttribute(UInt16Value(minValue), UInt16Value(maxValue))
    new (minValue : int16, maxValue : int16) = ScalarRangeAttribute(Int16Value(minValue), Int16Value(maxValue))
    new (minValue : uint32, maxValue : uint32) = ScalarRangeAttribute(UInt32Value(minValue), UInt32Value(maxValue))
    new (minValue : int32, maxValue : int32) = ScalarRangeAttribute(Int32Value(minValue), Int32Value(maxValue))
    new (minValue : uint64, maxValue : uint64) = ScalarRangeAttribute(UInt64Value(minValue), UInt64Value(maxValue))
    new (minValue : int64, maxValue : int64) = ScalarRangeAttribute(Int64Value(minValue), Int64Value(maxValue))
    new (minValue : float32, maxValue : float32) = ScalarRangeAttribute(Float32Value(minValue), Float32Value(maxValue))
    new (minValue : float, maxValue : float) = ScalarRangeAttribute(Float64Value(minValue), Float64Value(maxValue))
    new (minValue : decimal,maxValue : decimal) = ScalarRangeAttribute(DecimalValue(minValue), DecimalValue(maxValue))

/// <summary>
/// Specifies the minimum date value of the element to which it is applied
/// </summary>
type MinDateAttribute(value : string) =
    inherit ElementFacetAttribute<DateTime>(DateTime.Parse(value))

            
/// <summary>
/// Specifies the maximum date value of the element to which it is applied
/// </summary>
type MaxDateAttribute(value : string) =
    inherit ElementFacetAttribute<DateTime>(DateTime.Parse(value))


/// <summary>
/// Specifies the inclusive lower and upper bounds of the date value of the element to which it applies
/// </summary>
type DateRangeAttribute(minValue : string, maxValue : string) =
    inherit ElementFacetAttribute<Range<DateTime>>(Range(DateTime.Parse(minValue), DateTime.Parse(maxValue)))

type XmlSchemaAttribute(value : string) =
    inherit ElementFacetAttribute<string>(value)


type RepresentationTypeAttribute(t : Type) =
    inherit ElementFacetAttribute<Type>(t)

type DataObjectNameFacetAttribute private (name : DataObjectName) =
    inherit ElementFacetAttribute<DataObjectName>(name)

    new(schemaName, localName) =
        DataObjectNameFacetAttribute(DataObjectName(schemaName, localName))
                       
/// <summary>
/// Defines the supported data facet names
/// </summary>
module DataFacetNames =
    [<Literal>]
    let Nullable = "Nullable"
    [<Literal>]
    let Position = "Position"
    [<Literal>]
    let DataKind = "DataKind"
    [<Literal>]
    let CustomObjectName = "CustomDataKind"
    [<Literal>]
    let FixedLength = "FixedLength"
    [<Literal>]
    let MinLength = "MinLength"
    [<Literal>]
    let MaxLength = "MaxLength"
    [<Literal>]
    let Precision = "Precision"
    [<Literal>]
    let Scale = "Scale"
    [<Literal>]
    let MinScalar = "MinScalar"
    [<Literal>]
    let MaxScalar = "MaxScalar"
    [<Literal>]
    let MinDate = "MinDate"
    [<Literal>]
    let MaxDate = "MaxDate"
    [<Literal>]
    let XmlSchema = "XmlSchema"
    [<Literal>]
    let RepresentationType = "RepresentationType"
    [<Literal>]
    let DataObjectName = "DataObjectName"

