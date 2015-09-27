// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Behavior

open System
open System.Collections.Generic
open System.Linq;
open System.Collections.Concurrent;

open IQ.Core.Data.Contracts


type ItemSelector<'Q> = Func<'Q, obj seq, obj seq>



        
    

module internal MemoryDataStore =    

    
    
    type private ProxyCache = ConcurrentDictionary<Type, ConcurrentBag<obj>>

    let private getCachedItems<'T>(cache : ProxyCache) =
        match typeof<'T> |> cache.TryGetValue with
        | (true, values) ->
            values.Cast<'T>()
        | (false, _) ->
            Seq.empty

    let private addItems (itemType : Type) (cache : ProxyCache) (items : seq<obj>) =
        let l = cache.GetOrAdd(itemType, fun t -> ConcurrentBag<obj>())
        l.Add(items);

    let private addItemsT<'T>(cache : ProxyCache) (items : 'T seq) =
        let l = cache.GetOrAdd(typeof<'T>, fun t -> ConcurrentBag<obj>())
        l.Add(items.Cast<obj>());



    type Realization<'Q>(cs, selector : ItemSelector<'Q>) =        
        let cache = ProxyCache()              
          
        interface IDataStore<'Q> with
            member x.InsertMatrix(m) = 
                nosupport()
            
            member x.MergeMatrix(m): unit = 
                nosupport()
            
            member x.SelectMatrix(q) = 
                nosupport()
            
            member this.Insert items  = 
                items |> addItemsT<'T> cache

            member this.Merge (items : seq<'T>) : unit = 
                nosupport()
            member this.Select q = 
                let allItems = (cache |> getCachedItems<'T>).Cast<obj>()
                selector.Invoke(q, allItems).Cast<'T>()
                
            member this.SelectAll() = cache |> getCachedItems<'T>

            member this.GetCommandContract() =
                nosupport()

            member this.GetQueryContract() =
                nosupport()

            member this.ExecuteCommand(c) =
                nosupport()

            member this.ConnectionString = cs

    let get<'Q>(itemSelector, keyBuilder) = 
        Realization(itemSelector) :> IDataStore<'Q>
        
        
type MemoryDataStoreProvider<'Q> private (selector : ItemSelector<'Q>) =
    inherit DataStoreProvider<'Q>(
        fun cs -> MemoryDataStore.Realization<'Q>(cs, selector) :> IDataStore<'Q>)   

    static member GetProvider(selector) =
        MemoryDataStoreProvider(selector) :> IDataStoreProvider

