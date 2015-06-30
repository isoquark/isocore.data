namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Generic
open System.Linq.Expressions

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
        BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Static ||| BindingFlags.Instance ||| BindingFlags.DeclaredOnly
    
    /// <summary>
    /// Gets the identified MethodInformation, searching non-public/public/static/instance methods
    /// </summary>
    /// <param name="name">The name of the method</param>
    let getMethod name (subject : Type) =
            subject.GetMethod(name, DefaultBindingFlags)        

    let private isPureMethod (m : MethodInfo) =
        let isGetOrSet (m : MethodInfo) =
            (m.IsSpecialName && m.Name.StartsWith "get_") || (m.IsSpecialName && m.Name.StartsWith "set_")
        m |> isGetOrSet |> not

    /// <summary>
    /// Gets methods that aren't implemented to serve as property getters/setters
    /// </summary>
    /// <param name="subject">The type</param>
    /// <remarks>
    /// This will probably never be foolproof because its too arbitrary
    /// </remarks>
    let getPureMethods (subject : Type) =
        subject.GetMethods(DefaultBindingFlags) |> Array.filter(fun x -> x |> isPureMethod) |> List.ofArray

    /// <summary>
    /// Gets the properties defined by the type
    /// </summary>
    /// <param name="subject">The type</param>
    let getProperties  (subject : Type) =
        subject.GetProperties(DefaultBindingFlags) |> List.ofArray

    let private isDeclaredField (f : FieldInfo) =
        (f.DeclaringType |> isRecordType |> not) && (f.DeclaringType |> isUnionType |> not)

            
    /// <summary>
    /// Gets members that are not artifacts of the compiler, such as auto-generated fields that are 
    /// backing stores for property getters/setters
    /// </summary>
    /// <param name="subject">The type</param>
    /// <remarks>
    /// This will probably never be foolproof because its too arbitrary
    /// </remarks>
    let getDeclaredMembers (subject : Type) =
        if subject |> isRecordType then
            FSharpType.GetRecordFields(subject, true) |> Array.map(fun x -> x :> MemberInfo)
        else       
            subject.GetMembers(DefaultBindingFlags) |> Array.filter(fun m ->
                match m with
                | :? MethodInfo as x  -> x |> isPureMethod
                | :? PropertyInfo  -> true
                | :? FieldInfo as x -> x |> isDeclaredField
                | :? ConstructorInfo -> false
                | :? Type -> false
                | :? EventInfo -> false
                | _ -> nosupport()
            ) 
        |> List.ofArray

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

    /// <summary>
    /// Gets types that are (directly) nested within a spcecified type
    /// </summary>
    /// <param name="subject">The type whose nested types are returned</param>
    let getNestedTypes (subject : Type) =
        subject.GetNestedTypes(BindingFlags.Public ||| BindingFlags.NonPublic) |> List.ofArray

    /// <summary>
    /// Gets a <see cref="System.Type" /> from a type name
    /// </summary>
    /// <param name="name"></param>
    /// <remarks>
    /// This is inlined to increase the cases in which the type can be obtained using only
    /// the full name and not the AQN; if the type is in the currently executing assembly
    /// (or mscorlib) it can be resolved with the full name
    /// </remarks>
    let inline fromName (name : ClrTypeName) =
        match name with ClrTypeName(_, fullName, aqName) ->  Type.GetType(defaultArg aqName fullName.Value)

    /// <summary>
    /// Gets the type's access specifier
    /// </summary>
    /// <param name="t"></param>
    let getAccess (t : Type) =
        if t.IsPublic  || t.IsNestedPublic then
            PublicAccess 
        else if t.IsNestedPrivate then
            PrivateAccess
        else if t.IsNotPublic || t.IsNestedAssembly then
            InternalAccess 
        else if t.IsNestedFamORAssem then
            ProtectedOrInternalAccess 
        else if t.IsNestedFamily then
            ProtectedAccess
        else
            nosupport()
        
        
       

[<AutoOpen>]
module TypeExtensions =
    /// <summary>
    /// Gets the properties defined by the type
    /// </summary>
    let props<'T> = typeof<'T> |> Type.getProperties

    type Type
    with
        member this.IsOptionType = this |> Option.isOptionType

        /// <summary>
        /// If optional type, gets the type of the underlying value; otherwise, the type itself
        /// </summary>
        member this.ItemValueType = this |> Type.getItemValueType

        member this.Access = this |> Type.getAccess


    /// <summary>
    /// Defines augmentations for the Assembly type
    /// </summary>
    type Assembly
    with
        /// <summary>
        /// Gets the short name of the assembly without version/culture/security information
        /// </summary>
        member this.ShortName = this.GetName().Name    

