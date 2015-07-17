// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System
open System.Reflection

open IQ.Core.Data

    
module CoreRegistration =
    /// <summary>
    /// Registers common core services
    /// </summary>
    /// <param name="registry"></param>
    let private register (rootAssembly : Assembly) (registry : ICompositionRegistry)  =
        
        registry.RegisterInstance({ConfigurationManagerConfig.Name="AppConfig"} |> ConfigurationManager.get)

        //Load all referenced user assemblies assemblies so we can engage 
        //in profitable reflection exercises
        rootAssembly |> Assembly.loadReferences (Some(CoreConfiguration.UserAssemblyPrefix))            
                    
        //Register transformer
        registry.RegisterFactory(fun config -> config |> Transformer.get)
            
        //Register time provider
        registry.RegisterInstance<ITimeProvider>(DefaultTimeProvider.get())
        

    let compose (rootAssembly : Assembly) (_register:ICompositionRegistry->unit) =
        let root = CompositionRoot.compose(fun registry ->                        
            registry |> register rootAssembly
            registry |> _register
        )
        root
            
