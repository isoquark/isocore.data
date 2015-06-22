namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Linq

[<AutoOpen>]
module ValueIndexVocabulary = 

    /// <summary>
    /// Responsible for uniquely identifying a value in a ValueMap
    /// </summary>
    [<DebuggerDisplay("{DebuggerDisplay, nq}")>]
    type ValueIndexKey = 
        | PositionKey of int
        | NameKey of string
    with
        member private this.DebuggerDisplay = 
            match this with
            | PositionKey(i) -> sprintf "%i" i
            | NameKey(n) -> sprintf "%s" n


    /// <summary>
    /// Represents a collection of name-indexed or position-indexed values
    /// </summary>
    type ValueIndex = ValueIndex of Map<ValueIndexKey,obj>
    with
        /// <summary>
        /// Gets the underlying map
        /// </summary>
        member internal this.IndexedValues = match this with ValueIndex(m) -> m
        
        /// <summary>
        /// Gets a value from the map that is identified by its name
        /// </summary>
        /// <param name="name">The name of the value</param>
        member this.Item (name : string) = this.IndexedValues.[NameKey(name)]

        /// <summary>
        /// Gets a value from the map that is identified by its position
        /// </summary>
        /// <param name="pos">The position of the value</param>
        member this.Item (pos : int) = this.IndexedValues.[PositionKey(pos)]

        /// <summary>
        /// Gets a value from the map that is identified by its key
        /// </summary>
        /// <param name="pos">The position of the value</param>
        member this.Item (key) = this.IndexedValues.[key]


module ValueIndex =
    let fromNamedItems (items : seq<string*obj>) =
        items |> Seq.map(fun (name,value) -> NameKey(name), value) |> Map.ofSeq |> ValueIndex  

    let toList (index : ValueIndex) =
        index.IndexedValues |> Map.toList

    let tryFindValue key (idx : ValueIndex) = if key |> idx.IndexedValues.ContainsKey then idx.[key] |> Some else None

