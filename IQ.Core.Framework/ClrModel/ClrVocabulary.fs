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
        | Public
        | Protected
        | Private
        | Internal
        | ProtectedInternal
                
    /// <summary>
    /// Describes an F#-specific field, either for a record or for a union case element, 
    /// that is  associated with a CLR property
    /// </summary>
    type PropertyFieldDescription = {
        /// The name of the field
        Name : string
        
        /// The CLR property that defines the field
        Property : PropertyInfo
        
        /// The CLR Type of the field
        FieldType : Type
        
        /// If the type is not optional, then the same as the field type. Otherwise, the type encapsulated
        /// by option
        ValueType : Type

        /// The position of the field relative to other fields in the declaring type
        Position : int
    }

    /// <summary>
    /// Describes an F#-specific record
    /// </summary>
    type RecordDescription = {
        /// The name of the record
        Name : string
                
        /// The CLR type of the record
        Type : Type
                
        /// The fields defined by the record
        Fields : PropertyFieldDescription list
    }

    /// <summary>
    /// Describes an F#-specific union case
    /// </summary>
    type UnionCaseDescription = {
        /// The name of the case
        Name : string

        /// The case being described
        Case : UnionCaseInfo
        
        /// The position of the case relative to other cases in the union
        Position : int

        /// The fields defined by the case
        Fields : PropertyFieldDescription list
    }
    
    /// <summary>
    /// Describes an F#-specific union
    /// </summary>
    type UnionDescription = {
        /// The name of the record
        Name : string
    
        /// The CLR type of the union
        Type : Type
        
        /// The cases defined by the union
        Cases : UnionCaseDescription list
    }

    /// <summary>
    /// Describes a CLR method parameter
    /// </summary>
    [<DebuggerDisplay("{Name, nq} : {ParameterType.Name, nq}")>]
    type MethodParameterDescription = {
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
    type MethodReturnDescription = {
        /// The return type of the method, if applicable
        ReturnType : Type option
        
        /// If the ReturnType is specified then either the ReturnType if ReturnType is 
        /// not an option type; otherwise, the encapsulated type
        ValueType : Type option

        /// The method 
        Method : MethodInfo
    }



    /// <summary>
    /// Describes a CLR method
    /// </summary>
    type MethodDescription = {
        /// The name of the method
        Name : string

        /// The CLR method being described
        Method : MethodInfo

        /// Description of the method return
        Return : MethodReturnDescription

        /// The parameters accepted by the method
        Parameters : MethodParameterDescription list
    }

    /// <summary>
    /// Describes a CLR property
    /// </summary>
    type PropertyDescription = {
        /// The name of the property
        Name : string

        /// The CLR property being described
        Property : PropertyInfo

        /// If the type is of option type (or actually optional) then the enclosed type; otherwise, same as PropertyType
        ValueType : Type

        /// The CLR type of the property
        PropertyType : Type
    }
 

    
    /// <summary>
    /// Describes a CLR interface member
    /// </summary>
    type InterfaceMemberDescription =
        | InterfaceMethod of MethodDescription
        | InterfaceProperty of PropertyDescription

    /// <summary>
    /// Describes a CLR interface
    /// </summary>
    type InterfaceDescription = {
        /// The name of the interface
        Name : string

        /// The CLR type of the interface
        Type : Type

        /// The members that belong to the interface
        Members : InterfaceMemberDescription list
    }

    /// <summary>
    /// Represents a CLR type
    /// </summary>
    type ClrType =
    | UnionType of UnionDescription
    | RecordType of RecordDescription
    | InterfaceType of InterfaceDescription


    /// <summary>
    /// Unifies the CLR element description hierarchy
    /// </summary>
    type ClrElement =
    | InterfaceElement of InterfaceDescription
    | PropertyElement of PropertyDescription
    | MethodElement of MethodDescription
    | MethodParameterElement of MethodParameterDescription
    | UnionElement of UnionDescription
    | UnionCaseElement of UnionCaseDescription
    | RecordElement of RecordDescription
    | PropertyFieldElement of PropertyFieldDescription



[<AutoOpen>]
module ClrVocabularyExtensions =
    /// <summary>
    /// Defines augmentations for the RecordDescription type
    /// </summary>
    type RecordDescription
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

