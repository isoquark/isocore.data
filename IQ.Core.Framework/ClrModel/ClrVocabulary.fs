namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent
open System.Diagnostics


open Microsoft.FSharp.Reflection

/// <summary>
/// Defines the CLR metamodel vocabulary
/// </summary>
[<AutoOpen>]
module ClrVocabulary =
    
    /// <summary>
    /// Specifies the visibility of a CLR element
    /// </summary>
    type ClrAccess =
        /// Indicates that the target is visible everywhere 
        | PublicAccess
        /// Indicates that the target is visible only to subclasses
        /// Not supported in F#
        | ProtectedAcces
        /// Indicates that the target is not visible outside its defining scope
        | PrivateAccess
        /// Indicates that the target is visible throughout the assembly in which it is defined
        | InternalAccess
        /// Indicates that the target is visible to subclasses and the defining assembly
        /// Not supported in F#
        | ProtectedInternalAccess
               
    /// <summary>
    /// Represents a type name
    /// </summary>
    type ClrTypeName = 
        ///The local name of the type; does not include enclosing type names or namespace
        | SimpleTypeName of string
        ///The namespace and nested type-qualified name of the type
        | FullTypeName of string
        ///The assembly-qualified full type name
        | AssemblyTypeName of string

    /// <summary>
    /// Represents an assembly name
    /// </summary>
    type ClrAssemblyName =
        | SimpleAssemblyName of string
        | FullAssemblyName of string

    /// <summary>
    /// Represents the name of a CLR element
    /// </summary>
    type ClrElementName =
        | AssemblyElementName of ClrAssemblyName
        | TypeElementName of ClrTypeName
        | BasicElementName of string

    type ClrSubjectReference<'T> = ClrSubjectReference of name : ClrElementName * position : int * element : 'T
    with
        member this.Name = match this with ClrSubjectReference(name=x) -> x
        member this.Position = match this with ClrSubjectReference(position=x) -> x
        member this.Element = match this with ClrSubjectReference(element=x) -> x

    type ClrSubjectDescription = ClrSubjectDescription of name : ClrElementName * position : int
    with
        member this.Name = match this with ClrSubjectDescription(name=x) -> x
        member this.Position = match this with ClrSubjectDescription(position=x) -> x

    /// <summary>
    /// Describes a CLR method parameter
    /// </summary>
    type ClrMethodParameterReference = {
        /// The CLR element being referenced
        Subject : ClrSubjectReference<ParameterInfo>
       
        /// The CLR type of the parameter
        ParameterType : Type
       
        /// If the type is of option type (or actually optional) then the enclosed type; otherwise, same as ParameterType
        ValueType : Type

        /// Whether the parameter is required
        IsRequired : bool

        /// The method 
        Method : MethodInfo
    } 
    

    /// <summary>
    /// Describes a CLR method return
    /// </summary>
    type ClrMethodReturnReference = {
        /// The return type of the method, if applicable
        ReturnType : Type option
        
        /// If the ReturnType is specified then either the ReturnType if ReturnType is 
        /// not an option type; otherwise, the encapsulated type
        ValueType : Type option

        /// The method with which the reference is associated
        Method : MethodInfo
    }



    /// <summary>
    /// Represents a CLR method
    /// </summary>
    type ClrMethodReference = {
        /// The CLR element being referenced
        Subject : ClrSubjectReference<MethodInfo>

        /// Description of the method return
        Return : ClrMethodReturnReference

        /// The parameters accepted by the method
        Parameters : ClrMethodParameterReference list
    }
    
    /// <summary>
    /// References a property
    /// </summary>
    type ClrPropertyReference = {
        /// The CLR element being referenced
        Subject : ClrSubjectReference<PropertyInfo>

        /// The CLR type of the property
        PropertyType : Type

        /// If the type is of option type (or actually optional) then the enclosed type; otherwise, same as PropertyType
        ValueType : Type    
    }
        

    /// <summary>
    /// Describes a property
    /// </summary>
    type ClrPropertyDescription = {
        /// The name of the property
        Name : ClrElementName 

        /// The position of the property relative to its declaration context
        Position : int

        /// The name of the type that declares the property
        DeclaringType : ClrTypeName       
    
        /// The type of the property value
        ValueType : ClrTypeName

        /// Specifies whether the property is of option<> type
        IsOptional : bool

        /// Specifies whether the property has a get accessor
        CanRead : bool

        /// Specifies the access of the get accessor if applicable
        ReadAccess : ClrAccess option

        /// Specifies whether the property has a set accessor
        CanWrite : bool

        /// Specifies the access of the set accessor if applicable
        WriteAccess : ClrAccess option
    }

    /// <summary>
    /// Represents a described or referenced CLR property
    /// </summary>
    type ClrProperty =
    | PropertyDescription of ClrPropertyDescription
    | PropertyReference of ClrPropertyReference


    /// <summary>
    /// Describes an F#-specific record
    /// </summary>
    type ClrRecordReference = {
        /// The CLR element being referenced
        Subject : ClrSubjectReference<Type>
                
        /// The fields defined by the record
        Fields : ClrPropertyReference list
    }

    /// <summary>
    /// Describes an F#-specific union case
    /// </summary>
    type ClrUnionCaseReference = {
        /// The CLR element being referenced        
        Subject : ClrSubjectReference<UnionCaseInfo>

        /// The fields defined by the case
        Fields : ClrPropertyReference list
    }
    
    /// <summary>
    /// Describes an F#-specific union
    /// </summary>
    type ClrUnionReference = {
        /// The CLR element being referenced        
        Subject : ClrSubjectReference<Type>        


        /// The cases defined by the union
        Cases : ClrUnionCaseReference list
    }

    
    /// <summary>
    /// Describes a CLR interface member
    /// </summary>
    type ClrInterfaceMemberReference =
        | InterfaceMethodReference of ClrMethodReference
        | InterfacePropertyReference of ClrPropertyReference

    /// <summary>
    /// Describes a CLR interface
    /// </summary>
    type ClrInterfaceReference = {
        /// The CLR element being referenced        
        Subject : ClrSubjectReference<Type>        

        /// The members that belong to the interface
        Members : ClrInterfaceMemberReference list
    
        /// The interfaces from which the subject inherits
        Bases : ClrInterfaceReference list
    }

    /// <summary>
    /// Unifies the CLR type reference taxonomy
    /// </summary>
    type ClrTypeReference =
    | UnionTypeReference of ClrUnionReference
    | RecordTypeReference of ClrRecordReference
    | InterfaceTypeReference of ClrInterfaceReference


    /// <summary>
    /// Unifies the CLR element reference taxonomy
    /// </summary>
    type ClrElementReference =
    | InterfaceElement of ClrInterfaceReference
    | PropertyElement of ClrPropertyReference
    | MethodElement of ClrMethodReference
    | MethodParameterElement of ClrMethodParameterReference
    | UnionElement of ClrUnionReference
    | UnionCaseElement of ClrUnionCaseReference
    | RecordElement of ClrRecordReference



module ClrAccess =
    /// <summary>
    /// Gets the method's access level
    /// </summary>
    /// <param name="m">The method</param>
    let getMethodAccess(m : MethodInfo) =
        if m.IsPublic then
            PublicAccess
        else if m.IsPrivate then
            PrivateAccess 
        else if m.IsAssembly then
            InternalAccess
        else if m.IsFamilyOrAssembly then
            ProtectedInternalAccess
        else
            NotSupportedException("Cannot deduce the access level of the method") |> raise

[<AutoOpen>]
module ClrVocabularyExtensions =
    /// <summary>
    /// Defines augmentations for the <see cref="ClrMethodReference"/> type
    /// </summary>
    type ClrMethodReference 
    with
        /// <summary>
        /// The name of the method
        /// </summary>
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position
        member this.Method = this.Subject.Element

    /// <summary>
    /// Defines augmentations for the <see cref="ClrPropertyReference"/> type
    /// </summary>
    type ClrPropertyReference    
    with
        /// <summary>
        /// The name of the property
        /// </summary>
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position
        member this.Property = this.Subject.Element

    /// <summary>
    /// Defines augmentations for the <see cref="ClrMethodParameterReference"/> type
    /// </summary>
    type ClrMethodParameterReference
    with
        /// <summary>
        /// The name of the parameter
        /// </summary>
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position
        member this.Parameter = this.Subject.Element

    /// <summary>
    /// Defines augmentations for the <see cref="ClrUnionCaseReference"/> type
    /// </summary>
    type ClrUnionCaseReference
    with
        /// <summary>
        /// The name of the union case
        /// </summary>
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position
        member this.Case = this.Subject.Element

    /// <summary>
    /// Defines augmentations for the <see cref="ClrRecordReference"/> type
    /// </summary>
    type ClrRecordReference
    with
        /// <summary>
        /// The name of the record
        /// </summary>
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position
        member this.Type = this.Subject.Element
        /// <summary>
        /// Retrieves a field identified by its name
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        member this.Item(fieldName) = 
            this.Fields |> List.find(fun field -> field.Name = fieldName)

        /// <summary>
        /// Retrieves a field identified by its position
        /// </summary>
        /// <param name="position">The position of the field</param>
        member this.Item(position) = 
            //Granted, this could have been done by simply indexing into the
            //list as it should be ordered correctly
            this.Fields |> List.find(fun field -> field.Position = position)

    /// <summary>
    /// Defines augmentations for the <see cref="ClrRecordReference"/> type
    /// </summary>
    type ClrUnionReference
    with
        /// <summary>
        /// The name of the record
        /// </summary>
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position
        member this.Type = this.Subject.Element

    /// <summary>
    /// Defines augmentations for the <see cref="ClrRecordReference"/> type
    /// </summary>
    type ClrInterfaceReference
    with
        /// <summary>
        /// The name of the record
        /// </summary>
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position
        member this.Type = this.Subject.Element
