// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework.Test

open System
open System.Reflection
open System.Linq.Expressions

open IQ.Core.Data
open IQ.Core.DataStructLib


module DataStructures =
    [<Literal>]
    let numLookups = 1000000 
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

    [<Benchmark(numLookups)>]
    type Benchmarks(ctx,log)  =
        inherit ProjectTestContainer(ctx,log)        

        [<Fact>]
        let ``Benchmark - Composite Key Container Init``() =
            let f() = 
                ContainerInit<CompositeKeyDictionaryContainer>()  |> ignore

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - CompositeKey Container Lookup``() =
            let container = ContainerInit<CompositeKeyDictionaryContainer>()
            let f() = 
                ContainerLookup<CompositeKeyDictionaryContainer> container numLookups

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Double Dictionary Container Init``() =
            let f() = 
                ContainerInit<DictionaryOfDictionariesContainer>()  |> ignore

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Double Dictionary Container Lookup``() =
            let container = ContainerInit<DictionaryOfDictionariesContainer>()
            let f() = 
                ContainerLookup<DictionaryOfDictionariesContainer> container numLookups

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Matrix Container Init``() =
            let f = 
                (fun () -> ContainerInit<MatrixContainer>() |> ignore) 
            
            f |> Benchmark.capture ctx


        [<Fact>]
        let ``Benchmark - Matrix Container Lookup``() =
            let container = ContainerInit<MatrixContainer>()
            let f() = 
                ContainerLookup<MatrixContainer> container numLookups

            f |> Benchmark.capture ctx

