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

    type IClrMetadataProvider =
        abstract DescribeTypes:ClrTypeQuery->ClrTypeDescription list
        abstract DescribeAssemblies:ClrAssemblyQuery->ClrAssemblyDescription list
        abstract DescribeProperties:ClrPropertyQuery->ClrPropertyDescription list

/// <summary>
/// Realizes client API for CLR metadata discovery
/// </summary>
module ClrMetadataProvider =    
    
    let private assemblyDescriptions = ConcurrentDictionary<Assembly, ClrAssemblyDescription>()        
    
    let private typeIndex = ConcurrentDictionary<ClrTypeName, ClrTypeDescription>()


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
            ClrParameterDescription.Name = p.ParameterName
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
                    ClrParameterDescription.Name = ClrParameterName(String.Empty)
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
        ClrMethodDescription.Name = m.Name |> ClrMemberName
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
            ClrPropertyDescription.Name = p.Name |> ClrMemberName 
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
            ClrFieldDescription.Name = f.Name |> ClrMemberName
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
            ClrConstructorDescription.Name = c.MemberName
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
            ClrEventDescription.Name = e.MemberName
            Position = pos
            ReflectedElement = e |> Some                
            Attributes = e.GetCustomAttributes() |> createAttributions e.ElementName
            DeclaringType = e.DeclaringType.TypeName
        }

    let private createMemberDescription pos (m : MemberInfo) =
        match m with
        | :? MethodInfo as x->
            x |> createMethodDescription pos |> MethodDescription
        | :? PropertyInfo as x->
            x |> createPropertyDescription pos |> PropertyDescription
        | :? FieldInfo as x -> 
            x |> createFieldDescription pos |> FieldDescription
        | :? ConstructorInfo as x ->
            x |> createConstructorDescription pos |> ConstructorDescription
        | :? EventInfo as x ->
            x |> createEventDescription pos |> EventDescription
        | _ ->
            nosupport()
    

    let private createTypeDescription pos (t : Type) =
        let isCollection = t |> Type.isCollectionType
        let collKind = if isCollection then t |> Type.getCollectionKind |> Some else None
        {
            ClrTypeDescription.Name = t.TypeName
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
                            typeof<DateTime>;typeof<String>; 
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
            | PropertyDescription(d) -> d.ReflectedElement = (Some(p))
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

    let describeParameter(subject : ParameterInfo) =
        let m = subject.Member |> describeMember
        match m with
        | MethodDescription(x) -> x.Parameters
        | ConstructorDescription(x) -> x.Parameters
        | _ -> nosupport()
        |> List.find(fun p -> p.Name = ClrParameterName(subject.Name))


    let describElement(o : obj) =
        match o with
        | :? Type as x -> x |> describeType |> TypeDescription
        | :? MemberInfo as x -> x |> describeMember |> MemberDescription 
        | :? Assembly as x -> x |> describeAssembly |> AssemblyDescription
        | :? ParameterInfo as x -> x |> describeParameter  |> ParameterDescription
        | _ -> nosupport()
    
    let findTypes(q : ClrTypeQuery) =
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
    
    let findType(q : ClrTypeQuery) =
        let types = q |> findTypes
        if types |> Seq.isEmpty then
            ArgumentException(sprintf "Query %O does not identify a known type" q) |> raise
        else
            types |> Seq.exactlyOne

    /// <summary>
    /// Finds the type with the specified name
    /// </summary>
    /// <param name="name"></param>
    let findNamedType(name : ClrTypeName) =
        name |> FindTypeByName |> findType |> fun x -> x.ReflectedElement.Value

    type private ClrMetadataStore(config : ClrMetadataProviderConfig) =
        do
            config.Assemblies  |> List.map AppDomain.CurrentDomain.AcquireAssembly |> List.map(fun x -> x |> describeAssembly) |> ignore

        interface IClrMetadataProvider with
            member this.DescribeTypes q = q |> findTypes
            member this.DescribeAssemblies q = q |> findAssemblies
            member this.DescribeProperties q = q |> findProperties
        
    let internal get(config) =
        ClrMetadataStore(config) :> IClrMetadataProvider
    
    let getCurrent() =
        CompositionRoot.resolve<IClrMetadataProvider>()
    
[<AutoOpen>]
module internal ClrMetadataProviderInstance =
    let ClrMetadata() = ClrMetadataProvider.getCurrent()        

[<AutoOpen>]
module ClrMetadataProviderExtensions =
    type IClrMetadataProvider 
    with
        /// <summary>
        /// Describes exactly one type identified by the query; raises error if not found
        /// </summary>
        /// <param name="q">Identifies the type</param>
        member this.DescribeType q = 
            q |> this.DescribeTypes |> Seq.exactlyOne
        
        /// <summary>
        /// Decribes exactly one type given its name
        /// </summary>
        /// <param name="name">The name of the type</param>
        member this.DescribeType name = 
            name |> FindTypeByName |> this.DescribeType 

        /// <summary>
        /// Describes the identified property
        /// </summary>
        /// <param name="q">Identifies the property</param>
        member this.DescribeProperty q =
            q |> this.DescribeProperties |> Seq.exactlyOne

        /// <summary>
        /// Describes exactly one asseembly identified by the query; raises error if not found
        /// </summary>
        /// <param name="q">Identifies the assembly</param>
        member this.DesribeAssembly q =
            q |> this.DescribeAssemblies |> Seq.exactlyOne
        
        /// <summary>
        /// Describes exactly one asseembly identified by name; raises error if not found
        /// </summary>
        /// <param name="q">Identifies the assembly</param>
        member this.DescribeAssembly name =
            name |> FindAssemblyByName |> this.DesribeAssembly
        


                      

module ClrElementDescription = 
    let getChildren element =
        match element with
        | MemberDescription(m) ->
            match m with
            | MethodDescription(x) -> 
                x.Parameters |> List.map(fun x -> x |> ParameterDescription)
            |_ -> []            
        | TypeDescription(d) -> 
            d.Members |> List.map(fun x -> x |> MemberDescription)
        | AssemblyDescription(z) ->
            z.Types |> List.map(fun x -> x |> TypeDescription)
        | ParameterDescription(_) -> []
        | UnionCaseDescription(_) -> []

    /// <summary>
    /// Recursively traverses the element hierarchy graph and invokes the supplied handler as each element is traversed
    /// </summary>
    /// <param name="handler">The handler that will be invoked for each element</param>
    /// <param name="element"></param>
    let rec walk (handler:ClrElementDescription->unit) element =
        element |> handler
        let children = element |> getChildren
        children |> List.iter (fun child -> child |> walk handler)

    /// <summary>
    /// Recursively traverses the element hierarchy graph and invokes each of the supplied handlers as each element is traversed
    /// </summary>
    /// <param name="handler">The handlers that will be invoked for each element</param>
    /// <param name="element"></param>
    let multiwalk (handlers: (ClrElementDescription->unit) list) element =
        let handler e =
            handlers |> List.iter(fun handler -> e|> handler)
        
        element |> walk handler

       
    /// <summary>
    /// Retrieves an attribute from the element if it exists and returns None if it does not
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let tryGetAttribute (element : ClrElementDescription) attribType =
        element.Attributes |> ClrAttribution.tryFind attribType
    
    /// <summary>
    /// Retrieves an attribute from the element if it exists and raises an exception otherwise
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getAttribute element attribType =
        attribType |> tryGetAttribute element |> Option.get

    /// <summary>
    /// Determines whether an attribute of a specified type has been applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let hasAttribute element attribType = 
        attribType  |> tryGetAttribute element |> Option.isSome

    /// <summary>
    /// Retrieves all attributes of a specified type that have been applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getAttributes (element : ClrElementDescription) attribType =
        element.Attributes |> List.filter(fun x -> x.AttributeName = attribType)

    /// <summary>
    /// Retrieves an attribute applied to a member, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let tryGetAttributeT<'T when 'T :> Attribute> (element : ClrElementDescription) =
        match  element.Attributes |> ClrAttribution.tryFind typeof<'T> with
        | Some(x) -> 
            match x.AttributeInstance with
            | Some(x) -> x :?> 'T |> Some
            | None -> None
               
        | None -> None

    /// <summary>
    /// Determines whether an attribute is applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    let hasAttributeT<'T when 'T :> Attribute>(element : ClrElementDescription) =
        element |> tryGetAttributeT<'T> |> Option.isSome
                    
    /// <summary>
    /// Retrieves an attribute from the element if it exists and raises an exception otherwise
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getAttributeT<'T when 'T :> Attribute> element  =
        element |> tryGetAttributeT<'T> |> Option.get

    
        
                                           
[<AutoOpen>]
module ClrDescriptionExtensions =

           
    /// <summary>
    /// Creates a property description
    /// </summary>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    /// <param name="p">The property to be referenced</param>
    let internal propinfo (p : PropertyInfo) = 
        FindPropertyByName(p.MemberName, p.DeclaringType.TypeName |> FindTypeByName) |> ClrMetadata().DescribeProperty
        

    /// <summary>
    /// Creates a property description map keyed by name
    /// </summary>        
    let propinfomap<'T> = 
        typeof<'T>.TypeName |> FindTypeByName |> FindPropertiesByType 
                            |> ClrMetadata().DescribeProperties 
                            |> List.map(fun x -> x.Name, x) 
                            |> Map.ofList
        
       

    /// <summary>
    /// Describes the type identified by a type prameter
    /// </summary>
    let typeinfo<'T> = typeof<'T>.TypeName |> ClrMetadata().DescribeType

    /// <summary>
    /// Gets the methods defined by a type
    /// </summary>
    let methodinfomap<'T> = typeinfo<'T>.Methods |> List.map(fun m -> m.Name, m) |> Map.ofList

