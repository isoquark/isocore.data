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


    let describe (t : Type) =
        if FSharpType.IsRecord(t, true) then
            t |> ClrRecord.describe |> RecordTypeReference
        else if FSharpType.IsUnion(t, true) then
            t |> ClrUnion.describe |> UnionTypeReference
        else if t.IsInterface then
            t |> ClrInterface.describe |> InterfaceTypeReference
        else
            NotImplementedException() |> raise

                


[<AutoOpen>]
module ClrTypeExtensions =
    /// <summary>
    /// Describes the type identified by the supplied type parameter
    /// </summary>
    let typeinfo<'T> = typeof<'T> |> ClrType.describe

    type ClrTypeReference 
    with
        member this.Name = this |> ClrType.getName
        

