﻿namespace IQ.Core.Data.Excel

open System
open System.Collections.Generic
open System.Data
open System.IO
open System.Linq


open IQ.Core.Data

open OfficeOpenXml
open OfficeOpenXml.Table


[<AutoOpen>]
module DataMatrixExtensions =
    type DataMatrixDescription
    with
        member this.Name = match this with | DataMatrixDescription(name,columns) -> name
        member this.Columns = match this with | DataMatrixDescription(name,columns) -> columns

module internal ExcelDataStore =
    

    let private hasValue (range : ExcelRange) =
        range <> null &&
        range.Value <> null &&
        range.Value.ToString() |> String.IsNullOrWhiteSpace |> not
    
    let private StandardDateFormats = [
        @"yyyy-mm-dd hh:mm:ss"
        @"yyyy\-mm\-dd\ hh:mm:ss"
    ]


    let private getValue (cell : ExcelRange) =
        match StandardDateFormats |> List.tryFind (fun x -> x = cell.Style.Numberformat.Format ) with
        | Some(x) ->
            DateTime.FromOADate(cell.Value :?> float) :> obj
        | None ->
            cell.Value


    let private readWorksheetMatrix(worksheet : ExcelWorksheet) =
        //This assumes we have a header row
        let rows = List<obj[]>()
        let matrixName = DataObjectName(String.Empty, worksheet.Name)
        let mutable i = 1
        let columns = [
            while(worksheet.Cells.[1,i] |> hasValue) do
                let colname = worksheet.Cells.[1,i].Value.ToString()
                yield {
                    ColumnDescription.Name = colname
                    Position = i-1
                    ParentName = matrixName
                    DataType = VariantDataType
                    DataKind = DataKind.Variant
                    Documentation = String.Empty
                    Nullable = false
                    AutoValue = AutoValueKind.None
                    Properties = []
                 
                }
                i <- i + 1
        ]
        let colcount = columns.Length
        let mutable j = 2
        while(worksheet.Cells.[j, 1] |> hasValue) do
            let row = Array.zeroCreate<obj>(colcount)
            rows.Add(row)
            for k in 0..i-2 do
                let cell = worksheet.Cells.[j, k + 1]
                if(cell.Value <> null && cell.Value.ToString() |> String.IsNullOrWhiteSpace |> not ) then
                    row.[k] <- cell |> getValue
            j <- j+ 1                    
        DataMatrix(DataMatrixDescription(matrixName, columns), rows) :> IDataMatrix

    let private getValueFormat (value : obj)  =
        match value with
        | :? DateTime | :? BclDateTime-> StandardDateFormats.Head
        | :? Decimal | :? float | :? single -> "#,##0.00"
        | :? int -> "0"
        | _ -> ""


    let private addWorksheet (workbook : ExcelWorkbook) (m : IDataMatrix) =
        let description = m.Description
        let rowcount = m.Rows.Count
        let colcount = description.Columns.Length
        let worksheet = workbook.Worksheets.Add(description.Name.LocalName)
        
        //Temporary
        let inline convertValue (value : obj) =
            match value with
            | :? DateTime as x -> x.ToDateTimeUnspecified() :> obj
            | _ -> value


        //Emit header
        for idx in [1..colcount] do
            let col = description.Columns.Item(idx - 1)
            worksheet.Cells.[1, idx].Value <- col.Name
            worksheet.Cells.[1, idx].Style.Font.Bold <- true

        //Emit data
        for rowidx in [2..rowcount+1] do
            for colidx in [1..colcount] do
                let row = m.Rows.[rowidx - 2]
                let value = row.[colidx - 1] |> convertValue
                let cell = worksheet.Cells.[rowidx,colidx]
                cell.Value <- value
                cell.Style.Numberformat.Format <- value |> getValueFormat

        //Apply global formatting options and create a table
        worksheet.Cells.AutoFitColumns()
        let tableRange = worksheet.Cells.[1,1, rowcount + 1, colcount]
        let table = worksheet.Tables.Add(tableRange, "table_" + worksheet.Name)        
        table.ShowTotal <- false
        table.TableStyle <- TableStyles.Light9

                                   
    type Realization(cs) =
        
        let openPackage() =
            new OfficeOpenXml.ExcelPackage(new FileInfo(cs))
        
        let readWorkbook() =        
            use workbook = openPackage()
            [for worksheet in workbook.Workbook.Worksheets ->
                worksheet  |> readWorksheetMatrix ] |> Seq.ofList


        interface IExcelDataStore with
            member this.SelectMatrix(q) =  
                match q with
                | FindWorksheetByName(worksheetName) ->
                    use workbook = openPackage()
                    match workbook.Workbook.Worksheets |> Seq.tryFind(fun x -> x.Name = worksheetName) with
                    | Some(ws) ->
                        ws |> readWorksheetMatrix 
                    | None ->
                        nosupport()
                | FindTableByName(tableName) ->
                    nosupport()
                | FindAllWorksheets ->
                     nosupport()

            member this.MergeMatrix m =
                nosupport()

            member this.Merge (items : seq<_>) : unit =
                nosupport()
            
            member this.InsertMatrix m = 
                use package = 
                    if File.Exists(cs) then
                        openPackage()
                     else
                        new ExcelPackage(new FileInfo(cs)) 
                m |> addWorksheet package.Workbook 
                package.Save()

            member this.Select(q) =
                let converter = DataMatrix.getTypedConverter<'T>()
                let matrix = (this :> IExcelDataStore).Select(q)
                if matrix.Count() <> 0 then
                    matrix |> Seq.exactlyOne |> converter.ToProxyValues
                else
                    Seq.empty

            member this.SelectAll() = 
                nosupport()
                                            
            member this.Insert (items : seq<'T>) =
                let converter = DataMatrix.getTypedConverter<'T>()                 
                (this :> IExcelDataStore).Insert(items |> converter.FromProxyValues |> Seq.singleton)

            member this.GetCommandContract() =
                nosupport()

            member this.GetQueryContract() =
                nosupport()

            member this.ExecuteCommand(c) =
                nosupport()

            member this.ConnectionString = cs



/// <summary>
/// Provides factory services for XLS data stores
/// </summary>
type ExcelDataStoreProvider private () =
    inherit DataStoreProvider<ExcelDataStoreQuery>(
        fun cs -> ExcelDataStore.Realization(cs) :> IDataStore<ExcelDataStoreQuery>)   

    static member GetProvider() =
        ExcelDataStoreProvider() :> IDataStoreProvider
    static member GetStore(cs) =
        ExcelDataStoreProvider.GetProvider().GetSpecificStore(cs)
