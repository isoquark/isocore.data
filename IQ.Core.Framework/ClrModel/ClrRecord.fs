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
    /// Creates a record description
    /// </summary>
    /// <param name="t">The CLR type of the record</param>
    let private createDescription (t : Type) =
        recordfac.[t] <- t |> createFactory
        {
            Name = t.Name
            Type = t
            Fields = FSharpType.GetRecordFields(t,true) 
               |> Array.mapi(fun i p -> 
                     {Name = p.Name 
                      Property = p 
                      FieldType = p.PropertyType 
                      ValueType =   p.ValueType
                      Position = i
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
    /// Gets the record information for the supplied type which, presumably, is a record
    /// </summary>
    /// <param name="t">The type</param>
    let describe(t : Type) =
        createDescription |> ClrTypeIndex.getOrAddRecord t

    /// <summary>
    /// Retrieves record field values indexed by field name
    /// </summary>
    /// <param name="record">The record whose values will be retrieved</param>
    /// <param name="info">Describes the record</param>
    let toValueMap (record : obj) =
        let description = record.GetType() |> describe
        description.Fields |> List.map(fun field -> field.Name, field.Property.GetValue(record)) |> ValueMap.fromNamedItems
    
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

/// <summary>
/// Defines record-related augmentations
/// </summary>
[<AutoOpen>]
module ClrRecordExtensions =    
    /// <summary>
    /// Describes the record identified by a supplied type parameter
    /// </summary>
    let recordinfo<'T> =
        typeof<'T> |> ClrRecord.describe

    /// <summary>
    /// Defines augmentations for the RecordDescription type
    /// </summary>
    type RecordDescription
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
        

