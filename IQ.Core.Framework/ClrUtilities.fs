// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System
open System.Reflection
open System.IO
open System.Collections.Generic
open System.Linq.Expressions
open System.Diagnostics
open System.Runtime.CompilerServices

open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns

type GenericList<'T> = System.Collections.Generic.List<'T>
type FSharpList<'T> = Microsoft.FSharp.Collections.List<'T>


[<AutoOpen>]
module ClrUtilityVocabulary =
    
    
    /// <summary>
    /// Encapsulates (selected) field attributes
    /// </summary>
    /// <remarks>
    /// See <see cref="System.Reflection.FieldAttributes"/> for the corresponding enumeration
    /// </remarks>
    type FieldFacetSet = {
        IsSpecialName : bool
        /// Specifies that the value of the field is asociated with the type in which it is 
        /// declared and not instances of the type
        IsStatic : bool
        /// Specifies that the field has a default value
        HasDefault : bool
        /// Specifies that the field can only be mutated within the body of constructor
        IsInitOnly : bool
        /// Specifies that the value of the field is baked-in during compilation and cannot be
        /// mutated at runtime
        IsLiteral : bool
    }
    with 
        static member Blank = 
            {
                IsSpecialName = false
                IsStatic = false
                HasDefault = false
                IsInitOnly = false
                IsLiteral = false
            }
    

/// <summary>
/// Defines <see cref="ClrElementName"/>-related augmentations 
/// </summary>
[<AutoOpen>]
module ClrNameExtensions =
    
    /// <summary>
    /// Defines augmentations for the <see cref="ClrTypeName"/> type
    /// </summary>
    type ClrTypeName 
    with
        member this.Text = 
            match this with 
                ClrTypeName(simpleName, fullName, aqn) ->
                    match aqn with
                    | Some(x) -> x
                    | None ->
                        match fullName with
                        | Some(x) -> x
                        | None ->
                            simpleName

        /// <summary>
        /// Gets the local name of the type (which does not include enclosing type names or namespace)
        /// </summary>
        member this.SimpleName = match this with ClrTypeName(simpleName=x) -> x
        /// <summary>
        /// Gets namespace and nested type-qualified name of the type
        /// </summary>
        member this.FullName = match this with ClrTypeName(fullName=x) -> x
        /// <summary>
        /// Gets the assembly-qualified type name of the type
        /// </summary>
        member this.AssemblyQualifiedName = match this with ClrTypeName(assemblyQualifiedName=x) -> x

        
    /// <summary>
    /// Defines augmentations for the <see cref="ClrAssemblyName"/> type
    /// </summary>
    type ClrAssemblyName
    with
        /// <summary>
        /// Gets the simple name of the assembly
        /// </summary>
        member this.SimpleName = match this with ClrAssemblyName(simpleName=x) -> x
        member this.FullName = match this with ClrAssemblyName(fullName=x) -> x
        member this.Text =
            match this with ClrAssemblyName(simpleName, fullName) -> match fullName with
                                                                        | Some(x) -> x
                                                                        | None ->
                                                                            simpleName    
    /// <summary>
    /// Defines augmentations for the <see cref="ClrMemberElementName"/> type
    /// </summary>
    type ClrMemberName
    with
        member this.Text = match this with ClrMemberName(x) -> x
    
    /// <summary>
    /// Defines augmentations for the <see cref="ClrParameterElementName"/> type
    /// </summary>
    type ClrParameterName
    with
        member this.Text = match this with ClrParameterName(x) -> x
        

    /// <summary>
    /// Defines augmentations for the <see cref="ClrElementName"/> type
    /// </summary>
    type ClrElementName
    with
        /// <summary>
        /// Gets the simple/unqualified name of the element
        /// </summary>
        member this.SimpleName =
            match this with 
                | AssemblyElementName(x) -> x.SimpleName
                | TypeElementName(x) -> x.SimpleName
                | MemberElementName(x) -> x.Text
                | ParameterElementName(x) -> x.Text           

        member this.Text =
            match this with 
                | AssemblyElementName(x) -> x.Text
                | TypeElementName(x) -> x.Text
                | MemberElementName(x) -> x.Text
                | ParameterElementName(x) -> x.Text

            


module ReflectedKind =
    let fromInstance (o : obj) =
        match o with
        | :? Assembly -> ReflectedKind.Assembly
        | :? Type -> ReflectedKind.Type
        | :? MethodInfo -> ReflectedKind.Method
        | :? PropertyInfo -> ReflectedKind.Property
        | :? FieldInfo -> ReflectedKind.Field
        | :? ConstructorInfo -> ReflectedKind.Constructor
        | :? EventInfo -> ReflectedKind.Event
        | :? ParameterInfo -> ReflectedKind.Parameter
        | :? UnionCaseInfo -> ReflectedKind.UnionCase
        | _ -> nosupport()



/// <summary>
/// Defines operations for working with collections
/// </summary>
module Collection =
    let private makeGenericType (baseType : Type) (types : Type list) = 

      if (not baseType.IsGenericTypeDefinition) then
        invalidArg "baseType" "baseType must be a generic type definition."

      baseType.MakeGenericType (types|> List.toArray)

    /// <summary>
    /// Creates an F# list
    /// </summary>
    /// <param name="itemType">The type of items that the list will contain</param>
    /// <param name="items">The items with which to populate the list</param>
    /// <remarks>
    /// I'm not crazy about this approach; it's logical but surely there's a better way.
    /// Source: http://blog.usermaatre.co.uk/programming/2013/07/24/fsharp-collections-reflection
    /// </remarks>
    let private createList itemType (items : obj list)  =  
         let listType = 
             makeGenericType <| typedefof<FSharpList<_>> <| [ itemType; ]
 
         let add =  
             let cons =  listType.GetMethod ("Cons")             
             fun item list -> 
                cons.Invoke (null, [| item; list; |])                
 
         let list =  
             let empty = listType.GetProperty ("Empty") 
             empty.GetValue (null) 

         list |> List.foldBack add items    
    
    /// <summary>
    /// Creates an array
    /// </summary>
    /// <param name="itemType">The type of items that the array will contain</param>
    /// <param name="items">The items with which to populate the list</param>
    let private createArray itemType (items : obj list) =
        let a = Array.CreateInstance(itemType, items.Length)
        [0..items.Length-1] |> List.iter(fun i -> a.SetValue(items.[i], i))
        a :> obj

    /// <summary>
    /// Creates a generic list
    /// </summary>
    /// <param name="itemType">The type of items that the list will contain</param>
    /// <param name="items">The items with which to populate the list</param>
    let private createGenericList itemType (items : obj list) =
        let listType = makeGenericType <| typedefof<GenericList<_>> <| [itemType;]
        let list = Activator.CreateInstance(listType)
        let add = listType.GetMethod("Add")
        items |> List.iter(fun item -> add.Invoke(list, [|item|]) |> ignore)
        list
        
    /// <summary>
    /// Creates a collection
    /// </summary>
    /// <param name="kind">The kind of collection</param>
    /// <param name="itemType">The type of items that the collection will contain</param>
    /// <param name="items">The items with which to populate the collection</param>
    let create kind itemType items =
        match kind with
        | ClrCollectionKind.FSharpList ->
            items |> createList itemType
        | ClrCollectionKind.Array ->
            items |> createArray itemType
        | ClrCollectionKind.GenericList ->
            items |> createGenericList itemType
        | _ -> nosupport()

/// <summary>
/// Defines utility methods for working with options
/// </summary>
module Option =
    
    /// <summary>
    /// Determines whether a type is an option type
    /// </summary>
    /// <param name="t">The type to examine</param>
    let isOptionType (t : Type) =
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>        

    /// <summary>
    /// Determines whether a value is an option
    /// </summary>
    /// <param name="value">The value to examine</param>
    let isOptionValue (value : obj) =
        if value <> null then
            value.GetType() |> isOptionType
        else
            false


    /// <summary>
    /// Gets the type of the encapsulated value
    /// </summary>
    /// <param name="optionType">The option type</param>
    let getOptionValueType (t : Type) =
        if t |> isOptionType  then t.GetGenericArguments().[0] |> Some else None

    
    /// <summary>
    /// Extracts the enclosed value if Some, otherwise yields None
    /// </summary>
    /// <param name="value">The option value</param>
    let unwrapValue (value : obj) =
        if value = null then 
            None
        else
            _assert "Value is not an option" (fun () -> isOptionValue(value) )
            let caseInfo, fields = FSharpValue.GetUnionFields(value, value.GetType(),true)
            if fields.Length = 0 then
                None
            else
                fields.[0] |> Some

    /// <summary>
    /// Encloses a supplied value within Some option
    /// </summary>
    /// <param name="value">The value to enclose</param>
    let makeSome (value : obj) =
        if value = null then
            ArgumentNullException() |> raise
        
        let valueType = value.GetType()
        let optionType = typedefof<option<_>>.MakeGenericType(valueType)
        let unionCase = FSharpType.GetUnionCases(optionType,true) |> Array.find(fun c -> c.Name = "Some")
        FSharpValue.MakeUnion(unionCase, [|value|], true)

    /// <summary>
    /// Creates an option with the case None
    /// </summary>
    /// <param name="valueType">The value's type</param>
    let makeNone (valueType : Type) =
        let optionType = typedefof<option<_>>.MakeGenericType(valueType)
        let unionCase = FSharpType.GetUnionCases(optionType,true) |> Array.find(fun c -> c.Name = "None")
        FSharpValue.MakeUnion(unionCase, [||], true)


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
                
    /// <summary>
    /// Determines whether a type is a nullable type
    /// </summary>
    /// <param name="t">The type to examine</param>
    let isNullableType (t : Type) =
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Nullable<_>>

    /// <summary>
    /// If encapsulated within a recognized "container", e.g., an optional value, a nullable value or a collection,
    /// returns the encapsulated type; otherwise, returns the passed type
    /// </summary>
    /// <param name="t"></param>
    let getItemValueType (t : Type)  =
        match t |> getCollectionValueType with
        | Some(t) -> t
        | None ->
            match t |> Option.getOptionValueType with
            | Some(t) -> t
            | None ->
                if t |> isNullableType then
                    t.GetGenericArguments().[0]
                else
                    t


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

    /// <summary>
    /// Attempts to determine whether a given method has been explicitly declared by the
    /// user, instead of an artifact generated by the compiler to support various
    /// language features
    /// </summary>
    /// <param name="f">The field to examine</param>
    let private isDeclaredMethod (m : MethodInfo) =
        let isGetOrSet (m : MethodInfo) =
            (m.Name.StartsWith "get_") || (m.Name.StartsWith "set_") || m.IsSpecialName
        m |> isGetOrSet |> not

    /// <summary>
    /// Gets methods that aren't implemented to serve as property getters/setters
    /// </summary>
    /// <param name="subject">The type</param>
    /// <remarks>
    /// This will probably never be foolproof because its too arbitrary
    /// </remarks>
    let getDelcaredMethods (t : Type) =
        t.GetMethods(DefaultBindingFlags) |> Array.filter(fun x -> x |> isDeclaredMethod) |> List.ofArray

    /// <summary>
    /// Gets the properties defined by the type
    /// </summary>
    /// <param name="subject">The type</param>
    let getProperties  (t : Type) =
        t.GetProperties(DefaultBindingFlags) |> List.ofArray

    /// <summary>
    /// Gets all of the fields declared by the type
    /// <param name="t">The type whose fields are to be retrieved</param>
    /// </summary>
    let getFields(t : Type) = t.GetFields(DefaultBindingFlags) |> List.ofArray


    /// <summary>
    /// Attempts to determine whether a given field has been explicitly declared by the
    /// user, instead of an artifact generated by the compiler to support various
    /// language features
    /// </summary>
    /// <param name="f">The field to examine</param>
    let private isDeclaredField (f : FieldInfo) =
        (f.DeclaringType |> isRecordType |> not) && (f.DeclaringType |> isUnionType |> not) && (f.IsSpecialName |> not)


    /// <summary>
    /// Retrieves all members declared by the type
    /// </summary>
    /// <param name="t">The type to examine</param>
    let getMembers (t : Type) = t.GetMembers(DefaultBindingFlags) |> List.ofArray
            
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
                | :? MethodInfo as x  -> x |> isDeclaredMethod
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
    let getKind (t : Type) =
        if t |> isCollectionType then
            ClrTypeKind.Collection
        else if t |> isRecordType then
            ClrTypeKind.Record
        else if t |> isModuleType then
            ClrTypeKind.Module
        else if t |> isUnionType then
            ClrTypeKind.Union 
        else if t.IsInterface then
            ClrTypeKind.Interface
        else if t.IsClass then
            ClrTypeKind.Class
        else if t.IsEnum then
            ClrTypeKind.Enum
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
    /// <param name="t">The type to examine</param>
    let getAccess (t : Type) =
        if t.IsPublic  || t.IsNestedPublic then
            ClrAccessKind.Public 
        else if t.IsNestedPrivate then
            ClrAccessKind.Private
        else if t.IsNotPublic || t.IsNestedAssembly then
            ClrAccessKind.Internal
        else if t.IsNestedFamORAssem then
            ClrAccessKind.ProtectedOrInternal
        else if t.IsNestedFamily then
            ClrAccessKind.Protected
        else
            nosupport()
    /// <summary>
    /// Gets the <see cref="System.Type"/> of the identified type
    /// </summary>
    /// <param name="name"></param>
    let get(name : ClrTypeName) =
        Type.GetType(name.Text, true)

        
        
/// <summary>
/// Defines System.MemberInfo helpers
/// </summary>
module Member =
    /// <summary>
    /// Determines the member kind from a supplied member
    /// </summary>
    /// <param name="m">The member</param>
    let getKind (m : MemberInfo) =
        match m with
        | :? EventInfo ->ClrMemberKind.Event
        | :? MethodInfo -> ClrMemberKind.Method
        | :? PropertyInfo -> ClrMemberKind.Property
        | :? FieldInfo -> ClrMemberKind.StorageField
        | :? ConstructorInfo -> ClrMemberKind.Constructor
        | _ -> nosupport()


/// <summary>
/// Defines <see cref="System.Reflection.MethodBase"/> helpers
/// </summary>
module MethodBase =
    /// <summary>
    /// Gets the methods access specificer
    /// </summary>
    /// <param name="m">The method to examine</param>
    let getAccess (m : MethodBase) =
        if m = null then
            ArgumentException() |> raise
        if m.IsPublic then
            ClrAccessKind.Public 
        else if m.IsPrivate then
            ClrAccessKind.Private
        else if m.IsAssembly then
            ClrAccessKind.Internal
        else if m.IsFamilyOrAssembly then
            ClrAccessKind.ProtectedOrInternal
        else if m.IsFamilyAndAssembly then
            ClrAccessKind.ProtectedAndInternal
        else if m.IsFamily then
            ClrAccessKind.Protected
        else
            nosupport()
    

/// <summary>
/// Defines <see cref="System.Reflection.FieldInfo"/> utilities
/// </summary>
module FieldInfo =
    /// <summary>
    /// Gets the methods access specifier
    /// </summary>
    /// <param name="m"></param>
    let getAccess (m : FieldInfo) =
        if m = null then
            ArgumentException() |> raise
        if m.IsPublic then
            ClrAccessKind.Public 
        else if m.IsPrivate then
            ClrAccessKind.Private
        else if m.IsAssembly then
            ClrAccessKind.Internal
        else if m.IsFamilyOrAssembly then
            ClrAccessKind.ProtectedOrInternal
        else if m.IsFamilyAndAssembly then
            ClrAccessKind.ProtectedAndInternal
        else if m.IsFamily then
            ClrAccessKind.Protected
        else
            nosupport()

    let getFacets (f : FieldInfo) =
        {
            FieldFacetSet.HasDefault = FieldAttributes.HasDefault |> f.Attributes.HasFlag
            IsSpecialName = FieldAttributes.SpecialName |> f.Attributes.HasFlag
            IsStatic =  FieldAttributes.Static|> f.Attributes.HasFlag
            IsInitOnly =  FieldAttributes.InitOnly|> f.Attributes.HasFlag
            IsLiteral =  FieldAttributes.Literal |> f.Attributes.HasFlag
        }


/// <summary>
/// Defines <see cref="System.Reflection.ConstructorInfo"/> utilities
/// </summary>
module ConstructorInfo = 
    /// <summary>
    /// Gets the methods access specificer
    /// </summary>
    /// <param name="m"></param>
    let getAccess (m : ConstructorInfo) =
        if m = null then
            ArgumentException() |> raise
        if m.IsPublic then
            ClrAccessKind.Public 
        else if m.IsPrivate then
            ClrAccessKind.Private
        else if m.IsAssembly then
            ClrAccessKind.Internal
        else if m.IsFamilyOrAssembly then
            ClrAccessKind.ProtectedOrInternal
        else if m.IsFamilyAndAssembly then
            ClrAccessKind.ProtectedAndInternal
        else if m.IsFamily then
            ClrAccessKind.Protected
        else
            nosupport()
            

/// <summary>
/// Defines System.Assembly helpers
/// </summary>
module Assembly =
    /// <summary>
    /// Retrieves a text resource embedded in the subject assembly if found
    /// </summary>
    /// <param name="shortName">The name of the resource, excluding the namespace path</param>
    /// <param name="subject">The assembly that contains the resource</param>
    let findTextResource shortName (subject : Assembly) =        
        match subject.GetManifestResourceNames() |> Array.tryFind(fun name -> name.Contains(shortName)) with
        | Some(resname) ->
            use s = resname |> subject.GetManifestResourceStream
            use r = new StreamReader(s)
            r.ReadToEnd() |> Some
        | None ->
            None

    /// <summary>
    /// Writes a text resource contained in an assembly to a file and returns the path
    /// </summary>
    /// <param name="shortName">The name of the resource, excluding the namespace path</param>
    /// <param name="outputDir">The directory into which the resource will be deposited</param>
    /// <param name="subject">The assembly that contains the resource</param>
    let writeTextResource shortName outputDir (subject : Assembly) =
        let path = Path.ChangeExtension(outputDir, shortName) 
        match subject |> findTextResource shortName with
        | Some(text) -> File.WriteAllText(path, text)
        | None ->
            ArgumentException(sprintf "Resource %s not found" shortName) |> raise
        path

    /// <summary>
    /// Writes an embedded resource to a file
    /// </summary>
    /// <param name="subject">The starting assembly</param>
    let emitResource (shortName : string) (outputDir : string) (subject : Assembly) =
        let path = Path.Combine(outputDir, shortName)
        let name = subject.GetManifestResourceNames() 
                    |> Array.find(fun x -> shortName |> x.Contains)
        use src = name |> subject.GetManifestResourceStream
        use dst = path |> File.Create
        dst |> src.CopyTo
        path
    

        


/// <summary>
/// Defines System.AppDomain helpers
/// </summary>
module AppDomain =
 
    let private findPotentialMatches (clrName : ClrAssemblyName) (domain : AppDomain) =
        let isPotentialMatch (_, assname : AssemblyName) =
            match clrName with 
                ClrAssemblyName(simpleName,fullName) ->
                    match fullName with
                    | Some(fullName) -> assname.FullName = fullName
                    | None -> simpleName = assname.Name
        
        domain.GetAssemblies() |> Array.map(fun x -> x, x.GetName()) |> Array.filter isPotentialMatch
    
    /// <summary>
    /// Searches the application domain for a specified assembly
    /// </summary>
    /// <param name="clrName">The name of the assembly</param>
    /// <param name="domain">The domain to search</param>
    let tryFindAssembly (name : ClrAssemblyName) (domain : AppDomain)=
        
        let matches = domain |> findPotentialMatches name 
        if matches.Length <> 0 then
            if matches.Length = 1 then
                matches.[0] |> fst |> Some
            else
                failwith "Ambiguous assembly match"
        else
            None

    /// <summary>
    /// Gets the identified assembly, attempting to load it if not currently loaded
    /// </summary>
    /// <param name="clrName">The name of the assembly</param>
    /// <param name="domain">The application domain into which the assembly will be loaded</param>
    let acquireAssembly (name : ClrAssemblyName)  (domain : AppDomain) =
        let matches = domain |> findPotentialMatches name 
        match tryFindAssembly name domain with
        | Some(a) -> a
        | None -> 
            AssemblyName(name.Text) |> domain.Load



[<AutoOpen>]
module ReflectionExtensions = 

    /// <summary>
    /// Gets the currently executing assembly
    /// </summary>
    /// <remarks>
    /// Note that since the method is designated inline, the call to get the executing
    /// assembly is injected at the call-site and so works as expected
    /// </remarks>
    let inline thisAssembly() = Assembly.GetExecutingAssembly()
        
    /// <summary>
    /// Gets the currently executing constructor
    /// </summary>
    /// <remarks>
    /// Note that since the method is designated inline, the call to get the executing
    /// method is injected at the call-site and so works as expected
    /// </remarks>
    let inline thisConstructor() = MethodInfo.GetCurrentMethod() :?> ConstructorInfo

    /// <summary>
    /// Gets the currently executing method (not to be used for constructors!)
    /// </summary>
    /// <remarks>
    /// Note that since the method is designated inline, the call to get the executing
    /// method is injected at the call-site and so works as expected
    /// </remarks>
    let inline thisMethod() = MethodInfo.GetCurrentMethod() :?> MethodInfo

    /// <summary>
    /// Extracts the MethodInfo of a function used in a lambda expression
    /// </summary>
    /// <remarks>
    /// Useful for quotations of the form <@fun () -> myfunc @>
    /// </remarks>
    let rec funcinfo q =
        match q with
        | Lambda(_, expr) -> funcinfo expr
        | Call(_,m,_) -> m
        | _ -> nosupport()

    /// <summary>
    /// Extracts the name of a function used in a lambda expression
    /// </summary>
    /// <remarks>
    /// Useful for quotations of the form <@fun () -> myfunc @>
    /// </remarks>
    let funcname q = q |> funcinfo |> fun x -> x.Name

            
    /// <summary>
    /// When supplied a property accessor quotation, retrieves the property information
    /// </summary>
    /// <param name="q">The property accessor quotation</param>
    let rec propinfo q =
       match q with
       | PropertyGet(_,p,_) -> p
       | Lambda(_, expr) -> propinfo expr
       | _ -> nosupport()

    /// <summary>
    /// When supplied a property accessor quotation, retrieves the field information
    /// </summary>
    /// <param name="q">The property accessor quotation</param>
    let rec fieldinfo q =
        match q with
        | FieldGet(_,f) -> f
        | Lambda(_, expr) -> fieldinfo expr
        | _ -> nosupport()

    /// <summary>
    /// When supplied a property accessor quotation, retrieves the name of the property
    /// </summary>
    /// <param name="q">The property accessor quotation</param>
    let rec propname q =
       match q with
       | PropertyGet(_,p,_) -> p.Name |> ClrMemberName 
       | Lambda(_, expr) -> propname expr
       | _ -> nosupport()

    /// <summary>
    /// Gets the properties declared by the type
    /// </summary>
    let props<'T> = typeof<'T> |> Type.getProperties

    /// <summary>
    /// Gets the fields declared by the type
    /// </summary>
    let fields<'T> = typeof<'T> |> Type.getFields            
    
    /// <summary>
    /// Gets the members declared by the type
    /// </summary>
    let members<'T> = typeof<'T> |> Type.getMembers

    type Enum
    with
        member this.MemberName = ClrMemberName(this.ToString())
    
    type MemberInfo
    with
        member this.MemberName = ClrMemberName(this.Name)
    

[<AutoOpen>]
module CommonExtensions = 
    /// <summary>
    /// Defines augmentations for the <see cref="System.Type"/> type
    /// </summary>
    type Type
    with
        /// <summary>
        /// Gets the type name
        /// </summary>
        member this.TypeName = 
                ClrTypeName(this.Name, this.FullName |> Some, this.AssemblyQualifiedName |> Some)

        /// <summary>
        /// Gets the element name of the type
        /// </summary>
        member this.ElementName =  this.TypeName |> TypeElementName

        /// <summary>
        /// Specifies whether the type is an F# option type
        /// </summary>
        member this.IsOptionType = this |> Option.isOptionType

        /// <summary>
        /// If optional type, gets the type of the underlying value; otherwise, the type itself
        /// </summary>
        member this.ItemValueType = this |> Type.getItemValueType


        /// <summary>
        /// Gets the access specifier applied to the type
        /// </summary>
        member this.Access = this |> Type.getAccess

        /// <summary>
        /// Gets the type kind
        /// </summary>
        member this.Kind = this |> Type.getKind


                
    /// <summary>
    /// Defines augmentations for the <see cref="System.Assembly"/> type
    /// </summary>
    type Assembly
    with
        /// <summary>
        /// Gets the assembly name
        /// </summary>
        member this.AssemblyName = ClrAssemblyName(this.GetName().Name, this.FullName |> Some)
        /// <summary>
        /// Gets the element name of the assembly
        /// </summary>
        member this.ElementName =  this.AssemblyName |> AssemblyElementName
        /// <summary>
        /// Gets the short/simple name of the assembly
        /// </summary>
        member this.SimpleName = this.GetName().Name

    /// <summary>
    /// Defines augmentations for the <see cref="System.AppDomain"/> type
    /// </summary>
    type AppDomain
    with
        member this.AcquireAssembly (name : ClrAssemblyName) =
            this |> AppDomain.acquireAssembly name

        /// <summary>
        /// Gets the user assemblies that are currently loaded in the domain
        /// </summary>
        member this.GetUserAssemblies() =
            AppDomain.CurrentDomain.GetAssemblies() 
            |> Array.filter(fun a -> a.SimpleName.StartsWith(CoreConfiguration.UserAssemblyPrefix))

        /// <summary>
        /// Gets the names of the assemblies that are currently loaded in the domain
        /// </summary>
        member this.GetUserAssemblyNames() =
             this.GetUserAssemblies() |> Array.map(fun a -> a.AssemblyName) |> List.ofArray


    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.PropertyInfo"/> type
    /// </summary>
    type PropertyInfo
    with
        /// <summary>
        /// Gets the element name of the field
        /// </summary>
        member this.ElementName =  this.MemberName |> MemberElementName
        /// <summary>
        /// Gets the ensconced property type
        /// </summary>
        member this.ItemValueType = this.PropertyType |> Type.getItemValueType

    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.FieldInfo"/> type
    /// </summary>
    type FieldInfo
    with
        /// <summary>
        /// Gets the element name of the field
        /// </summary>
        member this.ElementName =  this.MemberName |> MemberElementName
        /// <summary>
        /// Gets the ensconced field type
        /// </summary>
        member this.ItemValueType = this.FieldType |> Type.getItemValueType
        /// <summary>
        /// Gets the access specifier applied to the field
        /// </summary>
        member this.Access = this |> FieldInfo.getAccess
        /// <summary>
        /// Gets the field's facet set
        /// </summary>
        member this.Facets = this |> FieldInfo.getFacets
        

    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.MethodBase"/> type
    /// </summary>
    type MethodBase
    with
        /// <summary>
        /// Gets the element name of the method/constructor
        /// </summary>
        member this.ElementName = this.MemberName |> MemberElementName
        /// <summary>
        /// Gets the access specifier applied to the method/constructor
        /// </summary>
        member this.Access = this |> MethodBase.getAccess
        
    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.ParameterInfo"/> type
    /// </summary>
    type ParameterInfo
    with
        /// <summary>
        /// Gets the parameter name
        /// </summary>
        member this.ParameterName = this.Name |> ClrParameterName
        /// <summary>
        /// Gets the element name of the parameter
        /// </summary>
        member this.ElementName =  this.ParameterName |> ParameterElementName
        
    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.EventInfo"/> type
    /// </summary>
    type EventInfo
    with
        /// <summary>
        /// Gets the element name of the parameter
        /// </summary>
        member this.ElementName =  this.MemberName |> MemberElementName
        
