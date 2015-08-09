// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework.Contracts

open System

/// <summary>
/// Specifies operations for converting POCO values to/from alternate representations
/// </summary>
type IPocoConverter =
    /// <summary>
    /// Creates a <see cref="ValueIndex"/> from a record value
    /// </summary>
    /// <param name="record">The record whose values will be indexed</param>
    abstract ToValueIndex:record : obj->ValueIndex
    /// <summary>
    /// Creates an array of field values, in declaration order, for a specified record value
    /// </summary>
    /// <param name="record">The record from which a value array will be created</param>
    abstract ToValueArray:obj->obj[]
    /// <summary>
    /// Creates a record from an array of values that are specified in declaration order
    /// </summary>
    /// <param name="valueArray">An array of values in declaration order</param>
    /// <param name="t">The record type</param>
    abstract FromValueArray:valueArray : obj[] * t : Type->obj
    /// <summary>
    /// Creates a record from data supplied in a <see cref="ValueIndex"/>
    /// </summary>
    /// <param name="idx">The value index</param>
    /// <param name="t">The record type</param>
    abstract FromValueIndex: idx : ValueIndex * t : Type -> obj


/// <summary>
/// Defines the configuration contract for <see cref="IPocoConverter"/> realizations
/// </summary>
type PocoConverterConfig = PocoConverterConfig of clrMetadataProvider : IClrMetadataProvider
