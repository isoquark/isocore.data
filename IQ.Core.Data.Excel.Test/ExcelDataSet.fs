// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Excel.Test

open System
open System.Reflection
open System.Data

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Framework
open IQ.Core.Data.Excel


module ExcelDataSet =
    type Tests(ctx, log) = 
        inherit ProjectTestContainer(ctx,log)
        
        [<Fact>]
        let ``Hydrated dataset from Excel workbook - 01``() =
            let xlspath = thisAssembly() |> Assembly.emitResource "WB01.xlsx" ctx.OutputDirectory
            let csvpath = thisAssembly() |> Assembly.emitResource "WB01.WS01.csv" ctx.OutputDirectory
            let xlsTable = xlspath |> ExcelDataSet.read |> fun ds -> ds.Tables.["WS01"]
            let csvTable = csvpath |> CsvReader.readTable (CsvReader.getDefaultFormat())
           
            ()    




