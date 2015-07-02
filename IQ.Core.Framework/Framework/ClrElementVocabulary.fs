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
    /// Describes a property
    /// </summary>
    type ClrPropertyDescription = {
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
    /// Describes a field
    /// </summary>
    type ClrFieldDescription = {
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
    /// Describes a method parameter
    /// </summary>
    type ClrParameterDescription = {
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
    /// Describes a method
    /// </summary>
    type ClrMethodDescription = {
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
        Parameters : ClrParameterDescription list
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
    /// Describes a constructor
    /// </summary>
    type ClrConstructorDescription = {
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
        Parameters : ClrParameterDescription list
        /// The name of the type that declares the constructor
        DeclaringType : ClrTypeName           
    }

    /// <summary>
    /// Describes an event
    /// </summary>
    type ClrEventDescription = {
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
    /// Describes a union case
    /// </summary>
    type ClrUnionCaseDescription = {
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
    /// Describes a member
    /// </summary>
    [<DebuggerDisplay("{Name}")>]
    type ClrMemberDescription =
    | PropertyDescription of ClrPropertyDescription
    | FieldDescription of ClrFieldDescription
    | MethodDescription of ClrMethodDescription
    | EventDescription of ClrEventDescription
    | ConstructorDescription of ClrConstructorDescription
    with
        member this.Name =
            match this with
            | PropertyDescription(x) -> x.Name
            | FieldDescription(x) -> x.Name
            | MethodDescription(x) -> x.Name
            | EventDescription(x) -> x.Name
            | ConstructorDescription(x) -> x.Name

        member this.Kind =
            match this with
            | PropertyDescription(_) -> ClrMemberKind.Property
            | FieldDescription(_) -> ClrMemberKind.StorageField
            | MethodDescription(_) -> ClrMemberKind.Method
            | EventDescription(_) -> ClrMemberKind.Event
            | ConstructorDescription(_) -> ClrMemberKind.Constructor

        member this.Position =
            match this with
            | PropertyDescription(x) -> x.Position
            | FieldDescription(x) -> x.Position
            | MethodDescription(x) -> x.Position
            | EventDescription(x) -> x.Position
            | ConstructorDescription(x) -> x.Position

        member this.IsStatic =
            match this with
            | PropertyDescription(x) -> x.IsStatic
            | FieldDescription(x) -> x.IsStatic
            | MethodDescription(x) -> x.IsStatic
            | EventDescription(x) -> false
            | ConstructorDescription(x) -> x.IsStatic

        member this.Access = 
            match this with
            | PropertyDescription(x) -> None
            | FieldDescription(x) -> x.Access |> Some
            | MethodDescription(x) -> x.Access |> Some
            | EventDescription(x) -> None
            | ConstructorDescription(x) -> x.Access |> Some

        member this.ReflectedElement =
            match this with
            | PropertyDescription(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
            | FieldDescription(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
            | MethodDescription(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
            | EventDescription(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
            | ConstructorDescription(x) -> match x.ReflectedElement with |Some(x) -> x :> obj |> Some | None -> None
        
        /// <summary>
        /// Gets the type that declares the memeber
        /// </summary>
        member this.DeclaringType =
            match this with
            | PropertyDescription(x) -> x.DeclaringType
            | FieldDescription(x) -> x.DeclaringType
            | MethodDescription(x) -> x.DeclaringType
            | EventDescription(x) -> x.DeclaringType
            | ConstructorDescription(x) -> x.DeclaringType

        /// <summary>
        /// Gets the top-level attributions
        /// </summary>
        /// <remarks>
        /// Top-level, in this context, means attributes applied directly to the CLR element and not it's constituent pieces
        /// such as property getters/settters, method parameters, etc.
        /// </remarks>
        member this.Attributes =
            match this with
            | PropertyDescription(x) -> x.Attributes
            | FieldDescription(x) -> x.Attributes
            | MethodDescription(x) -> x.Attributes
            | EventDescription(x) -> x.Attributes
            | ConstructorDescription(x) -> x.Attributes
                                                    
    
    /// <summary>
    /// Describes a type
    /// </summary>
    [<DebuggerDisplay("{Name}")>]
    type ClrTypeDescription = {
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
        Members : ClrMemberDescription list
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
                | PropertyDescription(x) -> yield x
                |_ ->()
            ]

        /// <summary>
        /// Gets the methods declared by the type
        /// </summary>
        member this.Methods =
            [for x in this.Members do
                match x with
                | MethodDescription(x) -> yield x
                |_ ->()
            ]

        /// <summary>
        /// Gets the fields declared by the type
        /// </summary>
        member this.Fields =
            [for x in this.Members do
                match x with
                | FieldDescription(x) -> yield x
                |_ ->()
            ]

        /// <summary>
        /// Gets the fields declared by the type
        /// </summary>
        member this.Events =
            [for x in this.Members do
                match x with
                | EventDescription(x) -> yield x
                |_ ->()
            ]



    type ClrEnumDescription = ClrEnumDescription of t : ClrTypeDescription
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
    /// Describes an assembly
    /// </summary>
    type ClrAssemblyDescription = {
        /// The name of the assembly
        Name : ClrAssemblyName
        /// The reflected assembly, if applicable                
        ReflectedElement : Assembly option
        /// The position of the assembly relative to its specification/reflection context
        Position : int
        /// The types defined in the assembly
        Types : ClrTypeDescription list
        /// The attributes applied to the assembly
        Attributes : ClrAttribution list
    }

    /// <summary>
    /// Describes c CLR element
    /// </summary>
    type ClrElementDescription =
        | MemberDescription of description : ClrMemberDescription
        | TypeDescription of description : ClrTypeDescription
        | AssemblyDescription of description : ClrAssemblyDescription
        | ParameterDescription of description : ClrParameterDescription
        | UnionCaseDescription of description : ClrUnionCaseDescription
    with
        /// <summary>
        /// Gets the top-level attributes applied to the element
        /// </summary>
        member this.Attributes =
            match this with
            | MemberDescription(x) -> x.Attributes
            | TypeDescription(x) -> x.Attributes
            | AssemblyDescription(x) -> x.Attributes
            | ParameterDescription(x) -> x.Attributes
            | UnionCaseDescription(x) -> x.Attributes

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        member this.Name =
            match this with
            | MemberDescription(x) -> x.Name |> MemberElementName
            | TypeDescription(x) -> x.Name |> TypeElementName
            | AssemblyDescription(x) -> x.Name |> AssemblyElementName
            | ParameterDescription(x) -> x.Name |> ParameterElementName
            | UnionCaseDescription(x) -> x.Name |> MemberElementName

        member this.DeclaringType =
            match this with
            | MemberDescription(x) -> x.DeclaringType |> Some
            | TypeDescription(x) -> x.DeclaringType
            | AssemblyDescription(x) -> None
            | ParameterDescription(x) -> None
            | UnionCaseDescription(x) -> x.DeclaringType |> Some
            
        member this.Position =
            match this with
            | MemberDescription(x) -> x.Position
            | TypeDescription(x) -> x.Position
            | AssemblyDescription(x) -> x.Position
            | ParameterDescription(x) -> x.Position
            | UnionCaseDescription(x) -> x.Position
        
        member this.ReflectedElement =
            match this with
            | MemberDescription(x) -> x.ReflectedElement
            | TypeDescription(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None
            | AssemblyDescription(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None
            | ParameterDescription(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None
            | UnionCaseDescription(x) -> match x.ReflectedElement with |Some(y) -> y:> obj|>Some |None -> None

        member this.Kind = 
            match this with
            | MemberDescription(x) -> 
                match x.Kind with
                | ClrMemberKind.Constructor -> ClrElementKind.Constructor
                | ClrMemberKind.Event -> ClrElementKind.Event
                | ClrMemberKind.Method -> ClrElementKind.Method
                | ClrMemberKind.Property -> ClrElementKind.Property
                | ClrMemberKind.StorageField -> ClrElementKind.StorageField
                | _ -> nosupport()
            | TypeDescription(x) -> ClrElementKind.Type
            | AssemblyDescription(x) -> ClrElementKind.Assembly
            | ParameterDescription(x) -> ClrElementKind.Parameter
            | UnionCaseDescription(x) -> ClrElementKind.UnionCase

        member this.TryGetAttribute<'T when 'T :> Attribute>() =
            let attribName = ClrTypeName(typeof<'T>.Name, typeof<'T>.FullName |> Some, typeof<'T>.AssemblyQualifiedName |> Some) 
            this.Attributes |> List.tryFind(fun x -> x.AttributeName = attribName)
        
        member this.HasAttribute<'T when 'T:> Attribute>() = 
            this.TryGetAttribute<'T>() |> Option.isSome
                      
    /// <summary>
    /// Represents the intent to select one or more type descrptions
    /// </summary>
    type ClrTypeQuery =
        | FindTypeByName of name : ClrTypeName

    /// <summary>
    /// Represents the intent to select one or more properties from the identified types
    /// </summary>
    type ClrPropertyQuery = 
        | FindPropertyByName of name : ClrMemberName * typeQuery : ClrTypeQuery
        | FindPropertiesByType of typeQuery : ClrTypeQuery
         

    /// <summary>
    /// Represents the intent to fetch one ore more assembly descrptions
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
    let fromDescription description =
        match description with
        | MemberDescription(description=x) -> 
            match x with
                | PropertyDescription(_) ->
                    ClrElementKind.Property
                | FieldDescription(_) -> 
                    ClrElementKind.StorageField
                | MethodDescription(_) ->
                    ClrElementKind.Method
                | ConstructorDescription(_) ->
                    ClrElementKind.Constructor
                | EventDescription(_) ->
                    ClrElementKind.Event
        | TypeDescription(description=x) -> 
            ClrElementKind.Type
        | AssemblyDescription(_) ->
            ClrElementKind.Assembly
        | ParameterDescription(_) ->
            ClrElementKind.Parameter
        | UnionCaseDescription(_) ->
            ClrElementKind.UnionCase

    /// <summary>
    /// Determines whether the kind classifies a member
    /// </summary>
    /// <param name="kind">The kind</param>
    let isMemberKind kind =
        match kind with
        | ClrElementKind.Method | ClrElementKind.Property | ClrElementKind.StorageField | ClrElementKind.Event -> true
        | _ -> false

    /// <summary>
    /// Determines whether the kind classifies a data member
    /// </summary>
    /// <param name="kind">The kind</param>
    let isDataMemberKind kind =
        match kind with
        | ClrElementKind.Property | ClrElementKind.StorageField -> true
        | _ -> false

                

