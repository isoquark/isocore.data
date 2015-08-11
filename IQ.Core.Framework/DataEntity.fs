// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection


module internal DataEntity =
    type private PocoConverter(config : PocoConverterConfig) =
        let transforer = config.Transformer
        let metadataProvider = config.ClrMetadataProvider
        
        let getTypeMetadata (t : Type) =
            t.TypeName |> metadataProvider.FindType

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
