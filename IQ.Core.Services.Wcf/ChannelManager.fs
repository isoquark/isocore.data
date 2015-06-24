namespace IQ.Core.Services.Wcf

open System
open System.IO
open System.Linq
open System.Reflection
open System.Collections.Concurrent
open System.Collections.Generic
open System.Configuration
open System.ServiceModel.Configuration
open Channel
open Helpers

module ChannelAdapter =
    //single case DU for the key type of the endpoint map/dictionary
    type private ContractKey = ContractKey of endpointName : string option * contractType : string
    
    //map by endpoint name (when supporting multiple endpoints for same type)
    let private _endpointMap : ConcurrentDictionary<ContractKey, obj> = new ConcurrentDictionary<ContractKey, obj>() 
    let mutable private _mapByType : Map<string,int> = Map.empty    //bucket/map by contract type name with tuples of counts of endpoints and channels; 
                                                                    //if count > 1, then obj will be null, as it would be ambiguous to go by contract type; 
                                                                    //endpoint name is required in this case

    let private _filePattern = "*.dll" //TODO: read from config file

    let private _assemblies =
        let execDir = new DirectoryInfo(Environment.CurrentDirectory)
        let files = execDir.GetFiles(_filePattern); //todo: support file prefix or pattern
        seq { for f in files -> Assembly.LoadFile(f.FullName) }  |> Seq.toArray
       

    // Returns the assembly that contains the definition of the service contract (interface)
    // by introspecting all the assemblies loaded in the app domain.
    // Currently not handling assemblies that have not been loaded yet on the app domain. (TODO)
    let private findAssemblyThatContainsContract (contractName : string) =
        _assemblies |> Seq.tryFind (fun (a) -> a.GetType(contractName) <> null)  

    let private findItemByContractType (ct : string) =  
        if (_mapByType.ContainsKey(ct) && _mapByType.Item(ct) = 1)
        then      
            let k = _endpointMap.Keys |> Seq.tryFind (fun (ContractKey (_,c)) -> c = ct ) 
            match k with
                | Some x -> Some _endpointMap.[x] // |> Map.tryFind x
                | _ -> None       
        else None //multiple or none; if multiple, need to disambiguate using endpoint name when calling the channel

    let private buildMapByType () =
        //assumes the main map has already been built at startup   
        let keys = _endpointMap.Keys
        _mapByType <- bucketSort keys |> Seq.map (fun ((ContractKey (_,c)),y) -> c, y) |> Map.ofSeq

    let private createGenericChannelInstance contractTypeName (endpointName : string option) =
        let asm = findAssemblyThatContainsContract contractTypeName
        let contractType = match asm with
                            | Some(a) -> a.GetType(contractTypeName)
                            | _ -> raise (sprintf "Assembly containint service contract %s not found!" contractTypeName |>  ApplicationException)
        let channelType = typedefof<GenericChannel<_>>.MakeGenericType(contractType)
        let ctors = channelType.GetConstructors()
        let pt = typedefof<Option<_>>.MakeGenericType([| typedefof<string>|])
        let ctor = channelType.GetConstructor( [| pt |] )
        ctor.Invoke [| endpointName |] //channel instance
        

    let private processClientEndpoint (endpoint : ChannelEndpointElement) =
        let name = Some(endpoint.Name) //this may not be specified
        let contract = endpoint.Contract
        let key = ContractKey ( name, contract )
        let asmWithType = findAssemblyThatContainsContract contract
        let contractType = 
            match asmWithType with
                | Some(x) -> x.GetType(contract)
                | _ -> raise ( sprintf "No assembly that defines the contract %s can be found!" contract |> ApplicationException)        
        let channel = createGenericChannelInstance contract name
        _endpointMap.TryAdd(key, channel) |> ignore


    let private ReadAllClientEndpoints =             
        let props = getClientEndpoints()
        [ for prop in props -> 
                                    Console.WriteLine("prop contract = {0} ",  prop.Contract)
                                    processClientEndpoint prop 
                    ] |> ignore
        buildMapByType()

    //Returns a generic client channel of given type; If not found in map(s), it will create it first, then add to map(s)
    let internal GetChannel<'T> endpointName =
      let msg e c = 
        let baseMsg = sprintf "No Channel Factory for %scontract type %s"
        match e with
            | Some e -> baseMsg (sprintf "endpoint %s and " e) c
            | _ -> baseMsg "" c
      let contractType = typedefof<'T>.FullName
      match endpointName with
        | None -> let c = findItemByContractType contractType 
                  match c with
                    | Some x -> x :?> GenericChannel<'T>
                    | _ -> raise (msg None contractType |> ApplicationException)
        | Some x -> 
                let k = ContractKey (Some x, contractType)
                if (_endpointMap.ContainsKey(k)) then
                    _endpointMap.Item(k) :?> GenericChannel<'T>
                else
                    raise ( msg (Some x) contractType |> ApplicationException)

    let private disposeEndpointMap () =
        if not _endpointMap.IsEmpty then
            _endpointMap.Values |> Seq.iter  (fun v -> if v <> null then (v:?> IDisposable).Dispose())

    //the MAIN SINGLETON type which is accessible from the outside world
    type ChannelManager private () =
        static let instance = lazy (
                                ReadAllClientEndpoints 
                                new ChannelManager()
                              )
                        
        static member Instance = instance.Value

        //access point from outside (all overloaded members, to support easy calling both from C# and F#
        member x.Invoke<'T>(action : Action<'T>) =
            let factory = GetChannel<'T> None
            factory.Call (getFun action)

        member x.Invoke<'T>(action : 'T -> unit) = 
            let factory = GetChannel<'T> None
            factory.Call action

        member x.Invoke<'T>(action : Action<'T>, endpointName) =
            let factory = GetChannel<'T> endpointName
            factory.Call (getFun action)

        member x.Invoke<'T>(action : 'T -> unit, endpointName) = 
            let factory = GetChannel<'T> endpointName
            factory.Call action

        member x.Invoke<'T>(action : Action<'T>, endpointNameStr) =
            let endpointName = getOptionFromString endpointNameStr
            let factory = GetChannel<'T> endpointName
            factory.Call (getFun action)

        member x.Invoke<'T>(action : 'T -> unit, endpointNameStr) =
            let endpointName = getOptionFromString endpointNameStr
            let factory = GetChannel<'T> endpointName
            factory.Call action

        interface IDisposable with
            member x.Dispose() = disposeEndpointMap()
        //end acces point from outside
