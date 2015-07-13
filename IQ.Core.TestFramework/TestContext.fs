namespace IQ.Core.TestFramework

open System
open System.Reflection
open System.IO

open IQ.Core.Data
open IQ.Core.Framework



type ITestContext =
    abstract ConfigurationManager : IConfigurationManager
    abstract AppContext : IAppContext
    abstract ExecutionLog : ISqlDataStore
    abstract OutputDirectory : string

[<AbstractClass>]
type TestContext(assemblyRoot : Assembly, register:ICompositionRegistry->unit) = 

    let compose() =        
        let root = CompositionRoot.compose(fun registry ->                        
            registry |> CoreRegistration.register assemblyRoot
            registry |> register
        )
        root

    let root = compose()
    let context = root.CreateContext()
    let configManager = context.Resolve<IConfigurationManager>()
    let outdir = Path.Combine(@"C:\Temp\IQ\Tests\", assemblyRoot.SimpleName)
        
    let getSqlDataStore(): ISqlDataStore =
        ConfigSettingNames.LogConnectionString 
            |> configManager.GetValue 
            |> ConnectionString.parse 
            |> context.Resolve

    let store = getSqlDataStore()
        
    do
        if Directory.Exists(outdir) |> not then Directory.CreateDirectory(outdir) |>ignore

    new (assemblyRoot:Assembly) =
        new TestContext(assemblyRoot, fun _ -> ())

                 
    member this.ConfigurationManager = configManager
    member this.AppContext = context
    member this.SqlDataStore = store
    member this.OutputDirectory = outdir
                                                   
            
    interface IDisposable with
        member this.Dispose() =
            context.Dispose()
            root.Dispose()
        
    interface ITestContext with
        member this.ConfigurationManager = configManager
        member this.AppContext = context
        member this.ExecutionLog = store
        member this.OutputDirectory = outdir


    static member inline GetTempDir() =
        let dir = Path.Combine(@"C:\Temp\IQ\Tests\", thisAssembly().SimpleName)
        if dir |> Directory.Exists |> not then
            dir |> Directory.CreateDirectory |> ignore
        dir

