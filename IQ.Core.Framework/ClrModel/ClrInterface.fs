namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection


/// <summary>
/// Defines operations for working with interface instances and metadata
/// </summary>
module ClrInterface =
        
    let private describeMember(m : MemberInfo) =
        match m with
        | :? MethodInfo as x ->
            x |> ClrMethod.describe |> InterfaceMethod
        | :? PropertyInfo as x ->
            x |> ClrProperty.describe |> InterfaceProperty
        | _ ->
            NotSupportedException() |> raise


    let private createDescription(t : Type) =
        {
            InterfaceDescription.Type = t
            Name = t.Name 
            Members = 
                (t |> Type.getPureMethods |> List.map describeMember ) 
                |> List.append (t.GetProperties() |> Array.map describeMember |> List.ofArray)
        }

    /// <summary>
    /// Gets the interface information for the supplied type which, presumably, is an interface
    /// </summary>
    /// <param name="t">The type</param>
    let describe(t : Type) =
        createDescription |> ClrTypeIndex.getOrAddInterface t

module ClrInterfaceMember =
    let getAttribute<'T when 'T :> Attribute> m  =
        match m with
        | InterfaceMethod(m) -> 
            m.Method |> MethodInfo.getAttribute<'T>
        | InterfaceProperty(p) -> 
            p.Property |> PropertyInfo.getAttribute<'T>

[<AutoOpen>]
module ClrInterfaceExtensions =
    let interfaceinfo<'T> = typeof<'T> |> ClrInterface.describe

    /// <summary>
    /// Defines augmentations for the InterfaceMemberDescription type
    /// </summary>
    type InterfaceMemberDescription
    with
        member this.GetAttribute<'T when 'T :> Attribute>() = this |> ClrInterfaceMember.getAttribute<'T>