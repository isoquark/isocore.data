// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection

module PocoConverter =
    
    module private DataRecord = 
        let private factories = 
            ConcurrentDictionary<Type, obj[]->obj>()
    
        let private getRecordFactory t = 
            //TODO: This is obviously NOT the right way to use a concurrent dictionary
            if factories.ContainsKey(t) |> not then
                factories.[t] <- FSharpValue.PreComputeRecordConstructor(t, true)
            factories.[t]
        
        /// <summary>
        /// Creates a record from an array of values that are specified in declaration order
        /// </summary>
        /// <param name="valueArray">An array of values in declaration order</param>
        /// <param name="tref">Reference to type</param>
        let private fromValueArray (config : PocoConverterConfig)  (valueArray : obj[]) (t : Type) =
            if t |> Type.isRecordType |> not then
                argerrord "o" t "Not a record type"

            let types = 
                t.TypeName   
                    |> ClrMetadata().FindType 
                    |> fun x -> x.Properties
                    |> List.map(fun p -> p.ReflectedElement.Value.PropertyType) |> Array.ofList
            
            valueArray |> config.Transformer.TransformArray types 
                       |> getRecordFactory(t)

    
        /// <summary>
        /// Retrieves record field values indexed by field name
        /// </summary>
        /// <param name="record">The record whose values will be retrieved</param>
        let private toValueIndex (o : obj) =
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
        let private toValueList (config : PocoConverterConfig) (o : obj) =
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
        let private toValueArray (config : PocoConverterConfig) (o : obj) =
                o |> toValueList config |> Array.ofList


        /// <summary>
        /// Instantiates a type using the data supplied in a value map
        /// </summary>
        /// <param name="valueMap">The value map</param>
        /// <param name="tref"></param>
        let private fromValueIndex (config : PocoConverterConfig) (valueMap : ValueIndex) (t : Type) =
            if t |> Type.isRecordType |> not then
                argerrord "o" t "Not a record type"

            let fields =  t.TypeName   |> ClrMetadata().FindType |> fun x -> x.Properties

            let types = 
                 fields |> List.map(fun p -> p.ReflectedElement.Value.PropertyType) 
                        |> Array.ofList

            fields |> List.map(fun field -> valueMap.[field.Name.Text]) 
                    |> Array.ofList 
                    |> config.Transformer.TransformArray types
                    |> getRecordFactory(t)
                       
        type private Realization(config : PocoConverterConfig) =
            interface IPocoConverter with
                member this.ToValueIndex record  = record |> toValueIndex 
                member this.ToValueArray record = record |> toValueArray config
                member this.FromValueArray (valueArray, t) = fromValueArray config valueArray t
                member this.FromValueIndex (idx, t) = fromValueIndex config idx t
    
        let getPocoConverter(config) =
            Realization(config) :> IPocoConverter    
    
    module private DataEntity =
        type private PocoConverter(config : PocoConverterConfig) =
            let transforer = config.Transformer
        
            let getTypeMetadata (t : Type) =
                t.TypeName |> ClrMetadata().FindType

            let getProperties(t : Type) =
                t |> getTypeMetadata |> fun t -> t.Properties            

            let getPropertyTypes (props : ClrProperty list) =
                props |> List.map(fun p -> p.ReflectedElement.Value.PropertyType) |> Array.ofList                    
        
            /// <summary>
            /// Creates a list of field values, in declaration order, for a specified record value
            /// </summary>
            /// <param name="o"></param>
            let toValueArray (o : obj) =
                o.GetType() |> getProperties
                            |> List.map(fun p -> p.ReflectedElement.Value.GetValue(o))
                            |> Array.ofList

            interface IPocoConverter with
                member this.FromValueArray(valueArray, t) =
                
                    let props = t |> getProperties                
                    let types =  props |> getPropertyTypes
                    let values = valueArray |> config.Transformer.TransformArray types 
                
                    let o = Activator.CreateInstance(t);
                    props |> List.iteri(fun i p ->
                       p.ReflectedElement.Value.SetValue(o, values.[i])
                    ) 

                    o

                member this.FromValueIndex(idx,t) = 
                    let props = t |> getProperties                
                    let types =  props |> getPropertyTypes
                    let values = props |> List.map(fun p -> idx.[p.Name.Text]) 
                                       |> Array.ofList 
                                       |> config.Transformer.TransformArray types

                    let o = Activator.CreateInstance(t);
                    props |> List.iteri(fun i p ->
                       p.ReflectedElement.Value.SetValue(o, values.[i])
                    ) 

                    o
                               
                
                member this.ToValueArray(entity) = 
                    entity |> toValueArray
            
                member this.ToValueIndex(entity) = 
                    entity.GetType() |> getProperties
                    |> List.mapi(fun i field ->                 
                        let p = field.ReflectedElement |> Option.get
                        p.Name, i, p.GetValue(entity))                 
                        |> ValueIndex.create
                
                
        let getPocoConverter(config) =
            PocoConverter(config) :> IPocoConverter



    type private Realization(config : PocoConverterConfig) =
        let recordConverter = config |> DataRecord.getPocoConverter
        let entityConverter = config |> DataEntity.getPocoConverter
        interface IPocoConverter with
            member this.FromValueArray(valueArray, t) = 
                    (if t |> Type.isRecordType then recordConverter.FromValueArray 
                                              else entityConverter.FromValueArray)(valueArray, t)
                                    
            member this.FromValueIndex(idx,t) = 
                    (if t |> Type.isRecordType then recordConverter.FromValueIndex
                                              else entityConverter.FromValueIndex)(idx, t)
                                                
            member this.ToValueArray(entity) = 
                    (if entity.GetType() |> Type.isRecordType then recordConverter.ToValueArray
                                              else entityConverter.ToValueArray)(entity)

            member this.ToValueIndex(entity) = 
                    (if entity.GetType() |> Type.isRecordType then recordConverter.ToValueIndex
                                              else entityConverter.ToValueIndex)(entity)
        

    let get(config : PocoConverterConfig) =
        Realization(config) :> IPocoConverter

    let getDefault() =
        PocoConverterConfig(Transformer.getDefault())  |> get