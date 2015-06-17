namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent


open Microsoft.FSharp.Reflection


/// <summary>
/// Defines internal reflection cache for efficiency
/// </summary>
module internal ClrTypeIndex =
    let private types =  ConcurrentDictionary<Type, ClrType>()

    /// <summary>
    /// Retrieves an existing record description if present or constructs a new one and adds it to the index
    /// </summary>
    /// <param name="t">The record type</param>
    /// <param name="f">The description factory</param>
    let getOrAddRecord (t : Type) (f:Type->RecordDescription) =
        match types.GetOrAdd(t, fun t -> f(t) |> RecordType)  with | RecordType(x) -> x | _ -> failwith "Should never happen"

    /// <summary>
    /// Retrieves an existing union description if present or constructs a new one and adds it to the index
    /// </summary>
    /// <param name="t">The record type</param>
    /// <param name="f">The description factory</param>
    let getOrAddUnion (t : Type) (f:Type->UnionDescription) =
        match types.GetOrAdd(t, fun t -> f(t) |> UnionType)  with | UnionType(x) -> x | _ -> failwith "Should never happen"

    /// <summary>
    /// Retrieves an existing interface description if present or constructs a new one and adds it to the index
    /// </summary>
    /// <param name="t">The record type</param>
    /// <param name="f">The description factory</param>
    let getOrAddInterface(t : Type) (f:Type->InterfaceDescription) =
        match types.GetOrAdd(t, fun t -> f(t) |> InterfaceType)  with | InterfaceType(x) -> x | _ -> failwith "Should never happen"
