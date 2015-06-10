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
            if dstType |> Option.isOptionType then
                if value |> Option.isOptionValue then
                    //Convert an option value to an option type
                    Convert.ChangeType(value |> Option.unwrapValue |> Option.get, dstType |> Option.getValueType) |> Option.makeSome
                else
                    //Convert an non-option value to an option type
                    Convert.ChangeType(value, dstType |> Option.getValueType) |> Option.makeSome
            else
                if value |> Option.isOptionValue then
                    //Convert an option value to a non-option type
                    Convert.ChangeType(value |> Option.unwrapValue |> Option.get, dstType)
                else
                   //Convert a non-option value to a non-option type
                    Convert.ChangeType(value, dstType)
            

