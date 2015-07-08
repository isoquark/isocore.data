namespace IQ.Core.TestFramework

open System
open System.IO
open System.Diagnostics
open System.Reflection

open IQ.Core.Data
open IQ.Core.Framework

/// <summary>
/// Defines the Benchmark domain vocabulary
/// </summary>
[<AutoOpen>]
module BenchmarkVocabulary =
    
    type BenchmarkSummary = {
        /// The name of the benchmark
        Name : string
        
        /// The full name of the type that defines the benchmark
        DeclaringType : string

        /// The number of operations/iterations executed during the benchmark
        OperationCount : int

        /// The name of the machine on which the benchmark was executed
        MachineName : string
        
        /// The time at which benchmark function execution began
        StartTime : BclDateTime

        /// The time at which benchmark function execution ended
        EndTime : BclDateTime
        
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
        abstract pBenchmarkResultPut:BenchmarkName : string -> DeclaringType : string -> OpCount : int -> MachineName : string -> StartTime : BclDateTime -> EndTime : BclDateTime -> Duration : int -> [<return : RoutineParameter("Id", 5)>] int
 

    type BenchmarkParameter = BenchmarkParameter of name : string * value : string
    with
        member this.Name = match this with BenchmarkParameter(name=x) -> x
        member this.Value = match this with BenchmarkParameter(value=x) -> x

    type BenchmarkDesignator = BenchmarkDesignator of name: string * parameters : BenchmarkParameter list
    with
        member this.Name = match this with BenchmarkDesignator(name=x) -> x
        member this.Parameters = match this with BenchmarkDesignator(parameters=x) -> x
    
/// <summary>
/// Benchmarking implementation details; note that this cannot actually be internal
/// because the functions herein are used in inline methods
/// </summary>
module BenchmarkInternals =
    [<Literal>]
    let private BMC = "Benchmark - "
    
    let private gcwait() =
        GC.Collect()
        GC.WaitForPendingFinalizers()
        GC.Collect()
    
    /// <summary>
    /// Derives the name of the benchmark from the currently executing method
    /// </summary>
    let getBenchmarkName(testMethod : MethodInfo) =
        if testMethod.Name |> Txt.startsWith BMC |> not then
            raise <| ArgumentException(sprintf "Method name \"%s\" does not align with convention" testMethod.Name) 
        testMethod.Name |> Txt.rightOfFirst BMC
    
    let getAttribute(testMethod : MethodInfo) =
       if Attribute.IsDefined(testMethod, typeof<BenchmarkAttribute>) then
            testMethod.GetCustomAttribute<BenchmarkAttribute>()
        else
            let declaringTypes = 
                let mutable t = testMethod.DeclaringType
                [while(t <> null) do
                    yield t
                    t <- t.DeclaringType]
            declaringTypes |> List.find(fun t -> Attribute.IsDefined(t, typeof<BenchmarkAttribute>))
                           |> fun t -> t.GetCustomAttribute<BenchmarkAttribute>()

    /// <summary>
    /// Benchmarks a capability by capturing execution statistics when invoking a function
    /// that exercises the capability
    /// </summary>
    /// <param name="benchmarkName">The name of the benchmark</param>
    /// <param name="testMethod">The test method that configures benchmark parameters and executes it</param>
    /// <param name="f">The function that invokes the benchmarked capability</param>
    let run<'TDetail> (testMethod : MethodInfo) (f:unit -> 'TDetail) =
        //gcwait()
        let sw = Stopwatch()
        let startTime = BclDateTime.Now
        sw.Start()
        let detail = f()
        sw.Stop()
        let endTime = BclDateTime.Now

        BenchmarkResult(
            {
                Name = testMethod |> getBenchmarkName
                DeclaringType = testMethod.DeclaringType.FullName
                MachineName = Environment.MachineName                
                OperationCount = getAttribute(testMethod).OperationCount
                StartTime = startTime
                EndTime = endTime
                Duration = sw.Elapsed.TotalMilliseconds |> int
            }, detail)

    let record (store : ISqlDataStore) (result : BenchmarkResult<_>)  =
        let store = store.GetContract<ICoreTestFrameworkProcedures>()           
        let summary = result.Summary
        store.pBenchmarkResultPut summary.Name summary.DeclaringType summary.OperationCount summary.MachineName summary.StartTime summary.EndTime summary.Duration |> ignore

                    
                        
            

/// <summary>
/// Implements capabilities related to benchmarking
/// </summary>
module Benchmark =                                         

    let inline capture (ctx : ITestContext) (f:unit->unit) =
        f |> BenchmarkInternals.run (thisMethod()) |> BenchmarkInternals.record ctx.ExecutionLog
    
    let inline getBenchmarkName() =
        thisMethod() |> BenchmarkInternals.getBenchmarkName

//module BenchmarkExtensions =
    

