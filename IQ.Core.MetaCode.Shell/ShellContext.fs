namespace IQ.Core.MetaCode.Shell

open System

open IQ.Core.Framework
open IQ.Core.Data
open IQ.Core.Data.Sql

type ShellContext()= 
    let registerDependencies(registry : ICompositionRegistry) =
            registry.RegisterInstance<IDataStoreProvider>(SqlDataStore.getProvider())            

    let root = registerDependencies |> CoreRegistration.compose (thisAssembly())
    let context = root.CreateContext()
    let configManager = context.Resolve<IConfigurationManager>()

    member this.ConfigurationManager = configManager

    interface IDisposable with
        member this.Dispose() =
            context.Dispose()
            root.Dispose()
    
    member this.AppContext = context

