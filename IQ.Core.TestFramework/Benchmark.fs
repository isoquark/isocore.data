namespace IQ.Core.TestFramework

open System
open System.IO
open System.Diagnostics

open IQ.Core.Framework

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
        
        /// The time required to execute the benchmark function
        Duration : TimeSpan

        /// The iteration number assigned to the benchmark execution if applicable
        Iteration : int option
    }

    /// <summary>
    /// Encapsulates a benchmark execution result
    /// </summary>
    type BenchmarkResult<'T> = | BenchmarkResult of summary : BenchmarkSummary * detail : 'T
    with
        member this.Summary = 
            match this with BenchmarkResult(summary=x) -> x
        
        member this.Detail = 
            match this with BenchmarkResult(detail=x) ->x


/// <summary>
/// Implements capabilities related to benchmarking
/// </summary>
module Benchmark =    
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
                Duration = sw.Elapsed
                Iteration = None
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
                    Duration = sw.Elapsed
                    Iteration = Some(i)
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
        let duration = results |> List.map(fun x -> x.Summary.Duration) |> TimeSpan.Sum
        {
            Name = benchmarkName
            MachineName = machineName
            StartTime = startTime
            EndTime = endTime
            Duration = duration
            Iteration = None
        }
        
        
               


