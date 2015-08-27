// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql.Test

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Sql
open IQ.Core.Framework

module SqlMetadataProvider =
    
    type Tests(ctx, log)= 
        inherit ProjectTestContainer(ctx,log)

        let mdp = ctx.Store.MetadataProvider

        [<Fact>]
        let ``Discovered Tables``() =
            let tables = FindAllTables |> FindTables |> mdp.Describe
            ()

        [<Fact>]
        let ``Discovered Data Types``() =
            let reader = 
                {
                    ConnectionString = ctx.ConnectionString
                    IgnoreSystemObjects = true
                } |> SqlMetadataReader
            let dataTypes = reader.GetDataTypes()
            ()