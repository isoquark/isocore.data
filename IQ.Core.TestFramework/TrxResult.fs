namespace IQ.Core.TestFramework

open System
open System.IO
open System.Diagnostics


open NUnit.Framework
open FSharp.Data

module TrxResult =
    type TestRunXml = XmlProvider<"Resources/TrxSample1.trx", InferTypesFromValues = false>

    type TestResult = {
            TestId : string
            TestAssemblyPath : string
            TestClassName : string
            TestName : string
            ComputerName : string
            Duration : TimeSpan
            StartTime : DateTime
            EndTime : DateTime
            Succeeded : bool
        }

    type TestDefinition = {
        TestId : string
        TestName : string
        ClassName : string
        TestAssemblyPath : string
    }

    type TestRun = {
        TestRunId : string
        ResultFile : string
        StartTime : DateTime
        EndTime : DateTime
        Results : TestResult list
        }
    

    let getTestResult (definitions : Map<string, TestDefinition>) (result : TestRunXml.UnitTestResult) =
        let definition = definitions.[result.TestId]
        {
            TestResult.TestName = result.TestName
            TestClassName = definition.ClassName
            TestAssemblyPath = definition.TestAssemblyPath
            TestId = result.TestId
            ComputerName = result.ComputerName
            Duration = TimeSpan.Parse(result.Duration)
            StartTime = DateTime.Parse(result.StartTime)
            EndTime = DateTime.Parse(result.EndTime)
            Succeeded = result.Outcome = "Passed"
        }

    let getTestDefinition (definition : TestRunXml.UnitTest) =
        {
            TestDefinition.TestId = definition.Id
            TestName = definition.Name
            ClassName = definition.TestMethod.ClassName
            TestAssemblyPath = definition.TestMethod.CodeBase   
        }


    let export (path : string) (run : TestRun) =
        let header = "TestRunId,ComputerName,TestRunStartTime,TestRunEndTime,TestId,TestAssemblyName,TestClassName,TestName,TestStartTime,TestEndTime,Duration,Succeeded"
        use writer = new StreamWriter(path)
        header |> writer.WriteLine
        run.Results |> List.iter(fun result ->
            let format = sprintf "%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%i,%b" 
                                    run.TestRunId 
                                    result.ComputerName 
                                    (run.StartTime.ToString()) 
                                    (run.EndTime.ToString()) 
                                    result.TestId 
                                    result.TestAssemblyPath 
                                    result.TestClassName 
                                    result.TestName 
                                    (result.StartTime.ToString()) 
                                    (result.EndTime.ToString()) 
                                    (result.Duration.TotalMilliseconds |> int) 
                                    (result.Succeeded)
            format |> writer.WriteLine
        )

    let exportAll (trxFiles : string[]) =
        let results = 
            seq{
                for trxFile in trxFiles do
                    let trx = TestRunXml.Load(trxFile)
                    let definitions = 
                        trx.TestDefinitions.UnitTests |> Array.map getTestDefinition |> Array.map(fun d -> d.TestId, d) |> Map.ofArray
                    yield {
                        TestRun.TestRunId = trx.Id
                        ResultFile = trxFile
                        StartTime = DateTime.Parse(trx.Times.Start)
                        EndTime = DateTime.Parse(trx.Times.Finish)
                        Results = [for result in trx.Results.UnitTestResults do yield result |> getTestResult definitions]
                        }
            }

        results |> Seq.iter(fun result ->
            result |> export (Path.ChangeExtension(result.ResultFile, "csv")) 
        )
