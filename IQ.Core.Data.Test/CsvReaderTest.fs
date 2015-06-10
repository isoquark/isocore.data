namespace IQ.Core.Data.Test

open System

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data

[<TestContainer>]
module CsvReaderTest =
    
    type CsvTestCase1 = {
        Id : int
        FirstName : string
        LastName : string
        Birthday : DateTime
        NetWorth : decimal
    }

    [<Test>]
    let ``Read delimited text file - CsvTestCase1.csv``() =
        let resname = thisMethod().Name |> Txt.rightOf "-" |> Txt.trim 
        let text = thisAssembly() |> ClrAssembly.findTextResource resname |> Option.get
        let format = CsvReader.getDefaultFormat()
        let items = text |> CsvReader.readText<CsvTestCase1> format
        ()

