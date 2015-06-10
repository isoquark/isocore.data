namespace IQ.Core.Data.Test

open System
open System.IO
open System.Diagnostics

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data

[<TestContainer>]
module ``CsvReader Test`` =
    
    type CsvTestCase1 = {
        Id : int
        FirstName : string
        LastName : string
        Birthday : DateTime
        NetWorth : decimal option
    }

    let private resname<'T> = typeof<'T>.Name |> sprintf "%s.csv"

    [<Test>]
    let ``Hydrated proxies from CSV file - CsvTestCase1``() =
        let resname = resname<CsvTestCase1>
        let text = thisAssembly() |> ClrAssembly.findTextResource resname |> Option.get
        let format = CsvReader.getDefaultFormat()
        let items = text |> CsvReader.readText<CsvTestCase1> format

        let tom_actual = items |> List.find(fun x -> x.FirstName = "Tom")
        let tom_expect = {
            Id = 1
            FirstName = "Tom"
            LastName = "Thompson"
            Birthday = DateTime(1962, 5, 12)
            NetWorth = 380000m |> Some
        }

        tom_actual |> Claim.equal tom_expect
        
        let patty_actual = items |> List.find(fun x -> x.FirstName = "Patty")
        let patty_expect = {
            Id = 3
            FirstName = "Patty"
            LastName = "Pierce"
            Birthday = DateTime(1970, 11, 19)
            NetWorth = 50000m |> Some
        }
        patty_actual |> Claim.equal patty_expect

    [<Test>]
    let ``Desribed CSV file - CsvTestCase1``() =
        let resname = resname<CsvTestCase1>
        let path = thisAssembly() |> ClrAssembly.writeTextResource resname (TestContext.getTempDir())
        let format = CsvReader.getDefaultFormat()
        let actual = path |> CsvReader.describeFile format
        let expect = {
            CsvFileDescription.ColNames = recordinfo<CsvTestCase1>.Fields |> List.map(fun field -> field.Name)
            Format = format
            Filename = path
            FileSize = FileInfo(path).Length
            RowCount = 3
        }
        actual |> Claim.equal expect
        

    type CsvTestCase2 = {
        Id : int
        DateId : int
        InstrumentId : int
        O : float
        H : float
        L : float
        C : float
        V : int64
    }


//    let calculateBenchmark<'T> count resname=
//        let path = thisAssembly() |> ClrAssembly.writeTextResource resname (TestContext.getTempDir())        
//        let format = CsvReader.getDefaultFormat()
//        let benchmark() =
//            let items = path |> CsvReader.readFile<'T> format
//
//            ()


    [<Test; BenchmarkTrait>]
    let ``Benchmark - CsvReader 1``() =
        let resname = resname<CsvTestCase2>
        let path = thisAssembly() |> ClrAssembly.writeTextResource resname (TestContext.getTempDir())
        let format = CsvReader.getDefaultFormat()
        let  description = path |> CsvReader.describeFile format
        let benchmark() =
            let items = path |> CsvReader.readFile<CsvTestCase2> format
            true

        let results = benchmark |> Benchmark.repeat 10 (thisMethod().Name)
        ()

         
                
