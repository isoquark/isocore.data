namespace IQ.Core.Framework.Test

open System
open System.Collections.Generic

open IQ.Core.Data
open IQ.Core.Data.Sql

open IQ.Core.Synthetics

open DataValue




module OrderedSequence =
                     
    let inline genRefList min skip max = [min..skip..max]
        
    type LogicTests(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
            
        [<Fact>]
        let ``Generated Cyclical UInt8 sequences``() =        
        
            let createConfig1() : OrderedSequenceConfig =
                {
                    Name = "Cyclical UInt8 Sequence"
                    ItemDataKind = DataKind.UInt8
                    MinValue  = uint8(5uy)
                    MaxValue = uint8(UInt8.MaxValue)
                    InitialValue = uint8(5uy)
                    Increment = uint8(1uy)
                    Cycle = true
                }         
        
            let config1 = createConfig1()
            let test1() =        
                let s = config1 |> OrderedSequence.get<uint8>        
                let actual = s.NextRange(5) |> List.ofSeq
                let expect = genRefList 5uy 1uy 9uy
                actual |> Claim.seqCount expect.Length
                actual |> Claim.equal expect

            test1()

            let createConfig2() = 
                {config1 with MinValue = uint8(0uy); InitialValue = uint8(0uy); MaxValue = uint8(15uy)}
        
            let config2 = createConfig2()
            let test2() =
                let s = config2 |> OrderedSequence.get<uint8>        
                let actual = s.NextRange(28) |> List.ofSeq
                let expect =  (genRefList 0uy 1uy 11uy) |> List.append (genRefList 0uy 1uy 15uy)
                actual |> Claim.seqCount expect.Length
                actual |> Claim.equal expect

            test2()

        [<Fact>]
        let ``Failed when enumerating past the end of a non-cyclical sequence``() =
            let createConfig() : OrderedSequenceConfig =
                {
                    Name = "Non-Cyclical UInt8 Sequence"
                    ItemDataKind = DataKind.UInt8
                    MinValue  = uint8(0uy)
                    MaxValue = uint8(UInt8.MaxValue)
                    InitialValue = uint8(5uy)
                    Increment = uint8(1uy)
                    Cycle = false
                }         
            let f() =
                let c = createConfig()
                let s = c |> OrderedSequence.get<uint8>
                s.NextRange(300) |> Seq.iter(fun x -> ())
            f |> Claim.failWith<EndOfSequenceException>

    [<Category(Categories.Benchmark)>]
    type Benchmarks(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
                
        [<Fact>]
        let ``Benchmark - Int32 Framework Sequence Generation: 10^6 Calls``() =
            let name = Benchmark.deriveDesignator()
           
            let f() =
                let minValue = 0
                let maxValue = pown 10 6
                let c1() : OrderedSequenceConfig =
                    {
                        Name = name
                        ItemDataKind = DataKind.Int32
                        MinValue = int32(minValue)
                        MaxValue = int32(maxValue)
                        InitialValue = int32(minValue)
                        Increment = int32(1)
                        Cycle = false
                    }
        
                let s1 = c1() |> OrderedSequence.get<int32>
                for i in minValue..maxValue do
                    s1.NextValue() |> ignore

            f |> Benchmark.capture ctx.SqlDataStore

        [<Fact>]
        let ``Benchmark - Int64 Framework Sequence Generation: 10^6 Calls``() =
            let name = Benchmark.deriveDesignator()
           
            let f() =
                let minValue = 0L
                let maxValue = pown 10L 6
                let c1() : OrderedSequenceConfig =
                    {
                        Name = name
                        ItemDataKind = DataKind.Int64
                        MinValue = int64(minValue)
                        MaxValue = int64(maxValue)
                        InitialValue = int64(minValue)
                        Increment = int64(1L)
                        Cycle = false
                    }
        
                let s1 = c1() |> OrderedSequence.get<int64>
                for i in minValue..maxValue do
                    s1.NextValue() |> ignore

            f |> Benchmark.capture ctx.SqlDataStore


        [<Fact>]
        let ``Benchmark - Int32 Direct Sequence Generation: 10^6 Calls``() =
           
            let f() =
                let e = OrderedSequence.createEnumerator 0 0 1 (pown 10 6) false
                let mutable current = 0
                while (e.MoveNext()) do
                    current <- e.Current
            f |> Benchmark.capture ctx.SqlDataStore

            

