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
        
        /// If the type is not optional, then the same as the field type. Otherwise, the type encapsulated
        /// by option
        DataType : Type

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


[<AutoOpen>]
module ClrMetamodelExtensions =
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
