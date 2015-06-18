namespace IQ.Core.Data.Sql.Test

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Sql
open IQ.Core.Framework

[<TestContainer; DataStoreTrait>]
module ``Procedure Execution`` =
    let private config = Root.Resolve<IConfigurationManager>()
    let private cs = ConfigSettingNames.SqlTestDb |> config.GetValue 
    let store = Root.Resolve<ISqlDataStore>(ConfigSettingNames.SqlTestDb, cs |> ConnectionString.parse)
        
    [<Test>]
    let ``Executed [SqlTest].[pTable02Insert] procedure - Direct``() =
        let procName = thisMethod() |> SqlTestCaseMethod.getDbObjectName
        let procProxy = procproxies<SqlTestProxies.ISqlTestProcs> |> List.find(fun x -> x.DataElement.Name = procName)
        let proc = procProxy.DataElement
        let inputValues = ValueIndex.fromNamedItems [("col01", DBNull.Value :> obj); ("col02", DateTime(2015, 5, 16) :> obj); ("col03", 507L :> obj);]
        let outputvalues = proc |> Procedure.execute cs inputValues
        let col01Value = outputvalues.["col01"] :?> int
        Claim.greater col01Value 1

    [<Test>]
    let ``Executed [SqlTest].[pTable02Insert] procedure - Contract``() =
        let procs = store.GetContract<ISqlTestProcs>()
        let result = procs.pTable02Insert (DateTime(2015, 5, 16)) (507L)
        Claim.greater result 1


    [<Test>]
    let ``Executed [SqlTest].[pTable03Insert] procedure - Direct``() =
        let procName = thisMethod() |> SqlTestCaseMethod.getDbObjectName
        let procProxy = procproxies<SqlTestProxies.ISqlTestProcs> |> List.find(fun x -> x.DataElement.Name = procName)
        let proc = procProxy.DataElement
        let inputValues = ValueIndex.fromNamedItems [("Col01", 5uy :> obj); ("Col02", 10s :> obj); ("Col03", 15 :> obj); ("Col04", 20L :> obj)]
        let outputValues = proc |> Procedure.execute cs inputValues
        ()
    
    [<Test>]
    let ``Executed [SqlTest].[pTable03Insert] procedure - Contract``() =
        let procs = store.GetContract<ISqlTestProcs>()
        let result = procs.pTable03Insert 5uy 10s 15 20L
        0 |> Claim.greater result
        ()
    