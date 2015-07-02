namespace IQ.Core.TestFramework

open System
open System.IO
open System.Diagnostics
open System.Reflection

open IQ.Core.Framework
open IQ.Core.Data

/// <summary>
/// Defines the Benchmark domain vocabulary
/// </summary>
[<AutoOpen>]
module BenchmarkVocabulary =
    
    type BenchmarkSummary = {
        /// The name of the benchmark
        Name : string
        
        /// The name of the machine on which the benchmark was executed
        MachineName : string
        
        /// The time at which benchmark function execution began
        StartTime : DateTime

        /// The time at which benchmark function execution ended
        EndTime : DateTime
        
        /// The time required to execute the benchmark function (in ms)
        Duration : int
    }
    with 
        override this.ToString() = sprintf "%s benchmark required %i (ms)" this.Name this.Duration

    /// <summary>
    /// Encapsulates a benchmark execution result
    /// </summary>
    type BenchmarkResult<'T> = | BenchmarkResult of summary : BenchmarkSummary * detail : 'T
    with
        member this.Summary = 
            match this with BenchmarkResult(summary=x) -> x
        
        member this.Detail = 
            match this with BenchmarkResult(detail=x) ->x
        
        override this.ToString() = this.Summary.ToString()

    [<Schema("Core")>]
     type ICoreTestFrameworkProcedures =
        [<Procedure>]
        abstract pBenchmarkResultPut:BenchmarkName : string -> MachineName : string -> StartTime : DateTime -> EndTime : DateTime -> Duration : int -> [<return : RoutineParameter("Id", 5)>] int
 
/// <summary>
/// Implements capabilities related to benchmarking
/// </summary>
module Benchmark =    
    let private gcwait() =
        GC.Collect()
        GC.WaitForPendingFinalizers()
        GC.Collect()

    /// <summary>
    /// Benchmarks a capability by capturing execution statistics when invoking a function
    /// that exercises the capability
    /// </summary>
    /// <param name="benchmarkName">The name of the benchmark</param>
    /// <param name="f">The function that invokes the benchmarked capability</param>
    let run<'TDetail> benchmarkName (f:unit -> 'TDetail) =
        let sw = Stopwatch()
        let startTime = DateTime.Now
        sw.Start()
        let detail = f()
        sw.Stop()
        let endTime = DateTime.Now

        BenchmarkResult(
            {
                Name = benchmarkName
                MachineName = Environment.MachineName
                StartTime = startTime
                EndTime = endTime
                Duration = sw.Elapsed.TotalMilliseconds |> int
            }, detail)
        

    
    /// <summary>
    /// Benchmarks a capability by capturing execution statistics when repeatedly invoking a 
    /// function that exercises the capability
    /// </summary>
    /// <param name="benchmarkName">The name of the benchmark</param>
    /// <param name="f">The function that invokes the benchmarked capability</param>
    /// <param name="iterations">The number of times to execute the benchmark function</param>
    let repeat<'TDetail> (iterations : int) benchmarkName  (f:unit->'TDetail) =
        
        [for i in 0..iterations-1 do            
            gcwait()            
            let sw = Stopwatch()
            let startTime = DateTime.Now
            sw.Start()            
            let detail = f()
            sw.Stop()
            let endTime = DateTime.Now

            yield BenchmarkResult(
                {
                    Name = benchmarkName
                    MachineName = Environment.MachineName
                    StartTime = startTime
                    EndTime = endTime
                    Duration = sw.Elapsed.TotalMilliseconds |> int
                }, detail)
        ]

    /// <summary>
    /// Summarizes a collection of benchmark results
    /// </summary>
    /// <param name="results">The results to summarize</param>
    let summarize (results : BenchmarkResult<_> seq) =
        let results = results  |> Seq.sortBy(fun x -> x.Summary.StartTime) |> List.ofSeq
        let startTime = results.Head.Summary.StartTime
        let benchmarkName = results.Head.Summary.Name
        let machineName = results.Head.Summary.MachineName
        let results = results |> List.sortBy(fun x -> x.Summary.EndTime) |> List.rev
        let endTime = results.Head.Summary.EndTime
        let duration = results |> List.sumBy(fun x -> x.Summary.Duration)
        {
            Name = benchmarkName
            MachineName = machineName
            StartTime = startTime
            EndTime = endTime
            Duration = duration
        }
        
    [<Literal>]
    let private BMC = "Benchmark - "
    
    /// <summary>
    /// Derives the name of the benchmark from the currently executing method
    /// </summary>
    let inline deriveName() =
        let m = thisMethod()
        if m.Name |> Txt.startsWith BMC |> not then
            raise <| ArgumentException(sprintf "Method name \"%s\" does not align with convention" m.Name) 
        m.Name |> Txt.rightOfFirst BMC
               


