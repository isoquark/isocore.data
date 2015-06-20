namespace IQ.Core.Services.Wcf.SampleService

open System.ServiceModel
open DataContract

//module OpContract =
   
    [<ServiceContract>]
    type ISimpleService =
        [<OperationContract>]
        abstract member MyRequestReplyMessage : name: Name -> string

        [<OperationContract(IsOneWay=true)>]
        abstract member MyOneWayMessage : p : (int * bool) -> unit

    //the service implementation
    type SimpleService() =
        interface ISimpleService with
            member x.MyRequestReplyMessage name =
                sprintf "You name appears to be %s" name.FirstName
            member x.MyOneWayMessage pair =
                let (i,t) = pair
                let y = i + 1
                let b = not t 
                b |> ignore
                
            