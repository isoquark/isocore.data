namespace IQ.Core.Framework

open System
open System.Reflection

open Microsoft.FSharp.Reflection

module ClrType =
    
    /// <summary>
    /// Gets the name of the type
    /// </summary>
    /// <param name="t">The type description</param>
    let getName(t : ClrTypeReference) =
        match t with
        | UnionTypeReference(u) -> u.Name
        | RecordTypeReference(r) -> r.Name
        | InterfaceTypeReference(i) -> i.Name
        | ClassTypeReference(i) -> i.Name

       
    let reference (t : Type) =
        if FSharpType.IsRecord(t, true) then
            t |> ClrRecord.reference |> RecordTypeReference
        else if FSharpType.IsUnion(t, true) then
            t |> ClrUnion.reference |> UnionTypeReference
        else if t.IsInterface then
            t |> ClrInterface.reference |> InterfaceTypeReference
        else if t.IsClass then
            t |> ClrClass.reference |> ClassTypeReference
        else
            NotImplementedException() |> raise

    let getType (t : ClrTypeReference) =
        match t with
        | UnionTypeReference(u) -> u.Type
        | RecordTypeReference(r) -> r.Type
        | InterfaceTypeReference(i) -> i.Type
        | ClassTypeReference(x) -> x.Type

    /// <summary>
    /// Determines the type's multiplicity
    /// </summary>
    /// <param name="tref">The type reference</param>
    let getMultiplicity(tref : ClrTypeReference) =
        let t = tref |> getType
        if t.IsOptionType then
            if t.ItemValueType.IsGenericEnumerable then 
                Multiplicity.ZeroOrMore
            else
                Multiplicity.ZeroOrOne
        else 
            if t.ItemValueType.IsGenericEnumerable then
                Multiplicity.OneOrMore
            else
                Multiplicity.ExactlyOne
                    


    /// <summary>
    /// Reads an identified attribute from a type, if present
    /// </summary>
    /// <param name="t"></param>
    let getAttribute<'T when 'T :> Attribute> (t : ClrTypeReference) =
        t |> getType |> Type.getAttribute<'T>
                
    let fromValueMap (valueMap : ValueIndex) (t : ClrTypeReference) =
        match t with
        | RecordTypeReference(x) -> x |> ClrRecord.fromValueMap valueMap
        | _ ->
            NotSupportedException() |> raise

/// <summary>
/// Defines type-related augmentations and operators
/// </summary>
[<AutoOpen>]
module ClrTypeExtensions =
    /// <summary>
    /// Creates a reference to the type identified by the supplied type parameter
    /// </summary>
    let typeref<'T> = typeof<'T> |> ClrType.reference

    type ClrTypeReference 
    with
        member this.Name = this |> ClrType.getName
        member this.Type = this |> ClrType.getType
        

