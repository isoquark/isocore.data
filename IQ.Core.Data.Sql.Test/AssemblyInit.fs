namespace IQ.Core.Data.Sql.Test

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data.Sql

module ConfigSettingNames =
    let SqlTestDb = "csSqlDataStore"

[<AutoOpen>]
module Globals =
    //TODO: See how to use autofac in a unit test environment to avoid this sort of foolishness
    let mutable _Root = Unchecked.defaultof<ICompositionRoot>
    let mutable Context = Unchecked.defaultof<IAppContext>    

[<TestAssemblyInit>]
type AssemblyInit() =
    inherit TestAssemblyInitializer()
    
    override this.Initialize() =        
        _Root <- CompositionRoot.build(thisAssembly())
        _Root |> SqlServices.register
        _Root.Seal()
        Context <- _Root.CreateContext()

    override this.Dispose() =
        Context.Dispose()
        _Root.Dispose()

