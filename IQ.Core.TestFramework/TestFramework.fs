namespace IQ.Core.TestFramework


open NUnit.Framework


[<AutoOpen>]
module TestFramework =
    type TestAttribute = NUnit.Framework.TestAttribute
    type TestFixtureAttribute = NUnit.Framework.TestFixtureAttribute    


module Claim =
    /// <summary>
    /// Asserts the claim that the expected and actual values are identical
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="actual">The actual value</param>
    let equal (expected : 'T) (actual : 'T) =
        Assert.AreEqual(expected,actual)

    /// <summary>
    /// Asserts the claim that a supplied value is true
    /// </summary>
    /// <param name="value">The value to examine</param>
    let isTrue (value : bool) = 
         value |> Assert.True

    /// <summary>
    /// Asserts the claim that a supplied value is false
    /// </summary>
    /// <param name="value">The value to examine</param>
    let isFalse (value : bool) = 
         value |> Assert.False
        