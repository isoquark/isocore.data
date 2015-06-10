namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics

/// <summary>
/// Defines generally-applicable conversion utilities
/// </summary>
module Converter =
    let convert (dstType : Type) (value : obj) =
        if value = null then
            null                   
        else if value.GetType() = dstType then
            value
        else
            if dstType |> ClrOption.isOptionType then
                if value |> ClrOption.isOptionValue then
                    //Convert an option value to an option type
                    Convert.ChangeType(value |> ClrOption.unwrapValue |> Option.get, dstType |> ClrOption.getValueType) |> ClrOption.makeSome
                else
                    //Convert an non-option value to an option type
                    Convert.ChangeType(value, dstType |> ClrOption.getValueType) |> ClrOption.makeSome
            else
                if value |> ClrOption.isOptionValue then
                    //Convert an option value to a non-option type
                    Convert.ChangeType(value |> ClrOption.unwrapValue |> Option.get, dstType)
                else
                   //Convert a non-option value to a non-option type
                    Convert.ChangeType(value, dstType)
            

