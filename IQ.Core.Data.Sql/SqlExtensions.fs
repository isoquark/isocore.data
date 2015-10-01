// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Contracts

open System
open System.Data
open System.Text
open System.Data.SqlClient
open System.Collections.Generic
open System.Runtime.CompilerServices

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data


[<Extension>]
module SqlExtensions =
    
    [<Extension>]
    let ToSql(x : DataTypeReference) =  x |> SqlFormatter.formatDataTypeReference

    [<Extension>]
    let GetMetadataProvider (x : ISqlDataStore) =
            {
                ConnectionString = x.ConnectionString
                IgnoreSystemObjects = true
            } |> SqlMetadataProvider.get        

    [<Extension>]
    let GetFileTableRoot (store : ISqlDataStore) =
        match GetFileTableRoot() |> store.ExecuteCommand with GetFileTableRootResult(x) -> x

    [<Extension>]
    let TruncateTable(store : ISqlDataStore, tableName) =
        TruncateTable(tableName) |> store.ExecuteCommand  |> ignore

    [<Extension>]
    let CreateTable(store : ISqlDataStore, tableDescription) =
        CreateTable(tableDescription) |> store.ExecuteCommand |> ignore

    [<Extension>]
    let DropTable(store : ISqlDataStore, tableName) =
        DropTable(tableName) |> store.ExecuteCommand |> ignore

[<AutoOpen>]
module SqlAugmentations =
    type ISqlDataStore 
    with
        member this.GetMetadataProvider() = this |> SqlExtensions.GetMetadataProvider
            
            