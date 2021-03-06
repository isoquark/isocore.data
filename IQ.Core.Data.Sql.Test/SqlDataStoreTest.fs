﻿// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql.Test


open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Test
open IQ.Core.Data.Sql
open IQ.Core.Framework
open IQ.Core.Data.Contracts

module SqlDataStoreTest =
    type Tests(ctx, log) = 
        inherit ProjectTestContainer(ctx,log)
        
        let store = ctx.Store
        let mdp = ctx.Store.GetMetadataProvider()
        let proxyMetadata = DataProxyMetadataProvider.get()

        [<Fact>]
        let ``Executed Dynamic Queries over [SqlTest].[Table08]``() =
            
            let rowcount = 100
            let col2Values = [for rowid in 1..rowcount -> rowid, (sprintf "Row%i Description" rowid) :> obj] |> Map.ofList
            let createRow(rowid) =
                [| rowid:> obj; col2Values.[rowid]; (rowid * 5 |> int16) :> obj|]
                            
            let tabularName = DataObjectName("SqlTest", "Table08")
            tabularName |> TruncateTable |> store.ExecuteCommand |> ignore
            let description = mdp.DescribeTable(tabularName)
            let rowValues =  [| for i in [1..rowcount] ->i |> createRow |] 
            let m = DataMatrix(DataMatrixDescription(description.Name, description.Columns), rowValues) :> IDataMatrix
            store.InsertMatrix(m)

            let q1 = DynamicQueryBuilder(tabularName)
                        .Columns(description.Columns |> Seq.map(fun x -> x.Name) |> Array.ofSeq)
                        .Sort([|AscendingSort(description.Columns.[0].Name)|])
                        .Page(1, 10)
                        .Build() 
                        
            let result = q1 |> store.SelectMatrix
            result.Rows.Count |> Claim.equal 10
            for rowidx in 0..result.Rows.Count-1 do
                let row = result.Rows.[rowidx]
                let rowid = row.[0] :?> int
                let actual = row.[1]
                let expect = col2Values.[rowid]
                Claim.equal expect actual
            


