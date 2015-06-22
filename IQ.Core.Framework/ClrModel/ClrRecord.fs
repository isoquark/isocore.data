namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection

/// <summary>
/// Defines operations for working with record values and metadata
/// </summary>
module ClrRecord =     
    let private recordfac = ConcurrentDictionary<Type, obj[]->obj>()

    let private createFactory (t : Type) =
        FSharpValue.PreComputeRecordConstructor(t, true)

    /// <summary>
    /// Creates a record reference
    /// </summary>
    /// <param name="t">The CLR type of the record</param>
    let private createReference (t : Type) =
        recordfac.[t] <- t |> createFactory
        {
            ClrRecordReference.Subject = {Subject = ClrSubjectReference(t.ElementName, 0, t)}
            Fields = FSharpType.GetRecordFields(t,true) 
               |> Array.mapi(fun i p -> 
                     {Subject = ClrSubjectReference(p.ElementName, i, p)
                      PropertyType = p.PropertyType 
                      ValueType =   p.ValueType
                      }) 
               |> List.ofArray
        }

    /// <summary>
    /// Determines whether a supplied type is a record type
    /// </summary>
    /// <param name="t">The candidate type</param>
    let isRecordType(t : Type) =
        FSharpType.IsRecord(t, true)

    /// <summary>
    /// Determines whether a supplied type is a record type
    /// </summary>
    let isRecord<'T>() =
        typeof<'T> |> isRecordType
                       
    /// <summary>
    /// Create a record reference
    /// </summary>
    /// <param name="t">The type</param>
    let reference(t : Type) =
        if t |> isRecordType |> not then
            ArgumentException(sprintf "The type %O is not a record type" t) |> raise
        
        createReference |> ClrTypeReferenceIndex.getOrAddRecord t

    /// <summary>
    /// Retrieves record field values indexed by field name
    /// </summary>
    /// <param name="record">The record whose values will be retrieved</param>
    /// <param name="info">Describes the record</param>
    let toValueMap (record : obj) =
        let description = record.GetType() |> reference
        description.Fields |> List.map(fun field -> field.Name.Text, field.Property.GetValue(record)) |> ValueIndex.fromNamedItems
    
    /// <summary>
    /// Creates a record from a value map
    /// </summary>
    /// <param name="valueMap">The value map</param>
    /// <param name="info"></param>
    let fromValueMap (valueMap : ValueIndex) (info : ClrRecordReference) =
        info.Fields |> List.map(fun field -> valueMap.[field.Name.Text]) |> Array.ofList |> recordfac.[info.Subject.Type]
    
    /// <summary>
    /// Creates an array of field values, in declaration order, for a specified record value
    /// </summary>
    /// <param name="record"></param>
    let toValueArray (record : obj) =
        let description = record.GetType() |> reference
        [|for i in 0..description.Fields.Length - 1 ->
            record |> description.Fields.[i].Property.GetValue
        |]        
                
    /// <summary>
    /// Creates a record from an array of values that are specified in declaration order
    /// </summary>
    /// <param name="valueArray">An array of values in declaration order</param>
    /// <param name="description">The record description</param>
    let fromValueArray (valueArray : obj[]) (description : ClrRecordReference) =
        valueArray |> recordfac.[description.Subject.Type]    

/// <summary>
/// Defines record-related augmentations
/// </summary>
[<AutoOpen>]
module ClrRecordExtensions =    
    /// <summary>
    /// Describes the record identified by a supplied type parameter
    /// </summary>
    let recordref<'T> =
        typeof<'T> |> ClrRecord.reference

    /// <summary>
    /// Defines augmentations for the RecordDescription type
    /// </summary>
    type ClrRecordReference
    with
        /// <summary>
        /// Finds a field in the record by name
        /// </summary>
        /// <param name="name">The name of the field</param>
        member this.FindField(name) = this.Fields |> List.find(fun field -> field.Name = name)
        
        /// <summary>
        /// Indexer that fields a field in the record by name
        /// </summary>
        /// <param name="name">The name of the field</param>
        member this.Item(name) = name |> this.FindField

        /// <summary>
        /// Indexer that fields a field in the record by position
        /// </summary>
        /// <param name="position">The ordinal position of the field</param>
        member this.Item(position) = this.Fields.[position]
        

