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




module ClrElementKind =
    /// <summary>
    /// Classifies the described element
    /// </summary>
    /// <param name="element">The element to classify</param>
    let fromElement description =
        match description with
        | MemberElement(description=x) -> 
            match x with
                | PropertyMember(_) ->
                    ClrElementKind.Property
                | FieldMember(_) -> 
                    ClrElementKind.Field
                | MethodMember(_) ->
                    ClrElementKind.Method
                | ConstructorMember(_) ->
                    ClrElementKind.Constructor
                | EventMember(_) ->
                    ClrElementKind.Event
        | TypeElement(description=x) -> 
            ClrElementKind.Type
        | AssemblyElement(_) ->
            ClrElementKind.Assembly
        | ParameterElement(_) ->
            ClrElementKind.Parameter
        | UnionCaseElement(_) ->
            ClrElementKind.UnionCase

    /// <summary>
    /// Determines whether the kind classifies a member
    /// </summary>
    /// <param name="kind">The kind</param>
    let isMember kind =
        match kind with
        | ClrElementKind.Method | ClrElementKind.Property | ClrElementKind.Field | ClrElementKind.Event -> true
        | _ -> false

    /// <summary>
    /// Determines whether the kind classifies a data member
    /// </summary>
    /// <param name="kind">The kind</param>
    let isDataMember kind =
        match kind with
        | ClrElementKind.Property | ClrElementKind.Field -> true
        | _ -> false


module ClrElement =
    let getAttributes (e : ClrElement) =
            match e with
            | MemberElement(x) -> 
                match x with
                | PropertyMember(x) -> x.Attributes
                | FieldMember(x) -> x.Attributes
                | MethodMember(x) -> x.Attributes
                | EventMember(x) -> x.Attributes
                | ConstructorMember(x) -> x.Attributes
            | TypeElement(x) -> 
                match x with
                | ClassType(x) -> x.TypeInfo.Attributes
                | EnumType(x) -> x.TypeInfo.Attributes
                | ModuleType(x) -> x.TypeInfo.Attributes
                | CollectionType(x) -> x.TypeInfo.Attributes
                | StructType(x) -> x.TypeInfo.Attributes
                | UnionType(x) -> x.TypeInfo.Attributes
                | RecordType(x) -> x.TypeInfo.Attributes
                | InterfaceType(x) -> x.TypeInfo.Attributes
            | AssemblyElement(x) -> x.Attributes
            | ParameterElement(x) -> x.Attributes
            | UnionCaseElement(x) -> x.Attributes
        
    let getChildren element =
        match element with
        | MemberElement(m) ->
            match m with
            | MethodMember(x) -> 
                x.Parameters |> List.map(fun x -> x |> ParameterElement)
            |_ -> []            
        | TypeElement(d) -> 
            d.TypeInfo.Members |> List.map(fun x -> x |> MemberElement)
        | AssemblyElement(z) ->
            z.Types |> List.map(fun x -> x |> TypeElement)
        | ParameterElement(_) -> []
        | UnionCaseElement(_) -> []

    /// <summary>
    /// Recursively traverses the element hierarchy graph and invokes the supplied handler as each element is traversed
    /// </summary>
    /// <param name="handler">The handler that will be invoked for each element</param>
    /// <param name="element"></param>
    let rec walk (handler:ClrElement->unit) element =
        element |> handler
        let children = element |> getChildren
        children |> List.iter (fun child -> child |> walk handler)

    /// <summary>
    /// Recursively traverses the element hierarchy graph and invokes each of the supplied handlers as each element is traversed
    /// </summary>
    /// <param name="handler">The handlers that will be invoked for each element</param>
    /// <param name="element"></param>
    let multiwalk (handlers: (ClrElement->unit) list) element =
        let handler e =
            handlers |> List.iter(fun handler -> e|> handler)
        
        element |> walk handler

    /// <summary>
    /// Retrieves an attribute from the element if it exists and returns None if it does not
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let tryGetAttribute (element : ClrElement) attribType =
      element |> getAttributes |> ClrAttribution.tryFind attribType
    
    /// <summary>
    /// Retrieves an attribute from the element if it exists and raises an exception otherwise
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getAttribute element attribType =
        attribType |> tryGetAttribute element |> Option.get

    /// <summary>
    /// Determines whether an attribute of a specified type has been applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let hasAttribute element attribType = 
        attribType  |> tryGetAttribute element |> Option.isSome

    /// <summary>
    /// Retrieves all attributes of a specified type that have been applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getAttributesByType (element : ClrElement) attribType =
        element |> getAttributes |> List.filter(fun x -> x.AttributeName = attribType)

    /// <summary>
    /// Retrieves an attribute applied to a member, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let tryGetAttributeT<'T when 'T :> Attribute> (element : ClrElement) =
        match element |> getAttributes |> List.tryFind(fun a -> 
            match a.AttributeInstance with
            | Some(i) -> typeof<'T>.IsAssignableFrom(i.GetType())
            | None -> false
         ) with
         | Some(x) -> 
            match x.AttributeInstance with
            | Some(x) -> x :?> 'T |> Some
            | None -> None
         | None -> None

            
        
//        match   typeof<'T>.TypeName |> getAttributesByType element |> ClrAttribution.tryFind typeof<'T> with
//        | Some(x) -> 
//            match x.AttributeInstance with
//            | Some(x) -> x :?> 'T |> Some
//            | None -> None
//               
//        | None -> None

    /// <summary>
    /// Determines whether an attribute is applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    let hasAttributeT<'T when 'T :> Attribute>(element : ClrElement) =
        element |> tryGetAttributeT<'T> |> Option.isSome
                    
    /// <summary>
    /// Retrieves an attribute from the element if it exists and raises an exception otherwise
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getAttributeT<'T when 'T :> Attribute> element  =
        element |> tryGetAttributeT<'T> |> Option.get


module internal ClrProperty =
    let filter (members : ClrMember list) =
            [for x in members do
                match x with
                | PropertyMember(x) -> yield x
                |_ ->()
            ]

    let describe pos (p : PropertyInfo) =
        {
            ClrProperty.Name = p.Name |> ClrMemberName 
            Position = pos
            DeclaringType  = p.DeclaringType.TypeName
            ValueType = p.PropertyType.TypeName
            IsOptional = p.PropertyType |> Option.isOptionType
            IsNullable = p.PropertyType |> Type.isNullableType
            CanRead = p.CanRead
            ReadAccess = if p.CanRead then p.GetMethod.Access |> Some else None
            CanWrite = p.CanWrite
            WriteAccess = if p.CanWrite then p.SetMethod.Access |> Some else None
            ReflectedElement = p |> Some
            IsStatic = if p.CanRead then p.GetMethod.IsStatic else p.SetMethod.IsStatic
            Attributes = p.UserAttributions
            GetMethodAttributes = p.GetUserAttributions
            SetMethodAttributes = p.SetUserAttributions
        }


module internal ClrMethod =
    let filter (members : ClrMember list) =
        [for x in members do
            match x with
            | MethodMember(x) -> yield x
            |_ ->()
        ]

    let private describeParameter pos (p : ParameterInfo) =
        
        {
            ClrMethodParameter.Name = p.ParameterName
            Position = pos
            ReflectedElement = p |> Some
            Attributes = p.UserAttributions
            CanOmit = (p.IsOptional || p.IsDefined(typeof<OptionalArgumentAttribute>))
            ParameterType = p.ParameterType.TypeName
            DeclaringMethod = ClrMemberName(p.Member.Name)
            IsReturn = false
        }


    let private createParameterDescriptions (m : MethodInfo) = 
        [ yield! m.GetParameters() 
                    |> Array.mapi(fun pos p -> p |> describeParameter pos) 
                    |>List.ofArray
          
          if m.ReturnType <> typeof<Void> then            
              yield {
                    ClrMethodParameter.Name = ClrParameterName(String.Empty)
                    Position = -1
                    ReflectedElement = None
                    Attributes = m.UserReturnAttributions
                    CanOmit = false
                    ParameterType = m.ReturnType.TypeName
                    DeclaringMethod = ClrMemberName(m.Name)
                    IsReturn = true
                  }
        ]

    let describeConstructor pos (c : ConstructorInfo) =
        {
            ClrConstructor.Name = c.MemberName
            Position = pos
            ReflectedElement = c |> Some
            Access = c.Access
            IsStatic = c.IsStatic
            Parameters = c.GetParameters() |>  Array.mapi(fun pos p -> p |> describeParameter pos) |>List.ofArray
            Attributes = c.UserAttributions
            DeclaringType = c.DeclaringType.TypeName
        }
    
    let describe pos (m : MethodInfo) = {
        ClrMethod.Name = m.Name |> ClrMemberName
        Position = pos
        ReflectedElement = m |> Some
        Access = m.Access
        IsStatic = m.IsStatic
        Parameters = m |> createParameterDescriptions
        Attributes = m.UserAttributions
        ReturnType = if m.ReturnType = typeof<System.Void> then None else m.ReturnType.TypeName |> Some
        ReturnAttributes = m.UserReturnAttributions
        DeclaringType = m.DeclaringType.TypeName
    }

       
module internal ClrField =
    let filter (members : ClrMember list) =
            [for x in members do
                match x with
                | FieldMember(x) -> yield x
                |_ ->()
            ]

    let describe pos (f : FieldInfo) =
        let isLiteral = f.Facets.IsLiteral
        {
            ClrField.Name = f.Name |> ClrMemberName
            Position = pos
            ReflectedElement = f |> Some
            Access = f.Access
            IsStatic = f.IsStatic
            Attributes = f.UserAttributions
            FieldType = f.FieldType.TypeName
            DeclaringType = f.DeclaringType.TypeName
            IsLiteral = isLiteral
            LiteralValue = if isLiteral then f.GetValue(null) |> Some else None
        }


module internal ClrEvent =
    let filter (members : ClrMember list) =
            [for x in members do
                match x with
                | EventMember(x) -> yield x
                |_ ->()
            ]

    let describe pos (e : EventInfo) =
        {
            ClrEvent.Name = e.MemberName
            Position = pos
            ReflectedElement = e |> Some                
            Attributes = e.UserAttributions
            DeclaringType = e.DeclaringType.TypeName
        }


module internal ClrMember =
    let describe pos (m : MemberInfo) =
        match m with
        | :? MethodInfo as x->
            x |> ClrMethod.describe pos |> MethodMember
        | :? PropertyInfo as x->
            x |> ClrProperty.describe pos |> PropertyMember
        | :? FieldInfo as x -> 
            x |> ClrField.describe pos |> FieldMember
        | :? ConstructorInfo as x ->
            x |> ClrMethod.describeConstructor pos |> ConstructorMember
        | :? EventInfo as x ->
            x |> ClrEvent.describe pos |> EventMember
        | _ ->
            nosupport()

module internal ClrType =
    let describe pos (t : Type) = 
        let info = 
            {
                ClrTypeInfo.Name = t.TypeName
                Position = pos
                DeclaringType = if t.DeclaringType <> null then 
                                    t.DeclaringType.TypeName |> Some 
                                else 
                                    None
                DeclaredTypes = t.GetNestedTypes() |> Array.map(fun n -> n.TypeName) |> List.ofArray
                Members = t |> Type.getDeclaredMembers |> List.mapi(fun pos m -> m |> ClrMember.describe pos)
                Kind = t.Kind
                ReflectedElement = t |> Some
                IsOptionType = t.IsOptionType
                Access = t.Access
                IsStatic = t.IsAbstract && t.IsSealed
                Attributes = t.UserAttributions
                ItemValueType = t.ItemValueType.TypeName
                Namespace = t.Namespace
            }
        match t.Kind with
            /// <summary>
            /// Classifies a type as an F# discriminated union
            /// </summary>
            | ClrTypeKind.Union -> ClrUnion([], info) |> UnionType
            /// <summary>
            /// Classifies a type as a record
            /// </summary>
            | ClrTypeKind.Record -> ClrRecord(info) |> RecordType
            /// <summary>
            /// Classifies a type as an interface
            /// </summary>
            | ClrTypeKind.Interface -> ClrInterface(info) |> InterfaceType
            /// <summary>
            /// Classifies a type as a class
            /// </summary>
            | ClrTypeKind.Class -> ClrClass(info) |> ClassType
            /// <summary>
            /// Classifies a type as a collection of some sort
            /// </summary>
            | ClrTypeKind.Collection -> ClrCollection(t |> Type.getCollectionKind, info) |> CollectionType
            /// <summary>
            /// Classifies a type as a struct (a CLR value type)
            /// </summary>
            | ClrTypeKind.Struct -> ClrStruct(t |> Type.isNullableType, info) |> StructType
            /// <summary>
            /// Classifies a type as an F# module
            /// </summary>
            | ClrTypeKind.Module -> ClrModule(info) |> ModuleType
            /// <summary>
            /// Classifies a type as an enum
            /// </summary>
            | ClrTypeKind.Enum -> ClrEnum(t.GetEnumUnderlyingType().TypeName, info) |> EnumType
            | _ -> nosupport()
        
    

