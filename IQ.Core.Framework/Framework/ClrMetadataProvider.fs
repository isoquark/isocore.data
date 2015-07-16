// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection

[<AutoOpen>]
module ClrMetadataProviderVocbulary =
    type ClrMetadataProviderConfig = {
        Assemblies : ClrAssemblyName list
    }

    type ClrAttributionIndex = ClrAttributionIndex of attribToAttribution : Map<ClrTypeName, ClrAttribution>

/// <summary>
/// Realizes client API for CLR metadata discovery
/// </summary>
module ClrMetadataProvider =    
    
    let private assemblyDescriptions = ConcurrentDictionary<Assembly, ClrAssembly>()            
    let private typeIndex = ConcurrentDictionary<ClrTypeName, ClrType>()
    
    let private getAssemblyDescriptions() = assemblyDescriptions.Values |> List.ofSeq

    let private acquireTypeDescription pos (t : Type) =
        typeIndex.GetOrAdd(t.TypeName, fun n -> ClrType.describe pos t)
        
    let private describeAssembly (a : Assembly) =                               
        assemblyDescriptions.GetOrAdd(typeof<int>.Assembly, fun corlib ->
            {
                Name = ClrAssemblyName(corlib.SimpleName, corlib.FullName |> Some)
                Position = 0
                Types = [
                            typeof<Byte>; typeof<Int32>; typeof<Int16>
                            typeof<Int64>; typeof<UInt16>; typeof<UInt32>; typeof<UInt64>
                            typeof<BclDateTime>;typeof<String>; 
                            typeof<Decimal>; typeof<Double>; typeof<Single>;
                        ] |> List.mapi(fun pos t -> acquireTypeDescription pos t) 
                ReflectedElement = corlib |> Some
                Attributes = []
            }        
        )|>ignore

        assemblyDescriptions.GetOrAdd(a, fun a -> 
            {
                Name = ClrAssemblyName(a.SimpleName, a.FullName |> Some)
                Position = 0
                Types = a.GetTypes() |> Array.mapi(fun pos t -> ClrType.describe pos t) |> List.ofArray
                ReflectedElement = a |> Some
                Attributes = a.GetCustomAttributes() |> ClrAttribution.create a.ElementName
            })    

    let private describeType (t : Type) =
        let a = t.Assembly |> describeAssembly        
        a.Types |> List.find(fun x -> x.ReflectedElement = Some(t))    

    let private describeTypeProperties (t : Type) =
         t |> describeType |> (fun x -> x.Members |> List.filter(fun m -> m.Kind = ClrMemberKind.Property))

    let private describeProperty (p : PropertyInfo) =
        let members = [for t in p.DeclaringType.Assembly |> describeAssembly |> (fun x -> x.Types) do 
                        yield! t.Members
                    ]
        //Obviously, this is extremely inefficient
        members |> List.find(fun m ->
            match m with
            | PropertyMember(d) -> d.ReflectedElement = (Some(p))
            | _ -> false)
  
    let private describeMember(subject : MemberInfo) =
        let members = [for t in subject.DeclaringType.Assembly |> describeAssembly |> (fun x -> x.Types) do 
                        yield! t.Members
                    ]
        //Obviously, this is extremely inefficient                
        members |> List.find(fun m ->
            match m.ReflectedElement with
            | Some(e) -> (e :?> MemberInfo) = subject
            | None -> false)

    let private describeParameter(subject : ParameterInfo) =
        let m = subject.Member |> describeMember
        match m with
        | MethodMember(x) -> x.Parameters
        | ConstructorMember(x) -> x.Parameters
        | _ -> nosupport()
        |> List.find(fun p -> p.Name = ClrParameterName(subject.Name))


    let private describElement(o : obj) =
        match o with
        | :? Type as x -> x |> describeType |> TypeElement
        | :? MemberInfo as x -> x |> describeMember |> MemberElement 
        | :? Assembly as x -> x |> describeAssembly |> AssemblyElement
        | :? ParameterInfo as x -> x |> describeParameter  |> ParameterElement
        | _ -> nosupport()
    

    let private findTypes(q : ClrTypeQuery) =
        match q with
        | FindTypeByName(name) ->     
            match name |> typeIndex.TryGetValue with
            | (true,descrption) -> [descrption]                
            | (false,_)->  
                //TODO: whatever type this is needs to be discovered when the provider is created
                [Type.GetType(name.Text) |> acquireTypeDescription -1]
        | FindTypesByKind(kind) ->
            typeIndex.Values |> Seq.filter(fun t -> t.Kind = kind) |> List.ofSeq
            

    let private findTypeProperties(q : ClrTypeQuery) =
        [for t in (q |> findTypes) do yield! t.Properties]
    
    let private findProperties(q : ClrPropertyQuery) =
        match q with
        | FindPropertyByName(name, typeQuery) ->
            
            typeQuery |> findTypeProperties |> List.find(fun p -> p.Name = name) |> List.singleton
        | FindPropertiesByType(typeQuery) ->
            typeQuery |> findTypeProperties
                
    let private findAssemblies(q : ClrAssemblyQuery) =
        match q with
        | FindAssemblyByName(name) ->
            match getAssemblyDescriptions() |> List.tryFind(fun x -> x.Name = name) with
            | Some(x) -> [x]
            | None -> []
    
    let private findType(q : ClrTypeQuery) =
        let types = q |> findTypes
        if types |> Seq.isEmpty then
            ArgumentException(sprintf "Query %O does not identify a known type" q) |> raise
        else
            types |> Seq.exactlyOne

    let private findElements(q : ClrElementQuery) =
        match q with
        | FindAssemblyElement(q) -> q |> findAssemblies |> List.map AssemblyElement
        | FindPropertyElement(q) -> q |> findProperties |> List.chain2  PropertyMember  MemberElement
        | FindTypeElement(q) -> q |> findTypes |> List.map TypeElement

    /// <summary>
    /// Finds the type with the specified name
    /// </summary>
    /// <param name="name"></param>
    let private findNamedType(name : ClrTypeName) =
        name |> FindTypeByName |> findType |> fun x -> x.ReflectedElement.Value

    type private ClrMetadataStore(config : ClrMetadataProviderConfig) =
        do
            config.Assemblies  |> List.map AppDomain.CurrentDomain.AcquireAssembly |> List.map(fun x -> x |> describeAssembly) |> ignore

        interface IClrMetadataProvider with
            member this.FindTypes q = q |> findTypes
            member this.FindAssemblies q = q |> findAssemblies
            member this.FindProperties q = q |> findProperties
            member this.FindElements q = q |> findElements
        
    let get(config) =
        ClrMetadataStore(config) :> IClrMetadataProvider
    
    let getCurrent() =
        CompositionRoot.resolve<IClrMetadataProvider>()
    
[<AutoOpen>]
module ClrMetadataProviderExtensions =
    type IClrMetadataProvider 
    with
        /// <summary>
        /// Finds type identified by the query and raises error if not found
        /// </summary>
        /// <param name="q">Identifies the type</param>
        member this.FindType q = 
            q |> this.FindTypes |> Seq.exactlyOne
        
        /// <summary>
        /// Finds type identified by name and raises error if not found
        /// </summary>
        /// <param name="name">The name of the type</param>
        member this.FindType name = 
            name |> FindTypeByName |> this.FindType 

        /// <summary>
        /// Finds property identified by the query and raises error if not found
        /// </summary>
        /// <param name="q">Identifies the property</param>
        member this.FindProperty q =
            q |> this.FindProperties |> Seq.exactlyOne

        /// <summary>
        /// Finds assembly identified by the query and raises error if not found
        /// </summary>
        /// <param name="q">Identifies the assembly</param>
        member this.FindAssembly q =
            q |> this.FindAssemblies |> Seq.exactlyOne
        
        /// <summary>
        /// Finds assembly identified by name and raises error if not found
        /// </summary>
        /// <param name="name">Identifies the assembly</param>
        member this.FindAssembly name =
            name |> FindAssemblyByName |> this.FindAssembly

        member this.FindElement q =
            q |> this.FindElement |> Seq.exactlyOne                              

        /// <summary>
        /// Finds all modules available to the provider
        /// </summary>
        /// <param name="name">Identifies the assembly</param>
        member this.FindModules() =
            ClrTypeKind.Module |> FindTypesByKind |> this.FindTypes 
            

    
[<AutoOpen>]
module internal ClrMetadataProviderInstance =
    //This sucks, but we really have to do this if we are going to have the convenient operators below
    let ClrMetadata() = ClrMetadataProvider.getCurrent()        
        
                                           
[<AutoOpen>]
module ClrDescriptionExtensions =
    /// <summary>
    /// Creates a property description map keyed by name
    /// </summary>        
    let propinfos<'T> = 
        typeof<'T>.TypeName |> FindTypeByName |> FindPropertiesByType 
                            |> ClrMetadata().FindProperties 
                            |> List.map(fun x -> x.Name, x) 
                            |> Map.ofList               
    /// <summary>
    /// Describes the type identified by type prameter
    /// </summary>
    let typeinfo<'T> = typeof<'T>.TypeName |> ClrMetadata().FindType

    /// <summary>
    /// Describes the type identified by type prameter
    /// </summary>
    let enuminfo<'T> = match typeinfo<'T> with | EnumType(x) -> x | _ -> ArgumentException("Not an enumeration") |> raise

    /// <summary>
    /// Gets the methods defined by a type
    /// </summary>
    let methinfos<'T> = typeinfo<'T>.Methods |> List.map(fun m -> m.Name, m) |> Map.ofList

