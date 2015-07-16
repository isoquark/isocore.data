// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework
open System
open System.Collections.Generic

[<AutoOpen>]
module DataStructureVocabulary =

        
    type MatrixContainer<'TKey,'TValue when 'TValue : equality and 'TKey : equality>(keyedValues : ('TKey*'TKey*'TValue) seq) =               
        let keyedValues = keyedValues |> Array.ofSeq
        
        //Get the unique keys (this is like _types[])
        let keys = seq{ for (k1,k2,_) in keyedValues do
                            yield k1
                            yield k2
                      } |> HashSet
                        |> Array.ofSeq
        let keyCount = keys.Length

        
        //Compute the hash codes for the keys (using an immutable Map instead of concurrent collection
        let keyHashes = keys |> Array.mapi(fun i key -> key.GetHashCode(), i) |> Map.ofArray
        
        let createValueMatrix() =
            let m = Array2D.zeroCreate<'TValue> keyCount keyCount
            for i in 0..keyCount-1 do
                let k1 = keys.[i]
                let k1Hash = k1.GetHashCode()
                for j in 0..keyCount-1 do
                    let k2 = keys.[j]
                    let k2Hash = k2.GetHashCode()
                    
                    let valueRow = keyHashes.[k1Hash]
                    let valueCol = keyHashes.[k2Hash]
                    NotImplementedException() |> raise
                    //TODO: Put correct value in cell
            m
        let valueMatrix = createValueMatrix()

        /// <summary>
        /// Looks up identified value
        /// </summary>
        /// <param name="k1">The first key</param>
        /// <param name="k2">The second key</param>
        member this.GetValue (k1 :'TKey, k2 : 'TKey) =
            let valueRow = keyHashes.[k1.GetHashCode()]
            let valueCol = keyHashes.[k2.GetHashCode()]
            valueMatrix.[valueRow,valueCol]

        
        /// <summary>
        /// Indexer for looking up identified value
        /// </summary>
        /// <param name="k1">The first key</param>
        /// <param name="k2">The second key</param>
        member this.Item(k1,k2) = this.GetValue(k1,k2)             