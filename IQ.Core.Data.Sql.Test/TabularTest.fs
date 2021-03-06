﻿// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql.Test

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection
open System.Threading
open System.Collections.Generic

open Ploeh.AutoFixture

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Test
open IQ.Core.Data.Sql
open IQ.Core.Framework
open IQ.Core.Data.Contracts

open TestProxies


module Tabular =
    
    let private verifyBulkInsert<'T>(input : 'T list) (sortBy: 'T->IComparable) (store : ISqlDataStore)=
        let result = tableproxy<'T>.DataElement.Name |> TruncateTable |> store.ExecuteCommand 
        store.SelectAll<'T>() |> Claim.seqIsEmpty
        input |> store.Insert
        let output = store.SelectAll<'T>() |> Seq.sortBy sortBy |> List.ofSeq
        output |> Claim.equal input

    type Customer1() =
        member val Id = 0 with get, set
        member val Title = String.Empty with get, set
        member val FirstName = String.Empty with get, set
        member val LastName = String.Empty with get, set
    
    type Tests(ctx, log) = 
        inherit ProjectTestContainer(ctx,log)
        
        let store = ctx.Store
        let mdp = ctx.Store.GetMetadataProvider()
        let proxyMetadata = DataProxyMetadataProvider.get()


        [<Fact>]
        let ``Queried vDataType metadata - Partial column set A``() =
            let items = store.SelectAll<Metadata.vDataTypeA>() 
                     |> Seq.map(fun item -> item.DataTypeName, item) 
                     |> Map.ofSeq
            let money = items.["money"]
            money.IsUserDefined |> Claim.isFalse
            money.IsNullable |> Claim.isTrue
            money.SchemaName |> Claim.equal "sys"

        [<Fact>]
        let ``Queried vDataType metadata - Partial column set B``() =
            let items = store.SelectAll<Metadata.vDataTypeB>() 
                     |> Seq.map(fun item -> item.DataTypeName, item) 
                     |> Map.ofSeq
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


//        [<Fact>]
//        let ``Inferred dynamic query defaults``() =
//            let csaw = "Initial Catalog=AdventureWorksLT2012;Data Source=eXaCore03;Integrated Security=SSPI"
//            let provider = SqlDataStore.getProvider()
//            let store = provider.GetDataStore(csaw)
//            let query = DynamicQueryBuilder("SalesLT", "Customer").Build()
//            let data1 =  query |> store.SelectMatrix
//            ()

            
        [<Fact>]
        let ``Executed parametrized dynamic query``() =
            store.TrunctateTable(objectname<Table0A>)
            
            let fixture = Fixture();
            let input = [for i in 1..500 do
                                let proxy =  fixture.Create<Table0A>()
                                proxy.Col19 <- i
                                yield proxy
                          ] |> List.sortBy(fun x -> x.Col01)
            
            input |> store.Insert

            let query = DynamicQueryBuilder(objectname<Table0A>).Page(1, 50).AscendingSort("Col01").Build()
            let page1Actual : Table0A list = query |> store.Select |> List.ofSeq
            let page1Expect = input |> List.take 50
            let row1Actual = page1Actual.[0]
            let row1Expect = page1Expect.[0]

//            Claim.equal row1Actual row1Expect
//
//            Claim.equal page1Expect page1Actual


                     
            //TODO: 
            //2. Write a TVF that brings back the data and verify the results
            ()

