namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic


/// <summary>
/// Defines operations related to the <see cref="ClrTypeName"/> type
/// </summary>
module internal ClrTypeName =
    /// <summary>
    /// Gets the element name
    /// </summary>
    /// <param name="subject"></param>
    let fromType (subject : Type) =

        ClrTypeName(
              subject.Name
            , subject.FullName |> Some
            , subject.AssemblyQualifiedName |> Some)

    /// <summary>
    /// Gets the type name from the <see cref="ClrTypeElement"/>
    /// </summary>
    /// <param name="subject">The type element</param>
    let fromTypeElement (subject : ClrTypeElement) =
        match subject with ClrTypeElement(x) -> x.Primitive |> fromType
              
module internal ClrTypeElement =

    let private getType element =
        match element with ClrTypeElement(x) -> x.Primitive

    let create pos primitive=
        ClrTypeElement(ClrReflectionPrimitive(primitive,pos))        

         
/// <summary>
/// Defines operations related to the <see cref="ClrElementName"/> type
/// </summary>
module internal ClrElementName =

    /// <summary>
    /// Gets the name of the element
    /// </summary>
    /// <param name="element"></param>
    let fromElement (element : ClrElement) =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    match x with 
                        ClrPropertyElement(x) -> x.Primitive.Name |> ClrMemberElementName|> MemberElementName
                | FieldMember(x) -> 
                    match x with
                        ClrFieldElement(x) -> x.Primitive.Name |> ClrMemberElementName |> MemberElementName
            | MethodElement(x) ->
                  match x with
                    ClrMethodElement(x) -> x.Primitive.Name |> ClrMemberElementName |> MemberElementName
        | TypeElement(element=x) -> 
            x |> ClrTypeName.fromTypeElement |> TypeElementName
        | AssemblyElement(element=x) ->
            match x with 
                ClrAssemblyElement(x) -> 
                    ClrAssemblyName(x.Primitive.SimpleName, x.Primitive.FullName |> Some) |> AssemblyElementName
        | ParameterElement(x) ->
            match x with
                ClrParameterElement(x) ->
                    x.Primitive.Name |> ClrParameterElementName |> ParameterElementName
        | UnionCaseElement(element=x) ->
            match x with
                ClrUnionCaseElement(x) ->
                    x.Primitive.Name |> ClrMemberElementName |> MemberElementName
            
/// <summary>
/// Defines <see cref="ClrElementName"/>-related augmentations 
/// </summary>
[<AutoOpen>]
module ClrNameExtensions =
    /// <summary>
    /// Defines augmentations for the <see cref="ClrTypeName"/> type
    /// </summary>
    type ClrTypeName 
    with
        /// <summary>
        /// Gets the local name of the type (which does not include enclosing type names or namespace)
        /// </summary>
        member this.SimpleName = match this with ClrTypeName(simpleName=x) -> x
        /// <summary>
        /// Gets namespace and nested type-qualified name of the type
        /// </summary>
        member this.FullName = match this with ClrTypeName(fullName=x) -> x
        /// <summary>
        /// Gets the assembly-qualified type name of the type
        /// </summary>
        member this.AssemblyQualifiedName = match this with ClrTypeName(assemblyQualifiedName=x) -> x
        
        member this.Text = 
            match this with 
                ClrTypeName(simple,full, aqn) -> match aqn with                                    
                                                    | Some(x) -> x
                                                    | None ->
                                                        match full with
                                                        | Some(x) -> x
                                                        | None -> simple 

    /// <summary>
    /// Defines augmentations for the <see cref="ClrAssemblyName"/> type
    /// </summary>
    type ClrAssemblyName 
    with
        member this.SimpleName = match this with ClrAssemblyName(simpleName=x) -> x
        member this.FullName = match this with ClrAssemblyName(fullName=x) -> x
        member this.Text =
            match this with ClrAssemblyName(simpleName, fullName) -> match fullName with
                                                                        | Some(x) -> x
                                                                        | None ->
                                                                            simpleName    
    /// <summary>
    /// Defines augmentations for the <see cref="ClrMemberElementName"/> type
    /// </summary>
    type ClrMemberElementName
    with
        member this.Text = match this with ClrMemberElementName(x) -> x
    

    /// <summary>
    /// Defines augmentations for the <see cref="ClrParameterElementName"/> type
    /// </summary>
    type ClrParameterElementName
    with
        member this.Text = match this with ClrParameterElementName(x) -> x

    /// <summary>
    /// Represents the name of a CLR element
    /// </summary>
    type ClrElementName
    with
        member this.Text =
            match this with
            | AssemblyElementName x -> x.Text
            | TypeElementName x -> x.Text
            | MemberElementName x -> x.Text
            | ParameterElementName x -> x.Text


