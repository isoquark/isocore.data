namespace IQ.Core.Framework.Test

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


//module LocalTestVocabulary = 
//    
//    open XUnit
//
//    //This is instantiated/cleaned-up once per collection
//    type MyTestContext() = 
//        inherit TestContext()
//        member this.Text = "Hello"
//        
//        interface IDisposable with
//            member this.Dispose() = ()
//
//    [<Literal>]
//    let TestCollectionName = "My Test Collection"
//
//    //This class exists to feed the test infrastructure metadata
//    [<AbstractClass; TestCollectionMaker(TestCollectionName)>]
//    type MyTestCollectionMarker() = 
//        inherit TestCollection<MyTestContext>()
//
//    //Every subclass will automatically belong to the collection
//    [<AbstractClass; TestContainer(TestCollectionName)>]
//    type MyTestContainer(context, log) =
//        inherit TestContainer<MyTestContext>(log)
//        member this.Context : MyTestContext = context