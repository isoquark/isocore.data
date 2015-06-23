namespace IQ.Core.Framework
open System
open System.Reflection

open Autofac
open Autofac.Builder

[<AutoOpen>]
module CompositionRootVocabulary =
    type ICompositionRoot =
        inherit IDisposable
        abstract Seal:unit->unit
        abstract Resolve<'T> :unit->'T
        abstract Resolve<'T> :string*obj->'T
        abstract RegisterInstance: obj->unit



module CompositionRoot =
    
    let mutable private root = Unchecked.defaultof<ICompositionRoot>
        
    type CompositionRoot(assroot : Assembly) =
        let build() =
            let builder = ContainerBuilder()               
            let assfilter = "IQ."
            assroot |> Assembly.loadReferences (Some(assfilter))
            let assemblies = AppDomain.CurrentDomain.GetAssemblies() 
                           |> Array.filter(fun a -> a.FullName |> Txt.startsWith assfilter)
            builder.RegisterAssemblyTypes(assemblies).AsImplementedInterfaces() |> ignore
            builder
        
        let builder = build()
        let container = ref(Unchecked.defaultof<IContainer>)
       
        let c() = container.Value
        
        
        interface ICompositionRoot with
            member this.RegisterInstance instance = builder.RegisterInstance(instance) |> ignore
            member this.Seal() = container := builder.Build()
            member this.Resolve() = c().Resolve()
            member this.Resolve(paramName, paramValue) = c().Resolve(NamedParameter(paramName,paramValue))
            member this.Dispose() = container.Value.Dispose()

    let build(assroot : Assembly) =
        root <- new CompositionRoot(assroot) :> ICompositionRoot
        root


