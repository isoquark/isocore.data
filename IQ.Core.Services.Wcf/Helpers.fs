﻿namespace IQ.Core.Services.Wcf

open System
open System.IO
open System.Linq
open System.Configuration
open System.Reflection
open System.ServiceModel.Configuration

module Helpers =

    let getFun (a : Action<'T>) : 'T -> unit =
        Core.FuncConvert.ToFSharpFunc a

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


