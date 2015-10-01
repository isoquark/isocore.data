// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Test

open System
open System.IO
open System.Diagnostics

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data


open IQ.Core.Framework.Test

module CsvReader =
    
    type CsvTestCase1 = {
        Id : int
        FirstName : string
        LastName : string
        Birthday : BclDateTime
        NetWorth : decimal option
    }

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

    type CsvTestCase3 = {
        [<Column("ID")>]
        Id : int
        [<Column("FIRST_NAME")>]
        FirstName : string
        LastName : string
        [<Column("BDAY")>]
        Birthday : BclDateTime
        [<Column("NET_WORTH")>]
        NetWorth : decimal option
    }                

    let private resname<'T> = typeof<'T>.Name |> sprintf "%s.csv"

    let private hydrate<'T>(resname) =
        let text = thisAssembly() |> AssemblyResource.findResourceText resname |> Option.get
        let format = CsvReader.getDefaultFormat()
        text |> CsvReader.readText<'T> format
        
    type Tests(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
        
        [<Fact>]
        let ``Hydrated proxies from CSV file - no attribute overrides``() =
            let items = resname<CsvTestCase1> |> hydrate<CsvTestCase1> 

            let tom_actual = items |> List.find(fun x -> x.FirstName = "Tom")
            let tom_expect = {
                CsvTestCase1.Id = 1
                FirstName = "Tom"
                LastName = "Thompson"
                Birthday = BclDateTime(1962, 5, 12)
                NetWorth = 380000m |> Some
            }

            tom_actual |> Claim.equal tom_expect
        
            let patty_actual = items |> List.find(fun x -> x.FirstName = "Patty")
            let patty_expect = {
                CsvTestCase1.Id = 3
                FirstName = "Patty"
                LastName = "Pierce"
                Birthday = BclDateTime(1970, 11, 19)
                NetWorth = 50000m |> Some
            }
            patty_actual |> Claim.equal patty_expect

        [<Fact>]
        let ``Desribed CSV file - no attribute overrides``() =
            let resname = resname<CsvTestCase1>
            let path = thisAssembly() |> AssemblyResource.emitResourceText resname (ctx.OutputDirectory)
            let format = CsvReader.getDefaultFormat()
            let actual = path |> CsvReader.describeFile format
            let tinfo = typeinfo<CsvTestCase1>
            let colNames = tinfo.Properties |> List.map(fun x -> x.Name.Text)
            let expect = {
                CsvFileDescription.ColNames = colNames
                Format = format
                Filename = path
                FileSize = FileInfo(path).Length
                RowCount = 3
            }
            actual |> Claim.equal expect
               
        [<Fact; Benchmark>]
        let ``Benchmark - CsvReader 2A``() =
            let path = thisAssembly() |> AssemblyResource.emitResourceText "CsvTestCase2.csv" (ctx.OutputDirectory)
            let format = CsvReader.getDefaultFormat()
            let  description = path |> CsvReader.describeFile format
            let f() =
                path |> CsvReader.readFile<CsvTestCase2A> format |> ignore            
            f |> Benchmark.capture ctx


        [<Fact; Benchmark>]
        let ``Benchmark - CsvReader 2B``() =
            let path = thisAssembly() |> AssemblyResource.emitResourceText "CsvTestCase2.csv" (ctx.OutputDirectory)
            let format = CsvReader.getDefaultFormat()
            let  description = path |> CsvReader.describeFile format
            let f() =
                path |> CsvReader.readFile<CsvTestCase2B> format |> ignore            
            f |> Benchmark.capture ctx
         
        
        [<Fact>]
        let ``Hydrated proxies from CSV file - column name attribute override``() =
            let items = resname<CsvTestCase3> |> hydrate<CsvTestCase3> 

            let tom_actual = items |> List.find(fun x -> x.FirstName = "Tom")
            let tom_expect = {
                Id = 1
                FirstName = "Tom"
                LastName = "Thompson"
                Birthday = BclDateTime(1962, 5, 12)
                NetWorth = 380000m |> Some
            }

            tom_actual |> Claim.equal tom_expect
        
            let patty_actual = items |> List.find(fun x -> x.FirstName = "Patty")
            let patty_expect = {
                Id = 3
                FirstName = "Patty"
                LastName = "Pierce"
                Birthday = BclDateTime(1970, 11, 19)
                NetWorth = 50000m |> Some
            }
            patty_actual |> Claim.equal patty_expect
        
        [<Fact>]
        let ``Wrote proxies to CSV file - no optional values``() =
            let expect = "CsvTestCase2.csv" |> hydrate<CsvTestCase2A> 
            let filename = sprintf "%s-writer-test.csv" typeof<CsvTestCase2A>.Name
            let path = Path.Combine(ctx.OutputDirectory, filename)
            let format = CsvReader.getDefaultFormat()
            expect |> CsvWriter.writeFile format path

            let actual =  CsvReader.readFile<CsvTestCase2A> format path

            actual |> Claim.equal expect

        [<Fact>]
        let ``Wrote proxies to CSV file - optional values``() =
            let expect = "CsvTestCase2.csv" |> hydrate<CsvTestCase2B> 
            let filename = sprintf "%s-writer-test.csv" typeof<CsvTestCase2B>.Name
            let path = Path.Combine(ctx.OutputDirectory, filename)
            let format = CsvReader.getDefaultFormat()
            expect |> CsvWriter.writeFile format path

            let actual =  CsvReader.readFile<CsvTestCase2B> format path

            actual |> Claim.equal expect
