// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Excel.Test

open System
open System.Reflection
open System.Data
open System.IO

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Framework
open IQ.Core.Data.Excel


module ExcelDataStore =
    type Tests(ctx, log) = 
        inherit ProjectTestContainer(ctx,log)
        
        let dsProvider = ctx.AppContext.Resolve<IDataStoreProvider>()

        [<Fact>]
        let ``Found worksheet by index``() =
            let xlspath = thisAssembly() |> AssemblyResource.emit "WB04.xlsx" ctx.OutputDirectory
            let store = dsProvider.GetDataStore<IExcelDataStore>(xlspath)
            let m1 = 0us |> FindWorksheetByIndex |> store.SelectMatrix
            m1.Item(0,0).ToString() |> Claim.equal "WS01-A1";

            let m2 = 1us |> FindWorksheetByIndex |> store.SelectMatrix
            m2.Item(0,0).ToString() |> Claim.equal "WS02-A1";

            let m3 = 2us |> FindWorksheetByIndex |> store.SelectMatrix
            m3.Item(0,0).ToString() |> Claim.equal "WS03-A1";

            ()

        [<Fact>]
        let ``Hydrated data matrix from Excel workbook - WB01``() =
            let xlspath = thisAssembly() |> AssemblyResource.emit "WB01.xlsx" ctx.OutputDirectory
            let csvpath = thisAssembly() |> AssemblyResource.emit "WB01.WS01.csv" ctx.OutputDirectory
            let store = dsProvider.GetDataStore<IExcelDataStore>(xlspath)
            let matrix =  "WS01" |> FindWorksheetByName |> store.SelectMatrix 
            let xlsProxies = matrix |> DataMatrix.toProxyValuesT<WB01.WS01> 
            let csvTable = csvpath |> CsvReader.readTable (CsvReader.getDefaultFormat())
            let csvProxies = csvTable |> BclDataTable.toProxyValuesT<WB01.WS01> 
            Seq.zip xlsProxies csvProxies |> Seq.iter(fun (x,y) ->
                Claim.equal x y
            )


        [<Fact>]
        let ``Hydrated data matrix from Excel workbook - WB03``() =
            let xlspath = thisAssembly() |> AssemblyResource.emit "WB03.xlsx" ctx.OutputDirectory
            let store = dsProvider.GetDataStore<IExcelDataStore>(xlspath)
            let matrix = "WS01" |> FindWorksheetByName |> store.SelectMatrix
            let proxies = matrix |> DataMatrix.toProxyValuesT<WB03.WS01> |> List.ofSeq

            proxies.[0].Col01 |> Claim.equal "ABC"
            proxies.[0].Col02 |> Claim.equal 5
            proxies.[0].Col03 |> Claim.equal true
            proxies.[0].Col04 |> Claim.equal 33.95
            proxies.[0].Col05.Year |> Claim.equal 2015
            proxies.[0].Col05.Month |> Claim.equal 1
            proxies.[0].Col05.Day |> Claim.equal 5
            
            proxies.[1].Col01 |> Claim.equal "DEF"
            proxies.[1].Col02 |> Claim.equal 10
            proxies.[1].Col03 |> Claim.equal false
            proxies.[1].Col04 |> Claim.equal 44.81
            proxies.[1].Col05.Year |> Claim.equal 2015
            proxies.[1].Col05.Month |> Claim.equal 2
            proxies.[1].Col05.Day |> Claim.equal 5



        [<Fact>]
        let ``Wrote data tables to Excel workbook - WB01``() =
            let converter = DataMatrix.getTypedConverter<WB01.WS01>()
            
            let t0_in = new DataTable("WS01")
            t0_in.Columns.Add("Col01", typeof<string>) |> ignore
            t0_in.Columns.Add("Col02", typeof<int>) |> ignore
            t0_in.Columns.Add("Col03", typeof<decimal>) |> ignore
            t0_in.Columns.Add("Col04", typeof<BclDateTime>) |> ignore
            t0_in.LoadDataRow([|"ABC" :> obj; 34:> obj; 59.8m :> obj;  BclDateTime(2003, 7, 15,0,0,0) :> obj|], true) |> ignore
            t0_in.LoadDataRow([|"DEF" :> obj; 13:> obj; 12.24m :> obj;  BclDateTime(2012, 12, 12,0,0,0) :> obj|], true) |> ignore
            t0_in.LoadDataRow([|"HIJ" :> obj; 11:> obj; 44.95m :> obj;  BclDateTime(2011, 2, 17,0,0,0) :> obj|], true) |> ignore
            let t0_in_matrix = t0_in |> DataMatrix.fromDataTable

            let t0_in_proxies = t0_in_matrix |> converter.ToProxyValues


            let t1 = new DataTable("WS02")
            t1.Columns.Add("Name", typeof<string>) |> ignore
            t1.Columns.Add("Value", typeof<int>) |> ignore
            t1.LoadDataRow([|"This is the first name" :> obj; 15 :> obj|], true) |> ignore
            t1.LoadDataRow([|"This is the second name" :> obj; 17 :> obj|], true) |> ignore
            let t1_matrix = t1 |> DataMatrix.fromDataTable

            
            let xlspath = Path.Combine(ctx.OutputDirectory, "WB01_Generated.xlsx")
            if xlspath |> File.Exists then
                xlspath |> File.Delete
            
            let store = dsProvider.GetDataStore<IExcelDataStore>(xlspath)
            store.InsertMatrix(t0_in_matrix)
            store.InsertMatrix(t1_matrix)

            let t0_out = FindWorksheetByName("WS01") |> store.SelectMatrix
            let t0_out_proxies = t0_out |> converter.ToProxyValues

            Seq.zip t0_in_proxies t0_out_proxies |> Seq.iter(fun (x,y) ->
                Claim.equal x y
            )
            

            



