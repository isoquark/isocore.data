// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

/// <summary>
/// Responsible for identifying a value in a ValueMap
/// </summary>
type ValueIndexKey = ValueIndexKey of name : string  * position : int
with
    member this.Name = match this with |ValueIndexKey(name=x) -> x
    member this.Position = match this with |ValueIndexKey(position=x) -> x

/// <summary>
/// Represents a collection of name-indexed or position-indexed values
/// </summary>
type ValueIndex = ValueIndex of (ValueIndexKey*obj) list
