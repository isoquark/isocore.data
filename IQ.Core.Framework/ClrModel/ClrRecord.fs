namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection

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
        {
            Name = t.Name
            Type = t
            Namespace = t.Namespace
            Fields = FSharpType.GetRecordFields(t,true) 
               |> Array.mapi(fun i p -> 
                     {Name = p.Name 
                      Property = p 
                      FieldType = p.PropertyType 
                      DataType =   if p.PropertyType |> ClrOption.isOptionType then p.PropertyType |> ClrOption.getValueType else p.PropertyType
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
    /// Defines augmentations for the RecordDescription type
    /// </summary>
    type RecordDescription
    with
        /// <summary>
        /// Finds a field in the record by name
        /// </summary>
        /// <param name="name">The name of the field</param>
        member this.FindField(name) = this.Fields |> List.find(fun field -> field.Name = name)

