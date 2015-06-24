namespace global

open System
open System.Reflection
open System.Diagnostics
open System.Linq
open System.Collections.Generic

open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns

/// <summary>
/// Defines core global operations and types
/// </summary>
/// <remarks>
/// The content here is more-or-less random at the moment
/// </remarks>
[<AutoOpen>]
module Lang =
                                            
    /// <summary>
    /// Defines custom Seq module operations
    /// </summary>
    module Seq =
        /// <summary>
        /// Counts the number of items in the sequence
        /// </summary>
        /// <param name="items">The items to count</param>
        /// <remarks>
        /// Obviously, this assumes that the sequence is not interminable!
        /// </remarks>
        let count (items : seq<'T>) = items.Count()
    

    /// <summary>
    /// Raises a debugging assertion if a supplied predicate fails and emits a diagnostic message
    /// </summary>
    /// <param name="message">The diagnostic to emit if predicate evaluation fails</param>
    /// <param name="predicate">The predicate to evaluate</param>
    let _assert message (predicate: unit -> bool)  = 
        Debug.Assert(predicate(), message)

    /// <summary>
    /// Raises a <see cref="System.NotSupportedException"/>
    /// </summary>
    /// <param name="description"></param>
    let inline nosupport()  = NotSupportedException() |> raise

    /// <summary>
    /// Defines augmentations for the TimeSpan type
    /// </summary>
    type TimeSpan
    with
        static member Sum(timespans : TimeSpan seq) =
            timespans |> Seq.map(fun x -> x.Ticks) |> Seq.sum |> TimeSpan.FromTicks

    /// <summary>
    /// The default format string to use when applying the DebuggerDisplay attribute
    /// </summary>
    [<Literal>]
    let DebuggerDisplayDefault = "{ToString(),nq}"

    /// <summary>
    /// Realized by types whose instance that are capable of being faithfully rendered as text.
    /// </summary>
    /// <remarks>
    /// The semantic representation of an instance includes the state necessary to reconstitute the 
    /// instance from that representation
    /// </remarks>
    type ISemanticRepresentation =
        /// <summary>
        /// Faithfully renders an instance as text
        /// </summary>
        abstract ToSemanticString:unit->string

    /// <summary>
    /// Identifies a function that can parse the semantic representation of a type instance
    /// </summary>
    [<AttributeUsage(AttributeTargets.Method)>]
    type ParserAttribute(t) = 
        inherit Attribute()
        
        /// <summary>
        /// The type of element that can be parsed
        /// </summary>
        member this.ElementType : Type = t 

    type Enum 
    with
        static member Parse<'T when 'T:>Enum >(value) = Enum.Parse(typeof<'T>, value) :?> 'T

    /// <summary>
    /// Lookup operator to retrieve the value identified by a key in a map
    /// </summary>
    /// <param name="map">The map to search</param>
    /// <param name="key">The value key</param>
    let (?) (map : Map<string,_>) key = map.[key]

    /// <summary>
    /// Specifies the range of allowable values for a given element
    /// </summary>
    type Multiplicity = 
        | ExactlyZero
        | ZeroOrOne
        | ZeroOrMore
        | ExactlyOne
        | OneOrMore
        | BoundedRange of min : uint32 * max : uint32
            
        
    /// <summary>
    /// Specifies a concrete example of a regular expression
    /// </summary>
    type RegexExampleAttribute(text) =
        inherit Attribute()
        
        /// <summary>
        /// Gets the example text
        /// </summary>
        member this.Text : string = text    

/// <summary>
/// Defines utility methods for working with options
/// </summary>
module Option =
    
    /// <summary>
    /// Determines whether a type is an option type
    /// </summary>
    /// <param name="t">The type to examine</param>
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
    /// Gets the type of the encapsulated value
    /// </summary>
    /// <param name="optionType">The option type</param>
    let getOptionValueType (t : Type) =
        if t |> isOptionType  then t.GetGenericArguments().[0] |> Some else None

    
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

module Type =
    /// <summary>
    /// Determines whether a type is a generic enumerable
    /// </summary>
    /// <param name="t">The type to examine</param>
    let internal isNonOptionalCollectionType (t : Type) =
        let isEnumerable = t.GetInterfaces() |> Array.exists(fun x -> x.IsGenericType && x.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>)
        if t.IsArray |> not then
            t.IsGenericType && isEnumerable
        else
            isEnumerable            

    /// <summary>
    /// Determines whether the type is of the form option<IEnumerable<_>>
    /// </summary>
    /// <param name="t">The type to examine</param>
    let internal isOptionalCollectionType (t : Type) =
        t |> Option.isOptionType && t |> Option.getOptionValueType |> Option.get |> (fun x -> x |> isNonOptionalCollectionType)

    /// <summary>
    /// Determines whether a type represents a collection (optional or not)
    /// </summary>
    /// <param name="t">The type to examine</param>
    let internal isCollectionType (t : Type) =
        t |> isNonOptionalCollectionType || t |> isOptionalCollectionType
                
    /// <summary>
    /// Determines whether a supplied type is a record type
    /// </summary>
    /// <param name="t">The candidate type</param>
    let internal isRecordType(t : Type) =
        FSharpType.IsRecord(t, true)

    /// <summary>
    /// Determines whether a supplied type is a record type
    /// </summary>
    let internal isRecord<'T>() =
        typeof<'T> |> isRecordType

    /// <summary>
    /// Determines whether a supplied type is a union type
    /// </summary>
    /// <param name="t">The candidate type</param>
    let internal isUnionType (t : Type) =
        FSharpType.IsUnion(t, true)

    /// <summary>
    /// Determines whether a supplied type is a union type
    /// </summary>
    let internal isUnion<'T>() =
        typeof<'T> |> isUnionType
    
    let getCollectionValueType (t : Type) =
        //This is far from bullet-proof
        let colltype =
            if t |> isOptionalCollectionType then
                t |> Option.getOptionValueType 
            else if t |> isNonOptionalCollectionType then
                t |> Some
            else
                None
        match colltype with
        | Some(t) ->
            let i = t.GetInterfaces() |> Array.find(fun i -> i.IsGenericType && i.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>)    
            i.GetGenericArguments().[0] |> Some
        | None ->
            None
                
    let getItemValueType (t : Type)  =
        match t |> getCollectionValueType with
        | Some(t) -> t
        | None ->
            match t |> Option.getOptionValueType with
            | Some(t) -> t
            | None ->
                t

[<AutoOpen>]
module TypeExtensions =
    type Type
    with
        member this.IsOptionType = this |> Option.isOptionType

        /// <summary>
        /// If optional type, gets the type of the underlying value; otherwise, the type itself
        /// </summary>
        member this.ItemValueType = this |> Type.getItemValueType
            