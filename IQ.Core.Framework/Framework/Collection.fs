namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent
open System.Diagnostics

type GenericList<'T> = System.Collections.Generic.List<'T>
type FSharpList<'T> = Microsoft.FSharp.Collections.List<'T>

open Microsoft.FSharp.Reflection


/// <summary>
/// Defines operations for working with collections
/// </summary>
module Collection =
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
    let private createList itemType (items : obj list)  =  
         let listType = 
             makeGenericType <| typedefof<FSharpList<_>> <| [ itemType; ]
 
         let add =  
             let cons =  listType.GetMethod ("Cons")             
             fun item list -> 
                cons.Invoke (null, [| item; list; |])                
 
         let list =  
             let empty = listType.GetProperty ("Empty") 
             empty.GetValue (null) 

         list |> List.foldBack add items    
    
    /// <summary>
    /// Creates an array
    /// </summary>
    /// <param name="itemType">The type of items that the array will contain</param>
    /// <param name="items">The items with which to populate the list</param>
    let private createArray itemType (items : obj list) =
        let a = Array.CreateInstance(itemType, items.Length)
        [0..items.Length-1] |> List.iter(fun i -> a.SetValue(items.[i], i))
        a :> obj

    /// <summary>
    /// Creates a generic list
    /// </summary>
    /// <param name="itemType">The type of items that the list will contain</param>
    /// <param name="items">The items with which to populate the list</param>
    let private createGenericList itemType (items : obj list) =
        let listType = makeGenericType <| typedefof<GenericList<_>> <| [itemType;]
        let list = Activator.CreateInstance(listType)
        let add = listType.GetMethod("Add")
        items |> List.iter(fun item -> add.Invoke(list, [|item|]) |> ignore)
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
