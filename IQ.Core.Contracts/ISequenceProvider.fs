// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Synthetics

open System
open System.Collections.Generic

type ISequenceProvider<'T when 'T : comparison> =
    abstract NextValue:unit->'T
    abstract NextRange:int->'T IEnumerable
    
type ISequenceProvider =
    abstract NextValue:unit->'T when 'T : comparison
    abstract NextRange:int->'T IEnumerable when 'T : comparison




