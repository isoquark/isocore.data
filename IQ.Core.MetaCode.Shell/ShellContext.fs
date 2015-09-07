namespace IQ.Core.MetaCode.Shell

open System

open IQ.Core.Framework
open IQ.Core.Data
open IQ.Core.Data.Sql

type ShellContext()= 
    let registerDependencies(registry : ICompositionRegistry) =
            registry.RegisterFactory(fun (config : SqlDataStoreConfig) -> config |> TypedSqlDataStore.Get)

    let root = registerDependencies |> CoreRegistration.compose (thisAssembly())
    let context = root.CreateContext()
    let configManager = context.Resolve<IConfigurationManager>()
    let cs = "csSqlDataStore" |> configManager.GetValue  

    member this.ConfigurationManager = configManager

    interface IDisposable with
        member this.Dispose() =
            context.Dispose()
            root.Dispose()
    
    member this.AppContext = context

