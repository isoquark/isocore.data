namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent
open System.Diagnostics


open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns

module ClrTypeName = 
    /// <summary>
    /// Formats the typename text
    /// </summary>
    /// <param name="typeName">The typename</param>
    let format typeName =
        match typeName with
        | SimpleTypeName(n) | FullTypeName(n) | AssemblyTypeName(n) -> n
    
module ClrAssemblyName =
    let format assname =
        match assname with
        | SimpleAssemblyName(n) | FullAssemblyName(n) -> n


module ClrElementName =
    let format name =
        match name with
        | AssemblyElementName(n) -> n |> ClrAssemblyName.format
        | TypeElementName(n) -> n |> ClrTypeName.format
        | BasicElementName(n) -> n

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
        member this.ElementName = this.AssemblyQualifiedName |> AssemblyTypeName |> TypeElementName
    
    type Assembly
    with
        member this.ElementName = this.FullName |> FullAssemblyName  |> AssemblyElementName

    type MethodInfo
    with
        member this.ElementName = this.Name |> BasicElementName

    type ParameterInfo
    with
        member this.ElementName = this.Name |> BasicElementName

    type PropertyInfo
    with
        member this.ElementName = this.Name |> BasicElementName

    type UnionCaseInfo
    with
        member this.ElementName = this.Name |> BasicElementName


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
