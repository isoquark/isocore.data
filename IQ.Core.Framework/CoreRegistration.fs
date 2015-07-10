namespace IQ.Core.Framework

open System
open System.Reflection

open IQ.Core.Data

    
module CoreRegistration =
    /// <summary>
    /// Registers common core services
    /// </summary>
    /// <param name="registry"></param>
    let register (rootAssembly : Assembly) (registry : ICompositionRegistry)  =
        
        registry.RegisterInstance({ConfigurationManagerConfig.Name="AppConfig"} |> ConfigurationManager.get)
        //registry.RegisterInstance(DataProxyMetadataProvider.get())

        //Load all referenced user assemblies assemblies so we can engage 
        //in profitable reflection exercises
        rootAssembly |> Assembly.loadReferences (Some(CoreConfiguration.UserAssemblyPrefix))            
        let assemblyNames = AppDomain.CurrentDomain.GetUserAssemblyNames()
            
        //Register CLR metadata provider
        {Assemblies = assemblyNames} |>ClrMetadataProvider.get |> registry.RegisterInstance
        
        //Register transformer
        registry.RegisterFactory(fun config -> config |> Transformer.get)
            
        //Register time provider
        registry.RegisterInstance<ITimeProvider>(DefaultTimeProvider.get())
        
    
