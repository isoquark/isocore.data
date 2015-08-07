// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection


module DataEntity =
    type private PocoConverter(config : PocoConverterConfig) =
        let mdp = match config with PocoConverterConfig(x) -> x
        
        interface IPocoConverter with
            member this.FromValueArray(valueArray, t) = nosupport()
            member this.FromValueIndex(idx,t) = nosupport()
            member this.ToValueArray(entity) = nosupport()
            member this.ToValueIndex(entity) = nosupport()

