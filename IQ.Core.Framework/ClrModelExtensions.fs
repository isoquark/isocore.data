// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework


open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic
open System.Linq
open System.Runtime
open System.Runtime.CompilerServices

open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns




[<AutoOpen>]
module ClrTypeExtensions =

    /// <summary>
    /// Defines augmentations for the <see cref="ClrClass"/> type
    /// </summary>
    type ClrClass
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrClass(info=x) -> x

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access


    /// <summary>
    /// Defines augmentations for the <see cref="ClrEnum"/> type
    /// </summary>
    type ClrEnum
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrEnum(info=x) -> x
        
        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access

        /// <summary>
        /// Gets the enum's underlying integral type
        /// </summary>
        member this.NumericType = match this with ClrEnum(numericType=x) -> x
        
        /// <summary>
        /// Gets the literals defined by the enumeration
        /// </summary>
        member this.Literals = this.Info.Members |> ClrField.filter


        
    /// <summary>
    /// Defines augmentations for the <see cref="ClrModule"/> type
    /// </summary>
    type ClrModule
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrModule(info=x) -> x

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access


    /// <summary>
    /// Defines augmentations for the <see cref="ClrStruct"/> type
    /// </summary>
    type ClrStruct
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrStruct(info=x) -> x

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access

        /// <summary>
        /// Specifies whether the struct isf of type Nullable<_>
        /// </summary>
        member this.IsNullable = match this with ClrStruct(isNullable=x) -> x

    /// <summary>
    /// Defines augmentations for the <see cref="ClrUnion"/> type
    /// </summary>
    type ClrUnion
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrUnion(info=x) -> x
        
        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access
        
        /// <summary>
        /// Gets the cases defined for the union
        /// </summary>
        member this.Cases = match this with ClrUnion(cases=x) -> x

    /// <summary>
    /// Defines augmentations for the <see cref="ClrRecord"/> type
    /// </summary>
    type ClrRecord
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrRecord(info=x) -> x

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access


    /// <summary>
    /// Defines augmentations for the <see cref="ClrInterface"/> type
    /// </summary>
    type ClrInterface
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrInterface(info=x) -> x

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access



    /// <summary>
    /// Defines augmentations for the <see cref="ClrCollection"/> type
    /// </summary>
    type ClrCollection
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrCollection(info=x) -> x

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access
    
        /// <summary>
        /// Ges the collection kind
        /// </summary>
        member this.Kind = match this with ClrCollection(kind=x) -> x
       
    
    /// <summary>
    /// Defines augmentations for the <see cref="ClrType"/> type
    /// </summary>
    type ClrType 
    with

        /// <summary>
        /// The name of the type
        /// </summary>
        member this.Name = this.TypeInfo.Name
        
        /// <summary>
        /// The peer-relative position of the type 
        /// </summary>
        member this.Position = this.TypeInfo.Position
        
        /// <summary>
        /// The represented CLR type, if specified
        /// </summary>
        member this.ReflectedElement = this.TypeInfo.ReflectedElement
        
        /// <summary>
        /// The type that declares the type, if any
        /// </summary>
        member this.DeclaringType = this.TypeInfo.DeclaringType
        
        /// <summary>
        /// The types declared by the type, if any
        /// </summary>
        member this.DeclaredTypes = this.TypeInfo.DeclaredTypes
        
        /// <summary>
        /// Specifies the type's classification
        /// </summary>
        member this.Kind = this.TypeInfo.Kind
        
        /// <summary>
        /// Specifies whether the type is an F# opton
        /// </summary>
        member this.IsOptionType = this.TypeInfo.IsOptionType
        
        /// <summary>
        /// The type members
        /// </summary>
        member this.Members = this.TypeInfo.Members

        /// <summary>
        /// Specifies the types accessiblity
        /// </summary>
        member this.Access = this.TypeInfo.Access
        
        /// <summary>
        /// Specifies whether the type is static
        /// </summary>
        member this.IsStatic = this.TypeInfo.IsStatic
        
        /// <summary>
        /// The attributes applied to the type
        /// </summary>
        member this.Attributes = this.TypeInfo.Attributes
        
        /// <summary>
        /// The name of the actual or encapsulated type
        /// </summary>
        member this.ItemValueType = this.TypeInfo.ItemValueType

        /// <summary>
        /// Gets the properties declared by the type
        /// </summary>
        member this.Properties = this.Members |> ClrProperty.filter

        /// <summary>
        /// Gets the methods declared by the type
        /// </summary>
        member this.Methods = this.Members |> ClrMethod.filter

        /// <summary>
        /// Gets the fields declared by the type
        /// </summary>
        member this.Fields = this.Members |> ClrField.filter

        /// <summary>
        /// Gets the fields declared by the type
        /// </summary>
        member this.Events = this.Members |> ClrEvent.filter

    /// <summary>
    /// Defines augmentations for the <see cref="ClrMember"/> type
    /// </summary>
    type ClrMember
    with
        member this.Name =
            match this with
            | PropertyMember(x) -> x.Name
            | FieldMember(x) -> x.Name
            | MethodMember(x) -> x.Name
            | EventMember(x) -> x.Name
            | ConstructorMember(x) -> x.Name

        member this.Kind =
            match this with
            | PropertyMember(_) -> ClrMemberKind.Property
            | FieldMember(_) -> ClrMemberKind.StorageField
            | MethodMember(_) -> ClrMemberKind.Method
            | EventMember(_) -> ClrMemberKind.Event
            | ConstructorMember(_) -> ClrMemberKind.Constructor

        member this.Position =
            match this with
            | PropertyMember(x) -> x.Position
            | FieldMember(x) -> x.Position
            | MethodMember(x) -> x.Position
            | EventMember(x) -> x.Position
            | ConstructorMember(x) -> x.Position

        member this.IsStatic =
            match this with
            | PropertyMember(x) -> x.IsStatic
            | FieldMember(x) -> x.IsStatic
            | MethodMember(x) -> x.IsStatic
            | EventMember(x) -> false
            | ConstructorMember(x) -> x.IsStatic

        member this.Access = 
            match this with
            | PropertyMember(x) -> None
            | FieldMember(x) -> x.Access |> Some
            | MethodMember(x) -> x.Access |> Some
            | EventMember(x) -> None
            | ConstructorMember(x) -> x.Access |> Some

        member this.ReflectedElement =
            match this with
            | PropertyMember(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
            | FieldMember(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
            | MethodMember(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
            | EventMember(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
            | ConstructorMember(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
        
        /// <summary>
        /// Gets the type that declares the memeber
        /// </summary>
        member this.DeclaringType =
            match this with
            | PropertyMember(x) -> x.DeclaringType
            | FieldMember(x) -> x.DeclaringType
            | MethodMember(x) -> x.DeclaringType
            | EventMember(x) -> x.DeclaringType
            | ConstructorMember(x) -> x.DeclaringType

        /// <summary>
        /// Gets the top-level attributions
        /// </summary>
        /// <remarks>
        /// Top-level, in this context, means attributes applied directly to the CLR element and not it's constituent pieces
        /// such as property getters/settters, method parameters, etc.
        /// </remarks>
        member this.Attributes =
            match this with
            | PropertyMember(x) -> x.Attributes
            | FieldMember(x) -> x.Attributes
            | MethodMember(x) -> x.Attributes
            | EventMember(x) -> x.Attributes
            | ConstructorMember(x) -> x.Attributes


    type ClrMethod
    with
        member this.TryGetAttribute<'T when 'T :> Attribute>() =
            let attribName = ClrTypeName(typeof<'T>.Name, typeof<'T>.FullName |> Some, typeof<'T>.AssemblyQualifiedName |> Some) 
            this.Attributes |> List.tryFind(fun x -> x.AttributeName = attribName)
        
        member this.HasAttribute<'T when 'T:> Attribute>() = 
            this.TryGetAttribute<'T>() |> Option.isSome

        /// <summary>
        /// Gets all non-return parameters
        /// </summary>
        member this.InputParameters = this.Parameters |> List.filter(fun x -> x.IsReturn |> not)


    type ClrProperty
    with
        member this.TryGetAttribute<'T when 'T :> Attribute>() =
            let attribName = ClrTypeName(typeof<'T>.Name, typeof<'T>.FullName |> Some, typeof<'T>.AssemblyQualifiedName |> Some) 
            this.Attributes |> List.tryFind(fun x -> x.AttributeName = attribName)
        
        member this.HasAttribute<'T when 'T:> Attribute>() = 
            this.TryGetAttribute<'T>() |> Option.isSome
           

    type ClrElement
    with
        /// <summary>
        /// Gets the top-level attributes applied to the element
        /// </summary>
        member this.Attributes =
            match this with
            | MemberElement(x) -> x.Attributes
            | TypeElement(x) -> x.Attributes
            | AssemblyElement(x) -> x.Attributes
            | ParameterElement(x) -> x.Attributes
            | UnionCaseElement(x) -> x.Attributes

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name =
            match this with
            | MemberElement(x) -> x.Name |> MemberElementName
            | TypeElement(x) -> x.Name |> TypeElementName
            | AssemblyElement(x) -> x.Name |> AssemblyElementName
            | ParameterElement(x) -> x.Name |> ParameterElementName
            | UnionCaseElement(x) -> x.Name |> MemberElementName

        member this.DeclaringType =
            match this with
            | MemberElement(x) -> x.DeclaringType |> Some
            | TypeElement(x) -> x.DeclaringType
            | AssemblyElement(x) -> None
            | ParameterElement(x) -> None
            | UnionCaseElement(x) -> x.DeclaringType |> Some
            
        member this.Position =
            match this with
            | MemberElement(x) -> x.Position
            | TypeElement(x) -> x.Position
            | AssemblyElement(x) -> x.Position
            | ParameterElement(x) -> x.Position
            | UnionCaseElement(x) -> x.Position
        
        member this.ReflectedElement =
            match this with
            | MemberElement(x) -> x.ReflectedElement
            | TypeElement(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None
            | AssemblyElement(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None
            | ParameterElement(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None
            | UnionCaseElement(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None

        member this.Kind = 
            match this with
            | MemberElement(x) -> 
                match x.Kind with
                | ClrMemberKind.Constructor -> ClrElementKind.Constructor
                | ClrMemberKind.Event -> ClrElementKind.Event
                | ClrMemberKind.Method -> ClrElementKind.Method
                | ClrMemberKind.Property -> ClrElementKind.Property
                | ClrMemberKind.StorageField -> ClrElementKind.Field
                | _ -> nosupport()
            | TypeElement(x) -> ClrElementKind.Type
            | AssemblyElement(x) -> ClrElementKind.Assembly
            | ParameterElement(x) -> ClrElementKind.Parameter
            | UnionCaseElement(x) -> ClrElementKind.UnionCase

        member this.TryGetAttribute<'T when 'T :> Attribute>() =
            let attribName = ClrTypeName(typeof<'T>.Name, typeof<'T>.FullName |> Some, typeof<'T>.AssemblyQualifiedName |> Some) 
            this.Attributes |> List.tryFind(fun x -> x.AttributeName = attribName)
        
        member this.HasAttribute<'T when 'T:> Attribute>() = 
            this.TryGetAttribute<'T>() |> Option.isSome
        
        member this.GetAttibuteInstance<'T when 'T:> Attribute>() =
            this.TryGetAttribute<'T>().Value.AttributeInstance.Value :?> 'T

                
