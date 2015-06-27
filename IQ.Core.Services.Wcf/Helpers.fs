namespace IQ.Core.Services.Wcf

open System
open System.IO
open System.Linq
open System.Configuration
open System.Reflection
open System.ServiceModel.Configuration

module Helpers =

    let getFunFromAction (a : Action<'T>) : 'T -> unit =
        Core.FuncConvert.ToFSharpFunc a

    let getFunFromFunc (a : Func<'T, 'TResult>) : 'T -> 'TResult =
        fun x -> a.Invoke(x)

    let getOptionFromString s =
            match String.IsNullOrEmpty(s) with
                    | true -> None
                    | _ -> Some s

    let internal bucketSort s =
        let grp = s |> Seq.groupBy (fun x -> x)
        seq { for b in grp -> (fst b, (snd b) |> Seq.length ) } |> Seq.sortBy (fun (x,y) -> x)


    // Reads the client endpoints from the configuration file and 
    // returns a collection of such channel endpoints (ChannelEndpointElementCollection)
    // Throws an application exception if no client endpoints are found or if the configuration is missing. 
    let internal getClientEndpoints () =      
        try
            let clientSection = 
                ConfigurationManager.GetSection("system.serviceModel/client") :?> ClientSection
            clientSection.ElementInformation.Properties.[""].Value :?> ChannelEndpointElementCollection
          with
            | _ -> raise ( "Missing or invalid WCF client endpoint configurtion" |> ApplicationException)

    //same as above for service endpoints
    let internal getServiceEndpoints () =      
        try
            let servicesSection = 
                ConfigurationManager.GetSection("system.serviceModel/services") :?> ServicesSection
            servicesSection.ElementInformation.Properties.[""].Value :?> ServiceElementCollection
          with
            | _ -> raise ( "Missing or invalid WCF service endpoint configurtion" |> ApplicationException)

    let internal getItemFromAppSettingsWithDefault (appSettingsItemKey : string) (defaultAction : unit -> string) =
        let p1 = ConfigurationManager.AppSettings.Item(appSettingsItemKey)
        match p1  with
            | null -> defaultAction()
            | _ -> p1

    let internal getTypeImplementingServiceEndpoints (serviceElement : ServiceElement) (types : seq<Type>) =
        let contracts = [ for e in serviceElement.Endpoints -> e.Contract]
        let name = serviceElement.Name
        types |> Seq.choose (fun x ->
                                    match x with
                                        | x when 
                                                (x.GetInterfaces().Select(fun y -> y.FullName).Intersect(contracts).Any())
                                                -> Some(x)
                                        | _ -> None
                             ) |> Seq.distinct


//TODO: For your getOptionFromString helper, I would do something like this to make usage a little
//smoother and more natural (In the core library, there is a Txt module this would fit into;
//this is just illustrative rather than prescriptive. In fact, the reason I use the name Txt instead
//"String" is to avoid mixing my api terminology with the .Net api because the different naming/casing
//conventions. For idiomatic F# modules such as List and Seq, it's very natural
//to "extend" them with your own functions):

//Create a String module...
module String =
    let asOption s =
        match String.IsNullOrEmpty(s) with
                | true -> None
                | _ -> Some s

//...or augment the string type (note that for static methods I wouldn't do both as it's a little awkward
//but for "instance" methods its a pretty good pattern because it gives the consumer more flexibility
[<AutoOpen>]
module StringExtensions =
    type String
    with
        static member AsOption s = s |> String.asOption
    
module StringExamples =
    let example() =
        let o1 = "MyString" |> String.asOption
        let o2 = String.Empty |> String.AsOption
        let o3 = null |> String.AsOption
        ()
        