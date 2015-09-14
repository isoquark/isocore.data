// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Synthetics.Contracts

open System
open System.Collections.Generic

type ISequenceProvider<'T when 'T : comparison> =
    abstract NextValue:unit->'T
    abstract NextRange:int->'T IEnumerable
    
type ISequenceProvider =
    abstract NextValue:unit->'T when 'T : comparison
    abstract NextRange:int->'T IEnumerable when 'T : comparison

/// <summary>
/// Defines contract for weakly-typed value generation
/// </summary>
type IValueGenerator =
    abstract member NextValue:unit -> obj
    abstract member NextValues :count : int-> obj seq
        
/// <summary>
/// Defines contract for strongly-typed value generation
/// </summary>
type IValueGenerator<'T> =
    inherit IValueGenerator
    abstract member NextValue :unit->'T
    abstract member NextValues :count : int-> 'T seq

/// <summary>
/// Defines contract value generator factory
/// </summary>
type IValueGeneratorProvider =
    abstract member GetGenerator: [<ParamArray>]parms : obj[] -> IValueGenerator<'T>



