namespace IQ.Core.Framework

open System
open System.Reflection
open System.Data
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection


open IQ.Core.Framework

[<AutoOpen>]
module ClrMetadataVocabulary =
    
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

/// <summary>
/// Defines operations for working with CLR types
/// </summary>
module ClrType =
    /// <summary>
    /// Determines whether a supplied type is optional
    /// </summary>
    /// <param name="t">The type to test</param>
    let isOptionType (t : Type) =
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>

/// <summary>
/// Defines operations for working with record instances and metadata
/// </summary>
module ClrRecord =     
    let private recordidx = ConcurrentDictionary<Type, RecordDescription>()    
    let private recordfac = ConcurrentDictionary<Type, obj[]->obj>()

    let private createFactory (t : Type) =
        FSharpValue.PreComputeRecordConstructor(t, true)

    /// <summary>
    /// Creates a record description
    /// </summary>
    /// <param name="t">The CLR type of the record</param>
    let private createDescription (t : Type) =
        recordfac.[t] <- t |> createFactory
        {Name = t.Name
         Type = t
         Namespace = t.Namespace
         Fields = FSharpType.GetRecordFields(t,true) 
               |> Array.mapi(fun i p -> 
                    {Name = p.Name 
                     Property = p 
                     FieldType = p.PropertyType 
                     Position = i

                     }) 
               |> List.ofArray
        }
                       
    /// <summary>
    /// Gets the record information for the supplied type which, presumably, is a record
    /// </summary>
    /// <param name="t">The type</param>
    let describe(t : Type) =
        recordidx.GetOrAdd(t, fun t -> t |> createDescription) 

    /// <summary>
    /// Retrieves record field values indexed by field name
    /// </summary>
    /// <param name="record">The record whose values will be retrieved</param>
    /// <param name="info">Describes the record</param>
    let toValueMap (record : obj) =
        let description = record.GetType() |> describe
        description.Fields |> List.map(fun f -> f.Name, f.Property.GetValue(record)) |> Map.ofList
    
    /// <summary>
    /// Creates a record from a value map
    /// </summary>
    /// <param name="valueMap">The value map</param>
    /// <param name="info"></param>
    let fromValueMap (valueMap : ValueMap) (info : RecordDescription) =
        info.Fields |> List.map(fun field -> valueMap.[field.Name]) |> Array.ofList |> recordfac.[info.Type]
    
    /// <summary>
    /// Creates an array of field values, in declaration order, for a specified record value
    /// </summary>
    /// <param name="record"></param>
    let toValueArray (record : obj) =
        let description = record.GetType() |> describe
        [|for i in 0..description.Fields.Length - 1 ->
            record |> description.Fields.[i].Property.GetValue
        |]        
                
    /// <summary>
    /// Creates a record from an array of values that are specified in declaration order
    /// </summary>
    /// <param name="valueArray">An array of values in declaration order</param>
    /// <param name="description">The record description</param>
    let fromValueArray (valueArray : obj[]) (description : RecordDescription) =
        valueArray |> recordfac.[description.Type]    


        
                  
[<AutoOpen>]
module ClrMetadataOperators =
    let recinfo<'T> =
        typeof<'T> |> ClrRecord.describe
