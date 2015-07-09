namespace IQ.Core.Math.Test

open System
open System.Diagnostics
open System.Linq

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Math

open CalcOps

module CalculatorTests =
    
    let addBaseline(items : ('T*'T) seq) =
        if typeof<'T> = typeof<uint8> then
            for (x,y) in items.Cast<uint8*uint8>().ToList() do
                x + y |> ignore
        else if typeof<'T> = typeof<uint16> then
            for (x,y) in items.Cast<uint16*uint16>().ToList() do
                x + y |> ignore
        else if typeof<'T> = typeof<uint32> then
            for (x,y) in items.Cast<uint32*uint32>().ToList() do
                x + y |> ignore
        else if typeof<'T> = typeof<uint64> then
            for (x,y) in items.Cast<uint64*uint64>().ToList() do
                x + y |> ignore
        else if typeof<'T> = typeof<int8> then
            for (x,y) in items.Cast<int8*int8>().ToList() do
                x + y |> ignore
        else if typeof<'T> = typeof<int16> then
            for (x,y) in items.Cast<int16*int16>().ToList() do
                x + y |> ignore
        else if typeof<'T> = typeof<int32> then
            for (x,y) in items.Cast<int32*int32>().ToList() do
                x + y |> ignore
        else if typeof<'T> = typeof<int64> then
            for (x,y) in items.Cast<int64*int64>().ToList() do
                x + y |> ignore
        else if typeof<'T> = typeof<float32> then
            for (x,y) in items.Cast<float32*float32>().ToList() do
                x + y |> ignore
        else if typeof<'T> = typeof<float> then
            for (x,y) in items.Cast<float*float>().ToList() do
                x + y |> ignore
        else if typeof<'T> = typeof<decimal> then
            for (x,y) in items.Cast<decimal*decimal>().ToList() do
                x + y |> ignore
        else
            NotSupportedException(sprintf "I don't know how to run the baseline for the type %s" typeof<'T>.Name) |> raise
   
    [<Literal>]
    let opcount = 1000000

    let genItems<'T> minval maxval (count: int) =
        let calc = Calculator.get<'T>()
        let start = calc.Zero
        calc.Sequence(calc.Zero, minval, maxval, count) |> Seq.zip (calc.Sequence(calc.Zero, minval,maxval, count)) |> List.ofSeq

    let runCalc<'T> (f:seq<'T*'T>->unit) items  =
        items |> f


    let runBaseline<'T>(opname) op minval maxval =
        opcount   |> genItems<'T> minval maxval
                |> runCalc op    

    module private NumberLists =
        let Int8List = opcount |> genItems<int8> SByte.MinValue SByte.MaxValue
        let Int16List = opcount |> genItems<int16> 0s 10000s
        let Int32List = opcount |> genItems<int32> 0 10000 
        let Int64List = opcount |> genItems<int64> 0L 500000L
        let UInt16List = opcount |> genItems<uint16> 0us 10000us
        let UInt32List = opcount |> genItems<uint32> 0u 1000000u
        let UInt64List = opcount |> genItems<uint64> 0UL 500000UL
        let Float32List = opcount |> genItems<float32> Single.MinValue Single.MaxValue
        let Float64List =  opcount |> genItems<float> 0.0 1000000.0
        let DecimalList = opcount |> genItems<decimal> 0m 100000000m
        let init() = ()

    type LogicTests(ctx,log) =
        inherit ProjectTestContainer(ctx,log)

        [<Fact>]
        let ``Added numbers with generic calculators``() =
            (4uy, 5uy) |> Calculator.get().Add |> Claim.equal 9uy
            (4s, 5s) |> Calculator.get().Add |> Claim.equal 9s
            (4, 5) |> Calculator.get().Add |> Claim.equal 9
            (4L, 5L) |> Calculator.get().Add |> Claim.equal 9L
            (4y, 5y) |> Calculator.get().Add |> Claim.equal 9y
            (4us, 5us) |> Calculator.get().Add |> Claim.equal 9us
            (4u, 5u) |> Calculator.get().Add |> Claim.equal 9u
            (4UL, 5UL) |> Calculator.get().Add |> Claim.equal 9UL
            (4.0f, 5.0f)  |> Calculator.get().Add |> Claim.equal 9.0f          
            (4.0, 5.0)  |> Calculator.get().Add |> Claim.equal 9.0          
            (4.0m, 5.0m)  |> Calculator.get().Add |> Claim.equal 9.0m
            
         
    [<Benchmark(opcount)>]
    type Benchmarks(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
        
        do
            NumberLists.init()
            Calculator.init()

        //Int8
        //---------------------------------------------------------------------
        [<Fact>]
        let ``Benchmark - Int8 - Add - Baseline``() =
            let f() =
                for (x,y) in NumberLists.Int8List do
                    x + y |> ignore

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int8 - Add - Generic Calculator``() =
            let c = Calculator.get<int8>()
            let f() =
                for (x,y) in NumberLists.Int8List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int8 - Add - Generic Method``() =
            let c = Calculator.get1()
            let f() =
                for (x,y) in NumberLists.Int8List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int8 - Add - Generic Function``() =
            let f() =
                for (x,y) in NumberLists.Int8List do
                    add x y |> ignore
                
            f |> Benchmark.capture ctx

        //Int16
        //---------------------------------------------------------------------
        [<Fact>]
        let ``Benchmark - Int16 - Add - Baseline``() =
            let f() =
                for (x,y) in NumberLists.Int16List do
                    x + y |> ignore

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int16 - Add - Generic Calculator``() =
            let c = Calculator.get<int16>()
            let f() =
                for (x,y) in NumberLists.Int16List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int16 - Add - Generic Method``() =
            let c = Calculator.get1()
            let f() =
                for (x,y) in NumberLists.Int16List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int16 - Add - Generic Function``() =
            let f() =
                for (x,y) in NumberLists.Int16List do
                    add x y |> ignore
                
            f |> Benchmark.capture ctx

        //Int32
        //---------------------------------------------------------------------
        [<Fact>]
        let ``Benchmark - Int32 - Add - Baseline``() =
            let f() =
                for (x,y) in NumberLists.Int32List do
                    x + y |> ignore

            f |> Benchmark.capture ctx
        
        
        [<Fact>]
        let ``Benchmark - Int32 - Add - Generic Calculator``() =
            let c = Calculator.get<int32>()
            let f() =
                for (x,y) in NumberLists.Int32List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int32 - Add - Generic Method``() =
            let c = Calculator.get1()
            let f() =
                for (x,y) in NumberLists.Int32List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int32 - Add - Generic Function``() =
            let f() =
                for (x,y) in NumberLists.Int32List do
                    add x y |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int32 - Calculator Sequence - Baseline``() =
            let f() =
                seq{for i in 0..opcount do yield i} |> List.ofSeq |> ignore
                                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int32 - Calculator Sequence - Generic``() =
            let f() =
                let c = Calculator.get<int>()
                (0, 0, Int32.MaxValue, opcount) |> c.Sequence |> List.ofSeq |> ignore
                
            f |> Benchmark.capture ctx

            
        //Int64
        //---------------------------------------------------------------------
        [<Fact>]
        let ``Benchmark - Int64 - Add - Baseline``() =
            let f() =
                for (x,y) in NumberLists.Int64List do
                    x + y |> ignore

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int64 - Add - Generic Calculator``() =
            let c = Calculator.get<int64>()
            let f() =
                for (x,y) in NumberLists.Int64List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int64 - Add - Generic Method``() =
            let c = Calculator.get1()
            let f() =
                for (x,y) in NumberLists.Int64List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int64 - Add - Generic Function``() =
            let f() =
                for (x,y) in NumberLists.Int64List do
                    add x y |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Int64 - Increment - Baseline``() =
            let mutable x = 0L
            let f() =
                for i in 1..opcount do
                    x <- x + 1L
            f |> Benchmark.capture ctx
            Claim.equal (int64(opcount)) x
        
        [<Fact>]
        let ``Benchmark - Int64 - Increment - Generic Calculator``() =
            let mutable x = 0L
            let c = Calculator.get()
            let f() =
                for i in 1..opcount do
                    x <- x |> c.Increment
            f |> Benchmark.capture ctx
            Claim.equal (int64(opcount)) x

        
        //UInt16
        //---------------------------------------------------------------------        
        [<Fact>]
        let ``Benchmark - UInt16 - Add - Baseline``() =
            let f() =
                for (x,y) in NumberLists.UInt16List do
                    x + y |> ignore

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt16 - Add - Generic Calculator``() =
            let c = Calculator.get<uint16>()
            let f() =
                for (x,y) in NumberLists.UInt16List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt16 - Add - Generic Method``() =
            let c = Calculator.get()
            let f() =
                for (x,y) in NumberLists.UInt16List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt16 - Add - Generic Function``() =
            let f() =
                for (x,y) in NumberLists.UInt16List do
                    add x y |> ignore
                
            f |> Benchmark.capture ctx


        [<Fact>]
        let ``Benchmark - UInt32 - Add - Baseline``() =
            let f() =
                for (x,y) in NumberLists.UInt32List do
                    x + y |> ignore

            f |> Benchmark.capture ctx


        [<Fact>]
        let ``Benchmark - UInt32 - Add - Generic Calculator``() =
            let c = Calculator.get<uint32>()
            let f() =
                for (x,y) in NumberLists.UInt32List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt32 - Add - Generic Method``() =
            let c = Calculator.get()
            let f() =
                for (x,y) in NumberLists.UInt32List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt32 - Add - Generic Function``() =
            let f() =
                for (x,y) in NumberLists.UInt32List do
                    add x y |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt64 - Add - Baseline``() =
            let f() =
                for (x,y) in NumberLists.UInt64List do
                    x + y |> ignore

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt64 - Add - Generic Calculator``() =
            let c = Calculator.get<uint64>()
            let f() =
                for (x,y) in NumberLists.UInt64List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt64 - Add - Generic Method``() =
            let c = Calculator.get1()
            let f() =
                for (x,y) in NumberLists.UInt64List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - UInt64 - Add - Generic Function``() =
            let f() =
                for (x,y) in NumberLists.UInt64List do
                    add x y |> ignore
                
            f |> Benchmark.capture ctx


        [<Fact>]
        let ``Benchmark - Float32 - Add - Baseline``() =
            let f() =
                for (x,y) in NumberLists.Float32List do
                    x + y |> ignore

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Float32 - Add - Generic Calculator``() =
            let c = Calculator.get<float32>()
            let f() =
                for (x,y) in NumberLists.Float32List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Float32 - Add - Generic Method``() =
            let c = Calculator.get1()
            let f() =
                for (x,y) in NumberLists.Float32List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Float32 - Add - Generic Function``() =
            let f() =
                for (x,y) in NumberLists.Float32List do
                    add x y |> ignore
                
            f |> Benchmark.capture ctx


        [<Fact>]
        let ``Benchmark - Float64 - Add - Baseline``() =
            let f() =
                for (x,y) in NumberLists.Float64List do
                    x + y |> ignore

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Float64 - Add - Generic Calculator``() =
            let c = Calculator.get<float>()
            let f() =
                for (x,y) in NumberLists.Float64List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Float64 - Add - Generic Method``() =
            let c = Calculator.get()
            let f() =
                for (x,y) in NumberLists.Float64List do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Float64 - Add - Generic Function``() =
            let f() =
                for (x,y) in NumberLists.Float64List do
                    add x y |> ignore
                
            f |> Benchmark.capture ctx


        [<Fact>]
        let ``Benchmark - Decimal - Add - Baseline``() =
            let f() =
                for (x,y) in NumberLists.DecimalList do
                    x + y |> ignore

            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Decimal - Add - Generic Calculator``() =
            let c = Calculator.get<decimal>()
            let f() =
                for (x,y) in NumberLists.DecimalList do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Decimal - Add - Generic Method``() =
            let c = Calculator.get()
            let f() =
                for (x,y) in NumberLists.DecimalList do
                    c.Add(x,y) |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Decimal - Add - Generic Function``() =
            let f() =
                for (x,y) in NumberLists.DecimalList do
                    add x y |> ignore
                
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Number<int> - Add``() =
            let xList = NumberLists.Int32List |> List.map (fun (x,y) -> x |> Number)
            let yList = NumberLists.Int32List |> List.map (fun (x,y) -> y |> Number)
            let numbers = yList |> List.zip xList
            let f() =
                for (x,y) in numbers do
                    (+) x y |> ignore
            f |> Benchmark.capture ctx

        [<Fact>]
        let ``Benchmark - Number<int> - Subtract``() =
            let xList = NumberLists.Int32List |> List.map (fun (x,y) -> x |> Number)
            let yList = NumberLists.Int32List |> List.map (fun (x,y) -> y |> Number)
            let numbers = yList |> List.zip xList
            let f() =
                for (x,y) in numbers do
                    (-) x y |> ignore
            f |> Benchmark.capture ctx