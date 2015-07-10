namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic

open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns


module ClrAttribution =
    let tryFind (attribType  : Type) (attributions : ClrAttribution seq)= 
        attributions |> Seq.tryFind(fun x -> x.AttributeInstance |> Option.get |> fun instance -> instance |> attribType.IsInstanceOfType)

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


module ClrProperty =
    let filter (members : ClrMember list) =
            [for x in members do
                match x with
                | PropertyMember(x) -> yield x
                |_ ->()
            ]

module ClrMethod =
    let filter (members : ClrMember list) =
        [for x in members do
            match x with
            | MethodMember(x) -> yield x
            |_ ->()
        ]
       
module ClrField =
    let filter (members : ClrMember list) =
            [for x in members do
                match x with
                | FieldMember(x) -> yield x
                |_ ->()
            ]

module ClrEvent =
    let filter (members : ClrMember list) =
            [for x in members do
                match x with
                | EventMember(x) -> yield x
                |_ ->()
            ]

[<AutoOpen>]
module ClrTypeExtensions =

    /// <summary>
    /// Defines augmentations for the <see cref="ClrClass"/> type
    /// </summary>
    type ClrClass
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrClass(info=x) -> x

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access

    /// <summary>
    /// Defines augmentations for the <see cref="ClrEnum"/> type
    /// </summary>
    type ClrEnum
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrEnum(info=x) -> x
        
        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access

        /// <summary>
        /// Gets the enum's underlying integral type
        /// </summary>
        member this.NumericType = match this with ClrEnum(numericType=x) -> x
        
        /// <summary>
        /// Gets the literals defined by the enumeration
        /// </summary>
        member this.Literals = this.Info.Members |> ClrField.filter


        
    /// <summary>
    /// Defines augmentations for the <see cref="ClrModule"/> type
    /// </summary>
    type ClrModule
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrModule(info=x) -> x

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access


    /// <summary>
    /// Defines augmentations for the <see cref="ClrStruct"/> type
    /// </summary>
    type ClrStruct
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrStruct(info=x) -> x

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access

        /// <summary>
        /// Specifies whether the struct isf of type Nullable<_>
        /// </summary>
        member this.IsNullable = match this with ClrStruct(isNullable=x) -> x

    /// <summary>
    /// Defines augmentations for the <see cref="ClrUnion"/> type
    /// </summary>
    type ClrUnion
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrUnion(info=x) -> x
        
        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access
        
        /// <summary>
        /// Gets the cases defined for the union
        /// </summary>
        member this.Cases = match this with ClrUnion(cases=x) -> x

    /// <summary>
    /// Defines augmentations for the <see cref="ClrRecord"/> type
    /// </summary>
    type ClrRecord
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrRecord(info=x) -> x

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access


    /// <summary>
    /// Defines augmentations for the <see cref="ClrInterface"/> type
    /// </summary>
    type ClrInterface
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrInterface(info=x) -> x

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access



    /// <summary>
    /// Defines augmentations for the <see cref="ClrCollection"/> type
    /// </summary>
    type ClrCollection
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = match this with ClrCollection(info=x) -> x

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name = this.Info.Name

        /// <summary>
        /// Gets the access modifier specified for the element
        /// </summary>
        member this.Access = this.Info.Access
    
        /// <summary>
        /// Ges the collection kind
        /// </summary>
        member this.Kind = match this with ClrCollection(kind=x) -> x
       
    
    /// <summary>
    /// Defines augmentations for the <see cref="ClrType"/> type
    /// </summary>
    type ClrType 
    with
        /// <summary>
        /// Gets the related type information
        /// </summary>
        member this.Info = 
            match this with
            | ClassType x -> x.Info
            | EnumType x -> x.Info
            | ModuleType x -> x.Info
            | CollectionType x -> x.Info
            | StructType x -> x.Info
            | UnionType x -> x.Info
            | RecordType x -> x.Info
            | InterfaceType x -> x.Info

        member this.Name = this.Info.Name
        member this.Position = this.Info.Position
        member this.ReflectedElement = this.Info.ReflectedElement
        member this.DeclaringType = this.Info.DeclaringType
        member this.DeclaredTypes = this.Info.DeclaredTypes
        member this.Kind = this.Info.Kind
        member this.IsOptionType = this.Info.IsOptionType
        member this.Members = this.Info.Members
        member this.Access = this.Info.Access
        member this.IsStatic = this.Info.IsStatic
        member this.Attributes = this.Info.Attributes
        member this.ItemValueType = this.Info.ItemValueType

        /// <summary>
        /// Gets the properties declared by the type
        /// </summary>
        member this.Properties = this.Members |> ClrProperty.filter

        /// <summary>
        /// Gets the methods declared by the type
        /// </summary>
        member this.Methods = this.Members |> ClrMethod.filter

        /// <summary>
        /// Gets the fields declared by the type
        /// </summary>
        member this.Fields = this.Members |> ClrField.filter

        /// <summary>
        /// Gets the fields declared by the type
        /// </summary>
        member this.Events = this.Members |> ClrEvent.filter


[<AutoOpen>]
module ClrMemberExtensions =
    type ClrMember
    with
        member this.Name =
            match this with
            | PropertyMember(x) -> x.Name
            | FieldMember(x) -> x.Name
            | MethodMember(x) -> x.Name
            | EventMember(x) -> x.Name
            | ConstructorMember(x) -> x.Name

        member this.Kind =
            match this with
            | PropertyMember(_) -> ClrMemberKind.Property
            | FieldMember(_) -> ClrMemberKind.StorageField
            | MethodMember(_) -> ClrMemberKind.Method
            | EventMember(_) -> ClrMemberKind.Event
            | ConstructorMember(_) -> ClrMemberKind.Constructor

        member this.Position =
            match this with
            | PropertyMember(x) -> x.Position
            | FieldMember(x) -> x.Position
            | MethodMember(x) -> x.Position
            | EventMember(x) -> x.Position
            | ConstructorMember(x) -> x.Position

        member this.IsStatic =
            match this with
            | PropertyMember(x) -> x.IsStatic
            | FieldMember(x) -> x.IsStatic
            | MethodMember(x) -> x.IsStatic
            | EventMember(x) -> false
            | ConstructorMember(x) -> x.IsStatic

        member this.Access = 
            match this with
            | PropertyMember(x) -> None
            | FieldMember(x) -> x.Access |> Some
            | MethodMember(x) -> x.Access |> Some
            | EventMember(x) -> None
            | ConstructorMember(x) -> x.Access |> Some

        member this.ReflectedElement =
            match this with
            | PropertyMember(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
            | FieldMember(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
            | MethodMember(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
            | EventMember(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
            | ConstructorMember(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
        
        /// <summary>
        /// Gets the type that declares the memeber
        /// </summary>
        member this.DeclaringType =
            match this with
            | PropertyMember(x) -> x.DeclaringType
            | FieldMember(x) -> x.DeclaringType
            | MethodMember(x) -> x.DeclaringType
            | EventMember(x) -> x.DeclaringType
            | ConstructorMember(x) -> x.DeclaringType

        /// <summary>
        /// Gets the top-level attributions
        /// </summary>
        /// <remarks>
        /// Top-level, in this context, means attributes applied directly to the CLR element and not it's constituent pieces
        /// such as property getters/settters, method parameters, etc.
        /// </remarks>
        member this.Attributes =
            match this with
            | PropertyMember(x) -> x.Attributes
            | FieldMember(x) -> x.Attributes
            | MethodMember(x) -> x.Attributes
            | EventMember(x) -> x.Attributes
            | ConstructorMember(x) -> x.Attributes


//TODO: This logic needs to be mooved to a non-extension module
[<AutoOpen>]
module ClrMethodExtensions =
    type ClrMethod
    with
        member this.TryGetAttribute<'T when 'T :> Attribute>() =
            let attribName = ClrTypeName(typeof<'T>.Name, typeof<'T>.FullName |> Some, typeof<'T>.AssemblyQualifiedName |> Some) 
            this.Attributes |> List.tryFind(fun x -> x.AttributeName = attribName)
        
        member this.HasAttribute<'T when 'T:> Attribute>() = 
            this.TryGetAttribute<'T>() |> Option.isSome

        /// <summary>
        /// Gets all non-return parameters
        /// </summary>
        member this.InputParameters = this.Parameters |> List.filter(fun x -> x.IsReturn |> not)

    

//TODO: This logic needs to be mooved to a non-extension module
[<AutoOpen>]
module ClrElementExtensions =
    type ClrElement
    with
        /// <summary>
        /// Gets the top-level attributes applied to the element
        /// </summary>
        member this.Attributes =
            match this with
            | MemberElement(x) -> x.Attributes
            | TypeElement(x) -> x.Attributes
            | AssemblyElement(x) -> x.Attributes
            | ParameterElement(x) -> x.Attributes
            | UnionCaseElement(x) -> x.Attributes

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name =
            match this with
            | MemberElement(x) -> x.Name |> MemberElementName
            | TypeElement(x) -> x.Name |> TypeElementName
            | AssemblyElement(x) -> x.Name |> AssemblyElementName
            | ParameterElement(x) -> x.Name |> ParameterElementName
            | UnionCaseElement(x) -> x.Name |> MemberElementName

        member this.DeclaringType =
            match this with
            | MemberElement(x) -> x.DeclaringType |> Some
            | TypeElement(x) -> x.DeclaringType
            | AssemblyElement(x) -> None
            | ParameterElement(x) -> None
            | UnionCaseElement(x) -> x.DeclaringType |> Some
            
        member this.Position =
            match this with
            | MemberElement(x) -> x.Position
            | TypeElement(x) -> x.Position
            | AssemblyElement(x) -> x.Position
            | ParameterElement(x) -> x.Position
            | UnionCaseElement(x) -> x.Position
        
        member this.ReflectedElement =
            match this with
            | MemberElement(x) -> x.ReflectedElement
            | TypeElement(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None
            | AssemblyElement(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None
            | ParameterElement(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None
            | UnionCaseElement(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None

        member this.Kind = 
            match this with
            | MemberElement(x) -> 
                match x.Kind with
                | ClrMemberKind.Constructor -> ClrElementKind.Constructor
                | ClrMemberKind.Event -> ClrElementKind.Event
                | ClrMemberKind.Method -> ClrElementKind.Method
                | ClrMemberKind.Property -> ClrElementKind.Property
                | ClrMemberKind.StorageField -> ClrElementKind.Field
                | _ -> nosupport()
            | TypeElement(x) -> ClrElementKind.Type
            | AssemblyElement(x) -> ClrElementKind.Assembly
            | ParameterElement(x) -> ClrElementKind.Parameter
            | UnionCaseElement(x) -> ClrElementKind.UnionCase

        member this.TryGetAttribute<'T when 'T :> Attribute>() =
            let attribName = ClrTypeName(typeof<'T>.Name, typeof<'T>.FullName |> Some, typeof<'T>.AssemblyQualifiedName |> Some) 
            this.Attributes |> List.tryFind(fun x -> x.AttributeName = attribName)
        
        member this.HasAttribute<'T when 'T:> Attribute>() = 
            this.TryGetAttribute<'T>() |> Option.isSome

                
module ClrElement = 
    let getChildren element =
        match element with
        | MemberElement(m) ->
            match m with
            | MethodMember(x) -> 
                x.Parameters |> List.map(fun x -> x |> ParameterElement)
            |_ -> []            
        | TypeElement(d) -> 
            d.Members |> List.map(fun x -> x |> MemberElement)
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
        element.Attributes |> ClrAttribution.tryFind attribType
    
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
    let getAttributes (element : ClrElement) attribType =
        element.Attributes |> List.filter(fun x -> x.AttributeName = attribType)

    /// <summary>
    /// Retrieves an attribute applied to a member, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let tryGetAttributeT<'T when 'T :> Attribute> (element : ClrElement) =
        match  element.Attributes |> ClrAttribution.tryFind typeof<'T> with
        | Some(x) -> 
            match x.AttributeInstance with
            | Some(x) -> x :?> 'T |> Some
            | None -> None
               
        | None -> None

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

