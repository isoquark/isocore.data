﻿namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics

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
            if dstType |> Type.isOptionType then
                if value |> ClrOption.isOptionValue then
                    //Convert an option value to an option type
                    Convert.ChangeType(value |> ClrOption.unwrapValue |> Option.get, valueType) |> ClrOption.makeSome
                else
                    //Convert an non-option value to an option type
                    Convert.ChangeType(value, valueType) |> ClrOption.makeSome
            else
                if value |> ClrOption.isOptionValue then
                    //Convert an option value to a non-option type
                    Convert.ChangeType(value |> ClrOption.unwrapValue |> Option.get, valueType)
                else
                   //Convert a non-option value to a non-option type
                    Convert.ChangeType(value, valueType)

    /// <summary>
    /// Converts a value to generically-specified type
    /// </summary>
    /// <param name="value">The value to convert</param>
    let convertT<'T> (value : obj) =
        value |> convert typeof<'T> :?> 'T



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
