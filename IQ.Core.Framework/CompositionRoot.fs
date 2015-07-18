// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework
open System
open System.Reflection
open System.Collections.Generic


open Autofac
open Autofac.Builder

    
        

type RootNotInitializedException() =
    inherit Exception()


module internal CompositionRoot =
    

    let private registerFactory<'TConfig,'I when 'I : not struct> (f:ServiceFactory<'TConfig,'I>) (builder : ContainerBuilder) =
        let create (c : IComponentContext) (p : IEnumerable<Core.Parameter>) =                        
            let config = p.Named<'TConfig>("config")
            config |> f
        builder.Register<'I>(create).As<'I>() |> ignore                    
        
    let private registerInstance<'T when 'T : not struct>  (instance :'T) (builder : ContainerBuilder) =
        builder.RegisterInstance<'T>(instance).SingleInstance() |> ignore

    let private registerInterfaces<'T>(builder : ContainerBuilder) =
        builder.RegisterType<'T>().AsImplementedInterfaces() |> ignore                                
    
    type private AppContext(c : IContainer) =
        let scope = c.BeginLifetimeScope()

        interface IAppContext with
            member this.Resolve() = c.Resolve()
            member this.Dispose() = scope.Dispose()
            member this.Resolve(key,value) = scope.Resolve(NamedParameter(key,value))
            member this.Resolve<'C,'I>(c : 'C) = scope.Resolve<'I>(new NamedParameter("config", c))

    type private CompositionRoot() = 
        let builder = ContainerBuilder()
        let mutable container = ref(Unchecked.defaultof<IContainer>)        
        interface ICompositionRoot with
            member this.RegisterInstance instance = builder |> registerInstance instance
            member this.RegisterInterfaces<'T>() = builder |> registerInterfaces<'T>
            member this.RegisterFactory<'TConfig,'I when 'I : not struct> f = builder |> registerFactory<'TConfig,'I> f
            member this.Seal() = container := builder.Build()
            member this.Dispose() = container.Value.Dispose()
            member this.CreateContext() = new AppContext(container.Value) :> IAppContext
        
    let compose(register:ICompositionRegistry -> unit) =               
        let root = (new CompositionRoot()) :> ICompositionRoot
        root |> register
        root.Seal()
        root                        
                            
