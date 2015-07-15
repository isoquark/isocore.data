namespace IQ.Core.Math.Test

open IQ.Core.TestFramework
open IQ.Core.Math

module ArrayCalcTests =

    [<Literal>]
    let opcount = 1000000
    
    [<Benchmark(opcount)>]
    type Benchmarks(ctx,log) =
        inherit ProjectTestContainer(ctx,log)                        

        [<Fact>]
        let ``Benchmark - Int32 - Array Multiplication - Expressions``() =
            let calculator = ArrayCalculator.get()
            let x = [|2; 3; 4; 5|] 
            let y = [|1; 2; 6; 2|] 
            let f() =
                for i in 1..opcount do
                    calculator.Multiply(x,y) |> ignore
            f |> Benchmark.capture ctx         
                       
        [<Fact>]
        let ``Benchmark - Int32 - Array Multiplication - CPP``() =
            let calculator = MathServices.GetArrayCalculator()
            let x = [|2; 3; 4; 5|] 
            let y = [|1; 2; 6; 2|] 
            let f() =
                for i in 1..opcount do
                    calculator.Multiply(x,y) |> ignore
            f |> Benchmark.capture ctx         
