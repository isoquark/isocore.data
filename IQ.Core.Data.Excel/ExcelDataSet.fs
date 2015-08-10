namespace IQ.Core.Data.Excel

open System
open System.Data
open System.IO;

open OfficeOpenXml

module ExcelDataSet =
  
    let private hasValue (range : ExcelRange) =
        range <> null &&
        range.Value <> null &&
        range.Value.ToString() |> String.IsNullOrWhiteSpace |> not
    
    /// <summary>
    /// Hydrates a <see cref="DataSet"/> with the contents of an Excel file
    /// </summary>
    let read (path : string) =        
        let ds = new DataSet()
        use workbook = new OfficeOpenXml.ExcelPackage(new FileInfo(path))
        for worksheet in workbook.Workbook.Worksheets do
            let table = new DataTable(worksheet.Name)
            ds.Tables.Add(table)

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
                        row.[k] <- cell.Value
                j <- j+ 1                    
                        
                

        ds
