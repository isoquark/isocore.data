namespace IQ.Core.Services.Wcf

open System
open Helpers
open ChannelAdapter


type IChannelManager =
        abstract Invoke<'T> : action : ('T->unit) -> unit
        abstract Invoke<'T> : action : ('T->unit) * endpoint : string option -> unit
        abstract InvokeFun<'T,'TResult> : func : ('T->'TResult) -> 'TResult
        abstract InvokeFun<'T,'TResult> : func : ('T->'TResult) * endpoint : string option -> 'TResult

module ServiceFacade = 

    //the MAIN SINGLETON type which is accessible from the outside world
    type internal ChannelManager private () =
        static let instance = lazy (
                                ReadAllClientEndpoints 
                                new ChannelManager()
                              )
                        
        static member internal Instance = instance.Value

        member private x.Invoke<'T>(action : 'T -> unit) = 
            let factory = GetChannel<'T> None
            factory.Call action

        member private x.InvokeFun<'T, 'TResult>(func : 'T -> 'TResult) =
            let factory = GetChannel<'T> None
            factory.Call func

        member private x.Invoke<'T> (action : 'T -> unit, endpointName) = 
            let factory = GetChannel<'T> endpointName
            factory.Call action

         member private x.InvokeFun<'T, 'TResult>(func : 'T -> 'TResult, endpointName) =
            let factory = GetChannel<'T> endpointName
            factory.Call func

        interface IChannelManager with
            member this.Invoke action = action |> this.Invoke
            member this.Invoke  (action, endpoint) = this.Invoke(action, endpoint)
            member this.InvokeFun func = func |> this.InvokeFun
            member this.InvokeFun (func, endpoint) = this.InvokeFun(func, endpoint)

        interface IDisposable with
            member x.Dispose() = disposeEndpointMap()
        //end acces point from outside

    let create() =
        ChannelManager.Instance :> IChannelManager


