namespace IQ.Core.Framework

open System
open System.Reflection

open Microsoft.FSharp.Reflection

/// <summary>
/// Defines operations applicable to the unified ClrElement type
/// </summary>
module ClrElement =
    
    /// <summary>
    /// Reads an identified attribute from the element if applied
    /// </summary>
    /// <param name="element">The potentially attributed element</param>
    let getAttribute<'T when 'T :> Attribute>(element : ClrElementReference) =
        match element with
        | TypeElement(e) -> 
            e |> ClrTypeReference.getAttribute<'T>
        | MemberElement(e) ->
            match e with
            | MethodReference x -> 
                x.Method |> MethodInfo.getAttribute<'T>
            | PropertyReference x -> 
                x.Property |> PropertyInfo.getAttribute<'T>
        | MethodParameterReference(x) ->
            x.Parameter |> ParameterInfo.getAttribute
        | UnionCaseElement(x) -> 
            x.Case |> UnionCaseInfo.getAttribute


    /// <summary>
    /// Gets the name of the element
    /// </summary>
    /// <param name="element">The element</param>
    let getName(element : ClrElementReference) =
        match element with
        | TypeElement(e) -> 
            e |> ClrTypeReference.getName        
        | MemberElement(x) -> 
            x.Name
        | MethodParameterReference(x) ->
            x.Subject.Name
        | UnionCaseElement(x) -> 
            x.Name


    /// <summary>
    /// Gets the type that declares the element, if applicable
    /// </summary>
    /// <param name="element">The element</param>
    let getDeclaringType(element : ClrElementReference) =
        let declarer (t : Type) =
            if t = null then None else t |> Some
        
        let declaringType = 
            match element with
            | TypeElement(e) -> e |> ClrTypeReference.getDeclaringType |> declarer
            | MemberElement(e) ->
                match e with
                | PropertyReference(x) -> 
                    x.Property.DeclaringType |> declarer
                | MethodReference(x) -> 
                    x.Method.DeclaringType |> declarer
            | MethodParameterReference(x) ->
                None
            | UnionCaseElement(x) -> 
                x.Case.DeclaringType |> declarer

        match declaringType with
        |Some(x) -> x |> ClrType.reference |> Some
        | None -> None

    /// <summary>
    /// Gets the element from a supplied type
    /// </summary>
    /// <param name="t">The type to be expressed as an element</param>
    let fromTypeRef(t : ClrTypeReference) = t |> TypeElement
            
    /// <summary>
    /// Creates a CLR element reference from the type identified by the type parameter
    /// </summary>
    let fromType<'T> =
        typeref<'T> |> fromTypeRef

    /// <summary>
    /// Gets the element that declares a specified element, if any
    /// </summary>
    /// <param name="element">The element whose declaring element should be returned</param>
    let getDeclaringElement(element : ClrElementReference) =
        match element with
        | MethodParameterReference(x) ->
            x.Method |>ClrType.referenceMethod 0 |> MethodReference |> MemberElement |> Some
        | _ ->
            match element |> getDeclaringType with
            | Some(x) -> 
                x |> fromTypeRef |> Some
            | None -> 
                None

            
        
    

[<AutoOpen>]
module ClrElementExtensions =
    
    type ClrElementReference
    with
        member this.DeclaringType = this |> ClrElement.getDeclaringType
        member this.Name = this |> ClrElement.getName
        member this.GetAttribute<'T when 'T :> Attribute>(element) = element |> ClrElement.getAttribute<'T>
    
    /// <summary>
    /// Creates a reference to a CLR element
    /// </summary>
    let clrref(element : obj) =
        match element with 
        | :? Type as x -> ()
        | _ -> 
            NotSupportedException() |> raise
