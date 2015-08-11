// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework


module PocoConverter =
    

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
        PocoConverterConfig(ClrMetadataProvider.getDefault(), Transformer.getDefault())  |> get