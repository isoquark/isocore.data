﻿// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
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
                                
        //Register time provider
        registry.RegisterInstance<ITimeProvider>(DefaultTimeProvider.get())
        

    let compose (rootAssembly : Assembly) (_register:ICompositionRegistry->unit) =
        let root = CompositionRoot.compose(fun registry ->                        
            registry |> register rootAssembly
            ClrMetadataProvider.getDefault() |> registry.RegisterInstance
            registry |> _register
        )
        root

    let composeWithAction (rootAssembly : Assembly)  (_register: Action<ICompositionRegistry>) =
        compose rootAssembly (fun registry -> _register.Invoke(registry) )
