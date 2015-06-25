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
module ClrReferenceVocabulary =
                    
    type ClrSubject = ClrSubject of name : ClrElementName * position : int * element : ClrElement
    with
        member this.Name = match this with ClrSubject(name=x) -> x
        member this.Position = match this with ClrSubject(position=x) -> x
        member this.Element = match this with ClrSubject(element=x) -> x
    
    
    /// <summary>
    /// Describes a CLR method parameter
    /// </summary>
    type ClrMethodParameterReference = {
        /// The CLR element being referenced
        Subject : ClrSubject
       
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
    /// Represents a CLR method
    /// </summary>
    type ClrMethodReference = {
        /// The CLR element being referenced
        Subject : ClrSubject

        /// If the ReturnType is specified then either the ReturnType if ReturnType is 
        /// not an option type; otherwise, the encapsulated type
        ReturnType : Type option

        ReturnValueType : Type option

        /// The parameters accepted by the method
        Parameters : ClrMethodParameterReference list
    }
    
    /// <summary>
    /// References a property
    /// </summary>
    type ClrPropertyReference = {
        /// The CLR element being referenced
        Subject : ClrSubject

        /// The CLR type of the property
        PropertyType : Type

        /// If the type is of option type (or actually optional) then the enclosed type; otherwise, same as PropertyType
        ValueType : Type    
    }
        
    /// <summary>
    /// References a field
    /// </summary>
    type ClrFieldReference = {
        /// The field being referenced
        Subject : ClrSubject

        /// The CLR type of the field
        FieldType : Type

        /// If the type is of option type (or actually optional) then the enclosed type; otherwise, same as FieldType
        ValueType : Type        
    }

    /// <summary>
    /// References a data member (which by definition is a field or a property)
    /// </summary>
    type ClrDataMemberReference = 
    /// References a field member
    | FieldMemberReference of ClrFieldReference
    /// References a property member
    | PropertyMemberReference of ClrPropertyReference

    /// <summary>
    /// Represents a CLR member reference
    /// </summary>
    type ClrMemberReference =
    | MethodMemberReference of ClrMethodReference
    | DataMemberReference of ClrDataMemberReference

    /// <summary>
    /// Describes an F#-specific union case
    /// </summary>
    type ClrUnionCaseReference = {
        /// The CLR element being referenced        
        Subject : ClrSubject

        /// The fields defined by the case
        Fields : ClrPropertyReference list
    }
    


    /// <summary>
    /// Represents a reference to a CLR type
    /// </summary>
    type ClrTypeReference =
    | UnionTypeReference of subject : ClrSubject * cases : ClrUnionCaseReference list
    | RecordTypeReference of subject : ClrSubject * fields : ClrPropertyReference list
    | InterfaceTypeReference of subject : ClrSubject * members : ClrMemberReference list
    | ClassTypeReference of subject : ClrSubject * members : ClrMemberReference list
    | CollectionTypeReference of subject : ClrSubject * itemType : ClrTypeReference * collectionKind : ClrCollectionKind
    | StructTypeReference of subject : ClrSubject * members : ClrMemberReference list


    /// <summary>
    /// Unifies the CLR element reference taxonomy
    /// </summary>
    type ClrElementReference =
    | MethodParameterReference of ClrMethodParameterReference
    | UnionCaseReference of ClrUnionCaseReference
    | TypeReference of ClrTypeReference
    | MemberReference of ClrMemberReference

    
       
