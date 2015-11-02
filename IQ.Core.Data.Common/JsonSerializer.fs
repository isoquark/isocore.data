// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Collections
open System.Reflection
open System.Collections.Generic
open System.Data
open System.Runtime.CompilerServices

open IQ.Core.Data
open IQ.Core.Contracts

open Newtonsoft.Json
open Newtonsoft.Json.Converters
open Newtonsoft.Json.Linq


module JsonSerializer =
    let private converters = [|StringEnumConverter() :> JsonConverter|]
    
    type private Realization() =
        interface ISerializer<string> with
            member this.Emit(entity) = JsonConvert.SerializeObject(entity, converters)
            member this.Hydrate(data) = JsonConvert.DeserializeObject(data, converters)

    let get() =
        Realization() :> ISerializer<string>

