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
                        ClrPropertyElement(x) -> x.Primitive.Name |> ClrMemberName|> MemberElementName
                | FieldMember(x) -> 
                    match x with
                        ClrFieldElement(x) -> x.Primitive.Name |> ClrMemberName |> MemberElementName
            | MethodElement(x) ->
                  match x with
                    ClrMethodElement(x) -> x.Primitive.Name |> ClrMemberName |> MemberElementName
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
                    x.Primitive.Name |> ClrMemberName |> MemberElementName
            


