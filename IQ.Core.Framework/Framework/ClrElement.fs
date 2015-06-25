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
        
    
    type ClrDataMemberElement = 
        | PropertyMember of PropertyInfo
        | FieldMember of FieldInfo

    type ClrMemberElement =
        | DataMemberElement of ClrDataMemberElement
        | MethodElement of MethodInfo

    
    type ClrElement =
        | MemberElement of ClrMemberElement
        | TypeElement of ClrTypeElement
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
        | MemberElement(x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    x.DeclaringType |> Some
                | FieldMember(x) -> 
                    x.DeclaringType |> Some
            | MethodElement(x) ->
                x.DeclaringType |> Some
        | TypeElement(x) -> 
            if x.DeclaringType <> null then
                x.DeclaringType |> Some
            else
                None
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
        | MemberElement(x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    x.Name |> MemberElementName
                | FieldMember(x) -> 
                    x.Name |> MemberElementName
            | MethodElement(x) ->
                  x.Name |> MemberElementName
        | TypeElement(x) -> 
            x.Type |> getTypeNameFromType |> TypeElementName
        | AssemblyElement(x) ->
            x.FullName |> FullAssemblyName |> AssemblyElementName
        | ParameterElement(x) ->
            x.Name |> ParameterElementName
        | UnionCaseElement(x) ->
            x.Name |> MemberElementName

    let getDataMemberValue (instance : obj) (element : ClrElement) =
        match element with
        | MemberElement(x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    instance |> x.GetValue
                | FieldMember(x) -> 
                    instance |> x.GetValue
            | MethodElement(x) ->
                  nosupport()
        | _ -> nosupport()

    let getDataMemberType element =
        match element with
        | MemberElement(x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    x.PropertyType
                | FieldMember(x) -> 
                    x.FieldType
            | MethodElement(x) ->
                  nosupport()
        | _ -> nosupport()


    /// <summary>
    /// Gets the acess modifier applied to the element, if applicable
    /// </summary>
    /// <param name="element">The element to examine</param>
    let tryGetAccess (element : ClrElement)  =
        match element with
        | MemberElement(x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    None
                | FieldMember(x) -> 
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
            | MethodElement(x) ->
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
        | AssemblyElement(x) ->
            None
        | ParameterElement(x) ->
            None
        | UnionCaseElement(x) ->
             PublicAccess |> Some
        
    /// <summary>
    /// Gets the acess modifier applied to the element, if applicable; otherwise,
    /// raises an error
    /// </summary>
    /// <param name="element">The element to examine</param>
    let getAccess (element : ClrElement) = 
        element |> tryGetAccess |> Option.get

    /// <summary>
    /// Retrieves all attributes applied to the element
    /// </summary>
    /// <param name="element">The element to examine</param>
    let getAllAttributes(element : ClrElement) =
        match element with
        | MemberElement(x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    x |> Attribute.GetCustomAttributes
                | FieldMember(x) -> 
                    x |> Attribute.GetCustomAttributes
            | MethodElement(x) ->
                    x |> Attribute.GetCustomAttributes
        | TypeElement(x) -> 
            x.Type |> Attribute.GetCustomAttributes
        | AssemblyElement(x) ->
            x |> Attribute.GetCustomAttributes
        | ParameterElement(x) ->
            x |> Attribute.GetCustomAttributes
        | UnionCaseElement(x) ->
            [|for a in x.GetCustomAttributes() -> a :?> Attribute|]
        |> List.ofArray

    /// <summary>
    /// Determines whether an attribute of a specified type has been applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let hasAttribute (element : ClrElement) (attribType : Type) =
        match element with
        | MemberElement(x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    Attribute.IsDefined(x, attribType) 
                | FieldMember(x) -> 
                    Attribute.IsDefined(x, attribType) 
            | MethodElement(x) ->
                    Attribute.IsDefined(x, attribType) 
        | TypeElement(x) -> 
            Attribute.IsDefined(x.Type, attribType) 
        | AssemblyElement(x) ->
            Attribute.IsDefined(x, attribType) 
        | ParameterElement(x) ->
            Attribute.IsDefined(x, attribType) 
        | UnionCaseElement(x) ->
            x.GetCustomAttributes() |> Array.filter(fun a -> a.GetType() = attribType) |> Array.isEmpty |> not

    /// <summary>
    /// Retrieves an attribute from the element if it exists and returns None if it odes not
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let tryGetAttribute (element : ClrElement) (attribType : Type) =
        if attribType |> hasAttribute element then
            match element with
            | MemberElement(x) -> 
                match x with
                | DataMemberElement(x) ->
                    match x with
                    | PropertyMember(x) ->
                        Attribute.GetCustomAttribute(x, attribType)
                    | FieldMember(x) -> 
                        Attribute.GetCustomAttribute(x, attribType)
                | MethodElement(x) ->
                        Attribute.GetCustomAttribute(x, attribType)
            | TypeElement(x) -> 
                Attribute.GetCustomAttribute(x.Type, attribType)
            | AssemblyElement(x) ->
                Attribute.GetCustomAttribute(x, attribType)
            | ParameterElement(x) ->
                Attribute.GetCustomAttribute(x, attribType)
            | UnionCaseElement(x) ->
                x.GetCustomAttributes() |> Array.find(fun a -> a.GetType() = attribType) :?> Attribute
            |> Some
        else
            None    

    /// <summary>
    /// Retrieves an attribute from the element if it exists and raises an exception otherwise
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getAttribute (element : ClrElement) (attribType : Type) =
        attribType |> tryGetAttribute element |> Option.get

    /// <summary>
    /// Retrieves all attributes of a specified type that have been applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getAttributes (element : ClrElement) (attribType : Type)  =
        match element with
        | MemberElement(x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    Attribute.GetCustomAttributes(x, attribType)
                | FieldMember(x) -> 
                    Attribute.GetCustomAttributes(x, attribType)
            | MethodElement(x) ->
                    Attribute.GetCustomAttributes(x, attribType)
        | TypeElement(x) -> 
            Attribute.GetCustomAttributes(x.Type, attribType)
        | AssemblyElement(x) ->
            Attribute.GetCustomAttributes(x, attribType)
        | ParameterElement(x) ->
            Attribute.GetCustomAttributes(x, attribType)
        | UnionCaseElement(x) ->
            [|for a in x.GetCustomAttributes() do if a.GetType() = attribType then yield a :?> Attribute|]
        |> List.ofArray

    /// <summary>
    /// Determines whether an attribute is applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    let hasAttributeT<'T when 'T :> Attribute>(element : ClrElement) =
        typeof<'T> |> hasAttribute  element        

    /// <summary>
    /// Retrieves an attribute applied to a member, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let tryGetAttributeT<'T when 'T :> Attribute>(element : ClrElement) =
        match typeof<'T> |> tryGetAttribute element with
        | Some(x) -> x :?> 'T |> Some
        | None -> None
    
    /// <summary>
    /// Retrieves an attribute from the element if it exists and raises an exception otherwise
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getAttributeT<'T when 'T :> Attribute>(element : ClrElement) =
        element |> tryGetAttributeT<'T> |> Option.get

    /// <summary>
    /// Retrieves an arbitrary number of attributes of the same type applied to a member
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttributesT<'T when 'T :> Attribute>(subject : MemberInfo) =
        [for a in Attribute.GetCustomAttributes(subject, typeof<'T>) -> a :?> 'T]

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
        member this.Element = this |> ClrTypeElement |> TypeElement
        member this.ElementTypeName = this |> ClrElement.getTypeNameFromType
        member this.ElementName = this.Element.Name
        member this.AccessModifier = this.Element |> ClrElement.getAccess
    
    type Assembly
    with
        member this.Element = this |> AssemblyElement
        member this.ElementName = this.Element.Name

    type MethodInfo
    with
        member this.Element = this |> MethodElement |> MemberElement
        member this.ElementName = this.Element.Name
        member this.AccessModifier = this.Element |> ClrElement.getAccess

    type ParameterInfo
    with        
        member this.Element = this |> ParameterElement
        member this.ElementName = this.Element.Name

    type PropertyInfo
    with
        member this.Element = this |> PropertyMember |> DataMemberElement |> MemberElement
        member this.ElementName = this.Element.Name
        member this.AccessModifier = this.Element |> ClrElement.getAccess

    type FieldInfo
    with
        member this.Element = this |> FieldMember |> DataMemberElement |> MemberElement
        member this.ElementName = this.Element.Name
        member this.AccessModifier = this.Element |> ClrElement.getAccess

    type UnionCaseInfo
    with
        member this.Element = this |> UnionCaseElement
        member this.ElementName = this.Element.Name


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
