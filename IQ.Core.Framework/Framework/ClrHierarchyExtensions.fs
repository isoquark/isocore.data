namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic

open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns

/// <summary>
/// Defines CLR hierarchy upcasts and related augmentations 
/// </summary>
[<AutoOpen>]
module ClrHierarchyExtensions =
    /// <summary>
    /// Defines augmentations for the <see cref="ClrElement"/> type
    /// </summary>
    type ClrElement
    with
        member this.Name = this |> ClrElementName.fromElement

    /// <summary>
    /// Defines augmentations for the <see cref="System.Type"/> type
    /// </summary>
    type Type 
    with
        /// <summary>
        /// Interprets the type as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement

        /// <summary>
        /// Interprets the method as a <see cref="ClrTypeElement"/>
        /// </summary>
        //member this.TypeElement = this |> ClrTypeElement.create None 
        member this.TypeElement = match this.Element with | TypeElement(element=x) -> x | _ -> nosupport()        
        
        /// <summary>
        /// Gets the <see cref="ClrTypeName"/> of the type
        /// </summary>
        member this.ElementTypeName = this |> ClrTypeName.fromType
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the type
        /// </summary>
        member this.ElementName = this.Element.Name
    
    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.Assembly"/> type
    /// </summary>
    type Assembly
    with
        /// <summary>
        /// Interprets the assembly as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement
        /// <summary>
        /// Interprets the assembly as a <see cref="ClrAssemblyElement"/>
        /// </summary>
        member this.AssemblyElement =  match this.Element with | AssemblyElement(element=x) -> x | _ ->nosupport()
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the assembly
        /// </summary>
        member this.ElementName = this.Element.Name

    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.MethodInfo"/> type
    /// </summary>
    type MethodInfo
    with
        /// <summary>
        /// Interprets the method as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement
        /// <summary>
        /// Interprets the method as a <see cref="ClrMethodElement"/>
        /// </summary>
        member this.MethodElement = 
            match this.Element with 
            | MemberElement(element=x) ->  match x with MethodElement(x) ->x | _ -> nosupport()
            | _ -> nosupport()        
        /// <summary>
        /// Interprets the method as a <see cref="ClrMemberElement"/>
        /// </summary>
        member this.MemberElement = 
            match this.Element with 
            | MemberElement(element=x) ->  x
            | _ -> nosupport()
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the method
        /// </summary>
        member this.ElementName = this.Element.Name


    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.ParameterInfo"/> type
    /// </summary>
    type ParameterInfo
    with        
        /// <summary>
        /// Interprets the parameter as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement
        /// <summary>
        /// Interprets the parameter as a <see cref="ClrParameterElement"/>
        /// </summary>
        member this.ParameterElement = match this.Element with | ParameterElement(x) -> x |_ -> nosupport()
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the parameter
        /// </summary>
        member this.ElementName = this.Element.Name


    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.PropertyInfo"/> type
    /// </summary>
    type PropertyInfo
    with        
        /// <summary>
        /// Interprets the property as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement
        /// <summary>
        /// Interprets the property as a <see cref="ClrPropertyElement"/>
        /// </summary>
        member this.PropertyElement = 
            match this.Element with
            |MemberElement(element=x) ->
                match x with
                | DataMemberElement(x) ->
                    match x with PropertyMember(x) -> x | _ -> nosupport()
                | _ -> nosupport()
            |_ -> nosupport()

        /// <summary>
        /// Interprets the property as a <see cref="ClrDataMemberElement"/>
        /// </summary>
        member this.DataMemberElement = 
            match this.Element with
            |MemberElement(element=x) ->
                match x with
                | DataMemberElement(x) -> x                    
                | _ -> nosupport()
            |_ -> nosupport()
        
        /// <summary>
        /// Interprets the property as a <see cref="ClrMemberElement"/>
        /// </summary>
        member this.MemberElement = 
            match this.Element with
            |MemberElement(element=x) -> x
            |_ -> nosupport()
        
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the property
        /// </summary>
        member this.ElementName = this.Element.Name


    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.FieldInfo"/> type
    /// </summary>
    type FieldInfo
    with
        /// <summary>
        /// Interprets the field as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement
        /// <summary>
        /// Interprets the field as a <see cref="ClrFieldElement"/>
        /// </summary>
        member this.FieldElement = 
            match this.Element with
            |MemberElement(element=x) ->
                match x with
                | DataMemberElement(x) ->
                    match x with FieldMember(x) -> x | _ -> nosupport()
                | _ -> nosupport()
            |_ -> nosupport()

        /// <summary>
        /// Interprets the field as a <see cref="ClrDataMemberElement"/>
        /// </summary>
        member this.DataMemberElement = 
            match this.Element with
            |MemberElement(element=x) ->
                match x with
                | DataMemberElement(x) -> x                    
                | _ -> nosupport()
            |_ -> nosupport()
        /// <summary>
        /// Interprets the field as a <see cref="ClrMemberElement"/>
        /// </summary>        
        member this.MemberElement = 
            match this.Element with
            |MemberElement(element=x) -> x
            |_ -> nosupport()
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the field
        /// </summary>
        member this.ElementName = this.Element.Name

    /// <summary>
    /// Defines augmentations for the <see cref="Microsoft.FSharp.Reflection.UnionCaseInfo"/> type
    /// </summary>
    type UnionCaseInfo
    with
        /// <summary>
        /// Interprets the case as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement
        /// <summary>
        /// Interprets the case as a <see cref="ClrUnionCaseElement"/>
        /// </summary>
        member this.UnionCaseElement = match this.Element with |UnionCaseElement(element=x) -> x | _ -> nosupport()
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the case
        /// </summary>
        member this.ElementName = this.Element.Name


    /// <summary>
    /// Defines augmentations for the <see cref="ClrPopertyElement"/> type
    /// </summary>
    type ClrPropertyElement
    with
        /// <summary>
        /// Gets the encapluated Property
        /// </summary>
        member this.PropertyInfo = match this with ClrPropertyElement(x) -> x.Primitive
        /// <summary>
        /// Interprets the property as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.PropertyInfo.Element
        /// <summary>
        /// Upcasts the element to a <see cref="ClrDataMemberElement"/>
        /// </summary>
        member this.PropertyMemberElement = this.PropertyInfo.DataMemberElement
        /// <summary>
        /// Interprets the property as a <see cref="ClrMemberElement"/>
        /// </summary>
        member this.MemberElement = this.PropertyInfo.MemberElement

    /// <summary>
    /// Defines augmentations for the <see cref="ClrMemberElement"/> type
    /// </summary>
    type ClrMemberElement
    with
        member this.MemberInfo = 
            match this with 
                | DataMemberElement(x) ->
                    match x with
                    | PropertyMember(x) -> 
                        match x with ClrPropertyElement(x) -> x.Primitive :> MemberInfo
                    | FieldMember(x) -> 
                        match x with ClrFieldElement(x) -> x.Primitive :> MemberInfo
                | MethodElement(x) ->
                   match x with ClrMethodElement(x) -> x.Primitive :> MemberInfo
                              
        /// <summary>
        /// Upcasts the element to a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.MemberInfo |> ClrElementProvider.getElement
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the member 
        /// </summary>
        member this.ElementName = this.Element.Name


    /// <summary>
    /// Defines augmentations for the <see cref="ClrTypeElement"/> type
    /// </summary>
    type ClrTypeElement
    with
        /// <summary>
        /// Gets the encapluated Type
        /// </summary>
        member this.Type = match this with ClrTypeElement(x) -> x.Primitive
        /// <summary>
        /// Upcasts the element to a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.Type |> ClrElementProvider.getElement
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the member 
        /// </summary>
        member this.ElementName = this.Element.Name
        /// <summary>
        /// Gets the <see cref="ClrTypeNameName"/> of the member 
        /// </summary>
        member this.ElementTypeName = this.Type.ElementTypeName

    /// <summary>
    /// Defines augmentations for the <see cref="ClrAssemblyElement"/> type
    /// </summary>
    type ClrAssemblyElement
    with
        /// <summary>
        /// Gets the encapluated Assembly
        /// </summary>
        member this.Assembly = match this with ClrAssemblyElement(x) -> x.Primitive
        /// <summary>
        /// Upcasts the element to a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.Assembly |> ClrElementProvider.getElement
        
        /// <summary>
        /// Gets the name of the assembly
        /// </summary>
        member this.Name = match this.Element.Name with 
                            | AssemblyElementName(x) -> x 
                            | _ -> nosupport()

    /// <summary>
    /// Defines augmentations for the <see cref="ClrParameterElement"/> type
    /// </summary>
    type ClrParameterElement
    with
        /// <summary>
        /// Upcasts the element to a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ParameterElement
        /// <summary>
        /// Gets the encapluated Parameter
        /// </summary>
        member this.ParamerInfo = match this with ClrParameterElement(x) -> x.Primitive

    /// <summary>
    /// Defines augmentations for the <see cref="ClrFieldElement"/> type
    /// </summary>
    type ClrFieldElement
    with
        /// <summary>
        /// Gets the encapluated Field
        /// </summary>
        member this.FieldInfo = match this with ClrFieldElement(x) -> x.Primitive

    /// <summary>
    /// Defines augmentations for the <see cref="ClrMethodElement"/> type
    /// </summary>
    type ClrMethodElement
    with
        /// <summary>
        /// Gets the encapsulated method
        /// </summary>
        member this.MethodInfo = match this with ClrMethodElement(x) -> x.Primitive
        


    /// <summary>
    /// Defines augmentations for the <see cref="ClrUnionCaseElement"/> type
    /// </summary>
    type ClrUnionCaseElement 
    with
        /// <summary>
        /// Gets the encapsulated case
        /// </summary>
        member this.UnionCaseInfo = match this with ClrUnionCaseElement(x) -> x.Primitive


