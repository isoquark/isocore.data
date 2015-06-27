namespace IQ.Core.Services.Wcf

open System
open ServiceFacade
open ChannelAdapter
open Helpers

type IChannelManagerCS =
        abstract Invoke<'T> : action : Action<'T> -> unit
        abstract Invoke<'T,'TResult> : func : Func<'T,'TResult> -> 'TResult
        abstract Invoke<'T> : action : Action<'T> * endpoint : string -> unit
        abstract Invoke<'T,'TResult> : func : Func<'T,'TResult> * endpoint : string -> 'TResult

module ServiceFacadeCSharp =
    type private ChannelManager private () = 
        static let instance = lazy (
                                ReadAllClientEndpoints 
                                new ChannelManager()
                              )
                        
        static member internal Instance = instance.Value

        member x.Invoke<'T>(action : Action<'T>) =
            let factory = GetChannel<'T> None
            factory.Call (getFunFromAction action)

        member x.InvokeFun<'T,'TResult>(func : Func<'T,'TResult>) : 'TResult =
            let factory = GetChannel<'T> None
            factory.Call (getFunFromFunc func)

        member x.Invoke<'T>(action : Action<'T>, endpointName) =
            let factory = GetChannel<'T> (Some endpointName)
            factory.Call (getFunFromAction action)

        member x.InvokeFun<'T,'TResult>(func : Func<'T,'TResult>, endpointName) : 'TResult =
            let factory = GetChannel<'T> (Some endpointName)
            factory.Call (getFunFromFunc func)

        interface IChannelManagerCS with
            member this.Invoke action = action |> this.Invoke
            member this.Invoke func = func |> this.InvokeFun
            member this.Invoke  (action, endpoint) = this.Invoke(action, endpoint)
            member this.Invoke (func, endpoint) = this.InvokeFun (func, endpoint)

        interface IDisposable with
            member x.Dispose() = disposeEndpointMap()

    let create() = ChannelManager.Instance :> IChannelManagerCS

