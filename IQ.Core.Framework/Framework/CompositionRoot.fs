namespace IQ.Core.Framework
open System
open System.Reflection
open System.Collections.Generic


open Autofac
open Autofac.Builder

    
        

type RootNotInitializedException() =
    inherit Exception()


module CompositionRoot =
    
    let mutable private root = Unchecked.defaultof<ICompositionRoot>
    let mutable private container = ref(Unchecked.defaultof<IContainer>)
    let private _ConfigurationManager = lazy(ConfigurationManager.get({Name = ""})) 
    let internal ConfigurationManager() = _ConfigurationManager.Value

    let private registerFactory<'TConfig,'I when 'I : not struct> (f:ServiceFactory<'TConfig,'I>) (builder : ContainerBuilder) =
        let create (c : IComponentContext) (p : IEnumerable<Core.Parameter>) =                        
            let config = p.Named<'TConfig>("config")
            config |> f
        builder.Register<'I>(create).As<'I>() |> ignore                    
        
    let private registerInstance<'T when 'T : not struct>  (instance :'T) (builder : ContainerBuilder) =
        builder.RegisterInstance<'T>(instance).SingleInstance() |> ignore

    let private registerInterfaces<'T>(builder : ContainerBuilder) =
        builder.RegisterType<'T>().AsImplementedInterfaces() |> ignore
                                
    let private registerCore (asscore : Assembly) (builder : ContainerBuilder) =        
        builder |> registerInstance (ConfigurationManager())
        
        let regtype = asscore.GetType(CoreConfiguration.CoreServicesType)
        let regmethod = regtype.GetMethod(CoreConfiguration.CoreRervicesMethod, BindingFlags.Static ||| BindingFlags.Public ||| BindingFlags.NonPublic)
        let registry = {new ICompositionRegistry with
                            member this.RegisterInstance (instance : 'V) = builder |> registerInstance<'V> instance
                            member this.RegisterInterfaces<'T>() = builder |> registerInterfaces<'T>
                            member this.RegisterFactory<'TConfig,'I when 'I : not struct> f = builder |> registerFactory<'TConfig, 'I> f 
                        }
        regmethod.Invoke(null, [|registry|]) |> ignore        
    
    type private AppContext(c : IContainer) =
        let scope = c.BeginLifetimeScope()

        interface IAppContext with
            member this.Resolve() = c.Resolve()
            member this.Dispose() = scope.Dispose()
            member this.Resolve(key,value) = scope.Resolve(NamedParameter(key,value))
            member this.Resolve<'C,'I>(c : 'C) = scope.Resolve<'I>(new NamedParameter("config", c))
    
        
    
    type private CompositionRoot(assroot : Assembly) =
        let build() =
            let builder = ContainerBuilder()               
            let asscore = Assembly.GetExecutingAssembly()            
            assroot |> Assembly.loadReferences (Some(CoreConfiguration.UserAssemblyPrefix))
            builder |> registerCore asscore
            builder            
        
        let builder = build()

        let c() = container.Value
                        
        interface ICompositionRoot with
            member this.RegisterInstance instance = builder |> registerInstance instance
            member this.RegisterInterfaces<'T>() = builder |> registerInterfaces<'T>
            member this.RegisterFactory<'TConfig,'I when 'I : not struct> f = builder |> registerFactory<'TConfig,'I> f
            member this.Seal() = container := builder.Build()
            member this.Dispose() = container.Value.Dispose()
            member this.CreateContext() = new AppContext(c()) :> IAppContext

    type private CompositionRoot2() = 
        let builder = ContainerBuilder()
        
        interface ICompositionRoot with
            member this.RegisterInstance instance = builder |> registerInstance instance
            member this.RegisterInterfaces<'T>() = builder |> registerInterfaces<'T>
            member this.RegisterFactory<'TConfig,'I when 'I : not struct> f = builder |> registerFactory<'TConfig,'I> f
            member this.Seal() = container := builder.Build()
            member this.Dispose() = container.Value.Dispose()
            member this.CreateContext() = new AppContext(container.Value) :> IAppContext
        

    let build(assroot : Assembly) =
        root <- new CompositionRoot(assroot) :> ICompositionRoot
        root

    let compose(register:ICompositionRegistry -> unit) =               
        let _root = (new CompositionRoot2()) :> ICompositionRoot
        _root |> register
        _root.Seal()
        root <- _root
        _root
                        
                
    let internal resolve<'T when 'T : not struct>() =
        container.Value.Resolve<'T>()

    let createContext() =
        if container.Value = null then
            RootNotInitializedException() |> raise
        new AppContext(container.Value) :> IAppContext
            
        
       
     

[<AutoOpen>]
module CompositionRootExtensions = 
    let Configuration() = CompositionRoot.ConfigurationManager()    

