namespace IQ.Core.Framework.Test

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data.Sql


[<AutoOpen>]
module Globals =
    //TODO: See how to use autofac in a unit test environment to avoid this sort of foolishness
    let mutable Root = Unchecked.defaultof<ICompositionRoot>
    let mutable Configuration = Unchecked.defaultof<IConfigurationManager>

[<TestAssemblyInit>]
type AssemblyInit() =
    inherit TestAssemblyInitializer()
    
    override this.Initialize() =        
        Root <- CompositionRoot.build(thisAssembly())
        Root.Seal()
        Configuration <- Root.Resolve<IConfigurationManager>()

    override this.Dispose() =
        Root.Dispose()

module ConfigSettingNames =
    let SqlTestDb = "csSqlDataStore"

module DataStore =
    let private cs = lazy(ConfigSettingNames.SqlTestDb |> Configuration.GetValue)
    let private store = lazy(Root.Resolve<ISqlDataStore>(ConfigSettingNames.SqlTestDb, cs.Value |> ConnectionString.parse))
    let contract<'T when 'T : not struct>() = store.Value.GetContract<'T>()

module Benchmark =
    let record(result : BenchmarkResult<_>) =
        let store = DataStore.contract<ICoreTestFrameworkProcedures>()           
        let summary = result.Summary
        store.pBenchmarkResultPut summary.Name summary.MachineName summary.StartTime summary.EndTime summary.Duration |> ignore

    let inline capture (f:unit->unit) =
        f |> Benchmark.run (Benchmark.deriveName()) |> record
