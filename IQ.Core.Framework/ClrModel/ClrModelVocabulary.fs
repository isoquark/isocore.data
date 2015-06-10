namespace IQ.Core.Framework

open System
open System.Reflection

[<AutoOpen>]
module ClrMetamodelVocabulary =
    
    /// Specifies the visibility of a CLR element
    type Visibility =
        | Public
        | Protected
        | Private
        | Internal
        | ProtectedInternal
                
    /// Encapsulates information about a record field
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

    /// Encapsulates information about a record
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


