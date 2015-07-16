// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.TestFramework

open System


/// <summary>
/// Defines operations that assert the truth of various conditions
/// </summary>
module Claim =
    /// <summary>
    /// Asserts that a supplied value is true
    /// </summary>
    /// <param name="value">The value to examine</param>
    let isTrue (value : bool) = 
            value |> Assert.True

    /// <summary>
    /// Asserts that a supplied value is false
    /// </summary>
    /// <param name="value">The value to examine</param>
    let isFalse (value : bool) = 
            value |> Assert.False
    

    /// <summary>
    /// Asserts that the expected and actual values are identical
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="actual">The actual value</param>
    let equal (expected : 'T) (actual : 'T) =
        Assert.Equal<'T>(expected,actual)

    /// <summary>
    /// Asserts that a supplied optional value has a value
    /// </summary>
    /// <param name="value">The optional value to examine</param>
    let isSome (value : 'T option) =
        value |> Option.isSome |> isTrue

    /// <summary>
    /// Asserts that a supplied optional value does not have a value
    /// </summary>
    /// <param name="value">The optional value to examine</param>
    let isNone (value : 'T option) =
        value |> Option.isNone |> isTrue

    /// <summary>
    /// Asserts that a supplied value is null
    /// </summary>
    /// <param name="value">The value to examine</param>
    let isNull (value : obj) =
        value = null |> Assert.True

    /// <summary>
    /// Asserts that a supplied value is not null
    /// </summary>
    /// <param name="value">The value to examine</param>
    let isNotNull (value : obj) =
        value = null |> Assert.False

    /// <summary>
    /// Asserts unconditional failure
    /// </summary>
    let assertFalse() =
        Assert.True false
    
    /// <summary>
    /// Asserts that executing a supplied function will raise a specific exception
    /// </summary>
    /// <param name="f">The function to execute</param>
    let failWith<'T when 'T :> Exception>(f:unit->unit) =
        let result = ref (option<'T>.None)
        try
            f()
        with
            | e ->
                if e.GetType() = typeof<'T> then
                    result := Some(e :?> 'T)
        !result |> Option.isSome |> isTrue

    /// <summary>
    /// Asserts that the left value is greater than the right value
    /// </summary>
    /// <param name="l">The left value</param>
    /// <param name="r">The right value</param>
    let greater l r =
        (>) l r |> isTrue


    /// <summary>
    /// Assert that the left value is greater or equal than the right value
    /// </summary>
    /// <param name="l">The left value</param>
    /// <param name="r">The right value</param>
    let greaterOrEqual l r =
        (>=) l r |> isTrue

    /// <summary>
    /// Asserts that the left value is less than the right value
    /// </summary>
    /// <param name="l">The left value</param>
    /// <param name="r">The right value</param>
    let less l r =
        (<) l r |> isTrue


    /// <summary>
    /// Asserts that the left value is less or equal than the right value
    /// </summary>
    /// <param name="l">The left value</param>
    /// <param name="r">The right value</param>
    let lessOrEqual l r =
        (<=) l r |> isTrue

    /// <summary>
    /// Asserts that the supplied sequence has no items
    /// </summary>
    /// <param name="seq">The sequence to examine</param>
    let seqIsEmpty (s : seq<_>) =
        s |> Seq.isEmpty |> isTrue
    /// <summary>
    /// Asserts that the supplied sequence is not empty
    /// </summary>
    /// <param name="seq">The sequence to examine</param>
    let seqNotEmpty (s : seq<_>) =
        s |> Seq.isEmpty |> isFalse


    /// <summary>
    /// Asserts that an item is contained in a sequence
    /// </summary>
    /// <param name="list">The list to search</param>
    /// <param name="item">The item to search for</param>
    let seqIn s item =
        s |> Seq.exists(fun x -> x = item) |> isTrue

    /// <summary>
    /// Asserts that a sequence has a specified length
    /// </summary>
    /// <param name="list">The list to search</param>
    /// <param name="item">The item to search for</param>
    let seqCount count (s : seq<_>) =
        s |> Seq.length|> equal count


