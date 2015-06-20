namespace global

open System
open System.Reflection
open System.Diagnostics
open System.Linq

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
            
        
