namespace IQ.Core.Framework.Test

open System
open System.Reflection

open IQ.Core.Data
open IQ.Core.Data.Sql

module ConfigSettingNames =
    let SqlTestDb = "csSqlDataStore"

[<AutoOpen>]
module TestConfiguration =
        
    let private compose() =        
        let root = CompositionRoot.compose(fun registry ->                
            registry |> CoreRegistration.register (thisAssembly())
            registry.RegisterFactory(fun config -> config |> SqlDataStore.access)
        )
        root
                    
    //This is instantiated/cleaned-up once per collection
    type ProjectTestContext() = 
        inherit TestContext()
        
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
    let TestCollectionName = "Core Framework Tests"

    //This class exists to feed the test infrastructure metadata
    [<AbstractClass; TestCollectionMaker(TestCollectionName)>]
    type ProjectCollectionMarker() = 
        inherit TestCollection<ProjectTestContext>()

    [<AbstractClass; TestContainer(TestCollectionName)>]
    type ProjectTestContainer(context,log) =
        member this.Context : ProjectTestContext = context
        member this.Log : ITestLog = log


            