// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Behavior

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

open IQ.Core.Data.Contracts
    
module DataKind =
    type private DataTypeKindAspects = | DataTypeKindAspects of length : int option * precision : uint8 option * scale : uint8 option
    with
        member this.Length = match this with DataTypeKindAspects(length=x) -> x |> Option.get
        member this.Precision = match this with DataTypeKindAspects(precision=x) -> x |> Option.get
        member this.Scale = match this with DataTypeKindAspects(scale=x) -> x |> Option.get
    
    let private defaults = 
        [
            DataKind.BinaryFixed, DataTypeKindAspects(Some(250), None, None)
            DataKind.BinaryVariable, DataTypeKindAspects(Some(250), None, None)
            DataKind.AnsiTextFixed, DataTypeKindAspects(Some(250), None, None)
            DataKind.AnsiTextVariable, DataTypeKindAspects(Some(250), None, None)
            DataKind.UnicodeTextFixed, DataTypeKindAspects(Some(250), None, None)
            DataKind.UnicodeTextVariable, DataTypeKindAspects(Some(250), None, None)
            DataKind.DateTime, DataTypeKindAspects(None, Some(27uy), Some(7uy))
            DataKind.TimeOfDay, DataTypeKindAspects(None, Some(16uy), Some(7uy))
            DataKind.Decimal, DataTypeKindAspects(None, Some(19uy), Some(4uy))
            DataKind.Money, DataTypeKindAspects(None, Some(19uy), Some(4uy))
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
    [<Literal>]
    let RowversionDataTypeName = "Rowversion"
open DataTypeLongNames

/// <summary>
/// Defines operations for working with DataTypeType specifications
/// </summary>
module DataType =                
        

    /// <summary>
    /// Renders the DataTypeType as a semantic string
    /// </summary>
    /// <param name="t">The DataType type</param>
    let toSemanticString (t : DataTypeReference) =
        match t with
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
        | DateTimeDataType(p,s)-> sprintf "%s(%i,%i)" DateTimeDataTypeName p s
        | DateTimeOffsetDataType -> DateTimeOffsetDataTypeName
        | TimeOfDayDataType(p,s) -> sprintf "%s(%i,%i)" TimeOfDayDataTypeName p s
        | DateDataType -> DateDataTypeName
        | TimespanDataType -> TimespanDataTypeName            
        | Float32DataType -> Float32DataTypeName
        | Float64DataType -> Float64DataTypeName
        | DecimalDataType(precision,scale) -> sprintf "%s(%i,%i)" DecimalDataTypeName precision scale
        | MoneyDataType(p,s)-> sprintf "%s(%i,%i)" MoneyDataTypeName p s
        | GuidDataType -> GuidDataTypeName
        | XmlDataType(schema) -> schema |> sprintf "%s(%s)" XmlDataTypeName
        | JsonDataType -> JsonDataTypeName
        | VariantDataType -> VariantDataTypeName
        | TableDataType(name) -> name |> sprintf "%s%O" CustomTableDataTypeName
        | ObjectDataType(name,clrTypeName) -> sprintf "%s%O:%s" CustomObjectDataTypeName name clrTypeName
        | CustomPrimitiveDataType(name,baseType) -> sprintf "%s%O => %O" CustomPrimitiveDataTypeName name baseType
        | TypedDocumentDataType(t) -> sprintf "%s%s" TypedDocumentDataTypeName t.AssemblyQualifiedName
        | RowversionDataType -> RowversionDataTypeName
        
    /// <summary>
    /// Gets the kind of DataType required by the data type
    /// </summary>
    /// <param name="DataTypeType">The DataType type</param>
    let toKind (t : DataTypeReference) =
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
        | RowversionDataType -> DataKind.BinaryFixed         
            
        | BinaryFixedDataType(_) -> DataKind.BinaryFixed
        | BinaryVariableDataType(_) -> DataKind.BinaryVariable
        | BinaryMaxDataType -> DataKind.BinaryMax
            
        | AnsiTextFixedDataType(length) -> DataKind.AnsiTextFixed
        | AnsiTextVariableDataType(length) -> DataKind.AnsiTextVariable
        | AnsiTextMaxDataType -> DataKind.AnsiTextMax
            
        | UnicodeTextFixedDataType(_) -> DataKind.UnicodeTextFixed
        | UnicodeTextVariableDataType(_) -> DataKind.UnicodeTextVariable
        | UnicodeTextMaxDataType -> DataKind.UnicodeTextMax
            
        | DateTimeDataType(_,_)-> DataKind.DateTime
        | DateTimeOffsetDataType -> DataKind.DateTimeOffset
        | TimeOfDayDataType(_) -> DataKind.TimeOfDay
        | DateDataType -> DataKind.Date
        | TimespanDataType -> DataKind.Duration            
        | Float32DataType -> DataKind.Float32
        | Float64DataType -> DataKind.Float64
        | DecimalDataType(precision,scale) -> DataKind.Decimal
        | MoneyDataType(_,_) -> DataKind.Money
        | GuidDataType -> DataKind.Guid
        | XmlDataType(_) -> DataKind.Xml
        | JsonDataType -> DataKind.Json
        | VariantDataType -> DataKind.Flexible
        | TableDataType(_) -> DataKind.CustomTable
        | ObjectDataType(_) -> DataKind.CustomObject
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
                | CustomTableDataTypeName -> TableDataType(DataObjectName(groups?SchemaName, groups?LocalName)) |> Some
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
                | MoneyDataTypeName -> MoneyDataType(p,s) |> Some
                | DateTimeDataTypeName -> DateTimeDataType(p,s) |> Some
                | TimeOfDayDataTypeName -> TimeOfDayDataType(p,s) |> Some
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
        

