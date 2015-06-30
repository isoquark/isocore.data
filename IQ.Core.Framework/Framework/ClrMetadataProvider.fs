namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection

/// <summary>
/// Realizes client API for CLR metadata discovery
/// </summary>
module ClrMetadataProvider =    

    let private cache = Dictionary<obj, ClrElement>()        

    let private cacheElement (e : ClrElement) =
        cache.Add(e.IReflectionPrimitive.Primitive, e)
        e

    let rec private createElement pos (o : obj) =
        if o |> cache.ContainsKey then
            cache.[o]
        else
            match o with
            | :? Assembly as x->             
                let children = x.GetTypes() |> Array.mapi(fun i t -> t |> createElement i ) |> List.ofArray
                let e = ClrReflectionPrimitive(x, None) |> ClrAssemblyElement
                (e, children) |> AssemblyElement |> cacheElement            

            | :? Type as x-> 
                let children = x |> Type.getDeclaredMembers |> List.mapi(fun i m -> m |> createElement i) 
                let e = ClrReflectionPrimitive(x, pos |> Some) |> ClrTypeElement
                TypeElement(e, children) |> cacheElement
            | :? MethodInfo as x ->
                let children = x.GetParameters() |> Array.mapi(fun i p -> p |> createElement i) |> List.ofArray
                let e = (x, pos |> Some) |> ClrReflectionPrimitive |> ClrMethodElement |> MethodElement
                (e, children) |> MemberElement |> cacheElement
            | :? PropertyInfo as x-> 
                let e = (x, pos |> Some) |> ClrReflectionPrimitive |> ClrPropertyElement |> PropertyMember |> DataMemberElement
                (e, []) |> MemberElement |> cacheElement
            | :? FieldInfo  as x-> 
                let e = (x, pos |> Some) |> ClrReflectionPrimitive |> ClrFieldElement |> FieldMember |> DataMemberElement
                (e, []) |> MemberElement |> cacheElement
            | :? ParameterInfo as x -> 
                ClrReflectionPrimitive(x, pos |> Some) |> ClrParameterElement |> ParameterElement |> cacheElement
            | :? UnionCaseInfo as x ->
                let e = ClrReflectionPrimitive(x, pos |> Some) |> ClrUnionCaseElement
                (e, []) |> UnionCaseElement |> cacheElement
            | :? ConstructorInfo as x -> nosupport()
            | :? EventInfo as x -> nosupport()
            | _ -> nosupport()

    let private addAssembly (a : Assembly) =
        a |> createElement 0 

    let private getElementAssembly (o : obj) =
        match o with
        | :? Assembly as x-> x
        | :? Type as x-> x.Assembly
        | :? MethodInfo as x -> x.DeclaringType.Assembly
        | :? PropertyInfo as x-> x.DeclaringType.Assembly 
        | :? FieldInfo  as x-> x.DeclaringType.Assembly
        | :? ParameterInfo as x -> x.Member.DeclaringType.Assembly
        | :? ConstructorInfo as x -> x.DeclaringType.Assembly            
        | :? EventInfo as x -> x.DeclaringType.Assembly
        | :? UnionCaseInfo as x -> x.DeclaringType.Assembly
        | _ -> argerrord "o" o "Not a recognized reflection primitive"

    let private addElement (o : obj) =
        o |> getElementAssembly |> addAssembly |> ignore
    
    let getElement (o : obj) =        
        //This is not thread-safe and is temporary
        match  o |> cache.TryGetValue with
        |(false,_) -> 
            o |> addElement |> ignore
            //Note that at this point, element may still not be in cache (for example, it could be a closed generic type
            //that is created in the body of a function)
            //In any case, if it's not there now we add it; in the future, this will probably be removed because this
            //model isn't intended to capture such things
            if o |> cache.ContainsKey |> not then
                o |> createElement 0
            else
                cache.[o]
        |(_,element) -> element

    let getType (o : obj) = 
        let element = o |> getElement
        if element.Kind <> ClrElementKind.Type then
            argerrord "o" o "Not a type"
        else
            element |> ClrElement.asTypeElement

    /// <summary>
    /// Gets the parameters elements associated with the method
    /// </summary>
    /// <param name="m"></param>
    let getParameters (m : MethodInfo) =
        m.GetParameters() |> Array.map getElement 
                          |> Array.map ClrElement.asParameterElement 
                          |> List.ofArray

    let getAssemblyElement(name : ClrAssemblyName) =
        name |> AppDomain.CurrentDomain.AcquireAssembly |> getElement 

    
    let private assemblyDescriptions = ConcurrentDictionary<Assembly, ClrAssemblyDescription>()        
    
    let private getTypeDescriptions() = 
        [for a in assemblyDescriptions.Values do yield! a.Types]


   
    let describeAssembly (a : Assembly) =        
        let describeMethod pos (m : MethodInfo) = {
            ClrMethodDescription.Name = m.Name |> ClrMemberName
            Position = pos
            ReflectedElement = m |> Some
        }

        let describeProperty pos (p : PropertyInfo) =
            {
                ClrPropertyDescription.Name = p.Name |> ClrMemberName 
                Position = pos
                DeclaringType  = p.DeclaringType |> ClrTypeName.fromType
                ValueType = p.PropertyType |> Type.getItemValueType |> fun x -> x |> ClrTypeName.fromType
                IsOptional = p.PropertyType |> Option.isOptionType
                CanRead = p.CanRead
                CanWrite = p.CanWrite
                ReflectedElement = p |> Some
            }

        let describeField pos (f : FieldInfo) =
            {
                ClrStorageFieldDescription.Name = f.Name |> ClrMemberName
                Position = pos
                ReflectedElement = f |> Some
            }

        let describeConstructor pos (c : ConstructorInfo) =
            {
                ClrConstructorDescription.Name = c.Name |> ClrMemberName
                Position = pos
                ReflectedElement = c |> Some
            }

        let describeEvent pos (e : EventInfo) =
            {
                ClrEventDescription.Name = e.Name |> ClrMemberName
                Position = pos
                ReflectedElement = e |> Some
            }

        let describeMember pos (m : MemberInfo) =
            match m with
            | :? MethodInfo as x->
                x |> describeMethod pos |> MethodDescription
            | :? PropertyInfo as x->
                x |> describeProperty pos |> PropertyDescription
            | :? FieldInfo as x -> 
                x |> describeField pos |> FieldDescription
            | :? ConstructorInfo as x ->
                x |> describeConstructor pos |> ConstructorDescription
            | :? EventInfo as x ->
                x |> describeEvent pos |> EventDescription
            | _ ->
                nosupport()

        let describeType pos (t : Type) =
            {
                ClrTypeDescription.Name = t |> ClrTypeName.fromType
                Position = pos
                DeclaringType = if t.DeclaringType <> null then 
                                    t.DeclaringType|> ClrTypeName.fromType |> Some 
                                else 
                                    None
                DeclaredTypes = t.GetNestedTypes() |> Array.map(fun n -> n |> ClrTypeName.fromType) |> List.ofArray
                Members = t |> Type.getDeclaredMembers |> List.mapi(fun pos m -> m |> describeMember pos)
                Kind = t |> Type.getTypeKind
                ReflectedElement = t |> Some
                CollectionKind = if t |> Type.isCollectionType then t |> Type.getCollectionKind |> Some else None
                IsOptionType = t |> Option.isOptionType
            }                                    

        assemblyDescriptions.GetOrAdd(a, fun a -> 
            {
                Name = ClrAssemblyName(a.SimpleName, a.FullName |> Some)
                Position = 0
                Types = a.GetTypes() |> Array.mapi(fun pos t -> describeType pos t) |> List.ofArray
                ReflectedElement = a |> Some
            })    

    let describeType (t : Type) =
        let a = t.Assembly |> describeAssembly        
        a.Types |> List.find(fun x -> x.ReflectedElement = Some(t))


    let describeTypeProperties (t : Type) =
         t |> describeType |> (fun x -> x.Members |> List.filter(fun m -> m.Kind = ClrMemberKind.Property))

    let describeProperty (p : PropertyInfo) =
        let members = [for t in p.DeclaringType.Assembly |> describeAssembly |> (fun x -> x.Types) do 
                        yield! t.Members
                    ]
        //Obviously, this is extremely inefficient
        members |> List.find(fun m ->
            match m with
            | PropertyDescription(d) -> d.ReflectedElement = (Some(p))
            | _ -> false)
        
    let findTypes(q : ClrTypeQuery) =
        match q with
        | FindTypeByName(name) ->
            match getTypeDescriptions() |> List.tryFind(fun x -> x.Name = name) with
            | Some(x) -> [x]
            | None -> []
                                       
    
[<AutoOpen>]
module ClrDescriptionExtensions =

    
        

    /// <summary>
    /// Creates a property description
    /// </summary>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    /// <param name="p">The property to be referenced</param>
    let internal propinfo (p : PropertyInfo) = p |> ClrMetadataProvider.describeProperty 

    /// <summary>
    /// Creates a property description map keyed by name
    /// </summary>
    //let propinfomap<'T> = props<'T> |> List.mapi propinfo |> List.map(fun p -> p.Name, p) |> Map.ofList
    let propinfomap<'T> = typeof<'T> |> ClrMetadataProvider.describeTypeProperties |> List.map(fun x -> x.Name, x) |> Map.ofList
       

    /// <summary>
    /// Describes the type identified by a type prameter
    /// </summary>
    let typeinfo<'T> = typeof<'T> |> ClrMetadataProvider.describeType
