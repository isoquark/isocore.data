namespace IQ.Core.TestFramework


open System
open System.Reflection
open System.IO

open IQ.Core.Data
open IQ.Core.Framework



type ITestContext =
    abstract ConfigurationManager : IConfigurationManager
    abstract AppContext : IAppContext
    abstract OutputDirectory : string
    abstract LogConnectionString : string

[<AbstractClass>]
type TestContext(root : ICompositionRoot) as this =
        
    let assemblyRoot = this.GetType().Assembly
    let context = root.CreateContext()
    let configManager = context.Resolve<IConfigurationManager>()
    let outdir = Path.Combine(@"C:\Temp\IQ\Tests\", assemblyRoot.GetName().Name)
    let cs = "csSqlDataStore" |> configManager.GetValue         
        
    do
        if Directory.Exists(outdir) |> not then Directory.CreateDirectory(outdir) |>ignore

                 
    member this.ConfigurationManager = configManager
    member this.AppContext = context
    member this.OutputDirectory = outdir
                                                   
            
    interface IDisposable with
        member this.Dispose() =
            context.Dispose()
        
    interface ITestContext with
        member this.ConfigurationManager = configManager
        member this.AppContext = context
        member this.OutputDirectory = outdir
        member this.LogConnectionString = cs



