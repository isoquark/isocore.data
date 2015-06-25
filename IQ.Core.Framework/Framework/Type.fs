namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Generic

open Microsoft.FSharp.Reflection

/// <summary>
/// Defines <see cref="System.Type"/> helpers
/// </summary>
module Type =
    /// <summary>
    /// Determines whether a type is a generic enumerable
    /// </summary>
    /// <param name="t">The type to examine</param>
    let internal isNonOptionalCollectionType (t : Type) =
        let isEnumerable = t.GetInterfaces() |> Array.exists(fun x -> x.IsGenericType && x.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>)
        if t.IsArray |> not then
            t.IsGenericType && isEnumerable
        else
            isEnumerable            

    /// <summary>
    /// Determines whether the type is of the form option<IEnumerable<_>>
    /// </summary>
    /// <param name="t">The type to examine</param>
    let internal isOptionalCollectionType (t : Type) =
        t |> Option.isOptionType && t |> Option.getOptionValueType |> Option.get |> (fun x -> x |> isNonOptionalCollectionType)

    /// <summary>
    /// Determines whether a type represents a collection (optional or not)
    /// </summary>
    /// <param name="t">The type to examine</param>
    let internal isCollectionType (t : Type) =
        t |> isNonOptionalCollectionType || t |> isOptionalCollectionType
                
    /// <summary>
    /// Determines whether a supplied type is a record type
    /// </summary>
    /// <param name="t">The candidate type</param>
    let internal isRecordType(t : Type) =
        FSharpType.IsRecord(t, true)

    /// <summary>
    /// Determines whether a supplied type is a record type
    /// </summary>
    let internal isRecord<'T>() =
        typeof<'T> |> isRecordType

    /// <summary>
    /// Determines whether a supplied type is a union type
    /// </summary>
    /// <param name="t">The candidate type</param>
    let internal isUnionType (t : Type) =
        FSharpType.IsUnion(t, true)

    /// <summary>
    /// Determines whether a supplied type is a union type
    /// </summary>
    let internal isUnion<'T>() =
        typeof<'T> |> isUnionType
    
    let getCollectionValueType (t : Type) =
        //This is far from bullet-proof
        let colltype =
            if t |> isOptionalCollectionType then
                t |> Option.getOptionValueType 
            else if t |> isNonOptionalCollectionType then
                t |> Some
            else
                None
        match colltype with
        | Some(t) ->
            let i = t.GetInterfaces() |> Array.find(fun i -> i.IsGenericType && i.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>)    
            i.GetGenericArguments().[0] |> Some
        | None ->
            None
                
    let getItemValueType (t : Type)  =
        match t |> getCollectionValueType with
        | Some(t) -> t
        | None ->
            match t |> Option.getOptionValueType with
            | Some(t) -> t
            | None ->
                t

    /// <summary>
    /// Determines whether a type is a nullable type
    /// </summary>
    /// <param name="t">The type to examine</param>
    let isNullableType (t : Type) =
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Nullable<_>>

    /// <summary>
    /// Determines whether the type icorresponds to an F# module
    /// </summary>
    /// <param name="t"></param>
    let internal isModuleType (t : Type) =
        t |> FSharpType.IsModule
        
    /// <summary>
    /// Determines whether a type is an array type
    /// </summary>
    /// <param name="t">The type to examine</param>
    let isArray(t : Type) =
        t.IsArray
            
    [<Literal>]
    let private DefaultBindingFlags = 
        BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Static ||| BindingFlags.Instance
    
    /// <summary>
    /// Gets the identified MethodInformation, searching non-public/public/static/instance methods
    /// </summary>
    /// <param name="name">The name of the method</param>
    let getMethod name (subject : Type) =
            subject.GetMethod(name, DefaultBindingFlags)        

    /// <summary>
    /// Gets methods that aren't implemented to serve as property getters/setters
    /// </summary>
    /// <param name="subject">The type</param>
    /// <remarks>
    /// This will probably never be foolproof because its too arbitrary
    /// </remarks>
    let getPureMethods (subject : Type) =
        let isGetOrSet (m : MethodInfo) =
            (m.IsSpecialName && m.Name.StartsWith "get_") || (m.IsSpecialName && m.Name.StartsWith "set_")
        subject.GetMethods(DefaultBindingFlags) |> Array.filter(fun x -> x |> isGetOrSet |> not) |> List.ofArray

    /// <summary>
    /// Gets the properties defined by the type
    /// </summary>
    /// <param name="subject">The type</param>
    let getProperties  (subject : Type) =
        subject.GetProperties(DefaultBindingFlags) |> List.ofArray

    /// <summary>
    /// Gets fields that are not artifacts of the compiler, such as auto-generated backing stores for
    /// property getters/setters
    /// </summary>
    /// <param name="subject">The type</param>
    /// <remarks>
    /// This will probably never be foolproof because its too arbitrary
    /// </remarks>
    let getPureFields (subject : Type) =
        if (subject |> isRecordType |> not) && (subject |> isUnionType |> not) then
            subject.GetFields(DefaultBindingFlags) |> List.ofArray
        else
            []
            
        

    /// <summary>
    /// Retrieves an attribute applied to a type, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttribute<'T when 'T :> Attribute>(subject : Type) =
        if Attribute.IsDefined(subject, typeof<'T>) then
            Attribute.GetCustomAttribute(subject, typeof<'T>) :?> 'T |> Some
        else
            None    

    /// <summary>
    /// Determines the collection kind
    /// </summary>
    /// <param name="t">The type to examine</param>
    let getCollectionKind t =
        let collType = if t |> Option.isOptionType then t |> Option.getOptionValueType |> Option.get else t
        if collType |> isArray then
            ClrCollectionKind.Array
            //This is not the best way to do this...but probably is the fastest
        else if collType.FullName.StartsWith("Microsoft.FSharp.Collections.FSharpList") then        
            ClrCollectionKind.FSharpList            
        else
            ClrCollectionKind.Unclassified
       
    
    /// <summary>
    /// Classifies a type
    /// </summary>
    /// <param name="t">The type to classify</param>
    let getTypeKind t =
        if t |> isCollectionType then
            ClrTypeKind.Collection
        else if t |> isRecordType then
            ClrTypeKind.Record
        else if t |> isModuleType then
            ClrTypeKind.Module
        else if t |> isUnionType then
            ClrTypeKind.Union 
        else if t |> isNullableType then
            ClrTypeKind.NullableValue
        else if t.IsInterface then
            ClrTypeKind.Interface
        else if t.IsClass then
            ClrTypeKind.Class
        else if t.IsValueType then
            ClrTypeKind.Struct
        else
            ClrTypeKind.Unclassified




