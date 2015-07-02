namespace IQ.Core.Data

open System
open System.Data
open System.Diagnostics
open System.Text
open System.Reflection
open System.Text.RegularExpressions
open System.Collections.Generic

open FSharp.Data

open IQ.Core.Framework

/// <summary>
/// Defines the Data DataType Type domain vocabulary
/// </summary>
[<AutoOpen>]
module DataTypeVocabulary =

    /// <summary>
    /// Specifies the available DataType classes
    /// </summary>
    /// <remarks>
    /// Note that the DataType class is not sufficient to characterize the DataType type and
    /// additional information, such as length or data object name is needed to store/instantiate
    /// a corresponding value
    /// </remarks>
    type DataKind =
        | Unspecified = 0uy
        | Bit = 10uy //bit
        | UInt8 = 20uy //tinyint
        | UInt16 = 21uy //no direct map, use int
        | UInt32 = 22uy // no direct map, use bigint
        | UInt64 = 23uy // no direct map, use varbinary(8)
        | Int8 = 30uy //no direct map, use smallint
        | Int16 = 31uy //smallint
        | Int32 = 32uy //int
        | Int64 = 33uy //bigint
        | BinaryFixed = 40uy //binary 
        | BinaryVariable = 41uy //varbinary
        | BinaryMax = 42uy
        | AnsiTextFixed = 50uy //char
        | AnsiTextVariable = 51uy //varchar
        | AnsiTextMax = 52uy
        | UnicodeTextFixed = 53uy //nchar
        | UnicodeTextVariable = 54uy //nvarchar
        | UnicodeTextMax = 55uy
        | DateTime32 = 60uy //corresponds to smalldatetime
        | DateTime64 = 61uy //corresponds to datetime
        | DateTime = 62uy //corresponds to datetime2
        | DateTimeOffset = 63uy
        | TimeOfDay = 64uy //corresponds to time
        | Date = 65uy //corresponds to date
        | Timespan = 66uy //no direct map, use bigint to store number of ticks
        | Float32 = 70uy //corresponds to real
        | Float64 = 71uy //corresponds to float
        | Decimal = 80uy
        | Money = 81uy
        | Guid = 90uy //corresponds to uniqueidentifier
        | Xml = 100uy
        | Variant = 110uy //corresponds to sql_variant
        | CustomTable = 150uy //a non-intrinsic table data type
        | CustomObject = 151uy //a non-intrinsic CLR type
        | CustomPrimitive = 152uy //a non-intrinsic primitive based on an intrinsic primitive
        | Geography = 160uy
        | Geometry = 161uy
        | Hierarchy = 162uy
        | TypedDocument = 180uy


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
        let DateTime32 = "datetime32"
        [<Literal>]
        let DateTime64 = "datetime64"
        [<Literal>]
        let DateTime = "datetime"
        [<Literal>]
        let DateTimeOffset = "dtoffset"
        [<Literal>]
        let TimeOfDay = "tod"
        [<Literal>]
        let Date = "date"
        [<Literal>]
        let Timespan = "timespan"
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
        let Variant = "variant"
        [<Literal>]
        let CustomTable = "ctable"
        [<Literal>]
        let CustomObject = "cobject"
        [<Literal>]
        let CustomPrimitive = "cprim"
        [<Literal>]
        let Geography = "geography"
        [<Literal>]
        let Geometry = "geometry"
        [<Literal>]
        let Hierarchy = "hierarchy"
        [<Literal>]
        let TypedDocument = "typedoc"
        

    /// <summary>
    /// Defines the literals that specify the semantic names for the DataTypeType cases
    /// </summary>
    module internal DataTypeLongNames =
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
        let DateTime32DataTypeName = "DateTime32"
        [<Literal>]
        let DateTime64DataTypeName = "DateTime64"
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
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
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
        | DateTime32DataType
        | DateTime64DataType
        | DateTimeDataType of precision : uint8
        | DateTimeOffsetDataType
        | TimeOfDayDataType
        | TimespanDataType 
        | DateDataType
        | Float32DataType
        | Float64DataType
        | DecimalDataType of precision : uint8 * scale : uint8
        | MoneyDataType
        | GuidDataType
        | XmlDataType of schema : string
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
            | DateTime32DataType -> DateTime32DataTypeName
            | DateTime64DataType -> DateTime64DataTypeName
            | DateTimeDataType(precision)-> precision |> sprintf "%s(%i)" DateTimeDataTypeName
            | DateTimeOffsetDataType -> DateTimeOffsetDataTypeName
            | TimeOfDayDataType -> TimeOfDayDataTypeName
            | DateDataType -> DateDataTypeName
            | TimespanDataType -> TimespanDataTypeName            
            | Float32DataType -> Float32DataTypeName
            | Float64DataType -> Float64DataTypeName
            | DecimalDataType(precision,scale) -> sprintf "%s(%i,%i)" DecimalDataTypeName precision scale
            | MoneyDataType -> MoneyDataTypeName
            | GuidDataType -> GuidDataTypeName
            | XmlDataType(schema) -> schema |> sprintf "%s(%s)" XmlDataTypeName
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

    type DataValue =
        | BitValue of bool
        | UInt8Value of uint8
        | UInt16Value of uint16
        | UInt32Value of uint32
        | UInt64Value of uint64
        | Int8Value of int8
        | Int16Value of int16
        | Int32Value of int32
        | Int64Value of int64
        | BinaryFixedValue of uint8[] 
        | BinaryVariableValue of uint8[]
        | BinaryMaxValue of uint8
        | AnsiTextFixedValue of string
        | AnsiTextVariableValue of string
        | AnsiTextMaxValue of string
        | UnicodeTextFixedValue of string
        | UnicodeTextVariableValue of string
        | UnicodeTextMaxValue of string
        | DateTime32Value of DateTime
        | DateTime64Value of DateTime
        | DateTimeValue of DateTime
        | DateTimeOffsetValue of DateTimeOffset
        | TimeOfDayValue of DateTime
        | DateValue of DateTime
        | TimespanValue of TimeSpan
        | Float32DValue of float32
        | Float64Value of float
        | DecimalValue of decimal
        | MoneyValue of decimal
        | GuidValue of Guid
        | XmlValue of string
        | VariantValue of obj
        | CustomTableValue of obj
        | CustomObjectValue of obj
        | CustomPrimitiveValue of obj
        | TypedDocumentValue of string

    type DataPoint = | DataPoint of v : DataValue * t : DataType 
        
        
      

module DataValue =
    let inline private cast<'T>(o : obj) = o :?> 'T
    
    let unwrap<'T>(v : DataValue) =
        match v with
        | BitValue(x) -> x |> cast<'T>
        | UInt8Value(x) -> x |> cast<'T> 
        | UInt16Value(x) -> x |> cast<'T>
        | UInt32Value(x) -> x |> cast<'T>
        | UInt64Value(x) -> x |> cast<'T>
        | Int8Value(x) -> x |> cast<'T>
        | Int16Value(x) -> x |> cast<'T>
        | Int32Value(x) -> x |> cast<'T>
        | Int64Value(x) -> x |> cast<'T>
        | BinaryFixedValue(x) -> x |> cast<'T>
        | BinaryVariableValue(x) -> x |> cast<'T>
        | BinaryMaxValue(x) -> x |> cast<'T>
        | AnsiTextFixedValue(x) -> x |> cast<'T>
        | AnsiTextVariableValue(x) -> x |> cast<'T>
        | AnsiTextMaxValue(x) -> x |> cast<'T>
        | UnicodeTextFixedValue(x) -> x |> cast<'T>
        | UnicodeTextVariableValue(x) -> x |> cast<'T>
        | UnicodeTextMaxValue(x) -> x |> cast<'T>
        | DateTime32Value(x) -> x |> cast<'T>
        | DateTime64Value(x) -> x |> cast<'T>
        | DateTimeValue(x) -> x |> cast<'T>
        | DateTimeOffsetValue(x) -> x |> cast<'T>
        | TimeOfDayValue(x) -> x |> cast<'T>
        | DateValue(x) -> x |> cast<'T>
        | TimespanValue(x) -> x |> cast<'T>
        | Float32DValue(x) -> x |> cast<'T>
        | Float64Value(x) -> x |> cast<'T>
        | DecimalValue(x) -> x |> cast<'T>
        | MoneyValue(x) -> x |> cast<'T>
        | GuidValue(x) -> x |> cast<'T>
        | XmlValue(x) -> x |> cast<'T>
        | VariantValue(x) -> x |> cast<'T>
        | CustomTableValue(x) -> x |> cast<'T>
        | CustomObjectValue(x) -> x |> cast<'T>
        | CustomPrimitiveValue(x) -> x |> cast<'T>
        | TypedDocumentValue(x) -> x |> cast<'T>

    let inline bit(x) = BitValue(x)
    let inline uint8(x) = UInt8Value(x)
    let inline uint16(x) = UInt16Value(x)
    let inline uint32(x) = UInt32Value(x)
    let inline uint64(x) = UInt64Value(x)
    let inline int8(x) = Int8Value(x)
    let inline int16(x) = Int16Value(x)
    let inline int32(x) = Int32Value(x)
    let inline int64(x) = Int64Value(x)
    let inline binf(x) = BinaryFixedValue(x)
    let inline binv(x) = BinaryVariableValue(x)
    let inline binm(x) = BinaryMaxValue(x)
    let inline atextf(x) = AnsiTextFixedValue(x)
    let inline atextv(x) = AnsiTextVariableValue(x)
    let inline atextm(x) = AnsiTextMaxValue(x)
    let inline utextf(x) = UnicodeTextFixedValue(x)
    let inline utextv(x) = UnicodeTextVariableValue(x)
    let inline utextm(x) = UnicodeTextMaxValue(x)
    
module DataKind =
    [<Literal>]
    let private DefaultDataTypeKindAspectsResource = "Data/Resources/DefaultStorageKindAspects.csv"                
    type private DefaultDataTypeKindAspects = CsvProvider<DefaultDataTypeKindAspectsResource, Separators="|", PreferOptionals=true>
        
    type private DataTypeKindAspects = | DataTypeKindAspects of length : int option * precision : uint8 option * scale : uint8 option
    with
        member this.Length = match this with DataTypeKindAspects(length=x) -> x |> Option.get
        member this.Precision = match this with DataTypeKindAspects(precision=x) -> x |> Option.get
        member this.Scale = match this with DataTypeKindAspects(scale=x) -> x |> Option.get

    let private defaults : IDictionary<DataKind, DataTypeKindAspects> = 
        [for row in (DefaultDataTypeKindAspectsResource |> DefaultDataTypeKindAspects.Load).Cache().Rows ->
            (DataKind.Parse row.DataTypeName, DataTypeKindAspects(row.Length, row.Precision |> Convert.ToUInt8Option , row.Scale |> Convert.ToUInt8Option))
        ] |> dict        
        
    /// <summary>
    /// Gets the DataType kind's default length
    /// </summary>
    /// <param name="kind">The kind of DataType</param>
    let getDefaultLength kind =
        defaults.[kind].Length 

    /// <summary>
    /// Gets the DataType kind's default precision
    /// </summary>
    /// <param name="kind">The kind of DataType</param>
    let getDefaultPrecision kind =
        defaults.[kind].Precision

    /// <summary>
    /// Gets the DataType kind's default scale
    /// </summary>
    /// <param name="kind">The kind of DataType</param>
    let getDefaultScale kind =
        defaults.[kind].Scale
    
    


/// <summary>
/// Defines operations for working with DataTypeType specifications
/// </summary>
module DataType =                
        
        open DataTypeLongNames

        /// <summary>
        /// Renders the DataTypeType as a semantic string
        /// </summary>
        /// <param name="t">The DataType type</param>
        let toSemanticString (t : DataType) =
            t.ToSemanticString()            
        
        /// <summary>
        /// Gets the kind of DataType required by the data type
        /// </summary>
        /// <param name="DataTypeType">The DataType type</param>
        let toKind (t : DataType) =
            match t with
            | BitDataType -> DataKind.Bit
            | UInt8DataType -> DataKind.UInt8
            | UInt16DataType -> DataKind.UInt64
            | UInt32DataType -> DataKind.UInt32
            | UInt64DataType -> DataKind.UInt64
            | Int8DataType -> DataKind.Int8
            | Int16DataType -> DataKind.Int16
            | Int32DataType -> DataKind.Int32
            | Int64DataType -> DataKind.Int64           
            
            | BinaryFixedDataType(_) -> DataKind.BinaryFixed
            | BinaryVariableDataType(_) -> DataKind.BinaryVariable
            | BinaryMaxDataType -> DataKind.BinaryMax
            
            | AnsiTextFixedDataType(length) -> DataKind.AnsiTextFixed
            | AnsiTextVariableDataType(length) -> DataKind.AnsiTextVariable
            | AnsiTextMaxDataType -> DataKind.AnsiTextMax
            
            | UnicodeTextFixedDataType(length) -> DataKind.UnicodeTextFixed
            | UnicodeTextVariableDataType(length) -> DataKind.UnicodeTextVariable
            | UnicodeTextMaxDataType -> DataKind.UnicodeTextMax
            
            | DateTime32DataType -> DataKind.DateTime32
            | DateTime64DataType -> DataKind.DateTime64
            | DateTimeDataType(precision)-> DataKind.DateTime
            | DateTimeOffsetDataType -> DataKind.DateTimeOffset
            | TimeOfDayDataType -> DataKind.TimeOfDay
            | DateDataType -> DataKind.Date
            | TimespanDataType -> DataKind.Timespan            
            | Float32DataType -> DataKind.Float32
            | Float64DataType -> DataKind.Float64
            | DecimalDataType(precision,scale) -> DataKind.Decimal
            | MoneyDataType -> DataKind.Money
            | GuidDataType -> DataKind.Guid
            | XmlDataType(_) -> DataKind.Xml
            | VariantDataType -> DataKind.Variant
            | CustomTableDataType(_) -> DataKind.CustomTable
            | CustomObjectDataType(_) -> DataKind.CustomObject
            | CustomPrimitiveDataType(_) -> DataKind.CustomPrimitive
            | TypedDocumentDataType(_) -> DataKind.TypedDocument

        let toSqlDbType (t : DataType) =
            match t with
            | BitDataType -> SqlDbType.Bit
            | UInt8DataType -> SqlDbType.TinyInt
            | UInt16DataType -> SqlDbType.Int
            | UInt32DataType -> SqlDbType.BigInt
            | UInt64DataType -> SqlDbType.VarBinary 
            | Int8DataType -> SqlDbType.SmallInt
            | Int16DataType -> SqlDbType.SmallInt
            | Int32DataType -> SqlDbType.Int
            | Int64DataType -> SqlDbType.BigInt
                        
            | BinaryFixedDataType(_) -> SqlDbType.Binary
            | BinaryVariableDataType(_) -> SqlDbType.VarBinary
            | BinaryMaxDataType -> SqlDbType.VarBinary
            
            | AnsiTextFixedDataType(length) -> SqlDbType.Char
            | AnsiTextVariableDataType(length) -> SqlDbType.VarChar
            | AnsiTextMaxDataType -> SqlDbType.VarChar
            
            | UnicodeTextFixedDataType(length) -> SqlDbType.NChar
            | UnicodeTextVariableDataType(length) -> SqlDbType.NVarChar
            | UnicodeTextMaxDataType -> SqlDbType.NVarChar
            
            | DateTime32DataType -> SqlDbType.SmallDateTime
            | DateTime64DataType -> SqlDbType.DateTime
            | DateTimeDataType(precision)-> SqlDbType.DateTime2
            | DateTimeOffsetDataType -> SqlDbType.DateTimeOffset
            | TimeOfDayDataType -> SqlDbType.Time
            | DateDataType -> SqlDbType.Date
            | TimespanDataType -> SqlDbType.BigInt

            | Float32DataType -> SqlDbType.Real
            | Float64DataType -> SqlDbType.Float
            | DecimalDataType(precision,scale) -> SqlDbType.Decimal
            | MoneyDataType -> SqlDbType.Money
            | GuidDataType -> SqlDbType.UniqueIdentifier
            | XmlDataType(schema) -> SqlDbType.Xml
            | VariantDataType -> SqlDbType.Variant
            | CustomTableDataType(name) -> SqlDbType.Structured
            | CustomObjectDataType(name,t) -> SqlDbType.VarBinary 
            | CustomPrimitiveDataType(name) -> SqlDbType.Udt
            | TypedDocumentDataType(_) -> SqlDbType.NVarChar
                                    
                    
        /// <summary>
        /// Parses the semantic representation of a StorageType
        /// </summary>
        /// <param name="text">The semantic representation</param>        
        let parse text =        
            //TODO: Investigate using FParsec for this sort of thing
            let pattern4() =
                let parameters = ["StorageName"; "SchemaName"; "LocalName"]
                let expression = @"(?<StorageName>[a-zA-z]*)\((?<SchemaName>[^,]*),(?<LocalName>[^\)]*)\)"
                match text |> Txt.tryMatchGroups parameters expression  with
                | Some(groups) ->
                    match groups?StorageName with
                    | CustomTableDataTypeName -> CustomTableDataType(DataObjectName(groups?SchemaName, groups?LocalName)) |> Some
                    | _ ->
                        None
                | None ->
                    None                   
                        
            let pattern3() =
                //This obviously won't validate a uri, but it's good enough for now
                let parameters = ["StorageName"; "p"; "s"]
                let expression = @"(?<StorageName>[a-zA-z]*)\((?<uri>(.)*)\)"
                match text |> Txt.tryMatchGroups parameters expression  with
                | Some(groups) ->
                    match groups?StorageName with
                    | XmlDataTypeName -> XmlDataType(groups?uri) |> Some
                    | _ ->
                        None
                | None ->
                    None                   
                
            
            let pattern2() =
                let parameters = ["StorageName"; "p"; "s"]
                let expression = @"(?<StorageName>[a-zA-z]*)\((?<p>[0-9]*),(?<s>[0-9]*)\)"
                match text |> Txt.tryMatchGroups  parameters expression with
                | Some(groups) ->
                    let p = Byte.Parse(groups?p)
                    let s = Byte.Parse(groups?s)
                    match groups?StorageName with
                    | DecimalDataTypeName -> DecimalDataType(p,s) |> Some
                    | _ ->
                        None
                | None ->
                    pattern3()
            
            let pattern1() =
                let parameters = ["StorageName"; "n"]
                let expression = @"(?<StorageName>[a-zA-z]*)\((?<n>[0-9]*)\)" 
                match text |> Txt.tryMatchGroups parameters expression with
                | Some(groups) ->
                    let n = Int32.Parse(groups?n)
                    match groups?StorageName with
                        | BinaryFixedDataTypeName -> BinaryFixedDataType(n) |> Some
                        | BinaryVariableDataTypeName -> BinaryVariableDataType(n) |> Some
                        | AnsiTextFixedDataTypeName -> AnsiTextFixedDataType(n) |> Some      
                        | AnsiTextVariableDataTypeName -> AnsiTextVariableDataType(n) |> Some               
                        | UnicodeTextFixedDataTypeName -> UnicodeTextFixedDataType(n) |> Some                
                        | UnicodeTextVariableDataTypeName -> UnicodeTextVariableDataType(n) |> Some  
                        | DateTimeDataTypeName -> DateTimeDataType(uint8(n)) |> Some
                        | _ -> None
                | None -> pattern2()

            let pattern0() =
                match text with
                | BitDataTypeName -> BitDataType |> Some
                | UInt8DataTypeName -> UInt8DataType |> Some
                | UInt16DataTypeName -> UInt16DataType |> Some
                | UInt32DataTypeName -> UInt32DataType |> Some
                | UInt64DataTypeName -> UInt64DataType |> Some
                | Int8DataTypeName -> Int8DataType |> Some
                | Int16DataTypeName -> Int16DataType |> Some
                | Int32DataTypeName -> Int32DataType |> Some
                | Int64DataTypeName -> Int64DataType |> Some
                | BinaryMaxDataTypeName -> BinaryMaxDataType |> Some
                | AnsiTextMaxDataTypeName -> AnsiTextMaxDataType |> Some
                | UnicodeTextMaxDataTypeName -> UnicodeTextMaxDataType |> Some
                | DateTime32DataTypeName -> DateTime32DataType |> Some
                | DateTime64DataTypeName -> DateTime64DataType |> Some
                | DateTimeOffsetDataTypeName -> DateTimeOffsetDataType |> Some
                | DateDataTypeName -> DateDataType |> Some
                | TimeOfDayDataTypeName -> TimeOfDayDataType |> Some
                | Float32DataTypeName -> Float32DataType |> Some
                | Float64DataTypeName-> Float64DataType |> Some
                | MoneyDataTypeName -> MoneyDataType |> Some
                | GuidDataTypeName -> GuidDataType |> Some
                | VariantDataTypeName -> VariantDataType |> Some
                | _ -> pattern1()
        
            pattern0()

            //Txt.matchRegexGroups 
            

[<AutoOpen>]
module DataTypeExtensions =
    type DataKind
    with
        member this.DefaultLength = this |> DataKind.getDefaultLength
        member this.DefaultPrecision = this |> DataKind.getDefaultPrecision
        member this.DefaultScale = this |> DataKind.getDefaultScale        

    type DataValue
    with
        member this.Unwrap<'T>() = this |> DataValue.unwrap<'T>