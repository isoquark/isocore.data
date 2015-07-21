// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection


type RecordValueConverterConfig = RecordValueConverterConfig of clrMetadataProvider : IClrMetadataProvider

module RecordValueConverter = 
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
    /// Creates a list of field values, in declaration order, for a specified record value
    /// </summary>
    /// <param name="o"></param>
    let toValueList (o : obj) =
        let t = o.GetType()
        if t |> Type.isRecordType |> not then
            argerrord "o" o "Not a record value"
        t.TypeName   
            |> ClrMetadata().FindType
            |> fun x -> x.Properties
            |> List.map(fun p -> p.ReflectedElement.Value.GetValue(o))
    
    /// <summary>
    /// Creates an array of field values, in declaration order, for a specified record value
    /// </summary>
    /// <param name="o"></param>
    let toValueArray (o : obj) =
            o |> toValueList |> Array.ofList

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
                       
    type private Realization(config : RecordValueConverterConfig) =
        interface IRecordValueConverter with
            member this.ToValueIndex record  = record |> toValueIndex 
            member this.ToValueArray record = record |> toValueArray
            member this.FromValueArray (valueArray, t) = fromValueArray valueArray t
            member this.FromValueIndex (idx, t) = fromValueIndex idx t
    
    let get(config) =
        Realization(config) :> IRecordValueConverter