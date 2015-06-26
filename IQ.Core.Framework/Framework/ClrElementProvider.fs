namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic

open Microsoft.FSharp.Reflection

/// <summary>
/// Realizes client API for CLR metadata discovery
/// </summary>
module ClrElementProvider =    

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
                let children = x |> Type.getPureMembers |> List.mapi(fun i m -> m |> createElement i) 
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

    let getTypeElement (o : obj) = 
        let element = o |> getElement
        if (element |> ClrElement.getKind) <> ClrElementKind.Type then
            argerrord "o" o "Not a type"
        else
            element |> ClrElement.asTypeElement
        
        