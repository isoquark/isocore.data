namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework
open IQ.Core.Data

open System
open System.Reflection
open System.Linq.Expressions

open IQ.Core.DataStructLib

[<TestContainer>]
module DataStructTest =

    //all in one function: initialized container with random data and executes lookups
    let ContainerInitAndRun<'T when 'T :> IDataContainer 
                        and 'T : (new : unit -> 'T)
                        and 'T :> IDisposable>() =
        use c = new 'T()
        c.Init()
        c.ExecuteRandomLookup() |> ignore

    //(STEP 1) initializes container with random data
    let ContainerInit<'T when 'T :> IDataContainer 
                        and 'T : (new : unit -> 'T)
                        and 'T :> IDisposable>() =
        let c = new 'T()
        c.Init()
        c

    //(STEP 2) executes lookups on an already initialized container and disposes container
    let ContainerLookup<'T when 'T :> IDataContainer 
                        and 'T : (new : unit -> 'T)
                        and 'T :> IDisposable> (container : 'T) (numberOfLookups : int) =
        container.ExecuteRandomLookup(numberOfLookups) |> ignore
        container.Dispose()

    let numLookups = pown 10 6


    [<Test; BenchmarkTrait>]
    let ``Benchmark - DataStruct Executed Composite Key Container Init and 10^6 Lookups on all System Types``() =
        let f() = 
            ContainerInitAndRun<CompositeKeyDictionaryContainer>() 

        f |> Benchmark.capture

    [<Test; BenchmarkTrait>]
    let ``Benchmark - DataStruct CompositeKey Container 3228Types 10^6Lookups``() =
        let container = ContainerInit<CompositeKeyDictionaryContainer>()
        let f() = 
            ContainerLookup<CompositeKeyDictionaryContainer> container numLookups

        f |> Benchmark.capture

    [<Test; BenchmarkTrait>]
    let ``Benchmark - DataStruct Executed Dictionary of Dictionaries Container 10^6 Lookups on all System Types``() =
        let f() = 
            ContainerInitAndRun<DictionaryOfDictionariesContainer>() 

        f |> Benchmark.capture

    [<Test; BenchmarkTrait>]
    let ``Benchmark - DataStruct DictOfDict Container 3228Types 10^6Lookups``() =
        let container = ContainerInit<DictionaryOfDictionariesContainer>()
        let f() = 
            ContainerLookup<DictionaryOfDictionariesContainer> container numLookups

        f |> Benchmark.capture

    [<Test; BenchmarkTrait>]
    let ``Benchmark - DataStruct Executed Matrix with Type Indexing Container 10^6 Lookups on all System Types``() =
        let f() = 
            ContainerInitAndRun<MatrixContainer>() 

        f |> Benchmark.capture

    [<Test; BenchmarkTrait>]
    let ``Benchmark - DataStruct Matrix Container 3228Types 10^6Lookups``() =
        let container = ContainerInit<MatrixContainer>()
        let f() = 
            ContainerLookup<MatrixContainer> container numLookups

        f |> Benchmark.capture

