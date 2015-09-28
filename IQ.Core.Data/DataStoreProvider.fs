// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Behavior

open System

[<AbstractClass>]
type DataStoreProvider<'Q>( factory: string -> IDataStore<'Q>) =
                
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
            }
           
    interface IDataStoreProvider with
        member this.GetDataStore cs = 
            cs |> factory |> adapt

        member this.GetSpecificStore cs =
            cs |> factory :> obj :?> 'T    

module DataStore =
    let get(cs : string) = nosupport()
        //TODO: read the "StoreKind" parameter value from the connection string
        //and instantiate the appropriate store