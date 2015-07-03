namespace IQ.Core.TestFramework

open System
open System.IO
open System.Diagnostics

open IQ.Core.Framework

open NUnit.Framework


[<AutoOpen>]
module NUnit =
    /// <summary>
    /// Identifies a test method
    /// </summary>
    type FactAttribute = NUnit.Framework.TestAttribute

    /// <summary>
    /// Identifies a test container
    /// </summary>
    type TestContainerAttribute = NUnit.Framework.TestFixtureAttribute


    /// <summary>
    /// Identifies a type that defines the per-assembly initialization
    /// </summary>
    /// <remarks>
    /// In NUnit, for some bizarre reason this initialization actually 
    /// executes once per namespace/per assembly
    /// </remarks>
    type TestAssemblyInitAttribute = NUnit.Framework.SetUpFixtureAttribute

    type TestInitAttribute = NUnit.Framework.SetUpAttribute

    type TestCleanupAttribute = NUnit.Framework.TearDownAttribute

    type ExpectedErrorAttribute = NUnit.Framework.ExpectedExceptionAttribute

    /// <summary>
    /// Defines base type for test assembly initializers
    /// </summary>
    [<AbstractClass>]
    type TestAssemblyInitializer() =
        [<TestInit>]
        abstract member Initialize:unit->unit

        [<TestCleanup>]
        abstract member Dispose:unit->unit
    
        default this.Dispose() = ()

        interface IDisposable with
            member this.Dispose() = this.Dispose()
        
    module TestContext =     
        [<Literal>]
        let private BaseDirectory = @"C:\Temp\IQ\Tests\"

        let inline getTempDir() =
            let dir = Path.Combine(BaseDirectory, thisAssembly().SimpleName)
            if dir |> Directory.Exists |> not then
                dir |> Directory.CreateDirectory |> ignore
            dir
    
    /// <summary>
    /// Defines trait attributes that enable test categorization
    /// </summary>
    [<AutoOpen>]
    module TraitsVocabulary = 

        /// <summary>
        /// Classifies a test method
        /// </summary>
        type TraitAttribute = NUnit.Framework.CategoryAttribute


        /// <summary>
        /// Identifies benchmark tests
        /// </summary>
        type BenchmarkTraitAttribute() =
            inherit TraitAttribute("Benchmark")

        /// <summary>
        /// Identifies tests that verify error condition detection/reporting
        /// </summary>
        type FailureVerificationAttribute() =
            inherit TraitAttribute("Failure Verification")


        /// <summary>
        /// Identifies integration tests that exercise a data store
        /// </summary>
        type DataStoreTraitAttribute() = 
            inherit TraitAttribute("Data Store")
    

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
