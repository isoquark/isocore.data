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

    let private hydrate<'T>(resname) =
        let text = thisAssembly() |> Assembly.findTextResource resname |> Option.get
        let format = CsvReader.getDefaultFormat()
        text |> CsvReader.readText<'T> format
        

    [<Test>]
    let ``Hydrated proxies from CSV file - no attribute overrides``() =
        let items = resname<CsvTestCase1> |> hydrate<CsvTestCase1> 

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
    let ``Desribed CSV file - no attribute overrides``() =
        let resname = resname<CsvTestCase1>
        let path = thisAssembly() |> Assembly.writeTextResource resname (TestContext.getTempDir())
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
        

    type CsvTestCase2A = {
        Id : int
        DateId : int
        InstrumentId : int
        O : float
        H : float
        L : float
        C : float
        V : int64
    }

    type CsvTestCase2B = {
        Id : int
        DateId : int
        InstrumentId : int
        O : float option
        H : float option
        L : float option
        C : float option
        V : int64 option    
    }

    let private captureBenchmark<'T> resname =
        let path = thisAssembly() |> Assembly.writeTextResource resname (TestContext.getTempDir())
        let format = CsvReader.getDefaultFormat()
        let  description = path |> CsvReader.describeFile format
        let benchmark() =
            let items = path |> CsvReader.readFile<'T> format
            true
        benchmark |> Benchmark.repeat 10 (thisMethod().Name) |> Benchmark.summarize
        
    [<Test; BenchmarkTrait>]
    let ``Benchmark - CsvReader 2A``() =
        //This will eventually be persisted to a data store
        //that maintains test execution history
        let benchmark = captureBenchmark<CsvTestCase2A> "CsvTestCase2.csv"
        ()

    [<Test; BenchmarkTrait>]
    let ``Benchmark - CsvReader 2B``() =
        //This will eventually be persisted to a data store
        //that maintains test execution history
        let benchmark = captureBenchmark<CsvTestCase2B> "CsvTestCase2.csv"
        ()
         

    type CsvTestCase3 = {
        [<Column("ID")>]
        Id : int
        [<Column("FIRST_NAME")>]
        FirstName : string
        LastName : string
        [<Column("BDAY")>]
        Birthday : DateTime
        [<Column("NET_WORTH")>]
        NetWorth : decimal option
    }                

        
    [<Test>]
    let ``Hydrated proxies from CSV file - column name attribute override``() =
        let items = resname<CsvTestCase3> |> hydrate<CsvTestCase3> 

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
