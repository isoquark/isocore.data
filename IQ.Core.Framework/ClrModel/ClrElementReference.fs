namespace IQ.Core.Framework

open System
open System.Reflection

open Microsoft.FSharp.Reflection

/// <summary>
/// Defines operations applicable to the unified ClrElement type
/// </summary>
module ClrElementReference =
            
    let internal getSubject(eref : ClrElementReference) =
        match eref with
            | MethodParameterReference(x) -> x.Subject
            | UnionCaseReference(x) -> x.Subject
            | TypeReference(x) -> x |> ClrTypeReference.getSubject
            | MemberReference(x) -> x.Subject
       

    /// <summary>
    /// Gets the name of the element
    /// </summary>
    /// <param name="element">The element</param>
    let getReferentName(eref : ClrElementReference) =
        match eref with
        | TypeReference(e) -> 
            e.ReferentName
        | MemberReference(x) -> 
            x.ReferentName
        | MethodParameterReference(x) ->
            x.Subject.Name
        | UnionCaseReference(x) -> 
            x.ReferentName

   
    /// <summary>
    /// Gets the CLR element being referenced
    /// </summary>
    /// <param name="eref"></param>
    let getReferent (eref : ClrElementReference) =
        eref |> getSubject |> fun x -> x.Element        

    /// <summary>
    /// Reads an identified attribute from the element if applied
    /// </summary>
    /// <param name="element">The potentially attributed element</param>
//    let getAttribute<'T when 'T :> Attribute>(eref : ClrElementReference) =
//        eref |> getReferent |> ClrElement.tryGetAttributeT<'T>

    /// <summary>
    /// Gets the type that declares the referent
    /// </summary>
    /// <param name="element">The element reference</param>
    let getDeclaringType(eref : ClrElementReference) =
        let declarer (t : Type) =
            if t = null then None else t |> Some
        
        match eref |> getReferent |> fun x -> x.DeclaringType with
        | Some(x) -> x.Type |> ClrTypeReference.reference |> Some
        | None -> None
                                
    /// <summary>
    /// Gets the element from a supplied type
    /// </summary>
    /// <param name="t">The type to be expressed as an element</param>
    let fromTypeRef(t : ClrTypeReference) = t |> TypeReference
            
    /// <summary>
    /// Gets the element that declares a specified element, if any
    /// </summary>
    /// <param name="element">The element whose declaring element should be returned</param>
    let getDeclaringElement(element : ClrElementReference) =
        match element with
        | MethodParameterReference(x) ->
            x.Method |>ClrTypeReference.referenceMethod 0 |> MethodMemberReference |> MemberReference |> Some
        | _ ->
            match element |> getDeclaringType with
            | Some(x) -> 
                x |> fromTypeRef |> Some
            | None -> 
                None

            
        
    

[<AutoOpen>]
module ClrElementReferenceExtensions =
    
    type ClrElementReference
    with
        member this.DeclaringType = this |> ClrElementReference.getDeclaringType
        member this.ReferentName = this |> ClrElementReference.getReferentName
        member this.Referent = this |> ClrElementReference.getReferent
        
        //member this.GetAttribute<'T when 'T :> Attribute>(element) = element |> ClrElementReference.getAttribute<'T>
    
    /// <summary>
    /// Creates a reference to a CLR element
    /// </summary>
    let clrref(element : obj) =
        match element with 
        | :? Type as x -> ()
        | _ -> 
            NotSupportedException() |> raise
