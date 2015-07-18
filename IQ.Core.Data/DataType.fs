﻿// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

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

    
module DataKind =
    [<Literal>]
    let private DefaultDataTypeKindAspectsResource = "Resources/DefaultStorageKindAspects.csv"                
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
    
    

open DataTypeLongNames

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

