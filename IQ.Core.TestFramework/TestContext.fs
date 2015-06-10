namespace IQ.Core.TestFramework

open System.IO

open IQ.Core.Framework

open NUnit.Framework

/// <summary>
/// Identifies a test
/// </summary>
type TestAttribute = NUnit.Framework.TestAttribute

/// <summary>
/// Identifies a test container
/// </summary>
type TestContainerAttribute = NUnit.Framework.TestFixtureAttribute

module TestContext = 
    
    [<Literal>]
    let private BaseDirectory = @"C:\Temp\IQ\Tests\"

    let inline getTempDir() =
        let dir = Path.Combine(BaseDirectory, thisAssembly().ShortName)
        if dir |> Directory.Exists |> not then
            dir |> Directory.CreateDirectory |> ignore
        dir


