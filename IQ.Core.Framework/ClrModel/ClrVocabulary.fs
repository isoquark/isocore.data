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
    type Visibility =
        /// Indicates that the target is visible everywhere 
        | Public
        /// Indicates that the target is visible only to subclasses
        /// Not supported in F#
        | Protected
        /// Indicates that the target is not visible outside its defining scope
        | Private
        /// Indicates that the target is visible throughout the assembly in which it is defined
        | Internal
        /// Indicates that the target is visible to subclasses and the defining assembly
        /// Not supported in F#
        | ProtectedInternal
               
    /// <summary>
    /// The name of a type
    /// </summary>
    type ClrTypeName = 
        ///The local name of the type; does not include enclosing type names or namespace
        | SimpleTypeName of string
        ///The namespace and nested type-qualified name of the type
        | FullTypeName of string
        ///The assembly-qualified full type name
        | AssemblyTypeName of string

    /// <summary>
    /// References a property
    /// </summary>
    type PropertyReference = {
        /// The name of the property
        Name : string

        /// The CLR property being described
        Property : PropertyInfo

        /// The CLR type of the property
        PropertyType : Type

        /// If the type is of option type (or actually optional) then the enclosed type; otherwise, same as PropertyType
        ValueType : Type
    
        /// The position of the property relative to some declaration context
        Position : int

    }
        

    /// <summary>
    /// Describes a property
    /// </summary>
    type PropertyDescription = {
        /// The name of the property
        Name : string 

        /// The name of the CLR property type
        PropertyType : ClrTypeName       
    
        /// Specifies whether the property has a get accessor
        CanRead : bool

        /// Specifies whether the property has a set accessor
        CanWrite : bool
    }

   


    /// <summary>
    /// Describes an F#-specific record
    /// </summary>
    type RecordReference = {
        /// The name of the record
        Name : string
                
        /// The CLR type of the record
        Type : Type
                
        /// The fields defined by the record
        Fields : PropertyReference list
    }

    /// <summary>
    /// Describes an F#-specific union case
    /// </summary>
    type UnionCaseReference = {
        /// The name of the case
        Name : string

        /// The case being described
        Case : UnionCaseInfo
        
        /// The position of the case relative to other cases in the union
        Position : int

        /// The fields defined by the case
        Fields : PropertyReference list
    }
    
    /// <summary>
    /// Describes an F#-specific union
    /// </summary>
    type UnionReference = {
        /// The name of the record
        Name : string
    
        /// The CLR type of the union
        Type : Type
        
        /// The cases defined by the union
        Cases : UnionCaseReference list
    }

    /// <summary>
    /// Describes a CLR method parameter
    /// </summary>
    [<DebuggerDisplay("{Name, nq} : {ParameterType.Name, nq}")>]
    type MethodParameterReference = {
        /// The name of the parameter
        Name : string
                
        /// The CLR parameter being described
        Parameter : ParameterInfo
       
        /// The CLR type of the parameter
        ParameterType : Type
       
        /// If the type is of option type (or actually optional) then the enclosed type; otherwise, same as ParameterType
        ValueType : Type

        /// The ordinal position of the parameter
        Position : int

        /// Whether the parameter is required
        IsRequired : bool

        /// The method 
        Method : MethodInfo
    } 
    

    /// <summary>
    /// Describes a CLR method return
    /// </summary>
    type MethodReturnReference = {
        /// The return type of the method, if applicable
        ReturnType : Type option
        
        /// If the ReturnType is specified then either the ReturnType if ReturnType is 
        /// not an option type; otherwise, the encapsulated type
        ValueType : Type option

        /// The method with which the reference is associated
        Method : MethodInfo
    }

    /// <summary>
    /// References a method parameter or return
    /// </summary>
    type MethodInputOutputReference =
    | MethodInputReference of MethodParameterReference
    | MethodOutputReference of MethodReturnReference


    /// <summary>
    /// Represents a CLR method
    /// </summary>
    type MethodReference = {
        /// The name of the method
        Name : string

        /// The referenced method
        Method : MethodInfo

        /// Description of the method return
        Return : MethodReturnReference

        /// The parameters accepted by the method
        Parameters : MethodParameterReference list
    }

 

    
    /// <summary>
    /// Describes a CLR interface member
    /// </summary>
    type InterfaceMemberReference =
        | InterfaceMethodReference of MethodReference
        | InterfacePropertyReference of PropertyReference

    /// <summary>
    /// Describes a CLR interface
    /// </summary>
    type InterfaceReference = {
        /// The name of the interface
        Name : string

        /// The CLR type of the interface
        Type : Type

        /// The members that belong to the interface
        Members : InterfaceMemberReference list
    
        /// The interfaces from which the subject inherits
        Bases : InterfaceReference list
    }



    /// <summary>
    /// Represents a CLR type
    /// </summary>
    type ClrTypeReference =
    | UnionTypeReference of UnionReference
    | RecordTypeReference of RecordReference
    | InterfaceTypeReference of InterfaceReference




    /// <summary>
    /// Unifies the CLR element description hierarchy
    /// </summary>
    type ClrElementReference =
    | InterfaceElement of InterfaceReference
    | PropertyElement of PropertyReference
    | MethodElement of MethodReference
    | MethodParameterElement of MethodParameterReference
    | UnionElement of UnionReference
    | UnionCaseElement of UnionCaseReference
    | RecordElement of RecordReference



[<AutoOpen>]
module ClrVocabularyExtensions =
    /// <summary>
    /// Defines augmentations for the RecordDescription type
    /// </summary>
    type RecordReference
    with
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

