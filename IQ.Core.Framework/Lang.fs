namespace global

open System
open System.Reflection
open System.Diagnostics

open Microsoft.FSharp.Reflection

[<AutoOpen>]
module Lang =
    /// <summary>
    /// Alias for a KVP map
    /// </summary>
    type ValueMap = Map<string,obj>

    /// <summary>
    /// Responsible for identifying a Data Store, Network Address or other resource
    /// </summary>
    type ConnectionString = ConnectionString of string list
    with
        /// <summary>
        /// The components of the connection string
        /// </summary>
        member this.Components = match this with ConnectionString(components) -> components

    let _assert message (predicate: unit -> bool)  = 
        Debug.Assert(predicate(), message)
        

/// <summary>
/// Defines utility methods for working with options
/// </summary>
module Option =
    /// <summary>
    /// Determines whether a type is an option type
    /// </summary>
    /// <param name="t"></param>
    let isOptionType (t : Type) =
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>        
    
    /// <summary>
    /// Determines whether a value is an option
    /// </summary>
    /// <param name="value">The value to examine</param>
    let isOptionValue (value : obj) =
        if value <> null then
            value.GetType() |> isOptionType
        else
            false
    
    /// <summary>
    /// Extracts the enclosed value if Some, otherwise yields None
    /// </summary>
    /// <param name="value">The option value</param>
    let unwrapValue (value : obj) =
        if value = null then 
            None
        else
            _assert "Value is not an option" (fun () -> isOptionValue(value) )
            let caseInfo, fields = FSharpValue.GetUnionFields(value, value.GetType(),true)
            if fields.Length = 0 then
                None
            else
                fields.[0] |> Some

    /// <summary>
    /// Encloses a supplied value within Some option
    /// </summary>
    /// <param name="value">The value to enclose</param>
    let makeSome (value : obj) =
        if value = null then
            ArgumentNullException() |> raise
        
        let valueType = value.GetType()
        let optionType = typedefof<option<_>>.MakeGenericType(valueType)
        let unionCase = FSharpType.GetUnionCases(optionType,true) |> Array.find(fun c -> c.Name = "Some")
        FSharpValue.MakeUnion(unionCase, [|value|], true)

    /// <summary>
    /// Creates an option with the case None
    /// </summary>
    /// <param name="valueType">The value's type</param>
    let makeNone (valueType : Type) =
        let optionType = typedefof<option<_>>.MakeGenericType(valueType)
        let unionCase = FSharpType.GetUnionCases(optionType,true) |> Array.find(fun c -> c.Name = "None")
        FSharpValue.MakeUnion(unionCase, [||], true)

    let getValueType (optionType : Type) =
        optionType.GetGenericArguments().[0]
        
            


