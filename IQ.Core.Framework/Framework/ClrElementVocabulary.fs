namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic

open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns

/// <summary>
/// Defines a rudimentary vocabulary for representing CLR metadata. By intent, it is not complete.
/// </summary>
[<AutoOpen>]
module ClrElementVocabulary = 

    /// <summary>
    /// Represents an association between an attribute and the element to which it applies
    /// </summary>
    type ClrAttribution = {
        /// The element to which the attribute is applied
        Target : ClrElementName
        /// The name of the attribute
        AttributeName : ClrTypeName
        /// The values applied by the attribute
        AppliedValues : ValueIndex
        /// The attribute instance if applicable
        AttributeInstance : Attribute option
    }

    type ClrAttributionIndex = ClrAttributionIndex of attribToAttribution : Map<ClrTypeName, ClrAttribution>

    type IClrElementDescription =
        abstract Name : ClrElementName
        abstract Position : int
        abstract ReflectedElemment : obj option
        abstract Attributions : ClrAttribution list
        abstract DeclaringType : ClrTypeName option
    
    /// <summary>
    /// Represents a CLR property
    /// </summary>
    type ClrProperty = {
        /// The name of the property 
        Name : ClrMemberName        
        /// The position of the property
        Position : int         
        /// The reflected property, if applicable
        ReflectedElement : PropertyInfo option        
        /// The attributes applied directly to the property
        Attributes : ClrAttribution list
        /// The attributes applied directly to the get method, if applicable
        GetMethodAttributes : ClrAttribution list
        /// The attributes applied directly to the set method, if applicable
        SetMethodAttributes : ClrAttribution list
        /// The name of the type that declares the property
        DeclaringType : ClrTypeName           
        /// The type of the property value
        ValueType : ClrTypeName
        /// Specifies whether the property is of option<> type
        IsOptional : bool
        /// Specifies whether the property has a get accessor
        CanRead : bool
        /// The access specifier of the get accessor if one exists
        ReadAccess : ClrAccess option      
        /// Specifies whether the property has a set accessor
        CanWrite : bool
        /// The access specifier of the set accessor if one exists
        WriteAccess : ClrAccess option        
        /// Specifies whether the property is static
        IsStatic : bool
    }

    /// <summary>
    /// Represents a field
    /// </summary>
    type ClrField = {
        /// The name of the property 
        Name : ClrMemberName        
        /// The reflected property, if applicable
        ReflectedElement : FieldInfo option
        /// The position of the property
        Position : int            
        /// The attributes applied to the field
        Attributes : ClrAttribution list
        /// The field's access specifier
        Access : ClrAccess
        /// Specifies whether the field is static
        IsStatic : bool
        /// Specifies the name of the field type
        FieldType : ClrTypeName
        /// The name of the type that declares the field
        DeclaringType : ClrTypeName   
        /// Specifies whether the field is a literal value
        IsLiteral : bool
        /// The value of the literal encoded as a string, if applicable
        LiteralValue : string option        
    }

    /// <summary>
    /// Represents a method parameter
    /// </summary>
    type ClrMethodParameter = {
        /// The name of the parameter
        Name : ClrParameterName
        /// The position of the parameter
        Position : int    
        /// The reflected method, if applicable                
        ReflectedElement : ParameterInfo option
        /// The attributes applied to the parameter
        Attributes : ClrAttribution list
        /// Specifies whether the parameter is required
        CanOmit : bool   
        /// Specifies the name of the parameter type
        ParameterType : ClrTypeName    
        /// The name of the method that declares the parameter
        DeclaringMethod : ClrMemberName
        /// Specifies whether the parameter represents a return
        IsReturn : bool
    }

    /// <summary>
    /// Represents a method
    /// </summary>
    type ClrMethod = {
        /// The name of the method
        Name : ClrMemberName
        /// The reflected method, if applicable        
        ReflectedElement : MethodInfo option
        /// The position of the method
        Position : int            
        /// The attributes applied to the method
        Attributes : ClrAttribution list
        /// The method's access specifier
        Access : ClrAccess
        /// Specifies whether the method is static
        IsStatic : bool
        /// The method parameters
        Parameters : ClrMethodParameter list
        /// The method return type
        ReturnType : ClrTypeName option
        /// The attributes applied to the method return
        /// TODO: calculate this from the return parameter attributes
        ReturnAttributes : ClrAttribution list
        /// The name of the type that declares the method
        DeclaringType : ClrTypeName           

    }
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

    /// <summary>
    /// Represents a constructor
    /// </summary>
    type ClrConstructor = {
        /// The name of the constructor    
        Name : ClrMemberName
        /// The reflected constructor, if applicable        
        ReflectedElement : ConstructorInfo option
        /// The position of the property
        Position : int            
        /// The attributes applied to the constructor
        Attributes : ClrAttribution list        
        /// The constructor's access specifier
        Access : ClrAccess
        /// Specifies whether the constructor is static
        IsStatic : bool
        /// The constructor parameters
        Parameters : ClrMethodParameter list
        /// The name of the type that declares the constructor
        DeclaringType : ClrTypeName           
    }

    /// <summary>
    /// Represents an event
    /// </summary>
    type ClrEvent = {
        /// The name of the event
        Name : ClrMemberName
        /// The reflected event, if applicable        
        ReflectedElement : EventInfo option
        /// The position of the event
        Position : int                
        /// The attributes applied to the event
        Attributes : ClrAttribution list
        /// The name of the type that declares the event
        DeclaringType : ClrTypeName           
    }

    /// <summary>
    /// Represents a union case
    /// </summary>
    type ClrUnionCase = {
        /// The name of the type
        Name : ClrMemberName
        /// The position of the type
        Position : int    
        /// The attributes applied to the case
        Attributes : ClrAttribution list
        /// The name of the type that declares the case
        DeclaringType : ClrTypeName           
        /// The reflected case, if applicable
        ReflectedElement : UnionCaseInfo option
    }
    
    /// <summary>
    /// Represents a member
    /// </summary>
    [<DebuggerDisplay("{Name}")>]
    type ClrMember =
    | PropertyMember of ClrProperty
    | FieldMember of ClrField
    | MethodMember of ClrMethod
    | EventMember of ClrEvent
    | ConstructorMember of ClrConstructor
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
                                                    
    
    /// <summary>
    /// Represents a type
    /// </summary>
    [<DebuggerDisplay("{Name}")>]
    type ClrType = {
        /// The name of the type
        Name : ClrTypeName
        /// The position of the type
        Position : int
        /// The reflected type, if applicable        
        ReflectedElement : Type option
        /// The name of the type that declares the type, if any
        DeclaringType : ClrTypeName option
        /// The nested types declared by the type
        DeclaredTypes : ClrTypeName list
        /// The kind of type, if recognized
        Kind : ClrTypeKind
        /// The kind of collection represented by the type, if applicable
        CollectionKind : ClrCollectionKind option
        //Specifies whether the type is of the form option<_>
        IsOptionType : bool
        //The type members
        Members : ClrMember list
        //The access specifier applied to the type
        Access : ClrAccess
        /// Specifies whether the type is static
        IsStatic : bool
        /// The attributes applied to the type
        Attributes : ClrAttribution list
        /// Specifies the type of the encapsulated value; will be different from
        /// the Name whenever dealing with options, collections and other
        /// parametrized types
        ItemValueType : ClrTypeName
    }
    with
        /// <summary>
        /// Gets the properties declared by the type
        /// </summary>
        member this.Properties = 
            [for x in this.Members do
                match x with
                | PropertyMember(x) -> yield x
                |_ ->()
            ]

        /// <summary>
        /// Gets the methods declared by the type
        /// </summary>
        member this.Methods =
            [for x in this.Members do
                match x with
                | MethodMember(x) -> yield x
                |_ ->()
            ]

        /// <summary>
        /// Gets the fields declared by the type
        /// </summary>
        member this.Fields =
            [for x in this.Members do
                match x with
                | FieldMember(x) -> yield x
                |_ ->()
            ]

        /// <summary>
        /// Gets the fields declared by the type
        /// </summary>
        member this.Events =
            [for x in this.Members do
                match x with
                | EventMember(x) -> yield x
                |_ ->()
            ]



    /// <summary>
    /// Represents an Enum
    /// </summary>
    type ClrEnum = ClrEnumDescription of t : ClrType
    with 
        member private this.Type = match this with ClrEnumDescription(t = x) -> x
        /// The name of the type
        member this.Name  = this.Type.Name
        /// The position of the type
        member this.Position = this.Type.Position
        /// The reflected type, if applicable        
        member this.ReflectedElement = this.ReflectedElement
        /// The name of the type that declares the type, if any
        member this.DeclaringType = this.DeclaringType

        

    /// <summary>
    /// Represents an assembly
    /// </summary>
    type ClrAssembly = {
        /// The name of the assembly
        Name : ClrAssemblyName
        /// The reflected assembly, if applicable                
        ReflectedElement : Assembly option
        /// The position of the assembly relative to its specification/reflection context
        Position : int
        /// The types defined in the assembly
        Types : ClrType list
        /// The attributes applied to the assembly
        Attributes : ClrAttribution list
    }

    /// <summary>
    /// Represents any CLR element
    /// </summary>
    type ClrElement =
        | MemberElement of description : ClrMember
        | TypeElement of description : ClrType
        | AssemblyElemement of description : ClrAssembly
        | ParameterElement of description : ClrMethodParameter
        | UnionCaseElement of description : ClrUnionCase
    with
        /// <summary>
        /// Gets the top-level attributes applied to the element
        /// </summary>
        member this.Attributes =
            match this with
            | MemberElement(x) -> x.Attributes
            | TypeElement(x) -> x.Attributes
            | AssemblyElemement(x) -> x.Attributes
            | ParameterElement(x) -> x.Attributes
            | UnionCaseElement(x) -> x.Attributes

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name =
            match this with
            | MemberElement(x) -> x.Name |> MemberElementName
            | TypeElement(x) -> x.Name |> TypeElementName
            | AssemblyElemement(x) -> x.Name |> AssemblyElementName
            | ParameterElement(x) -> x.Name |> ParameterElementName
            | UnionCaseElement(x) -> x.Name |> MemberElementName

        member this.DeclaringType =
            match this with
            | MemberElement(x) -> x.DeclaringType |> Some
            | TypeElement(x) -> x.DeclaringType
            | AssemblyElemement(x) -> None
            | ParameterElement(x) -> None
            | UnionCaseElement(x) -> x.DeclaringType |> Some
            
        member this.Position =
            match this with
            | MemberElement(x) -> x.Position
            | TypeElement(x) -> x.Position
            | AssemblyElemement(x) -> x.Position
            | ParameterElement(x) -> x.Position
            | UnionCaseElement(x) -> x.Position
        
        member this.ReflectedElement =
            match this with
            | MemberElement(x) -> x.ReflectedElement
            | TypeElement(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None
            | AssemblyElemement(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None
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
            | AssemblyElemement(x) -> ClrElementKind.Assembly
            | ParameterElement(x) -> ClrElementKind.Parameter
            | UnionCaseElement(x) -> ClrElementKind.UnionCase

        member this.TryGetAttribute<'T when 'T :> Attribute>() =
            let attribName = ClrTypeName(typeof<'T>.Name, typeof<'T>.FullName |> Some, typeof<'T>.AssemblyQualifiedName |> Some) 
            this.Attributes |> List.tryFind(fun x -> x.AttributeName = attribName)
        
        member this.HasAttribute<'T when 'T:> Attribute>() = 
            this.TryGetAttribute<'T>() |> Option.isSome
                      
    /// <summary>
    /// Represents the intent to select a set of <see cref="ClrType"/> representations
    /// </summary>
    type ClrTypeQuery =
        | FindTypeByName of name : ClrTypeName

    /// <summary>
    /// Represents the intent to select a set of <see cref="ClrProperty"/> representations
    /// </summary>
    type ClrPropertyQuery = 
        | FindPropertyByName of name : ClrMemberName * typeQuery : ClrTypeQuery
        | FindPropertiesByType of typeQuery : ClrTypeQuery
         
    /// <summary>
    /// Represents the intent to select a set of <see cref="ClrAssembly"/> representations
    /// </summary>
    type ClrAssemblyQuery =
        | FindAssemblyByName of name : ClrAssemblyName

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
        | AssemblyElemement(_) ->
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

                

