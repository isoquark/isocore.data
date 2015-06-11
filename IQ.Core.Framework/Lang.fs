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
    /// Responsible for uniquely identifying a value in a ValueMap
    /// </summary>
    type ValueMapKey = ValueMapKey of index : int option * name : string option
    
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
    /// Represents an indexed collection of key-value pairs
    /// </summary>
    type ValueMap = ValueMap of Map<ValueMapKey,obj>
    with
        /// <summary>
        /// Gets the underlying map
        /// </summary>
        member this.MappedValues = match this with ValueMap(m) -> m
        
        /// <summary>
        /// Gets a specified value from the map
        /// </summary>
        /// <param name="name">The name of the value</param>
        member this.Item (name : string) = this.MappedValues.[ValueMapKey(None, Some(name))]

    /// <summary>
    /// Responsible for identifying a Data Store, Network Address or other resource
    /// </summary>
    type ConnectionString = ConnectionString of string list
    with
        /// <summary>
        /// The components of the connection string
        /// </summary>
        member this.Components = match this with ConnectionString(components) -> components

    /// <summary>
    /// Raises a debugging assertion if a supplied predicate fails and emits a diagnostic message
    /// </summary>
    /// <param name="message">The diagnostic to emit if predicate evaluation fails</param>
    /// <param name="predicate">The predicate to evaluate</param>
    let _assert message (predicate: unit -> bool)  = 
        Debug.Assert(predicate(), message)

    /// <summary>
    /// When supplied a property accessor quotation, retrieves the name of the property
    /// </summary>
    /// <param name="q">The property accessor quotation</param>
    /// <remarks>
    /// Inspired heavily by: http://www.contactandcoil.com/software/dotnet/getting-a-property-name-as-a-string-in-f/
    /// </remarks>
    let rec propname q =
       match q with
       | PropertyGet(_,p,_) -> p.Name
       | Lambda(_, expr) -> propname expr
       | _ -> String.Empty

    /// <summary>
    /// Defines augmentations for the TimeSpan type
    /// </summary>
    type TimeSpan
    with
        static member Sum(timespans : TimeSpan seq) =
            timespans |> Seq.map(fun x -> x.Ticks) |> Seq.sum |> TimeSpan.FromTicks
            
module ValueMap =
    let fromNamedItems (items : seq<string*obj>) =
        items |> Seq.map(fun (name,value) -> ValueMapKey(None, Some(name)), value) |> Map.ofSeq |> ValueMap  

