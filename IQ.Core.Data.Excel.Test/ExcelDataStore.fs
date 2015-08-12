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
        
        

        [<Fact>]
        let ``Hydrated data tables from Excel workbook - 01``() =
            let xlspath = thisAssembly() |> Assembly.emitResource "WB01.xlsx" ctx.OutputDirectory
            let csvpath = thisAssembly() |> Assembly.emitResource "WB01.WS01.csv" ctx.OutputDirectory
            let store = xlspath |> ExcelDataStore.get
            let xlsTable =  "WS01" |> FindWorksheetByName |> store.SelectOne 
            let xlsProxies = xlsTable |> DataTable.toProxyValuesT<WB01.WS01> 
            let csvTable = csvpath |> CsvReader.readTable (CsvReader.getDefaultFormat())
            let csvProxies = csvTable |> DataTable.toProxyValuesT<WB01.WS01> 
            Seq.zip xlsProxies csvProxies |> Seq.iter(fun (x,y) ->
                Claim.equal x y
            )
            

        [<Fact>]
        let ``Wrote data tables to Excel workbook - WB01``() =
            let converter = DataTable.getTypedConverter<WB01.WS01>()
            
            let t0_in = new DataTable("WS01")
            t0_in.Columns.Add("Col01", typeof<string>) |> ignore
            t0_in.Columns.Add("Col02", typeof<int>) |> ignore
            t0_in.Columns.Add("Col03", typeof<decimal>) |> ignore
            t0_in.Columns.Add("Col04", typeof<DateTime>) |> ignore
            t0_in.LoadDataRow([|"ABC" :> obj; 34:> obj; 59.8m :> obj;  DateTime(2003, 7, 15,0,0,0) :> obj|], true) |> ignore
            t0_in.LoadDataRow([|"DEF" :> obj; 13:> obj; 12.24m :> obj;  DateTime(2012, 12, 12,0,0,0) :> obj|], true) |> ignore
            t0_in.LoadDataRow([|"HIJ" :> obj; 11:> obj; 44.95m :> obj;  DateTime(2011, 2, 17,0,0,0) :> obj|], true) |> ignore

            let t0_in_proxies = t0_in |> converter.ToProxyValues


            let t1 = new DataTable("WS02")
            t1.Columns.Add("Name", typeof<string>) |> ignore
            t1.Columns.Add("Value", typeof<int>) |> ignore
            t1.LoadDataRow([|"This is the first name" :> obj; 15 :> obj|], true) |> ignore
            t1.LoadDataRow([|"This is the second name" :> obj; 17 :> obj|], true) |> ignore
            
            
            let xlspath = Path.Combine(ctx.OutputDirectory, "WB01_Generated.xlsx")
            if xlspath |> File.Exists then
                xlspath |> File.Delete
            
            let store = xlspath |> ExcelDataStore.get
            store.Insert([t0_in; t1])

            let t0_out = FindWorksheetByName("WS01") |> store.SelectOne
            let t0_out_proxies = t0_out |> converter.ToProxyValues

            Seq.zip t0_in_proxies t0_out_proxies |> Seq.iter(fun (x,y) ->
                Claim.equal x y
            )
            

            



