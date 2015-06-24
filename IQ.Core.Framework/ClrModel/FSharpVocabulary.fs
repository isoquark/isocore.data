namespace IQ.Core.Framework

/// <summary>
/// Defines vocabulary for representing F# language artifacts
/// </summary>
module FSharpVocabulary =

    /// <summary>
    /// Represents an identifier
    /// </summary>
    type Identifier = Identifier of string

    /// <summary>
    /// Represents a code comment
    /// </summary>
    type Description = Description of string
    
    /// <summary>
    /// Represents a constant value of some (user-defined or system-defined) primitive
    /// </summary>
    type ConstantValue = | ConstantValue of rawValue : string * suffix : string

    /// <summary>
    /// Represents the value of an argument to a constructor
    /// </summary>
    type ArgumentValue = ArgumentValue of position : int * argName : Identifier * argValue : string
    
    /// <summary>
    /// Represents the application of an attribute to some attributable element
    /// </summary>
    type Attribution = | Attribution of attributeName : Identifier * arguments : ArgumentValue list

    /// <summary>
    /// Represents any element to which an attribution may be applied
    /// </summary>
    type AttributableElement = AttributableElement of name : Identifier * description : Description * attributions : Attribution list

    /// <summary>
    /// Represents a named let-bound constant value to which the LiteralAttribute is applied
    /// </summary>
    type LiteralValue = | LiteralValue  of element : AttributableElement * constant : ConstantValue

    /// <summary>
    /// Represents a function parameter
    /// </summary>
    type FunctionParameter = FunctionParameter of element : AttributableElement * argType : Identifier 

    /// <summary>
    /// Represents a function signature
    /// </summary>
    type FunctionSignature = FunctionSignature of element : AttributableElement * parameters : FunctionParameter list

    /// <summary>
    /// Represents an interface type
    /// </summary>
    type InterfaceType = InterfaceType of element : AttributableElement * functions : FunctionSignature list

    /// <summary>
    /// Represents a record field
    /// </summary>
    type RecordField = | RecordField of element : AttributableElement * position : int

    /// <summary>
    /// Represents a record type
    /// </summary>
    type RecordType = | RecordType of element : AttributableElement 
    
    /// <summary>
    /// Represents a field in an enumeration
    /// </summary>
    type EnumerationField = | EnumerationField of element : AttributableElement * constant : ConstantValue

    /// <summary>
    /// Represents an enumeration type
    /// </summary>
    type EnumerationType = | EnumerationType of element : AttributableElement * fields : EnumerationField

    /// <summary>
    /// Represents an element of the F# type system
    /// </summary>
    type TypeElement =
    | Record of element : RecordType * interfaces : InterfaceType list
    | Enumeration of element : EnumerationType * interfaces : InterfaceType list
    | Interface of element : InterfaceType * interfaces : InterfaceType list





