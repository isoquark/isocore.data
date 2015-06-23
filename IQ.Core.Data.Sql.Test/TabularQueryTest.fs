namespace IQ.Core.Data.Sql.Test

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
module ``Tabular Query Execution`` =
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
