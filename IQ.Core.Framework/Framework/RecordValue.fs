﻿namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection

module RecordValue = 
    let private factories = 
        ConcurrentDictionary<Type, obj[]->obj>()
    let private getRecordFactory t = 
        //TODO: This is obviously NOT the right way to use a concurrent dictionary
        if factories.ContainsKey(t) |> not then
            factories.[t] <- FSharpValue.PreComputeRecordConstructor(t, true)
        factories.[t]
        
    
    /// <summary>
    /// Retrieves record field values indexed by field name
    /// </summary>
    /// <param name="record">The record whose values will be retrieved</param>
    let toValueIndex (o : obj) =
        let t = o.GetType()
        if t |> Type.isRecordType |> not then
            argerrord "o" o "Not a record value"
        t.TypeName   
            |> ClrMetadata().FindType
            |> fun x -> x.Properties
            |> List.map(fun field ->                 
                let p = field.ReflectedElement |> Option.get
                field.Name.Text, field.Position, p.GetValue(o))                 
                |> ValueIndex.create

    /// <summary>
    /// Creates an array of field values, in declaration order, for a specified record value
    /// </summary>
    /// <param name="record"></param>
    let toValueArray (o : obj) =
        let t = o.GetType()
        if t |> Type.isRecordType |> not then
            argerrord "o" o "Not a record value"
        t.TypeName   
            |> ClrMetadata().FindType
            |> fun x -> x.Properties
            |> List.map(fun p -> p.ReflectedElement.Value.GetValue(o))
            |> Array.ofList

    /// <summary>
    /// Creates a record from an array of values that are specified in declaration order
    /// </summary>
    /// <param name="valueArray">An array of values in declaration order</param>
    /// <param name="tref">Reference to type</param>
    let fromValueArray (valueArray : obj[]) (t : Type) =
        if t |> Type.isRecordType |> not then
            argerrord "o" t "Not a record type"

        let types = 
            t.TypeName   
                |> ClrMetadata().FindType 
                |> fun x -> x.Properties
                |> List.map(fun p -> p.ReflectedElement.Value.PropertyType) |> Array.ofList
            
        valueArray |> Transformer.convertArray types 
                   |> getRecordFactory(t)

    /// <summary>
    /// Instantiates a type using the data supplied in a value map
    /// </summary>
    /// <param name="valueMap">The value map</param>
    /// <param name="tref"></param>
    let fromValueIndex (valueMap : ValueIndex) (t : Type) =
        if t |> Type.isRecordType |> not then
            argerrord "o" t "Not a record type"

        let fields =  t.TypeName   |> ClrMetadata().FindType |> fun x -> x.Properties

        let types = 
             fields |> List.map(fun p -> p.ReflectedElement.Value.PropertyType) 
                    |> Array.ofList

        fields |> List.map(fun field -> valueMap.[field.Name.Text]) 
                |> Array.ofList 
                |> Transformer.convertArray types
                |> getRecordFactory(t)
                       
