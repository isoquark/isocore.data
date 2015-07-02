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

    
    


