namespace IQ.Core.Framework.Test

open System

open IQ.Core.Data.Sql


[<AutoOpen>]
module internal Globals =
    //TODO: See how to use autofac in a unit test environment to avoid this sort of foolishness
    let mutable _ComposedRoot = Unchecked.defaultof<ICompositionRoot>
    let mutable _ClrMetadata = Unchecked.defaultof<IClrMetadataProvider>
    let mutable Context = Unchecked.defaultof<IAppContext>
    let ClrMetadata() = _ClrMetadata

[<TestAssemblyInit>]
type AssemblyInit() =
    inherit TestAssemblyInitializer()
    
    override this.Initialize() =        
        _ComposedRoot <- CompositionRoot.build(thisAssembly())
        _ComposedRoot |> SqlServices.register
        _ComposedRoot.Seal()
        Context <- _ComposedRoot.CreateContext()
        _ClrMetadata <- Context.Resolve<IClrMetadataProvider>()

    override this.Dispose() =
        _ComposedRoot.Dispose()
        Context.Dispose()

module ConfigSettingNames =
    let SqlTestDb = "csSqlDataStore"

module DataStore =
    let private cs = lazy(ConfigSettingNames.SqlTestDb |> Configuration().GetValue)
    let private store = lazy(Context.Resolve<ISqlDataStore>(ConfigSettingNames.SqlTestDb, cs.Value |> ConnectionString.parse))
    let contract<'T when 'T : not struct>() = store.Value.GetContract<'T>()

module Benchmark =
    let record(result : BenchmarkResult<_>) =
        let store = DataStore.contract<ICoreTestFrameworkProcedures>()           
        let summary = result.Summary
        store.pBenchmarkResultPut summary.Name summary.MachineName summary.StartTime summary.EndTime summary.Duration |> ignore

    let inline capture (f:unit->unit) =
        f |> Benchmark.run (Benchmark.deriveDesignator()) |> record



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

module Benchmark2 =
    let record (ctx :ProjectTestContext) (result : BenchmarkResult<_>)  =
        let store = ctx.SqlDataStore.GetContract<ICoreTestFrameworkProcedures>()           
        let summary = result.Summary
        store.pBenchmarkResultPut summary.Name summary.MachineName summary.StartTime summary.EndTime summary.Duration |> ignore

    let inline capture ctx (f:unit->unit) =
        f |> Benchmark.run (Benchmark.deriveDesignator()) |> record ctx
            