// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql

open System
open System.Data
open System.Text
open System.Data.SqlClient
open System.Collections.Generic
open System.Runtime.CompilerServices

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data
open IQ.Core.Data.Sql.Behavior


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

[<AutoOpen>]
module SqlAugmentations =
    type ISqlDataStore 
    with
        member this.GetMetadataProvider() = this |> SqlExtensions.GetMetadataProvider
            
            