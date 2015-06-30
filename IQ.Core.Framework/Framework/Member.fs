﻿namespace IQ.Core.Framework

open System
open System.Reflection
open System.IO

module Member =
    let getKind (m : MemberInfo) =
        match m with
        | :? EventInfo ->ClrMemberKind.Event
        | :? MethodInfo -> ClrMemberKind.Method
        | :? PropertyInfo -> ClrMemberKind.Property
        | :? FieldInfo -> ClrMemberKind.StorageField
        | :? ConstructorInfo -> ClrMemberKind.Constructor
        | _ -> nosupport()

/// <summary>
/// Defines System.MethodInfo helpers
/// </summary>
module MethodInfo =
    /// <summary>
    /// Retrieves an attribute applied to a method return, if present
    /// </summary>
    /// <param name="subject">The method to examine</param>
    let getReturnAttribute<'T when 'T :> Attribute>(subject : MethodInfo) =
        let attribs = subject.ReturnTypeCustomAttributes.GetCustomAttributes(typeof<'T>, true)
        if attribs.Length <> 0 then
            attribs.[0] :?> 'T |> Some
        else
            None

    /// <summary>
    /// Gets the methods access specificer
    /// </summary>
    /// <param name="m"></param>
    let getAccess (m : MethodInfo) =
        if m = null then
            ArgumentException() |> raise
        if m.IsPublic then
            PublicAccess 
        else if m.IsPrivate then
            PrivateAccess 
        else if m.IsAssembly then
            InternalAccess
        else if m.IsFamilyOrAssembly then
            ProtectedInternalAccess
        else
            nosupport()


module FieldInfo =
    /// <summary>
    /// Gets the methods access specificer
    /// </summary>
    /// <param name="m"></param>
    let getAccess (m : FieldInfo) =
        if m.IsPublic then
            PublicAccess 
        else if m.IsPrivate then
            PrivateAccess
        else if m.IsAssembly then
            InternalAccess
        else if m.IsFamilyOrAssembly then
            ProtectedInternalAccess
        else
            nosupport()

module ConstructorInfo = 
    /// <summary>
    /// Gets the methods access specificer
    /// </summary>
    /// <param name="m"></param>
    let getAccess (m : ConstructorInfo) =
        if m.IsPublic then
            PublicAccess 
        else if m.IsPrivate then
            PrivateAccess
        else if m.IsAssembly then
            InternalAccess
        else if m.IsFamilyOrAssembly then
            ProtectedInternalAccess
        else
            nosupport()

        
    

[<AutoOpen>]
module MemberExtensions = 

    /// <summary>
    /// Gets the currently executing method (not to be used for constructors!)
    /// </summary>
    /// <remarks>
    /// Note that since the method is designated inline, the call to get the executing
    /// method is injected at the call-site and so works as expected
    /// </remarks>
    let inline thisMethod() = MethodInfo.GetCurrentMethod() :?> MethodInfo

    type PropertyInfo
    with
        member this.ValueType = this.PropertyType |> Type.getItemValueType

    type FieldInfo
    with
        member this.ValueType = this.FieldType |> Type.getItemValueType
        member this.Access = this |> FieldInfo.getAccess


    type MethodInfo
    with
        member this.Access = this |> MethodInfo.getAccess

    type ConstructorInfo
    with
        member this.Access = this |> ConstructorInfo.getAccess