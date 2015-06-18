namespace IQ.Core.TestFramework

open System
open System.IO
open System.Diagnostics

open IQ.Core.Framework

open NUnit.Framework

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
