// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Reflection
open System.Linq
open System.Collections.Generic;

open System

[<AbstractClass>]
type DataStoreProvider<'Q>(storeKind : DataStoreKind, factory: string -> IDataStore<'Q>) =
                
    let adapt (store : IDataStore<'Q>) =
        {
            new IDataStore with
                member this.Insert items =
                    items |> store.Insert

                member this.InsertMatrix m =
                    m |> store.InsertMatrix
                        
                member this.Merge items =
                    items |> store.Merge
        
                member this.MergeMatrix m =
                    m |> store.MergeMatrix

                member this.SelectAll() =
                    store.SelectAll()
        
                member this.SelectMatrix(q) =
                    store.SelectMatrix(q :> obj :?> 'Q)

                member this.Select(q) = 
                    store.Select(q :> obj :?> 'Q)

                member this.GetCommandContract() : 'TContract =
                    store.GetCommandContract<'TContract>()

                member this.GetQueryContract() =
                    NotImplementedException() |> raise

                member this.ExecuteCommand(c) =
                    store.ExecuteCommand(c)
                member this.ConnectionString =
                    store.ConnectionString
                member this.ExecutePureCommand(c) =
                    store.ExecuteCommand(c) |> ignore
            }
           
    interface IDataStoreProvider with
        member this.GetDataStore cs = 
            cs |> factory |> adapt

        member this.GetDataStore cs =
            cs |> factory :> obj :?> 'T  
        
        member this.SupportedStores = [storeKind]

type DataStoreProvider(providers : IDataStoreProvider seq) =
    
    let providers = [for provider in providers do
                        if provider.SupportedStores.Length <> 1 then
                            ArgumentException("Only providers that support exactly one kind of store can be registered") |> raise
                        yield provider.SupportedStores.Head, provider] |> dict
                    
    static member Create([<ParamArray>] providers : IDataStoreProvider []) =
        DataStoreProvider(providers) :> IDataStoreProvider

    interface IDataStoreProvider with
        
        
        member this.GetDataStore (cs : string) : IDataStore =
            //TODO: read the StoreKind connection string parameter
            nosupport()
        
        member this.GetDataStore<'T> cs =
            //TODO: read the StoreKind connection string parameter
            let kind = typeof<'T>.GetCustomAttribute<DataStoreContractAttribute>().StoreKind
            if providers.ContainsKey(kind) |> not then
                ArgumentException("Provider not registred") |> raise
            providers.[kind].GetDataStore<'T>(cs)
        
        member this.SupportedStores = providers.Keys |> List.ofSeq

