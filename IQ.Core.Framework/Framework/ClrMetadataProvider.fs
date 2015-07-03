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

    /// <summary>
    /// Defines contract realized by CLR metadata provider
    /// </summary>
    type IClrMetadataProvider =
        /// <summary>
        /// Find types accessible to the provider
        /// </summary>
        abstract FindTypes:ClrTypeQuery->ClrType list
        /// <summary>
        /// Find assemblies accessible to the provider
        /// </summary>
        abstract FindAssemblies:ClrAssemblyQuery->ClrAssembly list
        /// <summary>
        /// Find properties accessible to the provider
        /// </summary>
        abstract FindProperties:ClrPropertyQuery->ClrProperty list

/// <summary>
/// Realizes client API for CLR metadata discovery
/// </summary>
module ClrMetadataProvider =    
    
    let private assemblyDescriptions = ConcurrentDictionary<Assembly, ClrAssembly>()        
    
    let private typeIndex = ConcurrentDictionary<ClrTypeName, ClrType>()


    let private getAssemblyDescriptions() =
        assemblyDescriptions.Values |> List.ofSeq
   
    /// <summary>
    /// Creates an attribution for supplied target and attributes
    /// </summary>
    /// <param name="target">Identifies the element to which the attributes apply</param>
    /// <param name="attributes">The applied attributes</param>
    let private createAttributions (target : ClrElementName) (attributes : Attribute seq) =
        
        let getValue (attrib : Attribute) (p : PropertyInfo) =
            try
                attrib |> p.GetValue
            with
                | :? TargetInvocationException as e ->
                    match e.InnerException with
                    | :? NotSupportedException  ->
                        null
                    | _ -> reraise()
                | _ -> reraise()
        
        [for attribute in attributes do
            let attribType = attribute.GetType()
            yield 
                {
                    ClrAttribution.AttributeName =  attribType.TypeName
                    Target = target
                    AppliedValues = attribType.GetProperties() 
                                  |> Array.filter(fun p -> p.CanRead)
                                  |> Array.mapi( fun i p -> ValueIndexKey(p.Name, i),  p |> getValue attribute  ) 
                                  |>List.ofArray 
                                  |> ValueIndex
                    AttributeInstance = attribute |> Some
                }            
        ]

    let private createInputParameterDescription pos (p : ParameterInfo) =
        
        {
            ClrMethodParameter.Name = p.ParameterName
            Position = pos
            ReflectedElement = p |> Some
            Attributes = p.GetCustomAttributes() |> createAttributions p.ElementName
            CanOmit = (p.IsOptional || p.IsDefined(typeof<OptionalArgumentAttribute>))
            ParameterType = p.ParameterType.TypeName
            DeclaringMethod = ClrMemberName(p.Member.Name)
            IsReturn = false
        }


    let private createParameterDescriptions (m : MethodInfo) = 
        [ yield! m.GetParameters() 
                    |> Array.mapi(fun pos p -> p |> createInputParameterDescription pos) 
                    |>List.ofArray
          
          if m.ReturnType <> typeof<Void> then            
              yield {
                    ClrMethodParameter.Name = ClrParameterName(String.Empty)
                    Position = -1
                    ReflectedElement = None
                    Attributes = m |> MethodInfo.getReturnAttributes |> createAttributions m.ElementName
                    CanOmit = false
                    ParameterType = m.ReturnType.TypeName
                    DeclaringMethod = ClrMemberName(m.Name)
                    IsReturn = true
                  }
        ]

    
    let private createMethodDescription pos (m : MethodInfo) = {
        ClrMethod.Name = m.Name |> ClrMemberName
        Position = pos
        ReflectedElement = m |> Some
        Access = m.Access
        IsStatic = m.IsStatic
        Parameters = m |> createParameterDescriptions
        Attributes = m.GetCustomAttributes() |> createAttributions m.ElementName
        ReturnType = if m.ReturnType = typeof<System.Void> then None else m.ReturnType.TypeName |> Some
        ReturnAttributes = m |> MethodInfo.getReturnAttributes |> createAttributions m.ElementName
        DeclaringType = m.DeclaringType.TypeName
    }

    let private createPropertyDescription pos (p : PropertyInfo) =
        {
            ClrProperty.Name = p.Name |> ClrMemberName 
            Position = pos
            DeclaringType  = p.DeclaringType.TypeName
            ValueType = p.PropertyType.TypeName
            IsOptional = p.PropertyType |> Option.isOptionType
            CanRead = p.CanRead
            ReadAccess = if p.CanRead then p.GetMethod.Access |> Some else None
            CanWrite = p.CanWrite
            WriteAccess = if p.CanWrite then p.SetMethod.Access |> Some else None
            ReflectedElement = p |> Some
            IsStatic = if p.CanRead then p.GetMethod.IsStatic else p.SetMethod.IsStatic
            Attributes = p.GetCustomAttributes() |> createAttributions p.ElementName
            GetMethodAttributes = if p.CanRead then 
                                        p.GetMethod.GetCustomAttributes() |> createAttributions p.GetMethod.ElementName 
                                  else []
            SetMethodAttributes = if p.CanWrite then
                                        p.SetMethod.GetCustomAttributes() |> createAttributions p.SetMethod.ElementName else []
        }
    

    let private createFieldDescription pos (f : FieldInfo) =
        let attributes = f.GetCustomAttributes() |> createAttributions f.ElementName
        let isLiteral = attributes |> ClrAttribution.tryFind typeof<LiteralAttribute> |> Option.isSome
        {
            ClrField.Name = f.Name |> ClrMemberName
            Position = pos
            ReflectedElement = f |> Some
            Access = f.Access
            IsStatic = f.IsStatic
            Attributes = attributes
            FieldType = f.FieldType.TypeName
            DeclaringType = f.DeclaringType.TypeName
            IsLiteral = isLiteral
            LiteralValue = if isLiteral then f.GetValue(null) |> fun x -> x.ToString() |> Some else None
        }

    let private createConstructorDescription pos (c : ConstructorInfo) =
        {
            ClrConstructor.Name = c.MemberName
            Position = pos
            ReflectedElement = c |> Some
            Access = c.Access
            IsStatic = c.IsStatic
            Parameters = c.GetParameters() |>  Array.mapi(fun pos p -> p |> createInputParameterDescription pos) |>List.ofArray
            Attributes = c.GetCustomAttributes() |> createAttributions c.ElementName
            DeclaringType = c.DeclaringType.TypeName
        }

    let private createEventDescription pos (e : EventInfo) =
        {
            ClrEvent.Name = e.MemberName
            Position = pos
            ReflectedElement = e |> Some                
            Attributes = e.GetCustomAttributes() |> createAttributions e.ElementName
            DeclaringType = e.DeclaringType.TypeName
        }

    let private createMemberDescription pos (m : MemberInfo) =
        match m with
        | :? MethodInfo as x->
            x |> createMethodDescription pos |> MethodMember
        | :? PropertyInfo as x->
            x |> createPropertyDescription pos |> PropertyMember
        | :? FieldInfo as x -> 
            x |> createFieldDescription pos |> FieldMember
        | :? ConstructorInfo as x ->
            x |> createConstructorDescription pos |> ConstructorMember
        | :? EventInfo as x ->
            x |> createEventDescription pos |> EventMember
        | _ ->
            nosupport()
    

    let private createTypeDescription pos (t : Type) =
        let isCollection = t |> Type.isCollectionType
        let collKind = if isCollection then t |> Type.getCollectionKind |> Some else None
        {
            ClrType.Name = t.TypeName
            Position = pos
            DeclaringType = if t.DeclaringType <> null then 
                                t.DeclaringType.TypeName |> Some 
                            else 
                                None
            DeclaredTypes = t.GetNestedTypes() |> Array.map(fun n -> n.TypeName) |> List.ofArray
            Members = t |> Type.getDeclaredMembers |> List.mapi(fun pos m -> m |> createMemberDescription pos)
            Kind = t |> Type.getKind
            ReflectedElement = t |> Some
            CollectionKind = collKind
            IsOptionType = t |> Option.isOptionType
            Access = t.Access
            IsStatic = t.IsAbstract && t.IsSealed
            Attributes = t.GetCustomAttributes() |> createAttributions t.ElementName
            ItemValueType = t.ItemValueType.TypeName
        }                                    

    let private acquireTypeDescription pos (t : Type) =
        typeIndex.GetOrAdd(t.TypeName, fun n -> createTypeDescription pos t)

    
        
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
                Types = a.GetTypes() |> Array.mapi(fun pos t -> createTypeDescription pos t) |> List.ofArray
                ReflectedElement = a |> Some
                Attributes = a.GetCustomAttributes() |> createAttributions a.ElementName
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
        | :? Assembly as x -> x |> describeAssembly |> AssemblyElemement
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

    let private findTypeProperties(q : ClrTypeQuery) =
        [for t in (q |> findTypes) do yield! t.Properties]
    
    let findProperties(q : ClrPropertyQuery) =
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
        
    let internal get(config) =
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
    /// Describes the type identified by a type prameter
    /// </summary>
    let typeinfo<'T> = typeof<'T>.TypeName |> ClrMetadata().FindType

    /// <summary>
    /// Gets the methods defined by a type
    /// </summary>
    let methinfos<'T> = typeinfo<'T>.Methods |> List.map(fun m -> m.Name, m) |> Map.ofList

