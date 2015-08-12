namespace IQ.Core.Data.Excel

open System
open System.Collections.Generic
open System.Data
open System.IO




open OfficeOpenXml
open OfficeOpenXml.Table

type IExcelDataStore =
    inherit IDataStore<DataTable,ExcelDataStoreQuery>

module ExcelDataStore =
    let private rol(items : seq<_>) = List<_>(items) :> IReadOnlyList<_>

    let private hasValue (range : ExcelRange) =
        range <> null &&
        range.Value <> null &&
        range.Value.ToString() |> String.IsNullOrWhiteSpace |> not
    
    let private StandardDateFormats = [
        @"yyyy-mm-dd hh:mm:ss"
        @"yyyy\-mm\-dd\ hh:mm:ss"
    ]


    let private readWorksheet(worksheet : ExcelWorksheet) =
        let table = new DataTable(worksheet.Name)

        let getValue (cell : ExcelRange) =
            match StandardDateFormats |> List.tryFind (fun x -> x = cell.Style.Numberformat.Format ) with
            | Some(x) ->
                DateTime.FromOADate(cell.Value :?> float) :> obj
            | None ->
                cell.Value
        
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


    let private getValueFormat (value : obj)  =
        match value with
        | :? DateTime | :? BclDateTime-> StandardDateFormats.Head
        | :? Decimal | :? float | :? single -> "#,##0.00"
        | :? int -> "0"
        | _ -> ""


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
            let tables = ResizeArray<DataTable>()
            use workbook = openPackage()
            for worksheet in workbook.Workbook.Worksheets do
                tables.Add( worksheet  |> readWorksheet )                                        
            tables :> IReadOnlyList<DataTable>


        interface IExcelDataStore with
            member this.Select(q) =  
                match q with
                | FindWorksheetByName(worksheetName) ->
                    use workbook = openPackage()
                    match workbook.Workbook.Worksheets |> Seq.tryFind(fun x -> x.Name = worksheetName) with
                    | Some(ws) ->
                        ws |> readWorksheet |> List.singleton |> rol
                    | None ->
                        [] |> rol
                | FindTableByName(tableName) ->
                    nosupport()
                | FindAllWorksheets ->
                     readWorkbook()
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
                tables |> Seq.iter(fun table -> table |> addWorksheet package.Workbook) 
                package.Save()                                                                                 

            member this.ConnectionString = ConnectionString([cs])


    let get(cs) =
        Realization(cs) :> IExcelDataStore
            