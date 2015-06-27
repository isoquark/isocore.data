namespace IQ.Core.Services.Wcf

open System
open System.Reflection;
open System.ServiceModel;
open System.Threading.Tasks;

module Channel =
   
    type internal GenericChannel<'T> (endpointName : string option) =
         
        let msgFct e t = 
            let msgRoot = sprintf "Failed to create channel factory for %sand serviceType %s"
            match e with
                | Some(x) -> msgRoot (sprintf "endpoint %s" x) t
                | _ -> msgRoot "" t

        //The channel factory instance whose lifcycle coincides with that of the containing class instance
        let fact =
                try
                    match endpointName with
                        | Some(x) -> new ChannelFactory<'T>(x)
                        | _ -> new ChannelFactory<'T>()
                 with 
                   | _ -> 
                          raise  (msgFct endpointName typedefof<'T>.Name |> ApplicationException)
       
        //call when no result is expected (as action)
        member this.Call(a : 'T -> unit)  =
            let proxy = fact.CreateChannel() //create channel
            a proxy //invoke service operation
            (box proxy :?> IClientChannel).Close()
            (box proxy :?> IClientChannel).Dispose()

        //call when expecting a result (as function)
        member this.Call (a : 'T -> 'S)  =
            let proxy = fact.CreateChannel() //create channel
            let result = a proxy //invoke service operation
            (box proxy :?> IClientChannel).Close()
            (box proxy :?> IClientChannel).Dispose()
            result

        interface IDisposable with
            member this.Dispose() = fact.Close() //disposing the channel factory
