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
module ClrCollection =
    let private makeGenericType (baseType : Type) (types : Type list) = 

      if (not baseType.IsGenericTypeDefinition) then
        invalidArg "baseType" "baseType must be a generic type definition."

      baseType.MakeGenericType (types|> List.toArray)

    //I'm not crazy about this approach; it's logical but surely there's a better way.
    //Source: http://blog.usermaatre.co.uk/programming/2013/07/24/fsharp-collections-reflection/
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
    
    let private createArray itemType (items : obj list) =
        let a = Array.CreateInstance(itemType, items.Length)
        [0..items.Length-1] |> List.iter(fun i -> a.SetValue(items.[i], i))
        a :> obj


    let private createGenericList itemType (items : obj list) =
        let listType = makeGenericType <| typedefof<GenericList<_>> <| [itemType;]
        let list = Activator.CreateInstance(listType)
        let add = listType.GetMethod("Add")
        items |> List.iter(fun item -> add.Invoke(list, [|item|]) |> ignore)
        list
        


    let create kind itemType items =
        match kind with
        | ClrCollectionKind.FSharpList ->
            items |> createList itemType
        | ClrCollectionKind.Array ->
            items |> createArray itemType
        | ClrCollectionKind.GenericList ->
            items |> createGenericList itemType
        | _ ->
            NotSupportedException() |> raise
