// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql.Test

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection
open System.Threading
open System.Collections.Generic

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Test
open IQ.Core.Data.Sql
open IQ.Core.Framework

open TestProxies


module Tabular =
    
    let private verifyBulkInsert<'T>(input : 'T list) (sortBy: 'T->IComparable) (store : ISqlDataStore)=
        let count = tableproxy<'T>.DataElement.Name |> TruncateTable |> store.ExecuteCommand 
        store.Get<'T>() |> Claim.seqIsEmpty
        input |> store.Insert
        let output = store.Get<'T>() |> RoList.sortBy sortBy |> RoList.toList
        output |> Claim.equal input

    type Customer1() =
        member val Id = 0 with get, set
        member val Title = String.Empty with get, set
        member val FirstName = String.Empty with get, set
        member val LastName = String.Empty with get, set
    
    type Tests(ctx, log) = 
        inherit ProjectTestContainer(ctx,log)
        
        let store = ctx.Store
        let sqlMetadata = ctx.Store.MetadataProvider
        let proxyMetadata = DataProxyMetadataProvider.get()


        [<Fact>]
        let ``Queried vDataType metadata - Partial column set A``() =
            let items = store.Get<Metadata.vDataTypeA>() 
                     |> RoList.map(fun item -> item.DataTypeName, item) 
                     |> Map.ofReadOnlyList
            let money = items.["money"]
            money.IsUserDefined |> Claim.isFalse
            money.IsNullable |> Claim.isTrue
            money.SchemaName |> Claim.equal "sys"

        [<Fact>]
        let ``Queried vDataType metadata - Partial column set B``() =
            let items = store.Get<Metadata.vDataTypeB>() 
                     |> RoList.map(fun item -> item.DataTypeName, item) 
                     |> Map.ofReadOnlyList
            let money = items.["money"]
            money.SchemaName |> Claim.equal "sys"
            money.MaxLength |> Claim.equal 8m
            money.Precision |> Claim.equal 19uy
            money.Scale |> Claim.equal 4uy
            money.IsNullable |> Claim.isTrue 
            money.IsUserDefined |> Claim.isFalse      
        
        [<Fact>]
        let ``Bulk inserted data into Table05``() =
            let input = [
                {Table05.Col01 = 1; Col02 = 2uy; Col03 = 3s; Col04=5L}
                {Table05.Col01 = 2; Col02 = 6uy; Col03 = 7s; Col04=8L}
                {Table05.Col01 = 3; Col02 = 9uy; Col03 = 10s; Col04=11L}
            ]
            store |> verifyBulkInsert input (fun x -> x.Col01 :> IComparable)
   

        [<Fact>]
        let ``Bulk inserted data into Table06``() =
            let input = [
                {Table06.Col01 = 1; Col02 = Some 2uy; Col03 = Some 3s; Col04=5L}
                {Table06.Col01 = 2; Col02 = Some 6uy; Col03 = Some 7s; Col04=8L}
                {Table06.Col01 = 3; Col02 = Some 9uy; Col03 = Some 10s; Col04=11L}
            ]
            store |> verifyBulkInsert input (fun x -> x.Col01 :> IComparable)


        [<Fact>]
        let ``Bulk inserted data into Table07``() =
            let input = [
                {Table07.Col01 = Some(1); Col02 = "ABC"; Col03 = "DEF"}
                {Table07.Col01 = Some(2); Col02 = "GHI"; Col03 = "JKL"}
                {Table07.Col01 = Some(3); Col02 = "MNO"; Col03 = "PQR"}
            ]
            store |> verifyBulkInsert input (fun x -> x.Col02 :> IComparable)

        [<Fact>]
        let ``Retrieved paged tabular data``() =
            
            let rowcount = 100
            let col2Values = [for rowid in 1..rowcount -> rowid, (sprintf "Row%i Description" rowid) :> obj] |> Map.ofList
            let createRow(rowid) =                
                [| rowid:> obj; col2Values.[rowid]; (rowid * 5 |> int16) :> obj|]
                            
            let tabularName = DataObjectName("SqlTest", "Table08")
            tabularName |> TruncateTable |> store.ExecuteCommand |> ignore            
            let description = sqlMetadata.DescribeTable(tabularName)
            let rowValues =  [| for i in [1..rowcount] ->i |> createRow |] :> rolist<_>
            let data = TabularData(description :> ITabularDescription, rowValues)
            store.InsertTable(data)

            let q1 = DynamicQueryBuilder(tabularName)
                        .Columns(description.Columns |> Seq.map(fun x -> x.Name) |> Array.ofSeq)
                        .Sort([|AscendingSort(description.Columns.[0].Name)|])
                        .Page(1, 10)
                        .Build() 
                        
            let result = store.GetTable(q1);
            result.RowValues.Count |> Claim.equal 10
            for rowidx in 0..result.RowValues.Count-1 do
                let row = result.RowValues.[rowidx]
                let rowid = row.[0] :?> int
                let actual = row.[1]
                let expect = col2Values.[rowid]
                Claim.equal expect actual
            ()

        let csaw = "Initial Catalog=AdventureWorksLT2012;Data Source=eXaCore03;Integrated Security=SSPI"
        [<Fact>]
        let ``Inferred dynamic query defaults``() =
            let store = SqlDataStore.Get(csaw);
            let query = DynamicQueryBuilder("SalesLT", "Customer").Build()
            let data1 =  query |> store.GetTable;
            ()

            
        [<Fact>]
        let ``Executed parametrized dynamic query``() =
            let info = proxyMetadata.DescribeTableProxy<Table10>()            
            //TODO: 1.Try to use autofixture to populate proxies and insert them into the table
            //2. Writ a TVF that brings back the data and verify the results
            ()

