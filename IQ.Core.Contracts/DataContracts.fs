// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Diagnostics
open System.Collections.Generic
open System.Runtime.CompilerServices

open IQ.Core.Framework

[<AutoOpen>]
module Contracts = 
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
    /// <remarks>
    /// This is really a DataTypeReference or "instantiation"
    /// </remarks>
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
    /// Describes a data type definition
    /// </summary>
    type DataTypeDescription = {
        /// The data type's name
        Name : DataObjectName
        MaxLength : int16
        Precision : uint8
        Scale : uint8
        IsNullable : bool
        IsTableType : bool
        IsCustomObject : bool
        IsUserDefined : bool
        BaseTypeName : DataObjectName option
    }

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
        Documentation : string             
        /// Specifies whether the column allows null
        Nullable : bool           
        /// Specifies the means by which the column is automatically populated, if applicable 
        AutoValue : AutoValueKind     
    }

    /// <summary>
    /// Describes a table or view
    /// </summary>
    type TabularDescription = {
        /// The name of the table
        Name : DataObjectName        
        /// The tabular's documentation
        Documentation : string 
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
        Documentation : string 
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
        Documentation : string 
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
        Documentation : string 
        /// The columns in the result set
        Columns : ColumnDescription list
    }

    /// <summary>
    /// Describes a sequence 
    /// </summary>
    type SequenceDescription = {
        Name : DataObjectName 
        StartValue : NumericValue
        Increment : NumericValue 
        MinimumValue : NumericValue
        MaximumValue : NumericValue 
        DataType : DataType
        CurrentValue : NumericValue
        IsCycling : bool
        IsExhaused : bool
        IsCached : bool
        CacheSize : int
    }

    /// <summary>
    /// Unifies data object types
    /// </summary>
    type DataObjectDescription =
    | TableFunctionDescription of TableFunctionDescription
    | ProcedureDescription of ProcedureDescription
    | TableDescription of TabularDescription
    | ViewDescription of TabularDescription
    | SequenceDescription of SequenceDescription
    | DataTypeDescription of DataTypeDescription


    type SchemaDescription = {
        Name : string
        Objects : DataObjectDescription list
    }

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

    type ISqlMetadataProvider =
        abstract Describe:SqlMetadataQuery->DataObjectDescription list


    type IQueryableObjectStore =
        abstract Select:obj->obj IReadOnlyList

        /// <summary>
        /// Gets the connection string used to connect to the store
        /// </summary>
        abstract ConnectionString : string

    /// <summary>
    /// Defines contract for unparameterized queryable data store
    /// </summary>
    type IQueryableDataStore =
        /// <summary>
        /// Retrieves a query-identified collection of data entities from the store
        /// </summary>
        abstract Select:'Q->'T IReadOnlyList
    
        /// <summary>
        /// Gets the connection string used to connect to the store
        /// </summary>
        abstract ConnectionString : string
    

    /// <summary>
    /// Defines contract for queryable data store parameterized by query type
    /// </summary>
    type IQueryableDataStore<'Q> =
        /// <summary>
        /// Retrieves a query-identified collection of data entities from the store
        /// </summary>
        abstract Select:'Q->'T IReadOnlyList
    
        /// <summary>
        /// Gets the connection string used to connect to the store
        /// </summary>
        abstract ConnectionString : string


    /// <summary>
    /// Defines contract for queryable data store parameterized by query and element type
    /// </summary>
    type IQueryableDataStore<'T,'Q> =
        /// <summary>
        /// Retrieves a query-identified collection of data entities from the store
        /// </summary>
        abstract Select:'Q->'T IReadOnlyList
    
        /// <summary>
        /// Gets the connection string used to connect to the store
        /// </summary>
        abstract ConnectionString : string

    /// <summary>
    /// Defines contract for weakly-typed data stores
    /// </summary>
    type IObjectStore =
        inherit IQueryableObjectStore
        /// <summary>
        /// Deletes a query-identified collection of data entities from the store
        /// </summary>
        abstract Delete:obj->unit

        /// <summary>
        /// Persists a collection of data entities to the store, inserting or updating as appropriate
        /// </summary>
        abstract Merge:obj seq->unit
    
        /// <summary>
        /// Inserts an arbitrary number of entities into the store, eliding existence checks
        /// </summary>
        abstract Insert:obj seq ->unit

    /// <summary>
    /// Defines contract for unparameterized mutable data store
    /// </summary>
    type IDataStore =
        inherit IQueryableDataStore
        /// <summary>
        /// Deletes a query-identified collection of data entities from the store
        /// </summary>
        abstract Delete:'Q->unit

        /// <summary>
        /// Persists a collection of data entities to the store, inserting or updating as appropriate
        /// </summary>
        abstract Merge:'T seq->unit
    
        /// <summary>
        /// Inserts an arbitrary number of entities into the store, eliding existence checks
        /// </summary>
        abstract Insert:'T seq ->unit

    /// <summary>
    /// Defines contract for mutable data store parametrized by query type
    /// </summary>
    type IDataStore<'Q> =
        inherit IQueryableDataStore<'Q>

        /// <summary>
        /// Deletes a query-identified collection of data entities from the store
        /// </summary>
        abstract Delete:'Q->unit

        /// <summary>
        /// Persists a collection of data entities to the store, inserting or updating as appropriate
        /// </summary>
        abstract Merge:'T seq->unit
    
        /// <summary>
        /// Inserts an arbitrary number of entities into the store, eliding existence checks
        /// </summary>
        abstract Insert:'T seq ->unit
    

    /// <summary>
    /// Defines contract for mutable data store parametrized by query and element type
    /// </summary>
    type IDataStore<'T,'Q> =
        inherit IQueryableDataStore<'T,'Q>
    
        /// <summary>
        /// Deletes a query-identified collection of data entities from the store
        /// </summary>
        abstract Delete:'Q->unit

        /// <summary>
        /// Persists a collection of data entities to the store, inserting or updating as appropriate
        /// </summary>
        abstract Merge:'T seq->unit
    
        /// <summary>
        /// Inserts an arbitrary number of entities into the store, eliding existence checks
        /// </summary>
        abstract Insert:'T seq ->unit


    /// <summary>
    /// Defines contract for a tabular data source
    /// </summary>
    type ITabularData =
        /// <summary>
        /// Describes the encapsulated data
        /// </summary>
        abstract Description : TabularDescription
        /// <summary>
        /// The encapsulared data
        /// </summary>
        abstract RowValues : IReadOnlyList<obj[]>

    type TabularData = TabularData of description : TabularDescription * rowValues : rolist<obj[]>
    with
        member this.RowValues = match this with TabularData(rowValues=x) -> x
        member this.Description = match this with TabularData(description=x) -> x
        interface ITabularData with
            member this.RowValues = this.RowValues
            member this.Description = this.Description
    
    type ITabularStore =
        abstract Merge:ITabularData->unit
        abstract Delete:'Q->unit
        abstract Select: 'Q->ITabularData
        abstract Insert:ITabularData->unit


    type ColumnFilter = 
        | Equal of columnName : string * value : obj
        | NotEqual of columnName : string * value : obj
        | GreaterThan of columnName : string * value : obj
        | NotGreaterThan of columnName : string * value : obj
        | LessThan of columnName : string * value : obj
        | NotLessThan of columnName : string * value : obj
        | GreaterThanOrEqual of columnName : string * value : obj
        | LessThanOrEqual of columnName : string * value : obj
        | StartsWith of columnName : string * value : obj
        | EndsWith of columnName : string * value : obj
        | Contains of columnName : string * value : obj

    type ColumnSortCriterion =
        | AscendingSort of columnName : string
        | DescendingSort of columnName : string

    type TabularPageInfo = TabularPageInfo of pageNumber : int option * pageSize : int option

    type ColumnFilterJoin =
        | AndFilter of ColumnFilter
        | OrFilter of ColumnFilter
    with
        member this.Filter = match this with |AndFilter(x)|OrFilter(x) -> x

    type TabularDataQuery = 
         TabularDataQuery of 
            tabularName : DataObjectName * 
            columnNames : string rolist *
            filters : ColumnFilterJoin rolist*
            sortCriteria : ColumnSortCriterion rolist*
            pageInfo : TabularPageInfo option
    with
        member this.ColumnNames = match this with TabularDataQuery(columnNames=x) -> x
        member this.TabularName = match this with TabularDataQuery(tabularName=x) -> x
       
    [<AutoOpen; Extension>]
    module DataStoreExtensions =
        [<Extension>]
        type IQueryableDataStore<'T,'Q> 
        with
            member this.SelectOne(q) = q |> this.Select |> fun x -> x.[0]
 
    /// <summary>
    /// Represents a query pameter
    /// </summary>
    type SqlQueryParameter = SqlQueryParameter of name : string * value : obj    


    /// <summary>
    /// Represents the intent to retrieve data from a SQL data store
    /// </summary>
    type SqlDataStoreQuery =
        | TabularQuery of tabularQuery : TabularDataQuery
        | TableFunctionQuery of  functionName : string * parameters : SqlQueryParameter list
        | ProcedureQuery of procedureName : string * parameters : SqlQueryParameter list

    /// <summary>
    /// Classifies supported data object/element types
    /// </summary>
    type SqlElementKind =
        /// Identifies a table
        | Table = 1uy
        /// Identifies a view
        | View = 2uy
        /// Identifies a stored procedure
        | Procedure = 3uy
        /// Identifies a table-valued function
        | TableFunction = 4uy
        /// Identifies a scalar function
        | ScalarFunction = 6uy
        /// Identifies a sequence object
        | Sequence = 5uy
        /// Identifies a column
        | Column = 1uy
        /// Identifies a schema
        | Schema = 1uy
        /// Identifies a custom primitive, e.g., an object created by issuing
        /// a CREATE TYPE command that is based on an intrinsic type
        | CustomPrimitive = 1uy

    /// <summary>
    /// Represents a store command
    /// </summary>
    type SqlStoreCommand =
        /// Represents the intent to truncate a table
        | TruncateTable of tableName : DataObjectName
        /// Represents the intent to allocate a range of sequence values
        | AllocateSequenceRange of sequenceName : DataObjectName * count : int

    /// <summary>
    /// Represents a store command result
    /// </summary>
    type SqlStoreCommandResult =
        | TruncateTableResult of rows : int
        | AllocateSequenceRangeResult of first : obj


    /// <summary>
    /// Defines the contract for a SQL Server Data Store
    /// </summary>
    type ISqlDataStore =
        /// <summary>
        /// Deletes and identified collection of data entities from the store
        /// </summary>
        abstract Delete:SqlDataStoreQuery -> unit
        /// <summary>
        /// Inserts the specified tabular data
        /// </summary>
        abstract Insert:ITabularData->unit
        /// <summary>
        /// Gets the connection string that identifies the represented store
        /// </summary>
        abstract ConnectionString :string
        /// <summary>
        /// Gets the store's metadata provider
        /// </summary>
        abstract MetadataProvider : ISqlMetadataProvider
        /// <summary>
        /// Obtains an identified contract from the store
        /// </summary>
        abstract GetContract: unit -> 'TContract when 'TContract : not struct
        /// <summary>
        /// Executes a supplied command against the store
        /// </summary>
        abstract ExecuteCommand:command : SqlStoreCommand -> SqlStoreCommandResult
        /// <summary>
        /// Retrieves an identified set of tabular data from the store
        /// </summary>
        abstract GetTabular:SqlDataStoreQuery -> ITabularData
        

    /// <summary>
    /// Defines the contract for a SQL Server Data Store that can persist and hydrate data via proxies
    /// </summary>
    type ISqlProxyDataStore =
        inherit ISqlDataStore
        /// <summary>
        /// Retrieves an identified collection of data entities from the store
        /// </summary>
        abstract Get:SqlDataStoreQuery -> 'T rolist

        /// <summary>
        /// Retrieves all entities of a given type from the store
        /// </summary>
        abstract Get:unit -> 'T rolist

        /// <summary>
        /// Persists a collection of data entities to the store, inserting or updating as appropriate
        /// </summary>
        abstract Merge:'T seq -> unit

        /// <summary>
        /// Inserts an arbitrary number of entities into the store, eliding existence checks
        /// </summary>
        abstract Insert:'T seq ->unit
   
    /// <summary>
    /// Represents the intent to retrieve data from an Excel data store
    /// </summary>
    type ExcelDataStoreQuery =
        /// Retrieves the data contained within a named worksheet
        | FindWorksheetByName of worksheetName : string
        /// Retrieves data contained in an excel table identified by name
        | FindTableByName of tableName : string
        /// Retrieves all worksheets in the workbook
        | FindAllWorksheets

    /// <summary>
    /// Describes a column proxy
    /// </summary>
    type ColumnProxyDescription = ColumnProxyDescription of field : ClrProperty * dataElement : ColumnDescription
    with
        /// <summary>
        /// Specifies the proxy record field
        /// </summary>
        member this.ProxyElement = 
            match this with ColumnProxyDescription(field=x) -> x
        
        /// <summary>
        /// Specifies the data column
        /// </summary>
        member this.DataElement = 
            match this with ColumnProxyDescription(dataElement=x) -> x

    /// <summary>
    /// Describes a proxy for a tabular result set
    /// </summary>
    type TableFunctionResultProxyDescription = TabularResultProxyDescription of proxy : ClrType  * dataElement : TableFunctionDescription * columns : ColumnProxyDescription list
    with
        /// <summary>
        /// Specifies the proxy record
        /// </summary>
        member this.ProxyElement = 
            match this with TabularResultProxyDescription(proxy=x) -> x

        /// <summary>
        /// Specifies the data table
        /// </summary>
        member this.DataElement =
            match this with TabularResultProxyDescription(dataElement=x) -> x

        /// <summary>
        /// Specifies the column proxies
        /// </summary>
        member this.Columns = 
            match this with TabularResultProxyDescription(columns=x) -> x

    /// <summary>
    /// Describes a table proxy
    /// </summary>
    type TabularProxyDescription = TablularProxyDescription of proxy : ClrType * dataElement : TabularDescription * columns : ColumnProxyDescription list
    with
        /// <summary>
        /// Specifies the proxy record
        /// </summary>
        member this.ProxyElement = 
            match this with TablularProxyDescription(proxy=x) -> x

        /// <summary>
        /// Specifies the data table
        /// </summary>
        member this.DataElement =
            match this with TablularProxyDescription(dataElement=x) -> x

        /// <summary>
        /// Specifies the column proxies
        /// </summary>
        member this.Columns = 
            match this with TablularProxyDescription(columns=x) -> x
    

    /// <summary>
    /// Describes a proxy for a routine parameter
    /// </summary>
    [<DebuggerDisplay("{DebuggerDisplay,nq}")>]
    type ParameterProxyDescription = ParameterProxyDescription of proxy : ClrMethodParameter * proxyParameterPosition : int * dataElement : RoutineParameterDescription 
    with   
        /// <summary>
        /// Specifies  the CLR proxy element
        /// </summary>
        member this.ProxyElement = 
            match this  with ParameterProxyDescription(proxy=x) -> x

        /// <summary>
        /// Specifies  the data element that the proxy represents
        /// </summary>
        member this.DataElement = 
            match this with ParameterProxyDescription(dataElement=x) -> x
        
        member this.ProxyParameterPosition =
            match this with ParameterProxyDescription(proxyParameterPosition = x) -> x

        /// <summary>
        /// Formats the element for presentation in the debugger
        /// </summary>
        member private this.DebuggerDisplay = 
            sprintf "@%s %O" this.DataElement.Name this.DataElement.StorageType
                
    /// <summary>
    /// Describes a proxy for a stored procedure
    /// </summary>
    type ProcedureCallProxyDescription = ProcedureCallProxyDescription of proxy : ClrMethod * dataElement : ProcedureDescription * parameters : ParameterProxyDescription list
    with
        /// <summary>
        /// Specifies  the CLR proxy element
        /// </summary>
        member this.ProxyElement = match this  with ProcedureCallProxyDescription(proxy=x) -> x

        /// <summary>
        /// Specifies  the data element that the proxy represents
        /// </summary>
        member this.DataElement = match this with ProcedureCallProxyDescription(dataElement=x) -> x

        /// <summary>
        /// Specifies the parameter proxies
        /// </summary>
        member this.Parameters = 
            match this with ProcedureCallProxyDescription(parameters=x) -> x

    type ProcedureProxyDescription = ProcedureCallProxyDescription

    /// <summary>
    /// Describes a proxy for calling a table-valued function
    /// </summary>
    type TableFunctionCallProxyDescription = TableFunctionCallProxyDescription of proxy : ClrMethod * dataElement : TableFunctionDescription * parameters : ParameterProxyDescription list
    with
        /// <summary>
        /// Specifies  the CLR proxy element
        /// </summary>
        member this.ProxyElement = match this  with TableFunctionCallProxyDescription(proxy=x) -> x

        /// <summary>
        /// Specifies  the data element that the proxy represents
        /// </summary>
        member this.DataElement = match this with TableFunctionCallProxyDescription(dataElement=x) -> x

        /// <summary>
        /// Specifies the parameter proxies
        /// </summary>
        member this.Parameters = 
            match this with TableFunctionCallProxyDescription(parameters=x) -> x
    
    
    type TableFunctionProxyDescription = TableFunctionProxyDescription of call : TableFunctionCallProxyDescription * result : TableFunctionResultProxyDescription
    with
        member this.CallProxy = match this with TableFunctionProxyDescription(call=x) -> x
        member this.ResultProxy = match this with TableFunctionProxyDescription(result=x) ->x

        /// <summary>
        /// Specifies  the data element that the proxy represents
        /// </summary>
        member this.DataElement = this.CallProxy.DataElement
    

    /// <summary>
    /// Unifies proxy description types
    /// </summary>
    type DataObjectProxy =
    | TabularProxy of TabularProxyDescription
    | ProcedureProxy of ProcedureProxyDescription
    | TableFunctionProxy of TableFunctionProxyDescription

    type IDataProxyMetadataProvider =
        abstract DescribeProxies:SqlElementKind->ClrElement->DataObjectProxy list
   