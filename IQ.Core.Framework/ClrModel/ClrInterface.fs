namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection


/// <summary>
/// Defines operations for working with interface instances and metadata
/// </summary>
module ClrInterface =
        
    let private describeMember pos (m : MemberInfo) =
        match m with
        | :? MethodInfo as x ->
            x |> ClrMethod.reference pos |> InterfaceMethodReference
        | :? PropertyInfo as x ->
            x |> ClrProperty.reference pos |> InterfacePropertyReference
        | _ ->
            NotSupportedException() |> raise

    /// <summary>
    /// Creates an interface reference
    /// </summary>
    /// <param name="t">The type of the interface to reference</param>
    let rec private createReference(t : Type) =
        {
            ClrInterfaceReference.Subject = ClrSubjectReference(t.ElementName, -1, t)
            Members = 
                (t |> Type.getPureMethods |> List.mapi describeMember ) 
                |> List.append (t.GetProperties() |> Array.mapi describeMember |> List.ofArray)
            Bases = [for b in t.GetInterfaces() ->
                        createReference |> ClrTypeReferenceIndex.getOrAddInterface b   
                    ]
        }

    /// <summary>
    /// Gets an interface reference
    /// </summary>
    /// <param name="t">The type that defines the type</param>
    let reference (t : Type) =
        if t.IsInterface |> not then
            ArgumentException(sprintf "The type %O is not an interface type" t) |> raise
        
        createReference |> ClrTypeReferenceIndex.getOrAddInterface t



module ClrInterfaceMember =
    let getAttribute<'T when 'T :> Attribute> m  =
        match m with
        | InterfaceMethodReference(m) -> 
            m.Subject.Element |> MethodInfo.getAttribute<'T>
        | InterfacePropertyReference(p) -> 
            p.Property |> PropertyInfo.getAttribute<'T>

/// <summary>
/// Defines interface-related augmentations and operators
/// </summary>
[<AutoOpen>]
module ClrInterfaceExtensions =
    let interfaceref<'T> = typeof<'T> |> ClrInterface.reference

    /// <summary>
    /// Defines augmentations for the InterfaceMemberDescription type
    /// </summary>
    type ClrInterfaceMemberReference
    with
        member this.GetAttribute<'T when 'T :> Attribute>() = this |> ClrInterfaceMember.getAttribute<'T>