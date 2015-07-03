namespace IQ.Core.Framework.Test

open System

open IQ.Core.Data.Sql


module ConfigSettingNames =
    let SqlTestDb = "csSqlDataStore"

[<AutoOpen>]
module TestConfiguration =
    
    let private compose() =
        let root = CompositionRoot.build(thisAssembly())
        root |> SqlServices.register
        root.Seal()
        root
                    
    //This is instantiated/cleaned-up once per collection
    type ProjectTestContext() = 
        inherit XUnit.TestContext()
        
        let root = compose()
        let appContext = root.CreateContext()
        let configManager = appContext.Resolve<IConfigurationManager>()
        let cs = ConfigSettingNames.SqlTestDb |> configManager.GetValue
        let store = appContext.Resolve<ISqlDataStore>(ConfigSettingNames.SqlTestDb, cs |> ConnectionString.parse)
                
        //member this.AppContext = appContext
        member this.ConfigurationManager = configManager
        member this.SqlDataStore = store
        member this.AppContext = appContext
        
        interface IDisposable with
            member this.Dispose() =
                appContext.Dispose()
                root.Dispose()

    [<Literal>]
    let TestCollectionName = "Core Framework Tests"

    //This class exists to feed the test infrastructure metadata
    [<AbstractClass; XUnit.TestCollectionMaker(TestCollectionName)>]
    type ProjectCollectionMarker() = 
        inherit XUnit.TestCollection<ProjectTestContext>()

    [<AbstractClass; XUnit.TestContainer(TestCollectionName)>]
    type ProjectTestContainer(context,log) =
        member this.Context : ProjectTestContext = context
        member this.Log : XUnit.ITestLog = log

module Benchmark =
    let record (ctx :ProjectTestContext) (result : BenchmarkResult<_>)  =
        let store = ctx.SqlDataStore.GetContract<ICoreTestFrameworkProcedures>()           
        let summary = result.Summary
        store.pBenchmarkResultPut summary.Name summary.MachineName summary.StartTime summary.EndTime summary.Duration |> ignore

    let inline capture ctx (f:unit->unit) =
        f |> Benchmark.run (Benchmark.deriveDesignator()) |> record ctx


            