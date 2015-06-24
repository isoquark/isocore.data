namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent
open System.Diagnostics


open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns

module ClrTypeName = 
    /// <summary>
    /// Renders the type name as text
    /// </summary>
    /// <param name="typeName">The type name</param>
    let format typeName =
        match typeName with
        | SimpleTypeName(n) | FullTypeName(n) | AssemblyQualifiedTypeName(n) -> n
    
module ClrAssemblyName =
    let format assname =
        match assname with
        | SimpleAssemblyName(n) | FullAssemblyName(n) -> n


module ClrElementName =
    let format name =
        match name with
        | AssemblyElementName(n) -> n |> ClrAssemblyName.format
        | TypeElementName(n) -> n |> ClrTypeName.format
        | MemberElementName(n) -> n
        | ParameterElementName(n) -> n

[<AutoOpen>]
module ClrElementNameExtensions =
                
    type ClrAssemblyName
    with
        member this.Text = this |> ClrAssemblyName.format

    type ClrTypeName
    with
        member this.Text = this |> ClrTypeName.format

    type ClrElementName
    with
        member this.Text = this |> ClrElementName.format

    type Type 
    with
        member this.ElementName = this.AssemblyQualifiedName |> AssemblyQualifiedTypeName |> TypeElementName
    
    type Assembly
    with
        member this.ElementName = this.FullName |> FullAssemblyName  |> AssemblyElementName

    type MethodInfo
    with
        member this.ElementName = this.Name |> MemberElementName

    type ParameterInfo
    with
        member this.ElementName = this.Name |> ParameterElementName

    type PropertyInfo
    with
        member this.ElementName = this.Name |> MemberElementName

    type FieldInfo
    with
        member this.ElementName = this.Name |> MemberElementName

    type UnionCaseInfo
    with
        member this.ElementName = this.Name |> MemberElementName


    /// <summary>
    /// When supplied a property accessor quotation, retrieves the name of the property
    /// </summary>
    /// <param name="q">The property accessor quotation</param>
    /// <remarks>
    /// Inspired heavily by: http://www.contactandcoil.com/software/dotnet/getting-a-property-name-as-a-string-in-f/
    /// </remarks>
    let rec propname q =
       match q with
       | PropertyGet(_,p,_) -> p.ElementName
       | Lambda(_, expr) -> propname expr
       | _ -> NotSupportedException() |> raise
