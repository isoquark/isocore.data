namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic

open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns

                 


module ClrElement =         
    /// <summary>
    /// Gets the name of the element
    /// </summary>
    /// <param name="element"></param>
    let internal getName (element : ClrElement) = element |> ClrElementName.fromElement
    
         
    /// <summary>
    /// Gets the type that declares the element if applicable
    /// </summary>
    /// <param name="element">The element</param>
    let getDeclaringType (element : ClrElement) =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    x.PropertyInfo.DeclaringType |> ClrElementProvider.getType |> Some
                | FieldMember(x) -> 
                    x.FieldInfo.DeclaringType |> ClrElementProvider.getType |> Some
            | MethodElement(x) ->
                x.MethodInfo.DeclaringType |> ClrElementProvider.getType |> Some
        | TypeElement(element=x) -> 
            if x.Type.DeclaringType <> null then
                x.Type.DeclaringType |> ClrElementProvider.getType |> Some
            else
                None
        | AssemblyElement(element=x) ->
            None
        | ParameterElement(element=x) ->
            None
        | UnionCaseElement(element=x) ->
            x.UnionCaseInfo.DeclaringType |> ClrElementProvider.getType |> Some


    /// <summary>
    /// Gets the assembly in which the element is defined
    /// </summary>
    /// <param name="element">The element</param>
    let getDeclaringAssembly(element : ClrElement) =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    x.PropertyInfo.DeclaringType.Assembly
                | FieldMember(x) -> 
                    x.FieldInfo.DeclaringType.Assembly
            | MethodElement(x) ->
                x.MethodInfo.DeclaringType.Assembly
        | TypeElement(element=x) -> 
                x.Type.Assembly
        | AssemblyElement(element=x) ->
            x.Assembly
        | ParameterElement(x) ->
            x.ParamerInfo.Member.DeclaringType.Assembly
        | UnionCaseElement(element=x) ->
            x.UnionCaseInfo.DeclaringType.Assembly
        |> ClrElementProvider.getElement


                
    /// <summary>
    /// Retrieves the (direct) children of the element
    /// </summary>
    /// <param name="element">The element to examine</param>
    let getChildren(element : ClrElement) =
        match element with
        | MemberElement(children=x) -> 
            x
        | TypeElement(children=x) -> 
            x
        | AssemblyElement(children=x) -> 
            x
        | ParameterElement(_) ->
            []
        | UnionCaseElement(children=x) ->
            x

    /// <summary>
    /// Gets the acess modifier applied to the element, if applicable
    /// </summary>
    /// <param name="element">The element to examine</param>
    let tryGetAccess (element : ClrElement)  =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    None
                | FieldMember(x) -> 
                    match x with 
                    | ClrFieldElement(x) ->
                        match x with 
                            ClrReflectionPrimitive(primitive=x) ->
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
                        match x with 
                            ClrReflectionPrimitive(primitive=x) ->
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
        | TypeElement(element=x) -> 
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
        | AssemblyElement(element=x) ->
            None
        | ParameterElement(x) ->
            None
        | UnionCaseElement(element=x) ->
             PublicAccess |> Some

    /// <summary>
    /// Determines whether the element is static
    /// </summary>
    /// <param name="element">The element to examine</param>
    let isStatic (element : ClrElement) =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    x.PropertyInfo.GetMethod.IsStatic && x.PropertyInfo.SetMethod.IsStatic
                | FieldMember(x) -> 
                    x.FieldInfo.IsStatic
            | MethodElement(x) ->
                x.MethodInfo.IsStatic
        | TypeElement(element=x) -> 
                x.Type.IsAbstract && x.Type.IsSealed
        | AssemblyElement(element=x) ->
            false
        | ParameterElement(x) ->
            false
        | UnionCaseElement(element=x) ->
            false
        
        
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
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    x.PropertyInfo |> Attribute.GetCustomAttributes
                | FieldMember(x) -> 
                    x.FieldInfo |> Attribute.GetCustomAttributes
            | MethodElement(x) ->
                    x.MethodInfo |> Attribute.GetCustomAttributes
        | TypeElement(element=x) -> 
            x.Type |> Attribute.GetCustomAttributes
        | AssemblyElement(element=x) ->
            x.Assembly |> Attribute.GetCustomAttributes
        | ParameterElement(x) ->
            x.ParamerInfo |> Attribute.GetCustomAttributes
        | UnionCaseElement(element=x) ->
            [|for a in x.UnionCaseInfo.GetCustomAttributes() -> a :?> Attribute|]
        |> List.ofArray

    /// <summary>
    /// Determines whether an attribute of a specified type has been applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let hasAttribute (element : ClrElement) (attribType : Type) =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    Attribute.IsDefined(x.PropertyInfo, attribType) 
                | FieldMember(x) -> 
                    Attribute.IsDefined(x.FieldInfo, attribType) 
            | MethodElement(x) ->
                    Attribute.IsDefined(x.MethodInfo, attribType) 
        | TypeElement(element=x) -> 
            Attribute.IsDefined(x.Type, attribType) 
        | AssemblyElement(element=x) ->
            Attribute.IsDefined(x.Assembly, attribType) 
        | ParameterElement(x) ->
            Attribute.IsDefined(x.ParamerInfo, attribType) 
        | UnionCaseElement(element=x) ->
            x.UnionCaseInfo.GetCustomAttributes() |> Array.filter(fun a -> a.GetType() = attribType) |> Array.isEmpty |> not

    /// <summary>
    /// Retrieves an attribute from the element if it exists and returns None if it odes not
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let tryGetAttribute (element : ClrElement) (attribType : Type) =
        if attribType |> hasAttribute element then
            match element with
            | MemberElement(element=x) -> 
                match x with
                | DataMemberElement(x) ->
                    match x with
                    | PropertyMember(x) ->
                        Attribute.GetCustomAttribute(x.PropertyInfo, attribType)
                    | FieldMember(x) -> 
                        Attribute.GetCustomAttribute(x.FieldInfo, attribType)
                | MethodElement(x) ->
                        Attribute.GetCustomAttribute(x.MethodInfo, attribType)
            | TypeElement(element=x) -> 
                Attribute.GetCustomAttribute(x.Type, attribType)
            | AssemblyElement(element=x) ->
                Attribute.GetCustomAttribute(x.Assembly, attribType)
            | ParameterElement(x) ->
                Attribute.GetCustomAttribute(x.ParamerInfo, attribType)
            | UnionCaseElement(element=x) ->
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
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    Attribute.GetCustomAttributes(x.PropertyInfo, attribType)
                | FieldMember(x) -> 
                    Attribute.GetCustomAttributes(x.FieldInfo, attribType)
            | MethodElement(x) ->
                    Attribute.GetCustomAttributes(x.MethodInfo, attribType)
        | TypeElement(element=x) -> 
            Attribute.GetCustomAttributes(x.Type, attribType)
        | AssemblyElement(element=x) ->
            Attribute.GetCustomAttributes(x.Assembly, attribType)
        | ParameterElement(x) ->
            Attribute.GetCustomAttributes(x.ParamerInfo, attribType)
        | UnionCaseElement(element=x) ->
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

    /// <summary>
    /// Recursively traverses the element hierarchy graph and invokes the supplied handler as each element is traversed
    /// </summary>
    /// <param name="handler">The handler that will be invoked for each element</param>
    /// <param name="element"></param>
    let rec walk (handler:ClrElement->unit) element =
        element |> handler
        let children = 
            match element with
            | MemberElement(x,children) -> 
                children 
            | TypeElement(x,children) -> 
                children
            | AssemblyElement(x,children) ->
                children
            | ParameterElement(x) ->
                []
            | UnionCaseElement(x,children) ->
                children 
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
        member this.DeclaringAssembly = this |> ClrElement.getDeclaringAssembly
        member this.IsStatic = this |> ClrElement.isStatic

    /// <summary>
    /// Defines augmentations for the <see cref="System.Type"/> type
    /// </summary>
    type Type 
    with
        /// <summary>
        /// Gets the applied access modifier
        /// </summary>
        member this.AccessModifier = this.Element |> ClrElement.getAccess
    

    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.MethodInfo"/> type
    /// </summary>
    type MethodInfo
    with
        /// <summary>
        /// Gets the applied access modifier
        /// </summary>
        member this.AccessModifier = this.Element |> ClrElement.getAccess



    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.PropertyInfo"/> type
    /// </summary>
    type PropertyInfo
    with        
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
        /// Gets the applied access modifier
        /// </summary>
        member this.AccessModifier = this.Element |> ClrElement.getAccess


    /// <summary>
    /// Defines augmentations for the <see cref="ClrMemberElement"/> type
    /// </summary>
    type ClrMemberElement
    with
        /// <summary>
        /// Gets the member's declaring type
        /// </summary>
        member this.DeclaringType = this.Element |> ClrElement.getDeclaringType |> Option.get
     

module ClrAssembly =

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
