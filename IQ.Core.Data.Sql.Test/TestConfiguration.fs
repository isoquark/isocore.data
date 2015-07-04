﻿namespace IQ.Core.Data.Sql.Test

open System
open System.Reflection

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data.Sql



module ConfigSettingNames =
    let SqlTestDb = "csSqlDataStore"

[<AutoOpen>]
module TestConfiguration =
    


    let private compose() =        
        let root = CompositionRoot.compose(fun registry ->        
        
            registry.RegisterInstance({ConfigurationManagerConfig.Name="AppConfig"} |> ConfigurationManager.get)
        
            //Load all referenced user assemblies assemblies so we can engage 
            //in profitable reflection exercises
            thisAssembly() |> Assembly.loadReferences (Some(CoreConfiguration.UserAssemblyPrefix))            
            let assemblyNames = AppDomain.CurrentDomain.GetUserAssemblyNames()
            
            //Register CLR metadata provider
            {Assemblies = assemblyNames} |>ClrMetadataProvider.get |> registry.RegisterInstance
        
            //Register transformer
            registry.RegisterFactory(fun config -> config |> Transformer.get)
            
            //Register time provider
            registry.RegisterInstance<ITimeProvider>(DefaultTimeProvider.get())

            //registry |> SqlServices.register
            registry.RegisterFactory(fun config -> config |> SqlDataStore.access)

        )
        root

                    
    //This is instantiated/cleaned-up once per collection
    type ProjectTestContext() = 
        inherit XUnit.TestContext()
        
        let root = compose()
        let appContext = root.CreateContext()
        let configManager = appContext.Resolve<IConfigurationManager>()
        let cs = ConfigSettingNames.SqlTestDb |> configManager.GetValue |> ConnectionString.parse        
        let store : ISqlDataStore = cs |> appContext.Resolve
                
        member this.ConfigurationManager = configManager
        member this.SqlDataStore = store
        member this.AppContext = appContext
        
        interface IDisposable with
            member this.Dispose() =
                appContext.Dispose()
                root.Dispose()

    [<Literal>]
    let TestCollectionName = "Core Sql Tests"

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