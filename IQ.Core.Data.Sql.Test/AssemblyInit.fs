namespace IQ.Core.Data.Sql.Test

open IQ.Core.Framework

open IQ.Core.TestFramework

module ConfigSettingNames =
    let SqlTestDb = "csSqlDataStore"

[<AutoOpen>]
module Globals =
    //TODO: See how to use autofac in a unit test environment to avoid this sort of foolishness
    let mutable Root = Unchecked.defaultof<ICompositionRoot>

[<TestAssemblyInit>]
type AssemblyInit() =
    inherit TestAssemblyInitializer()
    
    override this.Initialize() =        
        Root <- CompositionRoot.build(thisAssembly())
        Root.Seal()

    override this.Dispose() =
        Root.Dispose()

