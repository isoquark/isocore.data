// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql.Test

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Sql
open IQ.Core.Framework
open IQ.Core.Data.Contracts
open IQ.Core.Data.Test


open TestProxies

module SqlMetadataProvider =
    
    type Tests(ctx, log)= 
        inherit ProjectTestContainer(ctx,log)

        let mdp = ctx.Store.GetMetadataProvider()
        let idx =
            let reader = 
                {
                    ConnectionString = ctx.ConnectionString
                    IgnoreSystemObjects = true
                } |> SqlMetadataReader
            let catalog = reader.ReadCatalog()
            SqlMetadataIndex(catalog)
            

        [<Fact>]
        let ``Discovered Tables``() =
            let tables = FindAllTables |> FindTables |> mdp.Describe
            ()

        [<Fact>]
        let ``Discovered Data Types``() =            
            let dataTypes = [for t in idx.GetSchemaDataTypes("SqlTest") -> t.ObjectName, t ] |> dict            

            let name = objectname<TableType01>
            name  |> Claim.containsKey dataTypes            
            match dataTypes.[name] with
            | DataTypeDescription(x) ->
                x.Documentation |> Claim.equal "Documentation for [SqlTest].[TableType01]"
                x.Columns.[0].Name |> Claim.equal "TTCol01"
                x.Columns.[1].Name |> Claim.equal "TTCol02"
                x.Columns.[2].Name |> Claim.equal "TTCol03"
            | _ ->
                nosupport()
            
            let name = objectname<TableType02>
            name  |> Claim.containsKey dataTypes
            let dataType = dataTypes.[name]
            match dataTypes.[name] with
            | DataTypeDescription(x) ->
                x.Columns.[0].Name |> Claim.equal (propname<@ fun (x : TableType02) -> x.Col01 @>)
            | _ ->
                nosupport()

            let name = DataObjectName("SqlTest", "PhoneNumber") 
            name  |> Claim.containsKey dataTypes
            let dataType = dataTypes.[name]

            
            ()