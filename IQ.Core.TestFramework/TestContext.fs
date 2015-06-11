namespace IQ.Core.TestFramework

open System
open System.IO
open System.Diagnostics

open IQ.Core.Framework

open NUnit.Framework

/// <summary>
/// Identifies a test method
/// </summary>
type TestAttribute = NUnit.Framework.TestAttribute

/// <summary>
/// Identifies a test container
/// </summary>
type TestContainerAttribute = NUnit.Framework.TestFixtureAttribute

/// <summary>
/// Applies a trait to a test method
/// </summary>
type TraitAttribute = NUnit.Framework.CategoryAttribute


type BenchmarkTraitAttribute() =
    inherit TraitAttribute("Benchmark")

module TestContext =     
    [<Literal>]
    let private BaseDirectory = @"C:\Temp\IQ\Tests\"

    let inline getTempDir() =
        let dir = Path.Combine(BaseDirectory, thisAssembly().ShortName)
        if dir |> Directory.Exists |> not then
            dir |> Directory.CreateDirectory |> ignore
        dir


