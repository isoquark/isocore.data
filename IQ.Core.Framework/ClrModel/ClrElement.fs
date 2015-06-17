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
    let getAttribute<'T when 'T :> Attribute>(element : ClrElement) =
        match element with
        | InterfaceElement(x) -> 
            x.Type |> Type.getAttribute<'T>
        | PropertyElement(x) -> 
            x.Property |> PropertyInfo.getAttribute<'T>
        | MethodElement(x) -> 
            x.Method |> MethodInfo.getAttribute<'T>
        | MethodParameterElement(x) ->
            x.Parameter |> ParameterInfo.getAttribute
        | UnionElement(x) ->
            x.Type |> Type.getAttribute<'T>
        | UnionCaseElement(x) -> 
            x.Case |> UnionCaseInfo.getAttribute
        | RecordElement(x) ->
            x.Type |> Type.getAttribute<'T>
        | PropertyFieldElement(x) ->
            x.Property |> PropertyInfo.getAttribute

    /// <summary>
    /// Gets the name of the element
    /// </summary>
    /// <param name="element">The element</param>
    let getName(element : ClrElement) =
        match element with
        | InterfaceElement(x) -> 
            x.Name
        | PropertyElement(x) -> 
            x.Name
        | MethodElement(x) -> 
            x.Name
        | MethodParameterElement(x) ->
            x.Name
        | UnionElement(x) ->
            x.Name
        | UnionCaseElement(x) -> 
            x.Name
        | RecordElement(x) ->
            x.Name
        | PropertyFieldElement(x) ->
            x.Name

    /// <summary>
    /// Gets the type that declares the element, if applicable
    /// </summary>
    /// <param name="element">The element</param>
    let getDeclaringType(element : ClrElement) =
        let declarer (t : Type) =
            if t = null then None else t |> Some
        let declaringType = 
            match element with
            | InterfaceElement(x) -> 
                x.Type.DeclaringType |> declarer
            | PropertyElement(x) -> 
                x.Property.DeclaringType |> declarer
            | MethodElement(x) -> 
                x.Method.DeclaringType |> declarer
            | MethodParameterElement(x) ->
                None
            | UnionElement(x) ->
                x.Type.DeclaringType |> declarer
            | UnionCaseElement(x) -> 
                x.Case.DeclaringType |> declarer
            | RecordElement(x) ->
                x.Type.DeclaringType |> declarer
            | PropertyFieldElement(x) ->
                x.Property.DeclaringType |> declarer
        match declaringType with
        |Some(x) -> x |> ClrType.describe |> Some
        | None -> None

    /// <summary>
    /// Gets the element from a supplied type
    /// </summary>
    /// <param name="t">The type to be expressed as an element</param>
    let fromType(t : ClrType) =
        match t with
        | UnionType(x) -> 
            x |> UnionElement
        | RecordType(x) -> 
            x |> RecordElement
        | InterfaceType(x) -> 
            x |> InterfaceElement
    
    /// <summary>
    /// Gets the element that declares a specified element, if any
    /// </summary>
    /// <param name="element">The element whose declaring element should be returned</param>
    let getDeclaringElement(element : ClrElement) =
        match element with
        | MethodParameterElement(x) ->
            x.Method |>ClrMethod.describe |> MethodElement |> Some
        | _ ->
            match element |> getDeclaringType with
            | Some(x) -> 
                x |> fromType |> Some
            | None -> 
                None

            
        
    

[<AutoOpen>]
module ClrElementExtensions =
    
    type ClrElement
    with
        member this.DeclaringType = this |> ClrElement.getDeclaringType
        member this.Name = this |> ClrElement.getName
        member this.GetAttribute<'T when 'T :> Attribute>(element) = element |> ClrElement.getAttribute<'T>
    
    /// <summary>
    /// Describes the type identified by the supplied type parameter
    /// </summary>
    let clrinfo(o : obj) =
        match o with 
        | :? Type as x -> ()
        | _ -> 
            NotSupportedException() |> raise
