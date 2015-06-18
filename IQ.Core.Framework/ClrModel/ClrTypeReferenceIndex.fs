namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent


open Microsoft.FSharp.Reflection


/// <summary>
/// Defines internal reflection cache for efficiency
/// </summary>
module internal ClrTypeReferenceIndex =
    let private types =  ConcurrentDictionary<Type, ClrTypeReference>()

    /// <summary>
    /// Retrieves an existing record description if present or constructs a new one and adds it to the index
    /// </summary>
    /// <param name="t">The record type</param>
    /// <param name="f">The description factory</param>
    let getOrAddRecord (t : Type) (f:Type->RecordReference) =
        match types.GetOrAdd(t, fun t -> f(t) |> RecordTypeReference)  with | RecordTypeReference(x) -> x | _ -> failwith "Should never happen"

    /// <summary>
    /// Retrieves an existing union description if present or constructs a new one and adds it to the index
    /// </summary>
    /// <param name="t">The record type</param>
    /// <param name="f">The description factory</param>
    let getOrAddUnion (t : Type) (f:Type->UnionReference) =
        match types.GetOrAdd(t, fun t -> f(t) |> UnionTypeReference)  with | UnionTypeReference(x) -> x | _ -> failwith "Should never happen"

    /// <summary>
    /// Retrieves an existing interface description if present or constructs a new one and adds it to the index
    /// </summary>
    /// <param name="t">The record type</param>
    /// <param name="f">The description factory</param>
    let getOrAddInterface(t : Type) (f:Type->InterfaceReference) =
        match types.GetOrAdd(t, fun t -> f(t) |> InterfaceTypeReference)  with | InterfaceTypeReference(x) -> x | _ -> failwith "Should never happen"
