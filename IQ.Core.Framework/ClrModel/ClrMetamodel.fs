namespace IQ.Core.Framework

open System
open System.Reflection

/// <summary>
/// Defines the CLR metamodel vocabulary
/// </summary>
[<AutoOpen>]
module ClrMetamodelVocabulary =
    
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
    /// Encapsulates information about a record field
    /// </summary>
    type RecordFieldDescription = {
        /// The name of the field
        Name : string
        
        /// The CLR property that defines the field
        Property : PropertyInfo
        
        /// The CLR Type of the field
        FieldType : Type
        
        /// The position of the field relative to other fields in the record
        Position : int
    }

    /// <summary>
    /// Encapsulates information about a record
    /// </summary>
    type RecordDescription = {
        /// The name of the record
        Name : string
        
        /// The CLR type of the record
        Type : Type
        
        /// The namespace in which the record is defined
        Namespace : string 
        
        /// The fields defined by the record
        Fields : RecordFieldDescription list

    }


