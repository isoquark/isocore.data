namespace IQ.Core.TestFramework

open System
open System.IO
open System.Diagnostics
open System.Collections.Generic
open System.Linq
open System.Reflection

open Xunit
open Xunit.Sdk
open Xunit.Abstractions

open IQ.Core.Data
open IQ.Core.Framework

//See: https://github.com/xunit/samples.xunit/blob/master/TraitExtensibility/

/// <summary>
/// This class discovers all of the tests and test classes that have
/// applied the Category attribute
/// </summary>
type CategoryDiscoverer() =
    interface ITraitDiscoverer with
        member this.GetTraits(attrib) =
            seq{
                let args = attrib.GetConstructorArguments().ToList()
                yield KeyValuePair("Category", args.[0].ToString())
                }
        
/// <summary>
/// Apply this attribute to your test method to specify a category.
/// </summary>
[<TraitDiscoverer("IQ.Core.TestFramework.CategoryDiscoverer", AssemblyLiterals.ShortAssemblyName)>]
[<AttributeUsage(AttributeTargets.All, AllowMultiple = true)>]
type CategoryAttribute(category) =
    inherit Attribute()

    member this.Category : string = category            

    with interface ITraitAttribute end


/// <summary>
/// This class discovers all of the tests and test classes to which the BenchmarkTrait
/// attribute has been applied
/// </summary>
type BenchmarkDiscoverer() =
    interface ITraitDiscoverer with
        member this.GetTraits(attrib) =
            seq{
                let args = attrib.GetConstructorArguments().ToList()
                //yield KeyValuePair("Benchmarks", args.[0].ToString())                
                yield KeyValuePair("Category", "Benchmarks")
                }
        
/// <summary>
/// Apply this attribute to your test method to specify a category.
/// </summary>
[<TraitDiscoverer("IQ.Core.TestFramework.BenchmarkDiscoverer", AssemblyLiterals.ShortAssemblyName)>]
[<AttributeUsage(AttributeTargets.All, AllowMultiple = true)>]
type BenchmarkAttribute(opcount) =
    inherit Attribute()

    new () =
        BenchmarkAttribute(0)
    
    member this.OperationCount : int = opcount

    with interface ITraitAttribute end

module TestContext =
    [<Literal>]
    let private BaseDirectory = @"C:\Temp\IQ\Tests\"

    let inline getTempDir() =
        let dir = Path.Combine(BaseDirectory, thisAssembly().SimpleName)
        if dir |> Directory.Exists |> not then
            dir |> Directory.CreateDirectory |> ignore
        dir

module ConfigSettingNames =
    [<Literal>]
    let LogConnectionString = "csSqlDataStore"


[<AutoOpen>]
module TestVocabulary =
    /// <summary>
    /// Identifies a test method
    /// </summary>
    type FactAttribute = Xunit.FactAttribute

    /// <summary>
    /// Applied to a test to asign it to a category
    /// </summary>
    type TraitAttribute = Xunit.TraitAttribute



    //See http://xunit.github.io/docs/shared-context.html
    [<AbstractClass>]
    type TestCollection<'T  when 'T:(new : unit->'T) and 'T: not struct>() =
        interface ICollectionFixture<'T> 
    
    type TestCollectionMakerAttribute = CollectionDefinitionAttribute
    
    type TestContainerAttribute = CollectionAttribute
        
    type Assert = Xunit.Assert
    
    type ITestLog = Xunit.Abstractions.ITestOutputHelper

    type ITestContext =
        abstract ConfigurationManager : IConfigurationManager
        abstract AppContext : IAppContext
        abstract ExecutionLog : ISqlDataStore

    [<AbstractClass>]
    type TestContext(assemblyRoot : Assembly, register:ICompositionRegistry->unit) = 
        let compose() =        
            let root = CompositionRoot.compose(fun registry ->                        
                registry |> CoreRegistration.register assemblyRoot
                registry |> register
            )
            root

        let root = compose()
        let context = root.CreateContext()
        let configManager = context.Resolve<IConfigurationManager>()
        
        let getSqlDataStore(): ISqlDataStore =
            ConfigSettingNames.LogConnectionString 
                |> configManager.GetValue 
                |> ConnectionString.parse 
                |> context.Resolve

        let store = getSqlDataStore()
        
        new (assemblyRoot:Assembly) =
            new TestContext(assemblyRoot, fun _ -> ())
                 
        member this.ConfigurationManager = configManager
        member this.AppContext = context
        member this.SqlDataStore = store
                                                   
            
        interface IDisposable with
            member this.Dispose() =
                context.Dispose()
                root.Dispose()
        
        interface ITestContext with
            member this.ConfigurationManager = configManager
            member this.AppContext = context
            member this.ExecutionLog = store
            


    module Categories =
        [<Literal>]
        let Benchmark = "Benchmark"

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

        
                                 
