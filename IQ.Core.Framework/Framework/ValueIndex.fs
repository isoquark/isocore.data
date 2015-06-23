namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Linq

[<AutoOpen>]
module ValueIndexVocabulary = 

    /// <summary>
    /// Responsible for identifying a value in a ValueMap
    /// </summary>
    type ValueIndexKey = ValueIndexKey of name : string  * position : int
    with
        member this.Name = match this with |ValueIndexKey(name=x) -> x
        member this.Position = match this with |ValueIndexKey(position=x) -> x

    /// <summary>
    /// Represents a collection of name-indexed or position-indexed values
    /// </summary>
    type ValueIndex = ValueIndex of (ValueIndexKey*obj) list
    with
        /// <summary>
        /// Gets the underlying map
        /// </summary>
        member internal this.IndexedValues = match this with ValueIndex(m) -> m
        
        /// <summary>
        /// Gets a value from the map that is identified by its name
        /// </summary>
        /// <param name="name">The name of the value</param>
        member this.Item name = 
            this.IndexedValues |> List.find(fun (x,y) -> x.Name = name) |> snd

        /// <summary>
        /// Gets a value from the map that is identified by its position
        /// </summary>
        /// <param name="pos">The position of the value</param>
        member this.Item pos =
            this.IndexedValues |> List.find(fun (x,y) -> x.Position = pos) |> snd

        /// <summary>
        /// Gets a value from the map that is identified by its key
        /// </summary>
        /// <param name="pos">The position of the value</param>
        member this.Item key =
            this.IndexedValues |> List.find(fun (x,y) -> x = key) |> snd


module ValueIndex =
    let create (items : seq<string*int*obj>) =
        items |> Seq.map(fun (name, pos, value) -> ValueIndexKey(name,pos), value) |> List.ofSeq |> ValueIndex

    let toList (index : ValueIndex) =
        index.IndexedValues 

    let tryFindNamedValue name (idx : ValueIndex) =
        match idx.IndexedValues |> List.tryFind(fun (x,y) -> x.Name = name) with
        | Some(x) -> x |> snd |> Some
        | None -> None
    
    let tryFindValue key (idx : ValueIndex) = 
        match idx.IndexedValues |> List.tryFind(fun (x,y) -> x = key) with
        | Some(x) -> x |> snd |> Some
        | None -> None

