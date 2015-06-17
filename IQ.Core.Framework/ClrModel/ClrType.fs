namespace IQ.Core.Framework

open System
open System.Reflection

open Microsoft.FSharp.Reflection

module ClrType =
    
    /// <summary>
    /// Gets the name of the type
    /// </summary>
    /// <param name="t">The type description</param>
    let getName(t : ClrType) =
        match t with
        | UnionType(u) -> u.Name
        | RecordType(r) -> r.Name
        | InterfaceType(i) -> i.Name


    let describe (t : Type) =
        if FSharpType.IsRecord(t, true) then
            t |> ClrRecord.describe |> RecordType
        else if FSharpType.IsUnion(t, true) then
            t |> ClrUnion.describe |> UnionType
        else if t.IsInterface then
            t |> ClrInterface.describe |> InterfaceType
        else
            NotImplementedException() |> raise

                


[<AutoOpen>]
module ClrTypeExtensions =
    /// <summary>
    /// Describes the type identified by the supplied type parameter
    /// </summary>
    let typeinfo<'T> = typeof<'T> |> ClrType.describe

    type ClrType 
    with
        member this.Name = this |> ClrType.getName
        

