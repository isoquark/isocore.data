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
    let numLookups = pown 10 6
    let itemcount = Nullable<int>(pown 8 3)
    
    //all in one function: initialized container with random data and executes lookups
    let ContainerInitAndRun<'T when 'T :> IDataContainer 
                        and 'T : (new : unit -> 'T)
                        and 'T :> IDisposable>() =
        use c = new 'T()
        c.Init(itemcount)
        c.ExecuteRandomLookup()

    //(STEP 1) initializes container with random data
    let ContainerInit<'T when 'T :> IDataContainer 
                        and 'T : (new : unit -> 'T)
                        and 'T :> IDisposable>() =
        let c = new 'T()
        c.Init(itemcount)
        c

    //(STEP 2) executes lookups on an already initialized container and disposes container
    let ContainerLookup<'T when 'T :> IDataContainer 
                        and 'T : (new : unit -> 'T)
                        and 'T :> IDisposable> (container : 'T) (numberOfLookups : int) =
        container.ExecuteRandomLookup(numberOfLookups) |> ignore
        container.Dispose()

    [<Fact; BenchmarkTrait>]
    let ``Benchmark - Composite Key Container Init 8^3 Types``() =
        let f() = 
            ContainerInit<CompositeKeyDictionaryContainer>()  |> ignore

        f |> Benchmark.capture

    [<Fact; BenchmarkTrait>]
    let ``Benchmark - CompositeKey Container Init 8^3 Types 10^6 Lookups``() =
        let container = ContainerInit<CompositeKeyDictionaryContainer>()
        let f() = 
            ContainerLookup<CompositeKeyDictionaryContainer> container numLookups

        f |> Benchmark.capture

    [<Fact; BenchmarkTrait>]
    let ``Benchmark - Double Dictionary Container Init 8^3 Types``() =
        let f() = 
            ContainerInit<DictionaryOfDictionariesContainer>()  |> ignore

        f |> Benchmark.capture

    [<Fact; BenchmarkTrait>]
    let ``Benchmark - Double Dictionary Container Init 8^3 Types 10^6 Lookups``() =
        let container = ContainerInit<DictionaryOfDictionariesContainer>()
        let f() = 
            ContainerLookup<DictionaryOfDictionariesContainer> container numLookups

        f |> Benchmark.capture

    [<Fact; BenchmarkTrait>]
    let ``Benchmark - Matrix Container Init 8^3 Types``() =
        (fun () -> ContainerInit<MatrixContainer>() |> ignore) |> Benchmark.capture

    [<Fact; BenchmarkTrait>]
    let ``Benchmark - Matrix Container Init 8^3 Types 10^6 Lookups``() =
        let container = ContainerInit<MatrixContainer>()
        let f() = 
            ContainerLookup<MatrixContainer> container numLookups

        f |> Benchmark.capture

