namespace IQ.Core.Services.Wcf

open System
open System.Reflection;
open System.ServiceModel;
open System.Threading.Tasks;

module Channel =
   
    type internal GenericChannel<'T> (endpointName0 : string option) =
        //TODO: Why does this exist? 
        let mutable endpointName = endpointName0
     
        let msgFct e t = 
            let msgRoot = sprintf "Failed to create channel factory for %sand serviceType %s"
            match e with
                | Some(x) -> msgRoot (sprintf "endpoint %s" x) t
                | _ -> msgRoot "" t

        //TODO: Why is this mutable?
        //The channel factory instance whose lifcycle coincides with that of the containing class instance
        let mutable fact =
                try
                    match endpointName with
                        | Some(x) -> new ChannelFactory<'T>(x)
                        | _ -> new ChannelFactory<'T>()
                 with 
                   | _ -> 
                          raise  (msgFct endpointName typedefof<'T>.Name |> ApplicationException)
       
       //main function to invoke a service: the factory creates a channel and invokes the service operation
        member this.Call(a : 'T -> unit)  =
            let proxy = fact.CreateChannel() //create channel
            a proxy //invoke service operation
            (box proxy :?> IClientChannel).Close()
            (box proxy :?> IClientChannel).Dispose()

        interface IDisposable with
            member this.Dispose() = fact.Close() //disposing the channel factory
