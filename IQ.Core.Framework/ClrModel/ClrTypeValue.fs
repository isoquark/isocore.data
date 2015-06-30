namespace IQ.Core.Framework
open System
open System.Reflection


/// <summary>
/// Defines operations for creating type values
/// </summary>
//module ClrTypeValue =
    /// <summary>
    /// Retrieves record field values indexed by field name
    /// </summary>
    /// <param name="record">The record whose values will be retrieved</param>
//    let toValueIndex (record : obj) =
//        match record.GetType() |> ClrTypeReference.reference with
//        | RecordTypeReference(subject, fields) ->
//            fields |> List.map(fun field ->                 
//                let value = field.Referent |> ClrElement.asDataMember |> ClrDataMemberElement.getValue record
//                field.ReferentName.Text, field.ReferentPosition, value)                 
//                |> ValueIndex.create
//        | _ -> nosupport()
    
    
    /// <summary>
    /// Creates an array of field values, in declaration order, for a specified record value
    /// </summary>
    /// <param name="record"></param>
//    let toValueArray (record : obj) =
//        match record.GetType() |> ClrTypeReference.reference with
//        | RecordTypeReference(subject, fields) ->
//            [|for i in 0..fields.Length - 1 ->
//                fields.[i].Referent |> ClrElement.asDataMember |> ClrDataMemberElement.getValue record 
//            |]        
//        | _ -> nosupport()

    /// <summary>
    /// Creates a record from an array of values that are specified in declaration order
    /// </summary>
    /// <param name="valueArray">An array of values in declaration order</param>
    /// <param name="tref">Reference to type</param>
//    let fromValueArray (valueArray : obj[]) (tref : ClrTypeReference) =
//        match tref with
//        | RecordTypeReference(subject, fields) ->
//            let types = fields |> List.map(fun field -> field.PropertyType) |> Array.ofList
//            valueArray |> Transformer.convertArray types 
//                       |> ClrTypeReference.getRecordFactory(tref)
//        | _ -> nosupport()


                
    /// <summary>
    /// Instantiates a type using the data supplied in a value map
    /// </summary>
    /// <param name="valueMap">The value map</param>
    /// <param name="tref"></param>
//    let fromValueIndex (valueMap : ValueIndex) (tref : ClrTypeReference) =
//        match tref with
//        | RecordTypeReference(subject, fields) ->
//            let types = fields |> List.map(fun field -> field.PropertyType) |> Array.ofList
//            fields |> List.map(fun field -> valueMap.[field.ReferentName.Text]) 
//                   |> Array.ofList 
//                   |> Transformer.convertArray types
//                   |> ClrTypeReference.getRecordFactory(tref)
//        | _ -> nosupport()



