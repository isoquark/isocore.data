﻿namespace IQ.Core.Framework

open System
open System.Reflection
open System.IO

open System.Collections.Generic

open Microsoft.FSharp.Reflection


/// <summary>
/// Defines System.Reflection.ParameterInfo helpers
/// </summary>
module ParameterInfo =
    let getAttribute<'T when 'T :> Attribute>(subject : ParameterInfo) =
        if Attribute.IsDefined(subject, typeof<'T>) then
            Attribute.GetCustomAttribute(subject, typeof<'T>) :?> 'T |> Some
        else
            None


/// <summary>
/// Defines System.MemberInfo helpers
/// </summary>
module MemberInfo =
    /// <summary>
    /// Retrieves an attribute applied to a member, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttribute<'T when 'T :> Attribute>(subject : MemberInfo) =
        if Attribute.IsDefined(subject, typeof<'T>) then
            Attribute.GetCustomAttribute(subject, typeof<'T>) :?> 'T |> Some
        else
            None    

/// <summary>
/// Defines System.MethodInfo helpers
/// </summary>
module MethodInfo =
    /// <summary>
    /// Retrieves an attribute applied to a member, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttribute<'T  when 'T :> Attribute>(subject : MethodInfo) =
        subject |> MemberInfo.getAttribute<'T>

    /// <summary>
    /// Retrieves an attribute applied to a method return, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getReturnAttribute<'T when 'T :> Attribute>(subject : MethodInfo) =
        let attribs = subject.ReturnTypeCustomAttributes.GetCustomAttributes(typeof<'T>, true)
        if attribs.Length <> 0 then
            attribs.[0] :?> 'T |> Some
        else
            None
        

/// <summary>
/// Defines Microsoft.FSharp.Reflection.UnionCaseInfo helpers
/// </summary>
module UnionCaseInfo =
    /// <summary>
    /// Retrieves identified custom attribute if applied
    /// </summary>
    let getAttribute<'T when 'T :> Attribute>(subject : UnionCaseInfo) =
        let attribs = subject.GetCustomAttributes(typeof<'T>)
        if attribs.Length <> 0 then
            attribs.[0] :?> 'T |> Some
        else
            None
    

[<AutoOpen>]
module MethodInfoExtensions =    
    /// <summary>
    /// Gets the currently executing method
    /// </summary>
    /// <remarks>
    /// Note that since the method is designated inline, the call to get the executing
    /// method is injected at the call-site and so works as expected
    /// </remarks>
    let inline thisMethod() = MethodInfo.GetCurrentMethod()

    

/// <summary>
/// Defines Sytem.Type helpers
/// </summary>
module Type =

    /// <summary>
    /// Determines whether a type is a nullable type
    /// </summary>
    /// <param name="t">The type to examine</param>
    let isNullableType (t : Type) =
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Nullable<_>>
        
    /// <summary>
    /// Determines whether a type is an array type
    /// </summary>
    /// <param name="t">The type to examine</param>
    let isArray(t : Type) =
        t.IsArray
            
    /// <summary>
    /// Retrieves an attribute applied to a type, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttribute<'T when 'T :> Attribute>(subject : Type) =
        subject |> MemberInfo.getAttribute<'T>

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
    /// Determines whether a named assembly has been loaded
    /// </summary>
    /// <param name="name">The name of the assembly</param>
    let isLoaded (name : AssemblyName) =
        AppDomain.CurrentDomain.GetAssemblies() 
            |> Array.map(fun a -> a.GetName()) 
            |> Array.exists (fun n -> n = name)
    
    /// <summary>
    /// Recursively loads assembly references into the application domain
    /// </summary>
    /// <param name="subject">The staring assembly</param>
    let rec loadReferences (filter : string option) (subject : Assembly) =
        let references = subject.GetReferencedAssemblies()
        let filtered = match filter with
                        | Some(filter) -> 
                            references |> Array.filter(fun x -> x.Name.StartsWith(filter)) 
                        | None ->
                            references

        filtered |> Array.iter(fun name ->
            if name |> isLoaded |>not then
                name |> AppDomain.CurrentDomain.Load |> loadReferences filter
        )

/// <summary>
/// Defines System.Assembly helpers
/// </summary>
[<AutoOpen>]
module AssemblyExtensions =

    /// <summary>
    /// Gets the currently executing assembly
    /// </summary>
    /// <remarks>
    /// Note that since the method is designated inline, the call to get the executing
    /// assembly is injected at the call-site and so works as expected
    /// </remarks>
    let inline thisAssembly() = Assembly.GetExecutingAssembly()

    /// <summary>
    /// Defines augmentations for the Assembly type
    /// </summary>
    type Assembly
    with
        /// <summary>
        /// Gets the short name of the assembly without version/culture/security information
        /// </summary>
        member this.ShortName = this.GetName().Name    


