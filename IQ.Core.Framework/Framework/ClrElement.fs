namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics

open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns

                 

/// <summary>
/// Defines a rudimentary vocabulary for representing CLR metadata. By intent, it is not complete.
/// </summary>
[<AutoOpen>]
module ClrElementVocabulary = 

    /// <summary>
    /// Represents a type name
    /// </summary>
    type ClrTypeName = ClrTypeName of simpleName : string * fullName : string option * assemblyQualifiedName : string option
    with
        ///The local name of the type; does not include enclosing type names or namespace
        member this.SimpleName = match this with ClrTypeName(simpleName=x) -> x
        ///The namespace and nested type-qualified name of the type
        member this.FullName = match this with ClrTypeName(fullName=x) -> x
        ///The assembly-qualified full type name
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
    /// Represents an assembly name
    /// </summary>
    type ClrAssemblyName =
        | SimpleAssemblyName of string
        | FullAssemblyName of string

    /// <summary>
    /// Represents the name of a CLR element
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrElementName =
        ///Specifies the name of an assembly 
        | AssemblyElementName of ClrAssemblyName
        ///Specifies the name of a type 
        | TypeElementName of ClrTypeName
        ///Specifies the name of a type member
        | MemberElementName of string
        ///Specifies the name of a parameter
        | ParameterElementName of string
    with
        override this.ToString() =
            match this with
            | AssemblyElementName x -> 
                match x with
                | SimpleAssemblyName(x) -> x
                | FullAssemblyName(x) -> x
            | TypeElementName x -> 
                x.Text
            | MemberElementName x -> x
            | ParameterElementName x -> x

    /// <summary>
    /// Represents a CLR type element
    /// </summary>
    type ClrTypeElement  = ClrTypeElement of Type
    with
        member this.Type = match this with ClrTypeElement(x) -> x
        member this.DeclaringType = this.Type.DeclaringType
        member this.AssemblyQualifiedName = this.Type.AssemblyQualifiedName

    /// <summary>
    /// Represents a CLR element
    /// </summary>
    type ClrElement =
        | MethodElement of MethodInfo
        | PropertyElement of PropertyInfo
        | TypeElement of ClrTypeElement
        | FieldElement of FieldInfo
        | AssemblyElement of Assembly
        | ParameterElement of ParameterInfo
        | UnionCaseElement of UnionCaseInfo


module ClrElement =         
    /// <summary>
    /// Gets the element name
    /// </summary>
    /// <param name="subject"></param>
    let internal getTypeNameFromType (subject : Type) =
        let rightOfLast (marker : string) (text : string) =
            let idx = marker |> text.LastIndexOf
            if idx <> -1 then
                (idx + marker.Length) |> text.Substring
            else
                String.Empty

        ClrTypeName(
            if subject.FullName.Contains("+") then
                subject.FullName |> rightOfLast "+"
            else
                subject.Name
            , subject.FullName |> Some
            , subject.AssemblyQualifiedName |> Some)


    /// <summary>
    /// Retrieves is element's declaring type, if applicable
    /// </summary>
    /// <param name="element"></param>
    let getDeclaringType (element : ClrElement) =
        match element with
        | MethodElement(x) -> 
            x.DeclaringType |> Some
        | PropertyElement(x) ->
            x.DeclaringType |> Some            
        | TypeElement(x) -> 
            if x.DeclaringType <> null then
                x.DeclaringType |> Some
            else
                None
        | FieldElement(x) ->
            x.DeclaringType |> Some            
        | AssemblyElement(x) ->
            None
        | ParameterElement(x) ->
            None
        | UnionCaseElement(x) ->
            x.DeclaringType |> Some
    
    /// <summary>
    /// Gets the name of the element
    /// </summary>
    /// <param name="element"></param>
    let getName (element : ClrElement) =
        match element with
        | MethodElement(x) -> 
            x.Name |> MemberElementName
        | PropertyElement(x) ->
            x.Name |> MemberElementName
        | TypeElement(x) -> 
            x.Type |> getTypeNameFromType |> TypeElementName
        | FieldElement(x) ->
            x.Name |> MemberElementName
        | AssemblyElement(x) ->
            x.FullName |> FullAssemblyName |> AssemblyElementName
        | ParameterElement(x) ->
            x.Name |> ParameterElementName
        | UnionCaseElement(x) ->
            x.Name |> MemberElementName

    let getDataMemberValue (instance : obj) (element : ClrElement) =
        match element with
        | PropertyElement(x) ->
            instance |> x.GetValue
        | FieldElement(x) ->
            instance |> x.GetValue
        | _ -> ArgumentException(sprintf "The element %O is not a data member" (element |> getName)) |> raise

    let getDataMemberType element =
        match element with
        | PropertyElement(x) ->
            x.PropertyType
        | FieldElement(x) ->
            x.FieldType
        | _ -> ArgumentException(sprintf "The element %O is not a data member" (element |> getName)) |> raise


    /// <summary>
    /// Gets the acess modifier applied to the element, if applicable
    /// </summary>
    /// <param name="element">The element to examine</param>
    let tryGetAccess (element : ClrElement)  =
        match element with
        | MethodElement(x) -> 
            if x.IsPublic then
                PublicAccess |> Some
            else if x.IsPrivate then
                PrivateAccess  |> Some
            else if x.IsAssembly then
                InternalAccess |> Some
            else if x.IsFamilyOrAssembly then
                ProtectedInternalAccess |> Some
            else
                None
        | PropertyElement(x) ->
            None        
        | TypeElement(x) -> 
            if x.Type.IsPublic  || x.Type.IsNestedPublic then
                PublicAccess |> Some
            else if x.Type.IsNestedPrivate then
                PrivateAccess |> Some
            else if x.Type.IsNotPublic || x.Type.IsNestedAssembly then
                InternalAccess |> Some
            else if x.Type.IsNestedFamORAssem then
                ProtectedInternalAccess |> Some
            else
                nosupport()
        | FieldElement(x) ->
            if x.IsPublic then
                PublicAccess |> Some
            else if x.IsPrivate then
                PrivateAccess |> Some 
            else if x.IsAssembly then
                InternalAccess |> Some
            else if x.IsFamilyOrAssembly then
                ProtectedInternalAccess |> Some
            else
                nosupport()
        | AssemblyElement(x) ->
            None
        | ParameterElement(x) ->
            None
        | UnionCaseElement(x) ->
             PublicAccess |> Some
        
    let getAccess (element : ClrElement) = element |> tryGetAccess |> Option.get

[<AutoOpen>]
module ClrElementExtensions = 
    module internal ClrAssemblyName =
        let format assname =
            match assname with
            | SimpleAssemblyName(n) | FullAssemblyName(n) -> n


    module internal ClrElementName =
        let format name =
            match name with
            | AssemblyElementName(n) -> n |> ClrAssemblyName.format
            | TypeElementName(n) -> n.Text
            | MemberElementName(n) -> n
            | ParameterElementName(n) -> n


        
    type ClrElement
    with
        member this.DeclaringType = this |> ClrElement.getDeclaringType
        member this.Name = this |> ClrElement.getName


    type ClrAssemblyName
    with
        member this.Text = this |> ClrAssemblyName.format

    type ClrTypeName
    with
        member this.Text = this.Text

    type ClrElementName
    with
        member this.Text = this |> ClrElementName.format

    type Type 
    with
        member this.ElementTypeName = this |> ClrElement.getTypeNameFromType
        member this.ElementName = this |> ClrElement.getTypeNameFromType |> TypeElementName
    
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
       | _ -> nosupport()
