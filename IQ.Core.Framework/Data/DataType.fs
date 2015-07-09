﻿namespace IQ.Core.Data

open System
open System.Data
open System.Diagnostics
open System.Text
open System.Reflection
open System.Text.RegularExpressions
open System.Collections.Generic

open FSharp.Data

open IQ.Core.Contracts
open IQ.Core.Framework

/// <summary>
/// Defines the Data DataType Type domain vocabulary
/// </summary>
[<AutoOpen>]
module DataTypeVocabulary =

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

        
        
      

    
module DataKind =
    [<Literal>]
    let private DefaultDataTypeKindAspectsResource = "Data/Resources/DefaultStorageKindAspects.csv"                
    type private DefaultDataTypeKindAspects = CsvProvider<DefaultDataTypeKindAspectsResource, Separators="|", PreferOptionals=true>
        
    type private DataTypeKindAspects = | DataTypeKindAspects of length : int option * precision : uint8 option * scale : uint8 option
    with
        member this.Length = match this with DataTypeKindAspects(length=x) -> x |> Option.get
        member this.Precision = match this with DataTypeKindAspects(precision=x) -> x |> Option.get
        member this.Scale = match this with DataTypeKindAspects(scale=x) -> x |> Option.get

    let private loadDefaults() =
        [for row in (DefaultDataTypeKindAspectsResource |> DefaultDataTypeKindAspects.Load).Cache().Rows ->
            (DataKind.Parse row.DataTypeName, DataTypeKindAspects(row.Length, row.Precision |> Convert.ToUInt8Option , row.Scale |> Convert.ToUInt8Option))
        ] |> dict        

    let private defaults = loadDefaults() 
        
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
            
            | UnicodeTextFixedDataType(_) -> DataKind.UnicodeTextFixed
            | UnicodeTextVariableDataType(_) -> DataKind.UnicodeTextVariable
            | UnicodeTextMaxDataType -> DataKind.UnicodeTextMax
            
            | DateTimeDataType(precision)-> DataKind.DateTime
            | DateTimeOffsetDataType -> DataKind.DateTimeOffset
            | TimeOfDayDataType(_) -> DataKind.TimeOfDay
            | DateDataType -> DataKind.Date
            | TimespanDataType -> DataKind.Duration            
            | Float32DataType -> DataKind.Float32
            | Float64DataType -> DataKind.Float64
            | DecimalDataType(precision,scale) -> DataKind.Decimal
            | MoneyDataType -> DataKind.Money
            | GuidDataType -> DataKind.Guid
            | XmlDataType(_) -> DataKind.Xml
            | JsonDataType -> DataKind.Json
            | VariantDataType -> DataKind.Flexible
            | CustomTableDataType(_) -> DataKind.CustomTable
            | CustomObjectDataType(_) -> DataKind.CustomObject
            | CustomPrimitiveDataType(_) -> DataKind.CustomPrimitive
            | TypedDocumentDataType(_) -> DataKind.TypedDocument

                                    
                    
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
                        | TimeOfDayDataTypeName -> TimeOfDayDataType(uint8(n)) |> Some

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
                | DateTimeOffsetDataTypeName -> DateTimeOffsetDataType |> Some
                | DateDataTypeName -> DateDataType |> Some
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

