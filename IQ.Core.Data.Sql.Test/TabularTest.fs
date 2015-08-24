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
open IQ.Core.Data.Test.ProxyTestCases
open IQ.Core.Data.Sql
open IQ.Core.Framework





module Tabular =
    
    let private verifyBulkInsert<'T>(input : 'T list) (sortBy: 'T->IComparable) (store : ISqlProxyDataStore)=
        let count = tabularproxy<'T>.DataElement.Name |> TruncateTable |> store.ExecuteCommand 
        store.Get<'T>() |> Claim.seqIsEmpty
        input |> store.Insert
        let output = store.Get<'T>() |> RoList.sortBy sortBy |> RoList.toList
        output |> Claim.equal input

    type Tests(ctx, log) = 
        inherit ProjectTestContainer(ctx,log)
        
        let store = ctx.Store

        [<FactAttribute>]
        let ``Queried vDataType metadata - Partial column set A``() =
            let items = store.Get<Metadata.vDataTypeA>() 
                     |> RoList.map(fun item -> item.DataTypeName, item) 
                     |> Map.ofReadOnlyList
            let money = items.["money"]
            money.IsUserDefined |> Claim.isFalse
            money.IsNullable |> Claim.isTrue
            money.SchemaName |> Claim.equal "sys"

        [<FactAttribute>]
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
        
        [<FactAttribute>]
        let ``Bulk inserted data into Table05``() =
            let input = [
                {Table05.Col01 = 1; Col02 = 2uy; Col03 = 3s; Col04=5L}
                {Table05.Col01 = 2; Col02 = 6uy; Col03 = 7s; Col04=8L}
                {Table05.Col01 = 3; Col02 = 9uy; Col03 = 10s; Col04=11L}
            ]
            store |> verifyBulkInsert input (fun x -> x.Col01 :> IComparable)
   

        [<FactAttribute>]
        let ``Bulk inserted data into Table06``() =
            let input = [
                {Table06.Col01 = 1; Col02 = Some 2uy; Col03 = Some 3s; Col04=5L}
                {Table06.Col01 = 2; Col02 = Some 6uy; Col03 = Some 7s; Col04=8L}
                {Table06.Col01 = 3; Col02 = Some 9uy; Col03 = Some 10s; Col04=11L}
            ]
            store |> verifyBulkInsert input (fun x -> x.Col01 :> IComparable)


        [<FactAttribute>]
        let ``Bulk inserted data into Table07``() =
            let input = [
                {Table07.Col01 = Some(1); Col02 = "ABC"; Col03 = "DEF"}
                {Table07.Col01 = Some(2); Col02 = "GHI"; Col03 = "JKL"}
                {Table07.Col01 = Some(3); Col02 = "MNO"; Col03 = "PQR"}
            ]
            store |> verifyBulkInsert input (fun x -> x.Col02 :> IComparable)

        [<Fact>]
        let ``Retrieved paged tabular data``() =
            
            let rowidx = ref(0)
            
            let createNextRow() =                
                let nextid() =
                    Interlocked.Increment(rowidx)

                let id = nextid()
                [| id:> obj; (sprintf "Row%i Description" id) :> obj; (id * 5 |> int16) :> obj|]
                            
            let colnames = [|"Col01"; "Col02"; "Col03"|] :> rolist<_>
            let tabularName = DataObjectName("SqlTest", "Table08")
            tabularName |> TruncateTable |> store.ExecuteCommand |> ignore
            let builder = TabularDescriptionBuilder(tabularName)
            let description = 
                builder.AddColumn(colnames.[0], "Int32")
                       .AddColumn(colnames.[1], "UnicodeTextVariable(50)")
                       .AddColumn(colnames.[2], "Int16")
                       .Finish()
            
            let rowValues = 
                    [|
                        for i in [1..100] ->
                            createNextRow()
                    |] :> rolist<_>
            let data = {                                
                new ITabularData with
                    member this.Description = description
                    member this.RowValues =  rowValues
            }
            store.Insert(data)

            let sort = [|AscendingSort(colnames.[0])|] :> rolist<_>
            let page = TabularPageInfo(Some(1), Some(10))
            let q = TabularDataQuery(tabularName, colnames, ReadOnlyList.Empty(), sort, Some(page)) |> TabularQuery
            let result = store.GetTabular(q);
            ()
            
            

