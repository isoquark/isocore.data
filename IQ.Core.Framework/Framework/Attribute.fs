namespace IQ.Core.Framework

open System
open System.Reflection

/// <summary>
/// Defines non-generic attribute discovery helper methods
/// </summary>
module Attribute =
    /// <summary>
    /// Retrieves all attributes applied to the element
    /// </summary>
    /// <param name="element">The element to examine</param>
    let getAll(element : ClrElement) =
        match element with
        | MethodElement(x) -> 
            x |> Attribute.GetCustomAttributes
        | PropertyElement(x) ->
            x |> Attribute.GetCustomAttributes
        | TypeElement(x) -> 
            x.Type |> Attribute.GetCustomAttributes
        | FieldElement(x) ->
            x |> Attribute.GetCustomAttributes
        | AssemblyElement(x) ->
            x |> Attribute.GetCustomAttributes
        | ParameterElement(x) ->
            x |> Attribute.GetCustomAttributes
        | UnionCaseElement(x) ->
            [|for a in x.GetCustomAttributes() -> a :?> Attribute|]
        |> List.ofArray
    
    /// <summary>
    /// Determines whether an attribute of a specified type has been applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let isApplied (element : ClrElement) (attribType : Type) =
        match element with
        | MethodElement(x) -> 
            Attribute.IsDefined(x, attribType) 
        | PropertyElement(x) ->
            Attribute.IsDefined(x, attribType) 
        | TypeElement(x) -> 
            Attribute.IsDefined(x.Type, attribType) 
        | FieldElement(x) ->
            Attribute.IsDefined(x, attribType) 
        | AssemblyElement(x) ->
            Attribute.IsDefined(x, attribType) 
        | ParameterElement(x) ->
            Attribute.IsDefined(x, attribType) 
        | UnionCaseElement(x) ->
            x.GetCustomAttributes() |> Array.filter(fun a -> a.GetType() = attribType) |> Array.isEmpty |> not

    /// <summary>
    /// Retrieves an attribute from the element if it exists and returns None if it odes not
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let tryGetOne (element : ClrElement) (attribType : Type) =
        if attribType |> isApplied element then
            match element with
            | MethodElement(x) -> 
                Attribute.GetCustomAttribute(x, attribType) 
            | PropertyElement(x) ->
                Attribute.GetCustomAttribute(x, attribType) 
            | TypeElement(x) -> 
                Attribute.GetCustomAttribute(x.Type, attribType) 
            | FieldElement(x) ->
                Attribute.GetCustomAttribute(x, attribType) 
            | AssemblyElement(x) ->
                Attribute.GetCustomAttribute(x, attribType) 
            | ParameterElement(x) ->
                Attribute.GetCustomAttribute(x, attribType)             
            | UnionCaseElement(x) ->
                x.GetCustomAttributes() |> Array.find(fun a -> a.GetType() = attribType) :?> Attribute
            |> Some
        else
            None    

    /// <summary>
    /// Retrieves an attribute from the element if it exists and raises an exception otherwise
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getOne (element : ClrElement) (attribType : Type) =
        attribType |> tryGetOne element |> Option.get
    
    /// <summary>
    /// Retrieves all matching attributes applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getMany (element : ClrElement) (attribType : Type)  =
        match element with
        | MethodElement(x) -> 
            Attribute.GetCustomAttributes(x, attribType) 
        | PropertyElement(x) ->
            Attribute.GetCustomAttributes(x, attribType) 
        | TypeElement(x) -> 
            Attribute.GetCustomAttributes(x.Type, attribType) 
        | FieldElement(x) ->
            Attribute.GetCustomAttributes(x, attribType) 
        | AssemblyElement(x) ->
            Attribute.GetCustomAttributes(x, attribType) 
        | ParameterElement(x) ->
            Attribute.GetCustomAttributes(x, attribType)      
        | UnionCaseElement(x) ->
            [|for a in x.GetCustomAttributes() do if a.GetType() = attribType then yield a :?> Attribute|]
        |> List.ofArray
        

/// <summary>
/// Defines generic attribute discovery helper methods
/// </summary>
module AttributeT = 
    
    /// <summary>
    /// Determines whether an attribute is applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    let isDefined<'T when 'T :> Attribute>(element : ClrElement) =
        typeof<'T> |> Attribute.isApplied  element        

    /// <summary>
    /// Retrieves an attribute applied to a member, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let tryGetOne<'T when 'T :> Attribute>(element : ClrElement) =
        match typeof<'T> |> Attribute.tryGetOne element with
        | Some(x) -> x :?> 'T |> Some
        | None -> None
    
    /// <summary>
    /// Retrieves an attribute from the element if it exists and raises an exception otherwise
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getOne<'T when 'T :> Attribute>(element : ClrElement) =
        element |> tryGetOne<'T> |> Option.get

    /// <summary>
    /// Retrieves an arbitrary number of attributes of the same type applied to a member
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getMany<'T when 'T :> Attribute>(subject : MemberInfo) =
        [for a in Attribute.GetCustomAttributes(subject, typeof<'T>) -> a :?> 'T]


