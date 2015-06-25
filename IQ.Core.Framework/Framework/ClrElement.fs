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
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
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
        override this.ToString() = this.Text

    /// <summary>
    /// Represents an assembly name
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrAssemblyName = ClrAssemblyName of simpleName : string * fullName : string option
    with
        member this.SimpleName = match this with ClrAssemblyName(simpleName=x) -> x
        member this.FullName = match this with ClrAssemblyName(fullName=x) -> x
        member this.Text =
            match this with ClrAssemblyName(simpleName, fullName) -> match fullName with
                                                                        | Some(x) -> x
                                                                        | None ->
                                                                            simpleName
        override this.ToString() = this.Text

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
            | AssemblyElementName x -> x.Text
            | TypeElementName x -> x.Text
            | MemberElementName x -> x
            | ParameterElementName x -> x


    /// <summary>
    /// Represents and encapsulates a CLR Type
    /// </summary>
    type ClrTypeElement  = ClrTypeElement of Type
    with
        /// <summary>
        /// Gets the encapluated Type
        /// </summary>
        member this.Type = match this with ClrTypeElement(x) -> x
    
    /// <summary>
    /// Represents and encapsulates a CLR Assembly
    /// </summary>
    type ClrAssemblyElement = ClrAssemblyElement of Assembly   
    with
        /// <summary>
        /// Gets the encapluated Assembly
        /// </summary>
        member this.Assembly = match this with ClrAssemblyElement(x) -> x
    
    /// <summary>
    /// Represents and encapsulates a CLR (method) parameter 
    /// </summary>
    type ClParameterElement = ClrParameterElement of ParameterInfo
    with
        /// <summary>
        /// Gets the encapluated Parameter
        /// </summary>
        member this.ParamerInfo = match this with ClrParameterElement(x) -> x

    /// <summary>
    /// Represents and encapsulates a CLR property
    /// </summary>
    type ClrPropertyElement = ClrPropertyElement of PropertyInfo
    with
        /// <summary>
        /// Gets the encapluated Property
        /// </summary>
        member this.PropertyInfo = match this with ClrPropertyElement(x) -> x

    /// <summary>
    /// Represents and encapsulates a CLR field
    /// </summary>
    type ClrFieldElement = ClrFieldElement of FieldInfo
    with
        /// <summary>
        /// Gets the encapluated Field
        /// </summary>
        member this.FieldInfo = match this with ClrFieldElement(x) -> x

    /// <summary>
    /// Represents and encapsulates a union case
    /// </summary>
    type ClrUnionCaseElement = ClrUnionCaseElement of UnionCaseInfo
    with
        /// <summary>
        /// Gets the encapsulated case
        /// </summary>
        member this.UnionCaseInfo = match this with ClrUnionCaseElement(x) -> x

    /// <summary>
    /// Represents and encapsulates a CLR method
    /// </summary>
    type ClrMethodElement = ClrMethodElement of MethodInfo
    with
        /// <summary>
        /// Gets the encapsulated method
        /// </summary>
        member this.MethodInfo = match this with ClrMethodElement(x) -> x
    
    type ClrDataMemberElement = 
        | PropertyMember of ClrPropertyElement
        | FieldMember of ClrFieldElement

    type ClrMemberElement =
        | DataMemberElement of ClrDataMemberElement
        | MethodElement of ClrMethodElement


    type ClrElement =
        | MemberElement of ClrMemberElement
        | TypeElement of ClrTypeElement
        | AssemblyElement of ClrAssemblyElement
        | ParameterElement of ClParameterElement
        | UnionCaseElement of ClrUnionCaseElement
        


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
                    x.PropertyInfo.DeclaringType |> Some
                | FieldMember(x) -> 
                    x.FieldInfo.DeclaringType |> Some
            | MethodElement(x) ->
                x.MethodInfo.DeclaringType |> Some
        | TypeElement(x) -> 
            if x.Type.DeclaringType <> null then
                x.Type.DeclaringType |> Some
            else
                None
        | AssemblyElement(x) ->
            None
        | ParameterElement(x) ->
            None
        | UnionCaseElement(x) ->
            x.UnionCaseInfo.DeclaringType |> Some
    
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
                    x.PropertyInfo.Name |> MemberElementName
                | FieldMember(x) -> 
                    x.FieldInfo.Name |> MemberElementName
            | MethodElement(x) ->
                  x.MethodInfo.Name |> MemberElementName
        | TypeElement(x) -> 
            x.Type |> getTypeNameFromType |> TypeElementName
        | AssemblyElement(x) ->
            ClrAssemblyName(x.Assembly.SimpleName, x.Assembly.FullName |> Some) |> AssemblyElementName
        | ParameterElement(x) ->
            x.ParamerInfo.Name |> ParameterElementName
        | UnionCaseElement(x) ->
            x.UnionCaseInfo.Name |> MemberElementName

    /// <summary>
    /// Determines whether the element is a member 
    /// </summary>
    /// <param name="element">The element to test</param>
    let isMember (element : ClrElement) =
        match element with
        | MemberElement(x) -> true
        |_ -> false

    /// <summary>
    /// Inteprets the CLR element as a type element if possible; otherwise, an error is raised
    /// </summary>
    /// <param name="element">The element to interpret</param>
    let asMemberElement (element : ClrElement)   =            
        match element with
        | MemberElement(x) -> x
        | _ ->
            ArgumentException(sprintf"Element %O is not a member"  (element |> getName)) |> raise            
    
    
    /// <summary>
    /// Determines whether the element is a type
    /// </summary>
    /// <param name="element">The element to test</param>
    let isType (element : ClrElement) =
        match element with
        | TypeElement(_) -> true
        | _ -> false

    /// <summary>
    /// Inteprets the CLR element as a type element if possible; otherwise, an error is raised
    /// </summary>
    /// <param name="element">The element to interpret</param>
    let asTypeElement (element : ClrElement)   =            
        match element with
        | TypeElement(x) -> x
        | _ ->
            ArgumentException(sprintf"Element %O is not a type"  (element |> getName)) |> raise            

    /// <summary>
    /// Determines whether the element is a data member
    /// </summary>
    /// <param name="element">The element to test</param>
    let isDataMember (element : ClrElement) =
        match element with
        | MemberElement(x) -> 
            match x with
            | DataMemberElement(_) -> true
            | MethodElement(_) -> false
        | _ -> false
        
    /// <summary>
    /// Inteprets the CLR element as a data member if possible; otherwise, an error is raised
    /// </summary>
    /// <param name="element">The element to interpret</param>
    let asDataMember (element : ClrElement) =
        match element with
        | MemberElement(x) -> 
            match x with
            | DataMemberElement(x) -> x
            | _ -> ArgumentException(sprintf"Element %O is not a data member"  (element |> getName)) |> raise            
        | _ -> ArgumentException(sprintf"Element %O is not a data member"  (element |> getName)) |> raise            

    

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
                    match x with 
                    | ClrFieldElement(x) ->
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
                match x with 
                | ClrMethodElement(x) ->
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
                    x.PropertyInfo |> Attribute.GetCustomAttributes
                | FieldMember(x) -> 
                    x.FieldInfo |> Attribute.GetCustomAttributes
            | MethodElement(x) ->
                    x.MethodInfo |> Attribute.GetCustomAttributes
        | TypeElement(x) -> 
            x.Type |> Attribute.GetCustomAttributes
        | AssemblyElement(x) ->
            x.Assembly |> Attribute.GetCustomAttributes
        | ParameterElement(x) ->
            x.ParamerInfo |> Attribute.GetCustomAttributes
        | UnionCaseElement(x) ->
            [|for a in x.UnionCaseInfo.GetCustomAttributes() -> a :?> Attribute|]
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
                    Attribute.IsDefined(x.PropertyInfo, attribType) 
                | FieldMember(x) -> 
                    Attribute.IsDefined(x.FieldInfo, attribType) 
            | MethodElement(x) ->
                    Attribute.IsDefined(x.MethodInfo, attribType) 
        | TypeElement(x) -> 
            Attribute.IsDefined(x.Type, attribType) 
        | AssemblyElement(x) ->
            Attribute.IsDefined(x.Assembly, attribType) 
        | ParameterElement(x) ->
            Attribute.IsDefined(x.ParamerInfo, attribType) 
        | UnionCaseElement(x) ->
            x.UnionCaseInfo.GetCustomAttributes() |> Array.filter(fun a -> a.GetType() = attribType) |> Array.isEmpty |> not

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
                        Attribute.GetCustomAttribute(x.PropertyInfo, attribType)
                    | FieldMember(x) -> 
                        Attribute.GetCustomAttribute(x.FieldInfo, attribType)
                | MethodElement(x) ->
                        Attribute.GetCustomAttribute(x.MethodInfo, attribType)
            | TypeElement(x) -> 
                Attribute.GetCustomAttribute(x.Type, attribType)
            | AssemblyElement(x) ->
                Attribute.GetCustomAttribute(x.Assembly, attribType)
            | ParameterElement(x) ->
                Attribute.GetCustomAttribute(x.ParamerInfo, attribType)
            | UnionCaseElement(x) ->
                x.UnionCaseInfo.GetCustomAttributes() |> Array.find(fun a -> a.GetType() = attribType) :?> Attribute
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
                    Attribute.GetCustomAttributes(x.PropertyInfo, attribType)
                | FieldMember(x) -> 
                    Attribute.GetCustomAttributes(x.FieldInfo, attribType)
            | MethodElement(x) ->
                    Attribute.GetCustomAttributes(x.MethodInfo, attribType)
        | TypeElement(x) -> 
            Attribute.GetCustomAttributes(x.Type, attribType)
        | AssemblyElement(x) ->
            Attribute.GetCustomAttributes(x.Assembly, attribType)
        | ParameterElement(x) ->
            Attribute.GetCustomAttributes(x.ParamerInfo, attribType)
        | UnionCaseElement(x) ->
            [|for a in x.UnionCaseInfo.GetCustomAttributes() do if a.GetType() = attribType then yield a :?> Attribute|]
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

module ClrDataMemberElement =
    let getValue (instance : obj) (element : ClrDataMemberElement) =
        match element with
        | PropertyMember(x) ->
            instance |> x.PropertyInfo.GetValue
        | FieldMember(x) -> 
            instance |> x.FieldInfo.GetValue

    let getType element =
        match element with
        | PropertyMember(x) ->
            x.PropertyInfo.PropertyType
        | FieldMember(x) -> 
            x.FieldInfo.FieldType

        

[<AutoOpen>]
module ClrElementExtensions = 

    /// <summary>
    /// Defines augmentations for the <see cref="ClrElement"/> type
    /// </summary>
    type ClrElement
    with
        member this.DeclaringType = this |> ClrElement.getDeclaringType
        member this.Name = this |> ClrElement.getName

    /// <summary>
    /// Defines augmentations for the <see cref="ClrElementName"/> type
    /// </summary>
    type ClrElementName
    with
        /// <summary>
        /// Renders the name as text
        /// </summary>
        member this.Text = 
            match this with
            | AssemblyElementName(n) -> n.Text
            | TypeElementName(n) -> n.Text
            | MemberElementName(n) -> n
            | ParameterElementName(n) -> n


    /// <summary>
    /// Defines augmentations for the <see cref="System.Type"/> type
    /// </summary>
    type Type 
    with
        /// <summary>
        /// Interprets the method as a <see cref="ClrTypeElement"/>
        /// </summary>
        member this.TypeElement = this |> ClrTypeElement
        member this.Element = this.TypeElement |> TypeElement
        member this.ElementTypeName = this |> ClrElement.getTypeNameFromType
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the type
        /// </summary>
        member this.ElementName = this.Element.Name
        member this.AccessModifier = this.Element |> ClrElement.getAccess
    
    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.Assembly"/> type
    /// </summary>
    type Assembly
    with
        /// <summary>
        /// Interprets the assembly as a <see cref="ClrAssemblyElement"/>
        /// </summary>
        member this.AssemblyElement = this |> ClrAssemblyElement
        /// <summary>
        /// Interprets the assembly as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.AssemblyElement |> AssemblyElement
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
        /// Interprets the method as a <see cref="ClrMethodElement"/>
        /// </summary>
        member this.MethodElement = this |> ClrMethodElement
        /// <summary>
        /// Interprets the method as a <see cref="ClrMemberElement"/>
        /// </summary>
        member this.MemberElement = this.MethodElement |> MethodElement
        /// <summary>
        /// Interprets the method as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.MemberElement |> MemberElement
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the method
        /// </summary>
        member this.ElementName = this.Element.Name
        /// <summary>
        /// Gets the applied access modifier
        /// </summary>
        member this.AccessModifier = this.Element |> ClrElement.getAccess


    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.ParameterInfo"/> type
    /// </summary>
    type ParameterInfo
    with        
        /// <summary>
        /// Interprets the parameter as a <see cref="ClrParameterElement"/>
        /// </summary>
        member this.ParameterElement = this |> ClrParameterElement
        /// <summary>
        /// Interprets the parameter as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.ParameterElement |> ParameterElement
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
        /// Interprets the property as a <see cref="ClrPropertyElement"/>
        /// </summary>
        member this.PropertyElement = this |> ClrPropertyElement
        /// <summary>
        /// Interprets the property as a <see cref="ClrDataMemberElement"/>
        /// </summary>
        member this.DataMemberElement = this.PropertyElement |> PropertyMember
        /// <summary>
        /// Interprets the property as a <see cref="ClrMemberElement"/>
        /// </summary>
        member this.MemberElement = this.DataMemberElement |> DataMemberElement
        /// <summary>
        /// Interprets the property as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.MemberElement |> MemberElement
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the property
        /// </summary>
        member this.ElementName = this.Element.Name
        /// <summary>
        /// Gets the applied access modifier
        /// </summary>
        member this.AccessModifier = this.Element |> ClrElement.getAccess


    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.FieldInfo"/> type
    /// </summary>
    type FieldInfo
    with
        /// <summary>
        /// Interprets the field as a <see cref="ClrFieldElement"/>
        /// </summary>
        member this.FieldElement = this |> ClrFieldElement
        member this.DataMemberElement = this.FieldElement |> FieldMember
        member this.MemberElement = this.DataMemberElement |> DataMemberElement
        /// <summary>
        /// Interprets the field as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.MemberElement |> MemberElement
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the field
        /// </summary>
        member this.ElementName = this.Element.Name
        /// <summary>
        /// Gets the applied access modifier
        /// </summary>
        member this.AccessModifier = this.Element |> ClrElement.getAccess

    /// <summary>
    /// Defines augmentations for the <see cref="Microsoft.FSharp.Reflection.UnionCaseInfo"/> type
    /// </summary>
    type UnionCaseInfo
    with
        /// <summary>
        /// Interprets the case as a <see cref="ClrUnionCaseElement"/>
        /// </summary>
        member this.UnionCaseElement = this |> ClrUnionCaseElement
        /// <summary>
        /// Interprets the case as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.UnionCaseElement |> UnionCaseElement
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the case
        /// </summary>
        member this.ElementName = this.Element.Name


    /// <summary>
    /// Defines augmentations for the <see cref="ClrPopertyElement"/> type
    /// </summary>
    type ClrPropertyElement
    with
        member this.PropertyMemberElement = this.PropertyInfo.DataMemberElement
        member this.DataMemberElement = this.PropertyInfo.MemberElement
        /// <summary>
        /// Interprets the property as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.PropertyInfo.Element

    /// <summary>
    /// Defines augmentations for the <see cref="ClrMemberElement"/> type
    /// </summary>
    type ClrMemberElement
    with
        /// <summary>
        /// Interprets the member as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> MemberElement
        /// <summary>
        /// Gets the member's declaring type
        /// </summary>
        member this.DeclaringType = this.Element |> ClrElement.getDeclaringType |> Option.get
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the member 
        /// </summary>
        member this.ElementName = this.Element.Name



module ClrAssembly =
    /// <summary>
    /// Gets the type elements defined in the assembly
    /// </summary>
    /// <param name="subject"></param>
    let getTypeElements (subject : ClrAssemblyElement) =
        subject.Assembly |> Assembly.getTypes |> List.map ClrTypeElement

    /// <summary>
    /// Retrieves a text resource embedded in the subject assembly if found
    /// </summary>
    /// <param name="shortName">The name of the resource, excluding the namespace path</param>
    /// <param name="subject">The assembly that contains the resource</param>
    let findTextResource shortName (subject : ClrAssemblyElement) =
        subject.Assembly |> Assembly.findTextResource shortName        

    /// <summary>
    /// Writes a text resource contained in an assembly to a file and returns the path
    /// </summary>
    /// <param name="shortName">The name of the resource, excluding the namespace path</param>
    /// <param name="outputDir">The directory into which the resource will be deposited</param>
    /// <param name="subject">The assembly that contains the resource</param>
    let writeTextResource shortName outputDir (subject : ClrAssemblyElement) =
        subject.Assembly |> Assembly.writeTextResource shortName outputDir

        
module ClrTypeElement =
    let getMembers (subject : ClrTypeElement) =
        [
            yield! subject.Type |> Type.getPureMethods |> List.map(fun x -> x.MemberElement)
            yield! subject.Type |> Type.getProperties |> List.map(fun x -> x.MemberElement)
            yield! subject.Type |> Type.getPureFields |> List.map(fun x -> x.MemberElement)
        ]

