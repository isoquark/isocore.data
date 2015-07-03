namespace IQ.Core.Framework

open System
open System.Reflection

module internal CoreServices =
    
    let register(registry : ICompositionRegistry) =
        //By this point, all assemblies we are interested in should be loaded
        let assemblies = AppDomain.CurrentDomain.GetAssemblies()        
        let assemblyNames = assemblies
                          |> Array.filter(fun a -> a.SimpleName.StartsWith(CoreConfiguration.UserAssemblyPrefix))
                          |> Array.map(fun a -> a.AssemblyName) 
                          |> List.ofArray
        {Assemblies = assemblyNames} |>ClrMetadataProvider.get |> registry.RegisterInstance
        
        registry.RegisterFactory(fun config -> config |> Transformer.get)
        registry.RegisterInstance(SystemClock())
    
        
    
