﻿// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic
open System.Linq
open System.Runtime
open System.Runtime.CompilerServices

open IQ.Core.Contracts

module internal ClrAttribute =
    let private filter (exclusions : Type seq) (input : Attribute seq) =
        input |> Seq.filter(fun a -> a.GetType() |> exclusions.Contains |> not)


    let private exclusions = 
        [
            typeof<CompilationMappingAttribute> 
            typeof<DebuggerNonUserCodeAttribute> 
            typeof<CompilerGeneratedAttribute>
            typeof<DebuggerBrowsableAttribute>
        ]

    //The intent here is to exclude attributes emitted by the compiler
    //Of course, this will not always work because any attributes filtered
    //out could be manually applied by a developer...
    let getUserAttributes( reflectedElement : obj) =
        
        match reflectedElement with
        | :? Assembly as x -> 
            Attribute.GetCustomAttributes(x) |> filter exclusions
        | :? ParameterInfo as x ->
            Attribute.GetCustomAttributes(x) |> filter exclusions
        | :? Type as x ->
            let exclusions = 
                match  x |> Type.getKind with
                | ClrTypeKind.Enum 
                | ClrTypeKind.Record ->
                    exclusions |> List.append [typeof<SerializableAttribute>]
                | _ -> exclusions
            Attribute.GetCustomAttributes(x) |> filter exclusions 
        | :? MemberInfo as x ->
            Attribute.GetCustomAttributes(x) |> filter exclusions 
        | _ -> nosupport()
        |> List.ofSeq

    let getUserReturnAttributes(m : MethodInfo) =
        m.ReturnTypeCustomAttributes.GetCustomAttributes(false).Cast<Attribute>() |> filter exclusions |> List.ofSeq

module ClrAttribution =
    let tryFind (attribType  : Type) (attributions : ClrAttribution seq)= 
        attributions |> Seq.tryFind(fun x -> x.AttributeInstance |> Option.get |> fun instance -> instance |> attribType.IsInstanceOfType)

    /// <summary>
    /// Creates an attribution for supplied target and attributes
    /// </summary>
    /// <param name="target">Identifies the element to which the attributes apply</param>
    /// <param name="attributes">The applied attributes</param>
    let create (target : ClrElementName) (attributes : Attribute seq) =
        
        let getValue (attrib : Attribute) (p : PropertyInfo) =
            try
                attrib |> p.GetValue
            with
                | :? TargetInvocationException as e ->
                    match e.InnerException with
                    | :? NotSupportedException  ->
                        null
                    | _ -> reraise()
                | _ -> reraise()
        
        [for attribute in attributes do
            let attribType = attribute.GetType()
            let properties = attribType.GetProperties() |> Array.filter(fun p -> p.DeclaringType <> typeof<System.Attribute>)
            yield 
                {
                    ClrAttribution.AttributeName =  attribType.TypeName
                    Target = target
                    AppliedValues = properties
                                  |> Array.filter(fun p -> p.CanRead)
                                  |> Array.mapi( fun i p -> ValueIndexKey(p.Name, i),  p |> getValue attribute  ) 
                                  |>List.ofArray 
                                  |> ValueIndex
                    AttributeInstance = attribute |> Some
                }
        ]

[<AutoOpen>]
module internal ClrAttributionExtensions =

    type MemberInfo
    with
        member this.UserAttributions = 
            this |> ClrAttribute.getUserAttributes |> ClrAttribution.create (this.MemberName |> MemberElementName)
               
    type PropertyInfo
    with

        member this.SetUserAttributions = 
            if this.CanWrite then
                this |> ClrAttribute.getUserAttributes 
                        |> ClrAttribution.create this.SetMethod.ElementName 
            else []

        member this.GetUserAttributions = 
            if this.CanRead then
                this |> ClrAttribute.getUserAttributes 
                    |> ClrAttribution.create this.GetMethod.ElementName 
            else []

    type ParameterInfo
    with
        member this.UserAttributions = 
            this |> ClrAttribute.getUserAttributes |> ClrAttribution.create this.ElementName

    type MethodInfo
    with
        member this.UserReturnAttributions = 
            this |> ClrAttribute.getUserReturnAttributes |> ClrAttribution.create this.ElementName
