﻿namespace IQ.Core.Data.Sql.Test

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Test.ProxyTestCases
open IQ.Core.Data.Sql
open IQ.Core.Framework


[<TestContainer; DataStoreTrait>]
module ``Tabular Query and Manipulation`` =
    let private config = Root.Resolve<IConfigurationManager>()
    let private cs = ConfigSettingNames.SqlTestDb |> config.GetValue 
    let store = Root.Resolve<ISqlDataStore>(ConfigSettingNames.SqlTestDb, cs |> ConnectionString.parse)

    [<Test>]
    let ``Queried [Metadata].[vDataType] - Partial column set A``() =
        let items = store.Get<Metadata.vDataTypeA>() 
                 |> List.map(fun item -> item.DataTypeName, item) 
                 |> Map.ofList
        let money = items.["money"]
        money.IsUserDefined |> Claim.isFalse
        money.Nullable |> Claim.isTrue
        money.SchemaName |> Claim.equal "sys"

    [<Test>]
    let ``Queried [Metadata].[vDataType] - Partial column set B``() =
        let items = store.Get<Metadata.vDataTypeB>() 
                 |> List.map(fun item -> item.DataTypeName, item) 
                 |> Map.ofList
        let money = items.["money"]
        money.SchemaName |> Claim.equal "sys"
        money.MaxLength |> Claim.equal 8m
        money.Precision |> Claim.equal 19uy
        money.Scale |> Claim.equal 4uy
        money.Nullable |> Claim.isTrue 
        money.IsUserDefined |> Claim.isFalse      

    [<Test>]
    let ``Bulk inserted data into [SqlTest].[Table05]``() =
        let input = [
            {Table05.Col01 = 1; Col02 = 2uy; Col03 = 3s; Col04=5L}
            {Table05.Col01 = 2; Col02 = 6uy; Col03 = 7s; Col04=8L}
            {Table05.Col01 = 3; Col02 = 9uy; Col03 = 10s; Col04=11L}
        ]
        tabularproxy<Table05>.DataElement.Name |> TruncateTable |> store.ExecuteCommand
        store.Get<Table05>() |> Claim.seqIsEmpty
        input |> store.BulkInsert
        let output = store.Get<Table05>() |> List.sortBy(fun x -> x.Col01)
        output |> Claim.equal input

    [<Test>]
    let ``Bulk inserted data into [SqlTest].[Table06]``() =
        let input = [
            {Table06.Col01 = 1; Col02 = Some 2uy; Col03 = Some 3s; Col04=5L}
            {Table06.Col01 = 2; Col02 = Some 6uy; Col03 = Some 7s; Col04=8L}
            {Table06.Col01 = 3; Col02 = Some 9uy; Col03 = Some 10s; Col04=11L}
        ]
        tabularproxy<Table06>.DataElement.Name |> TruncateTable |> store.ExecuteCommand
        store.Get<Table06>() |> Claim.seqIsEmpty
        input |> store.BulkInsert
        let output = store.Get<Table06>() |> List.sortBy(fun x -> x.Col01)
        output |> Claim.equal input