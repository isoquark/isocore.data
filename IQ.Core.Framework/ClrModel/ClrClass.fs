namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection

/// <summary>
/// Defines operations for working with interface instances and metadata
/// </summary>
module ClrClass =
        
    let private describeMember pos (m : MemberInfo) =
        match m with
        | :? MethodInfo as x ->
            x |> ClrMethod.reference pos |> MethodReference
        | :? PropertyInfo as x ->
            x |> ClrProperty.reference pos |> PropertyReference
        | _ ->
            NotSupportedException() |> raise

    /// <summary>
    /// Creates an interface reference
    /// </summary>
    /// <param name="t">The type of the interface to reference</param>
    let rec private createReference(t : Type) =
        {
            ClrClassReference.Subject = {Subject = ClrSubjectReference(t.ElementName, -1, t)}
            Members = 
                (t |> Type.getPureMethods |> List.mapi describeMember ) 
                |> List.append (t.GetProperties() |> Array.mapi describeMember |> List.ofArray)
        }

    /// <summary>
    /// Gets an interface reference
    /// </summary>
    /// <param name="t">The type that defines the type</param>
    let reference (t : Type) =
        if t.IsClass |> not then
            ArgumentException(sprintf "The type %O is not an interface type" t) |> raise
        
        createReference |> ClrTypeReferenceIndex.getOrAddClass t




/// <summary>
/// Defines interface-related augmentations and operators
/// </summary>
[<AutoOpen>]
module ClrClassExtensions =
    let classref<'T> = typeof<'T> |> ClrClass.reference

    /// <summary>
    /// Defines augmentations for the InterfaceMemberDescription type
    /// </summary>
    type ClrMemberReference
    with
        member this.GetAttribute<'T when 'T :> Attribute>() = this |> ClrMember.getAttribute<'T>


