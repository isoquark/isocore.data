namespace IQ.Core.Framework
open System
open System.Reflection

open Autofac
open Autofac.Builder

[<AutoOpen>]
module CompositionRootVocabulary =
    type ICompositionRegistry =
        abstract RegisterInstance<'T> : 'T->unit when 'T : not struct
        
        /// <summary>
        /// Registers the interfaces implemented by the provided type
        /// </summary>
        abstract RegisterInterfaces<'T> : unit->unit

    type ICompositionScope =
        inherit IDisposable
        abstract Resolve<'T> :unit->'T
        abstract Resolve<'T> : key :string * value : obj->'T

    type ICompositionRoot =        
        inherit IDisposable
        inherit ICompositionRegistry
        abstract Seal:unit->unit
        abstract Resolve<'T> :unit->'T
        abstract Resolve<'T> :string*obj->'T
        abstract CreateScope:unit -> ICompositionScope
        



module CompositionRoot =
    
    let mutable private root = Unchecked.defaultof<ICompositionRoot>
    let private _ConfigurationManager = lazy(ConfigurationManager.get()) 
    let internal ConfigurationManager() = _ConfigurationManager.Value

        
    let private registerInstance<'T when 'T : not struct>  (instance :'T) (builder : ContainerBuilder) =
        builder.RegisterInstance<'T>(instance).SingleInstance() |> ignore

    let private registerInterfaces<'T>(builder : ContainerBuilder) =
        builder.RegisterType<'T>().AsImplementedInterfaces() |> ignore
                                
    let private registerCore (asscore : Assembly) (builder : ContainerBuilder) =        
        builder |> registerInstance (ConfigurationManager())
        
        let regtype = asscore.GetType(CoreConfiguration.CoreServicesType)
        let regmethod = regtype.GetMethod(CoreConfiguration.CoreRervicesMethod, BindingFlags.Static ||| BindingFlags.Public ||| BindingFlags.NonPublic)
        let registry = {new ICompositionRegistry with
                            member this.RegisterInstance (instance : 'T) = builder |> registerInstance<'T> instance
                            member this.RegisterInterfaces<'T>() = builder |> registerInterfaces<'T>
                        }
        regmethod.Invoke(null, [|registry|]) |> ignore        
    
    type private CompositionScope(c : IContainer) =
        let scope = c.BeginLifetimeScope()

        interface ICompositionScope with
            member this.Resolve() = c.Resolve()
            member this.Dispose() = scope.Dispose()
            member this.Resolve(key,value) = scope.Resolve(NamedParameter(key,value))
    
        
    
    type private CompositionRoot(assroot : Assembly) =
        let build() =
            let builder = ContainerBuilder()               
            let asscore = Assembly.GetExecutingAssembly()            
            assroot |> Assembly.loadReferences (Some(CoreConfiguration.UserAssemblyPrefix))
            builder |> registerCore asscore
            builder            
        
        let builder = build()
        let container = ref(Unchecked.defaultof<IContainer>)
        

        let c() = container.Value
                
        interface ICompositionRoot with
            member this.RegisterInstance instance = builder |> registerInstance instance
            member this.RegisterInterfaces<'T>() = builder |> registerInterfaces<'T>
            member this.Seal() = container := builder.Build()
            member this.Resolve() = c().Resolve()
            member this.Resolve(paramName, paramValue) = c().Resolve(NamedParameter(paramName,paramValue))
            member this.Dispose() = container.Value.Dispose()
            member this.CreateScope() = new CompositionScope(c()) :> ICompositionScope

    let build(assroot : Assembly) =
        root <- new CompositionRoot(assroot) :> ICompositionRoot
        root

    let internal resolve<'T when 'T : not struct>() =
        root.Resolve<'T>()
[<AutoOpen>]
module CompositionRootExtensions = 
    let Configuration() = CompositionRoot.ConfigurationManager()    

