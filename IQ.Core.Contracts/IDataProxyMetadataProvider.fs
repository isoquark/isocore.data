// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Diagnostics


open IQ.Core.Framework


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
    | BinaryFixed = 40uy //binary 
    | BinaryVariable = 41uy //varbinary
    | BinaryMax = 42uy
        
    //ANSI text types
    | AnsiTextFixed = 50uy //char
    | AnsiTextVariable = 51uy //varchar
    | AnsiTextMax = 52uy
        
    ///Unicode text types
    | UnicodeTextFixed = 53uy //nchar
    | UnicodeTextVariable = 54uy //nvarchar
    | UnicodeTextMax = 55uy
        
    ///Time-related types
    | DateTime = 62uy //corresponds to datetime2
    | DateTimeOffset = 63uy
    | TimeOfDay = 64uy //corresponds to time
    | Date = 65uy //corresponds to date        
    | Duration = 66uy //no direct map, use bigint to store number of ticks
        
    ///Approximate real types
    | Float32 = 70uy //corresponds to real
    | Float64 = 71uy //corresponds to float
        
    ///Exact real types
    | Decimal = 80uy
    | Money = 81uy
        
    | Guid = 90uy //corresponds to uniqueidentifier
    | Xml = 100uy
    | Json = 101uy
    | Flexible = 110uy //corresponds to sql_variant
                      
    ///Intrinsic SQL CLR types
    | Geography = 150uy
    | Geometry = 151uy
    | Hierarchy = 152uy
        
    /// A structured document of some sort; specification of an instance
    /// requires a DOM in code that represents the type (may be a simple record
    /// or something as involved as the HTML DOM) and a reader/writer type identifier
    /// than can be used to serialize/reconstitute document instances from ther
    /// storage format
    | TypedDocument = 160uy //a varchar(MAX) in sql; maybe a JSON serialized type in code

    //Custom Types
    | CustomTable = 170uy //a non-intrinsic table data type in sql; probably data DataTable or similar in CLR
    | CustomObject = 171uy //a non-intrinsic CLR type
    | CustomPrimitive = 172uy //a non-intrinsic primitive based on an intrinsic primitive; a custom type in code, e.g. a specialized struct, DU



module internal DataTypeShortNames =
    [<Literal>]
    let Bit = "bit"
    [<Literal>]
    let UInt8 = "uint8"
    [<Literal>]
    let UInt16 = "uint16"
    [<Literal>]
    let UInt32 = "uint32"
    [<Literal>]
    let UInt64 = "uint64"
    [<Literal>]
    let Int8 = "int8"
    [<Literal>]
    let Int16 = "int16"
    [<Literal>]
    let Int32 = "int32"
    [<Literal>]
    let Int64 = "int64"
    [<Literal>]
    let BinaryFixed = "binf"
    [<Literal>]
    let BinaryVariable = "binv"
    [<Literal>]
    let BinaryMax = "binm"
    [<Literal>]
    let AnsiTextFixed = "atextf"
    [<Literal>]
    let AnsiTextVariable = "atextv"
    [<Literal>]
    let AnsiTextMax = "atextm"
    [<Literal>]
    let UnicodeTextFixed = "utextf"
    [<Literal>]
    let UnicodeTextVariable = "utextv"
    [<Literal>]
    let UnicodeTextMax = "utextm"
    [<Literal>]
    let DateTime = "datetime"
    [<Literal>]
    let DateTimeOffset = "dtoffset"
    [<Literal>]
    let TimeOfDay = "tod"
    [<Literal>]
    let Date = "date"
    [<Literal>]
    let Duration = "duration"
    [<Literal>]
    let Float32 = "float32"
    [<Literal>]
    let Float64 = "float64"
    [<Literal>]
    let Decimal = "decimal"
    [<Literal>]
    let Money = "money"
    [<Literal>]
    let Guid = "guid"
    [<Literal>]
    let Xml = "xml"
    [<Literal>]
    let Json = "json"
    [<Literal>]
    let Flexible = "flexible"

    [<Literal>]
    let Geography = "geography"
    [<Literal>]
    let Geometry = "geometry"
    [<Literal>]
    let Hierarchy = "hierarchy"

    [<Literal>]
    let TypedDocument = "tdoc"

    [<Literal>]
    let CustomTable = "ctable"
    [<Literal>]
    let CustomObject = "cobject"
    [<Literal>]
    let CustomPrimitive = "cprimitive"
        

/// <summary>
/// Defines the literals that specify the semantic names for the DataTypeType cases
/// </summary>
module DataTypeLongNames =
    [<Literal>]
    let BitDataTypeName = "Bit"
    [<Literal>]
    let UInt8DataTypeName = "UInt8"
    [<Literal>]
    let UInt16DataTypeName = "UInt16"
    [<Literal>]
    let UInt32DataTypeName = "UInt32"
    [<Literal>]
    let UInt64DataTypeName = "UInt64"
    [<Literal>]
    let Int8DataTypeName = "Int8"
    [<Literal>]
    let Int16DataTypeName = "Int16"
    [<Literal>]
    let Int32DataTypeName = "Int32"
    [<Literal>]
    let Int64DataTypeName = "Int64"
    [<Literal>]
    let BinaryFixedDataTypeName = "BinaryFixed"
    [<Literal>]
    let BinaryVariableDataTypeName = "BinaryVariable"
    [<Literal>]
    let BinaryMaxDataTypeName = "BinaryMax"
    [<Literal>]
    let AnsiTextFixedDataTypeName = "AnsiTextFixed"
    [<Literal>]
    let AnsiTextVariableDataTypeName = "AnsiTextVariable"
    [<Literal>]
    let AnsiTextMaxDataTypeName = "AnsiTextMax"
    [<Literal>]
    let UnicodeTextFixedDataTypeName = "UnicodeTextFixed"
    [<Literal>]
    let UnicodeTextVariableDataTypeName = "UnicodeTextVariable"
    [<Literal>]
    let UnicodeTextMaxDataTypeName = "UnicodeTextMax"
    [<Literal>]
    let DateTimeDataTypeName = "DateTime"
    [<Literal>]
    let DateTimeOffsetDataTypeName = "DateTimeOffset"
    [<Literal>]
    let TimeOfDayDataTypeName = "TimeOfDay"
    [<Literal>]        
    let TimespanDataTypeName = "Timespan"
    [<Literal>]
    let DateDataTypeName = "Date"
    [<Literal>]
    let Float32DataTypeName = "Float32"
    [<Literal>]
    let Float64DataTypeName = "Float64"
    [<Literal>]
    let DecimalDataTypeName = "Decimal"
    [<Literal>]
    let MoneyDataTypeName = "Money"
    [<Literal>]
    let GuidDataTypeName = "Guid"
    [<Literal>]
    let XmlDataTypeName = "Xml"
    [<Literal>]
    let JsonDataTypeName = "Json"
    [<Literal>]
    let VariantDataTypeName = "Variant"
    [<Literal>]
    let CustomTableDataTypeName = "CustomTable"
    [<Literal>]
    let CustomObjectDataTypeName = "CustomObject"
    [<Literal>]
    let CustomPrimitiveDataTypeName = "CustomPrimitive"
    [<Literal>]
    let TypedDocumentDataTypeName = "TypedDocument"

        

open DataTypeLongNames
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
with        
    /// <summary>
    /// Renders a faithful representation of an instance as text
    /// </summary>
    member this.ToSemanticString() =
        match this with
        | BitDataType -> BitDataTypeName
        | UInt8DataType -> UInt8DataTypeName
        | UInt16DataType -> UInt16DataTypeName
        | UInt32DataType -> UInt32DataTypeName
        | UInt64DataType -> UInt64DataTypeName
        | Int8DataType -> Int8DataTypeName
        | Int16DataType -> Int16DataTypeName
        | Int32DataType -> Int32DataTypeName
        | Int64DataType -> Int64DataTypeName                        
        | BinaryFixedDataType(length) -> length |> sprintf "%s(%i)" BinaryFixedDataTypeName
        | BinaryVariableDataType(length) -> length |> sprintf "%s(%i)" BinaryVariableDataTypeName
        | BinaryMaxDataType -> BinaryMaxDataTypeName            
        | AnsiTextFixedDataType(length) -> length |> sprintf "%s(%i)" AnsiTextFixedDataTypeName
        | AnsiTextVariableDataType(length) -> length |> sprintf "%s(%i)" AnsiTextVariableDataTypeName
        | AnsiTextMaxDataType -> AnsiTextMaxDataTypeName            
        | UnicodeTextFixedDataType(length) -> length |> sprintf "%s(%i)" UnicodeTextFixedDataTypeName
        | UnicodeTextVariableDataType(length) -> length |> sprintf "%s(%i)" UnicodeTextVariableDataTypeName
        | UnicodeTextMaxDataType -> UnicodeTextMaxDataTypeName            
        | DateTimeDataType(precision)-> precision |> sprintf "%s(%i)" DateTimeDataTypeName
        | DateTimeOffsetDataType -> DateTimeOffsetDataTypeName
        | TimeOfDayDataType(precision) -> precision |> sprintf "%s(%i)" TimeOfDayDataTypeName
        | DateDataType -> DateDataTypeName
        | TimespanDataType -> TimespanDataTypeName            
        | Float32DataType -> Float32DataTypeName
        | Float64DataType -> Float64DataTypeName
        | DecimalDataType(precision,scale) -> sprintf "%s(%i,%i)" DecimalDataTypeName precision scale
        | MoneyDataType -> MoneyDataTypeName
        | GuidDataType -> GuidDataTypeName
        | XmlDataType(schema) -> schema |> sprintf "%s(%s)" XmlDataTypeName
        | JsonDataType -> JsonDataTypeName
        | VariantDataType -> VariantDataTypeName
        | CustomTableDataType(name) -> name |> sprintf "%s%O" CustomTableDataTypeName
        | CustomObjectDataType(name,t) -> sprintf "%s%O:%s" CustomObjectDataTypeName name t.AssemblyQualifiedName
        | CustomPrimitiveDataType(name) -> sprintf "%s%O" CustomPrimitiveDataTypeName name 
        | TypedDocumentDataType(t) -> sprintf "%s%s" TypedDocumentDataTypeName t.AssemblyQualifiedName

    /// <summary>
    /// Renders a representation of an instance as text
    /// </summary>
    override this.ToString() =
        this.ToSemanticString()

        
        
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
    /// Specifies the  purpose of the table
    Description : string option
    /// The columns in the table
    Columns : ColumnDescription list
}

type ParameterDirection = 
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
    /// The column's data type
    StorageType : DataType
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
