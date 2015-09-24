// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework


open System
open System.Reflection
open System.Collections.Generic
open System.Linq

type GenericList<'T> = System.Collections.Generic.List<'T>
type FSharpList<'T> = Microsoft.FSharp.Collections.List<'T>

/// <summary>
/// Defines operations for creating/populating collecitons
/// </summary>
module CollectionBuilder =
    let private makeGenericType (baseType : Type) (types : Type list) = 

      if (not baseType.IsGenericTypeDefinition) then
        invalidArg "baseType" "baseType must be a generic type definition."

      baseType.MakeGenericType (types|> List.toArray)

    /// <summary>
    /// Creates an F# list
    /// </summary>
    /// <param name="itemType">The type of items that the list will contain</param>
    /// <param name="items">The items with which to populate the list</param>
    /// <remarks>
    /// I'm not crazy about this approach; it's logical but surely there's a better way.
    /// Source: http://blog.usermaatre.co.uk/programming/2013/07/24/fsharp-collections-reflection
    /// </remarks>
    let private createList itemType (items : obj seq)  =  
         let listType = 
             makeGenericType <| typedefof<FSharpList<_>> <| [ itemType; ]
 
         let add =  
             let cons =  listType.GetMethod ("Cons")             
             fun item list -> 
                cons.Invoke (null, [| item; list; |])                
 
         let list =  
             let empty = listType.GetProperty ("Empty") 
             empty.GetValue (null) 

         list |> Seq.foldBack add items    
    
    /// <summary>
    /// Creates an array
    /// </summary>
    /// <param name="itemType">The type of items that the array will contain</param>
    /// <param name="items">The items with which to populate the list</param>
    let private createArray itemType (items : obj seq) =        
        let a = Array.CreateInstance(itemType, items.Count())
        items |> Seq.iteri(fun i item -> a.SetValue(item, i))
        //[0..items.Length-1] |> Seq.iter(fun i -> a.SetValue(items.[i], i))
        a :> obj

    /// <summary>
    /// Creates a generic list
    /// </summary>
    /// <param name="itemType">The type of items that the list will contain</param>
    /// <param name="items">The items with which to populate the list</param>
    let private createGenericList itemType (items : obj seq) =
        let listType = makeGenericType <| typedefof<GenericList<_>> <| [itemType;]
        let list = Activator.CreateInstance(listType)
        let add = listType.GetMethod("Add")
        items |> Seq.iter(fun item -> add.Invoke(list, [|item|]) |> ignore)
        list
        
    /// <summary>
    /// Creates a collection
    /// </summary>
    /// <param name="kind">The kind of collection</param>
    /// <param name="itemType">The type of items that the collection will contain</param>
    /// <param name="items">The items with which to populate the collection</param>
    let create kind itemType items =
        match kind with
        | ClrCollectionKind.FSharpList ->
            items |> createList itemType
        | ClrCollectionKind.Array ->
            items |> createArray itemType
        | ClrCollectionKind.GenericList ->
            items |> createGenericList itemType
        | _ -> nosupport()


