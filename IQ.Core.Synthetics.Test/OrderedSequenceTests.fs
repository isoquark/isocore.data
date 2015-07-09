namespace IQ.Core.Synthetics.Test

open System
open System.Collections.Generic

open IQ.Core.Data
open IQ.Core.Data.Sql
open IQ.Core.Math

open IQ.Core.Synthetics

open DataValue


module FsOps = FSharp.Core.Operators

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
                let s = config1 |> OrderedSequenceProvider.get0<uint8>        
                let actual = s.NextRange(5) |> List.ofSeq
                let expect = genRefList 5uy 1uy 9uy
                actual |> Claim.seqCount expect.Length
                actual |> Claim.equal expect

            test1()

            let createConfig2() = 
                {config1 with MinValue = uint8(0uy); InitialValue = uint8(0uy); MaxValue = uint8(15uy)}
        
            let config2 = createConfig2()
            let test2() =
                let s = config2 |> OrderedSequenceProvider.get0<uint8>        
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
                let s = c |> OrderedSequenceProvider.get0<uint8>
                s.NextRange(300) |> Seq.iter(fun x -> ())
            f |> Claim.failWith<EndOfSequenceException>

    
    [<Literal>]
    let opcount = 1000000


    let inline enumBaseline (initial : ^T) (min : ^T) (inc : ^S) (max : ^T) cycle =
        let e = ArithmeticEnumerator.createInline initial min inc max cycle
        seq {while (e.MoveNext()) do
                yield  e.Current 
            } |> Seq.iter(fun x -> ())

    let createConfig0 name dataKind (initialValue : 'T) (minValue : 'T) (increment : 'S) (maxValue :'T) cycle : OrderedSequenceConfig =
            {
                Name = "Cyclical UInt8 Sequence"
                ItemDataKind = dataKind
                MinValue  = uint8(5uy)
                MaxValue = uint8(UInt8.MaxValue)
                InitialValue = uint8(5uy)
                Increment = uint8(1uy)
                Cycle = cycle
            }         
    
    let createConfig1 name dataKind initialValue  minValue increment maxValue cycle : OrderedSequenceConfig<_>=
            {
                Name = name
                ItemDataKind = dataKind
                InitialValue = minValue
                MinValue = minValue
                Increment = increment
                MaxValue = maxValue
                Cycle = cycle
            }


    [<Benchmark(opcount)>]    
    type Benchmarks(ctx,log)  =
        inherit ProjectTestContainer(ctx,log)        
                
        [<Fact>]
        let ``Benchmark - Int32 Sequence Generation - Baseline``() =           
            let f() = enumBaseline 0 0 1 opcount  false
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int32 Sequence Generation - Provider``() =
            let name = Benchmark.getBenchmarkName()           
            let minValue = 0
            let maxValue = opcount
            let config = createConfig1 name DataKind.Int32 minValue minValue 1 maxValue false
            let f() =        
                let s1 = config |> OrderedSequenceProvider.get1
                for i in minValue..maxValue do
                    s1.NextValue() |> ignore

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt32 Sequence Generation - Baseline``() =           
            let maxValue = opcount |> FsOps.uint32
            let f() = enumBaseline 0u 0u 1u maxValue  false
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt32 Sequence Generation - Provider``() =
            let name = Benchmark.getBenchmarkName()           
            let minValue = 0u
            let maxValue = opcount |> FsOps.uint32
            let config = createConfig1 name DataKind.UInt32 minValue minValue 1u maxValue false
            let f() =        
                let s1 = config |> OrderedSequenceProvider.get1
                for i in minValue..maxValue do
                    s1.NextValue() |> ignore

            f |> Benchmark.capture ctx


        [<Fact>]
        let ``Benchmark - Int64 Sequence Generation - Baseline``() =           
            let maxValue = opcount |> FsOps.int64
            let f() = enumBaseline 0L 0L 1L maxValue  false
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int64 Sequence Generation - Provider``() =
            let name = Benchmark.getBenchmarkName()            
            let minValue = 0L
            let maxValue = opcount |> FsOps.int64
            let config = createConfig1 name DataKind.Int64 minValue minValue 1L maxValue false
            let f() =        
                let s1 = config |> OrderedSequenceProvider.get1
                for i in minValue..maxValue do
                    s1.NextValue() |> ignore
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int64 Sequence Generation - Inline``() =  
            let maxval = 1000000L
            let enumerator = ArithmeticEnumerator.createInline 0L 0L 1L maxval false
            
            let mutable current = 0L
            let f() =
                while(enumerator.MoveNext()) do
                    current <- enumerator.Current
            f |> Benchmark.capture ctx

            Claim.equal maxval current
                     
        [<Fact>]
        let ``Benchmark - Int64 Sequence Generation - Generic``() =  
            let maxval = 1000000L
            let enumerator = ArithmeticEnumerator.createGeneric 0L 0L 1L maxval false
            
            let mutable current = 0L
            let f() =
                while(enumerator.MoveNext()) do
                    current <- enumerator.Current
            f |> Benchmark.capture ctx

            Claim.equal maxval current
            


    
        
           
