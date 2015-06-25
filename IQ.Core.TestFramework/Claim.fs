namespace IQ.Core.TestFramework

open System

open NUnit.Framework

/// <summary>
/// Defines operations that assert the truth of various conditions
/// </summary>
module Claim =
    /// <summary>
    /// Asserts that the expected and actual values are identical
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="actual">The actual value</param>
    let equal (expected : 'T) (actual : 'T) =
        Assert.AreEqual(expected,actual)

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
    let assertFail() =
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
    /// Asserts that the supplied sequence has not items
    /// </summary>
    /// <param name="seq">The sequence to examine</param>
    let seqIsEmpty (seq : seq<_>) =
        seq |> Seq.isEmpty |> isTrue

    /// <summary>
    /// Asserts that an item is contained in a list
    /// </summary>
    /// <param name="list">The list to search</param>
    /// <param name="item">The item to search for</param>
    let inList list item =
       list |> List.exists(fun x -> x = item) |> isTrue