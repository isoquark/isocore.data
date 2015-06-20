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
    //TODO: 
    //1. Type names are almost always CamelCased
    //2. This type looks like it should be private to the module
    //3. I often use single case DU's for this purpose because they are more convenient to 
    //construct, e.g.,
    //type ContractKey = ContractKey of endpointName : string option * contractType : string
    //but this is just a personal preference
    type contractKey = 
        { 
        endpointName : string option;
        contractType : string
        }

    
    
        
    //TODO: I would not use this combination of functional data structures (Map) and 
    //mutable state. Instead, I would use a container which is itself mutable
    //ConcurrentDictionary would be perfect for this

    let mutable private _endpointMap : Map<contractKey, obj> = Map.empty //map by endpoint name (when supporting multiple endpoints for same type)
    let mutable private _mapByType : Map<string,int> = Map.empty    //bucket/map by contract type name with tuples of counts of endpoints and channels; 
                                                                //if count > 1, then obj will be null, as it would be ambiguous to go by contract type; 
                                                                //endpoint name is required in this case
    let mutable private _assemblies : Assembly[] = Array.empty
    let private _filePattern = "*.dll" //TODO: read from config file

    let private loadAndGetAllAssemblies =
        let execDir = new DirectoryInfo(Environment.CurrentDirectory)
        let files = execDir.GetFiles(_filePattern); //todo: support file prefix or pattern
        _assemblies <- seq { for f in files -> Assembly.LoadFile(f.FullName) }  |> Seq.toArray
       

    // Returns the assembly that contains the definition of the service contract (interface)
    // by introspecting all the assemblies loaded in the app domain.
    // Currently not handling assemblies that have not been loaded yet on the app domain. (TODO)
    let private findAssemblyThatContainsContract (contractName : string) =
        if _assemblies = Array.empty then //lazy init
           loadAndGetAllAssemblies  
        _assemblies |> Seq.tryFind (fun (a) -> a.GetType(contractName) <> null)  

    let private findItemByContractType (ct : string) =  
        if (_mapByType.ContainsKey(ct) && _mapByType.Item(ct) = 1)
        then      
            let k = _endpointMap |> Map.tryFindKey (fun k v -> k.contractType = ct)
            match k with
                | Some x -> _endpointMap |> Map.tryFind x
                | _ -> None       
        else None //multiple or none; if multiple, need to disambiguate using endpoint name when calling the channel

    let private buildMapByType () =
        //assumes the main map has already been built at startup   
        let keys = _endpointMap |> Map.toSeq |> Seq.map fst  //_endpointMap.Keys
        _mapByType <- bucketSort keys |> Seq.map (fun (x,y) -> x.contractType, y) |> Map.ofSeq

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
        let key = { endpointName = name; contractType = contract }
        let asmWithType = findAssemblyThatContainsContract contract
        let contractType = 
            match asmWithType with
                | Some(x) -> x.GetType(contract)
                | _ -> raise ( sprintf "No assembly that defines the contract %s can be found!" contract |> ApplicationException)        
        let channel = createGenericChannelInstance contract name
        _endpointMap <- _endpointMap.Add(key, channel) 


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
                let k = { endpointName = Some x; contractType = contractType }
                if (_endpointMap.ContainsKey(k)) then
                    _endpointMap.Item(k) :?> GenericChannel<'T>
                else
                    raise ( msg k.endpointName k.contractType |> ApplicationException)

    let disposeEndpointMap () =
        if not _endpointMap.IsEmpty then
            _endpointMap |> Map.iter  (fun k v -> (v:?> IDisposable).Dispose())

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
