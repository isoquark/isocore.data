namespace IQ.Core.Data.Excel

open System
open System.Collections.Generic
open System.Data
open System.IO


open IQ.Core.Data

open OfficeOpenXml
open OfficeOpenXml.Table


[<AutoOpen>]
module DataMatrixExtensions =
    type DataMatrixDescription
    with
        member this.Name = match this with | DataMatrixDescription(name,columns) -> name
        member this.Columns = match this with | DataMatrixDescription(name,columns) -> columns

module ExcelDataStore =
    

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

    let private readWorksheet(worksheet : ExcelWorksheet) =
        let table = new DataTable(worksheet.Name)
        
        //This assumes we have a header row
        let mutable i = 1
        while(worksheet.Cells.[1,i] |> hasValue) do
            table.Columns.Add(worksheet.Cells.[1,i].Value.ToString()) |> ignore
            i <- i + 1

        let mutable j = 2
        while(worksheet.Cells.[j, 1] |> hasValue) do
            let row = table.NewRow()
            table.Rows.Add(row) |> ignore
            for k in 0..i-2 do
                let cell = worksheet.Cells.[j, k + 1]
                if(cell.Value <> null && cell.Value.ToString() |> String.IsNullOrWhiteSpace |> not ) then
                    row.[k] <- cell |> getValue
            j <- j+ 1                    
        table

    let private readWorksheet2(worksheet : ExcelWorksheet) =
        //This assumes we have a header row
        let rows = List<obj[]>()
        let matrixName = DataObjectName(String.Empty, worksheet.Name)
        let mutable i = 1
        let columns = [
            while(worksheet.Cells.[1,i] |> hasValue) do
                let colname = worksheet.Cells.[1,i].Value.ToString()
                yield {
                    Position = i-1
                    ColumnDescription.Name = colname
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


    let private addWorksheet2 (workbook : ExcelWorkbook) (table : IDataMatrix) =
        let description = table.Description
        let rowcount = table.RowValues.Count
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
                let row = table.RowValues.[rowidx - 2]
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


    let private addWorksheet (workbook : ExcelWorkbook) (table : DataTable) =
        let worksheet = workbook.Worksheets.Add(table.TableName)
        
        //Temporary
        let inline convertValue (value : obj) =
            match value with
            | :? DateTime as x -> x.ToDateTimeUnspecified() :> obj
            | _ -> value


        //Emit header
        for idx in [1..table.Columns.Count] do
            let col = table.Columns.Item(idx - 1)
            worksheet.Cells.[1, idx].Value <- col.ColumnName
            worksheet.Cells.[1, idx].Style.Font.Bold <- true

        //Emit data
        for rowidx in [2..table.Rows.Count+1] do
            for colidx in [1..table.Columns.Count] do
                let value = table.Rows.[rowidx - 2].[colidx - 1] |> convertValue
                let cell = worksheet.Cells.[rowidx,colidx]
                cell.Value <- value
                cell.Style.Numberformat.Format <- value |> getValueFormat

        //Apply global formatting options and create a table
        worksheet.Cells.AutoFitColumns()
        let tableRange = worksheet.Cells.[1,1, table.Rows.Count + 1, table.Columns.Count]
        let table = worksheet.Tables.Add(tableRange, "table_" + worksheet.Name)        
        table.ShowTotal <- false
        table.TableStyle <- TableStyles.Light9
                                   
    type private Realization(cs) =
        
        let openPackage() =
            new OfficeOpenXml.ExcelPackage(new FileInfo(cs))
        
        let readWorkbook() =        
            use workbook = openPackage()
            [for worksheet in workbook.Workbook.Worksheets ->
                worksheet  |> readWorksheet ] |> Seq.ofList

        let readWorkbook2() =        
            use workbook = openPackage()
            [for worksheet in workbook.Workbook.Worksheets ->
                worksheet  |> readWorksheet2 ] |> Seq.ofList


        interface IExcelDataStore with
            member this.Select(q) =  
                match q with
                | FindWorksheetByName(worksheetName) ->
                    use workbook = openPackage()
                    match workbook.Workbook.Worksheets |> Seq.tryFind(fun x -> x.Name = worksheetName) with
                    | Some(ws) ->
                        ws |> readWorksheet2 |> Seq.singleton
                    | None ->
                        Seq.empty
                | FindTableByName(tableName) ->
                    nosupport()
                | FindAllWorksheets ->
                     readWorkbook2() 
            member this.Delete(q) = 
                nosupport()

            member this.Merge(table) = 
                nosupport()
            
            member this.Insert(tables) = 
                use package = 
                    if File.Exists(cs) then
                        openPackage()
                     else
                        new ExcelPackage(new FileInfo(cs)) 
                tables |> Seq.iter(fun table -> table |> addWorksheet2 package.Workbook) 
                package.Save()                                                                                 

            member this.ConnectionString = cs


    let get(cs) =
        Realization(cs) :> IExcelDataStore
            