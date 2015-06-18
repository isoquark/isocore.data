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


    let reference (t : Type) =
        if FSharpType.IsRecord(t, true) then
            t |> ClrRecord.reference |> RecordTypeReference
        else if FSharpType.IsUnion(t, true) then
            t |> ClrUnion.reference |> UnionTypeReference
        else if t.IsInterface then
            t |> ClrInterface.reference |> InterfaceTypeReference
        else
            NotImplementedException() |> raise

                

/// <summary>
/// Defines type-related augmentations and operators
/// </summary>
[<AutoOpen>]
module ClrTypeExtensions =
    /// <summary>
    /// Creats a reference to the type identified by the supplied type parameter
    /// </summary>
    let typeref<'T> = typeof<'T> |> ClrType.reference

    type ClrTypeReference 
    with
        member this.Name = this |> ClrType.getName
        

