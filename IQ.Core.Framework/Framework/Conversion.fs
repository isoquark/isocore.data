namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics

[<AutoOpen>]
module ConverterVocabulary =
    
    type ConversionIdentifier = ConversionIdentifier of category : String * srcType : Type * dstType : Type
    with
        member this.Category = match this with ConversionIdentifier(category=x) ->x
        member this.SrcType = match this with ConversionIdentifier(srcType=x) -> x
        member this.DstType = match this with ConversionIdentifier(dstType=x) ->x
    
     
    type ConversionFunction<'TSrc,'TDst> = 'TSrc->'TDst
    
    /// <summary>
    /// Applied to a function to identify it as a converter
    /// </summary>
    [<AttributeUsage(AttributeTargets.Method)>]
    type ConversionFunctionAttribute(category) =
        inherit Attribute()

        new() =
            ConversionFunctionAttribute(String.Empty)
        
        member this.Category = if category |> String.IsNullOrWhiteSpace then None else Some(category)

/// <summary>
/// Defines generally-applicable conversion utilities
/// </summary>
module Converter =
        
    /// <summary>
    /// Converts a value to specified type
    /// </summary>
    /// <param name="value">The value to convert</param>
    let convert (dstType : Type) (value : obj) =
        if value = null then
            null                   
        else if value.GetType() = dstType then
            value
        else
            let valueType = dstType.ItemValueType
            if dstType |> Option.isOptionType then
                if value |> Option.isOptionValue then
                    //Convert an option value to an option type
                    Convert.ChangeType(value |> Option.unwrapValue |> Option.get, valueType) |> Option.makeSome
                else
                    //Convert an non-option value to an option type; note though, special
                    //handling is required for DBNull
                    if value.GetType() = typeof<DBNull> then
                        dstType |> Option.makeNone
                    else
                        Convert.ChangeType(value, valueType) |> Option.makeSome
            else
                if value |> Option.isOptionValue then
                    //Convert an option value to a non-option type
                    Convert.ChangeType(value |> Option.unwrapValue |> Option.get, valueType)
                else
                   //Convert a non-option value to a non-option type
                    Convert.ChangeType(value, valueType)

    /// <summary>
    /// Converts a value to generically-specified type
    /// </summary>
    /// <param name="value">The value to convert</param>
    let convertT<'T> (value : obj) =
        value |> convert typeof<'T> :?> 'T

    /// <summary>
    /// Converts an array of values
    /// </summary>
    /// <param name="dstTypes">The destination types</param>
    /// <param name="values">The source values</param>
    let convertArray (dstTypes : Type[]) (values : obj[])  =
        if values.Length <> dstTypes.Length then
            raise <| ArgumentException(
                sprintf "Value array (length = %i) and type array (length = %i must be of the same length" values.Length dstTypes.Length)
        values |> Array.mapi (fun i value -> value |> convert dstTypes.[i])


[<AutoOpen>]
module ConvertExtensions =
    
    type Convert
    with
        /// <summary>
        /// Converts the supplied value to an optional UInt8
        /// </summary>
        /// <param name="value">The value to convert</param>
        static member ToUInt8Option(value : int option) =
            match value with
            | Some(v) -> Convert.ToByte(v) |> Some
            | None -> None
