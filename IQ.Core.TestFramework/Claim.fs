namespace IQ.Core.TestFramework

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
