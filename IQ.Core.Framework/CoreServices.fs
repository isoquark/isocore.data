namespace IQ.Core.Framework

open System
open System.Reflection

module internal CoreServices =
    
        

    let register(registry : ICompositionRegistry) =
        //By this point, all assemblies we are interested in should be loaded
        let assemblyNames = AppDomain.CurrentDomain.GetUserAssemblyNames()
        {Assemblies = assemblyNames} |>ClrMetadataProvider.get |> registry.RegisterInstance
        
        //registry.RegisterFactory(fun config -> config |> ClrMetadataProvider.get)
        registry.RegisterFactory(fun config -> config |> Transformer.get)
        registry.RegisterInstance<ITimeProvider>(DefaultTimeProvider.get())
    
        
    
