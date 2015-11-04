namespace IQ.Core.Data

open System
open System.Collections.Generic
open System.Data
open System.IO
open System.Linq
open System.Data.OleDb;


open IQ.Core.Data

open OfficeOpenXml
open OfficeOpenXml.Table


//    public static class ExcelMatrixReader
//    {
//        public static IDataMatrix ReadMatrix(string path, int wsidx)
//        {
//            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
//            {
//                using (var reader = Excel.ExcelReaderFactory.CreateOpenXmlReader(stream))
//                {
//                    reader.IsFirstRowAsColumnNames = true;
//                    return reader.AsDataSet().Tables[wsidx].ToDataMatrix();
//                }
//            }
//        }
//    }

module internal ExcelMatrixReader =
    let readMatrixByIndex path (wsidx : uint16) =
        use stream = File.Open(path, FileMode.Open, FileAccess.Read)
        use reader = Excel.ExcelReaderFactory.CreateOpenXmlReader(stream)
        reader.IsFirstRowAsColumnNames <-true
        reader.AsDataSet().Tables.[wsidx |> int] |> DataMatrixOps.fromDataTable

    let readMatrixByName path (wsname : string) =
        use stream = File.Open(path, FileMode.Open, FileAccess.Read)
        use reader = Excel.ExcelReaderFactory.CreateOpenXmlReader(stream)
        reader.IsFirstRowAsColumnNames <-true
        reader.AsDataSet().Tables.[wsname] |> DataMatrixOps.fromDataTable


module internal ExcelOleDb =

    let ExcelConnectionStringTemlate = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=Yes;ImportMixedTypes=Text'";

    [<AbstractClass>]
    type DataStoreConnection() =
    
        abstract member ReadTable:string->DataTable
        abstract member Dispose:unit->unit
        abstract member DescribeTables:unit -> TableDescription seq
        interface IDisposable with
            member this.Dispose() = this.Dispose()

    type OleDbStoreConnection(connection : OleDbConnection) =
        inherit DataStoreConnection()

        do
            connection.Open()

        new(cs : string) =
            new OleDbStoreConnection(new OleDbConnection(cs))

        override this.Dispose() =
            connection.Dispose()

        override this.DescribeTables() =
            let tables = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null)
            let columns = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, null) 
            List<TableDescription>() :> IEnumerable<TableDescription>

        member this.GetTableNames() =
            [for row in connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows ->
                row.["TABLE_NAME"] :?> string]
    
        override this.ReadTable(tableName : string) =
            let tableName = if tableName.EndsWith("$") |> not then tableName + "$" else tableName
            use command = new OleDbCommand(sprintf "select * from [%s]" tableName, connection)
            use adapter = new OleDbDataAdapter(command)
            let table = new DataTable()
            adapter.Fill(table) |> ignore
            table

        member this.ReadTable(idx : int) =
            this.GetTableNames().[idx] |> this.ReadTable
    
    type ExcelConnection(path : string) =
        inherit OleDbStoreConnection(String.Format(ExcelConnectionStringTemlate, path))
    
    
open ExcelOleDb
module ExcelReader =
    let readDataTableByName(wbpath : string, wsname : string) =
        use c = new ExcelConnection(wbpath)
        c.ReadTable(wsname)

    let readDataTableByIndex(wbpath : string, wsidx : int) =
        use c = new ExcelConnection(wbpath)
        c.ReadTable(wsidx)
        
    let readWorksheetByName<'T>(wbpath : string, wsname : string) =
        use t = readDataTableByName(wbpath, wsname)
        t |> BclDataTable.toProxyValuesT<'T>

    let readWorksheetByIndex<'T>(wbpath : string, wsidx : int) =
        use t = readDataTableByIndex(wbpath, wsidx)
        t |> BclDataTable.toProxyValuesT<'T>
    

[<AutoOpen>]
module DataMatrixExtensions =
    type DataMatrixDescription
    with
        member this.Name = match this with | DataMatrixDescription(name,columns) -> name
        member this.Columns = match this with | DataMatrixDescription(name,columns) -> columns

module ExcelDataStore =
           
    let private hasValue (range : ExcelRange) =
        range |> isNull |> not  &&
        range.Value |> isNull |> not &&
        range.Value.ToString() |> String.IsNullOrWhiteSpace |> not
    
    let private StandardDateFormats = [
        @"yyyy-mm-dd hh:mm:ss"
        @"yyyy\-mm\-dd\ hh:mm:ss"
    ]

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
        let mutable stop = false
        while worksheet.Cells.[j,1]  <> null && not(stop) do
            let mutable isEmptyRow = true
            let row = Array.zeroCreate<obj>(colcount)
            for k in 0..i-2 do
                let cell = worksheet.Cells.[j, k + 1]
                if cell.Value <> null then
                    isEmptyRow <-false
                    row.[k] <- if cell.Value.ToString().ToLower() <> "null" then cell.Value else null
            if isEmptyRow |> not then
                rows.Add(row)
            
            stop <- isEmptyRow
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

                                   
    type internal Realization(cs) =
        
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
//                    use workbook = openPackage()
//                    match workbook.Workbook.Worksheets |> Seq.tryFind(fun x -> x.Name = worksheetName) with
//                    | Some(ws) ->
//                        ws |> readWorksheetMatrix 
//                    | None ->
//                        ArgumentException(sprintf "The worksheet %s was not found" worksheetName ) |> raise
                    worksheetName |> ExcelMatrixReader.readMatrixByName cs
                | FindTableByName(tableName) ->
                    nosupport()
                | FindAllWorksheets ->
                     nosupport()
                | FindWorksheetByIndex(idx) ->
//                    use workbook = openPackage()
//                    workbook.Workbook.Worksheets.ElementAt(idx |> int) |> readWorksheetMatrix
                    idx |> ExcelMatrixReader.readMatrixByIndex cs
                    

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
                let converter = DataMatrixOps.getTypedConverter<'T>()
                (this :> IExcelDataStore).SelectMatrix(q) |> converter.ToProxyValues

            member this.SelectAll() = 
                nosupport()
                                            
            member this.Insert (items : seq<'T>) =
                let converter = DataMatrixOps.getTypedConverter<'T>()
                (this :> IExcelDataStore).InsertMatrix(items |> converter.FromProxyValues )

            member this.GetCommandContract() =
                nosupport()

            member this.GetQueryContract() =
                nosupport()

            member this.ExecuteCommand(c) =
                nosupport()

            member this.ExecutePureCommand(c) =
                nosupport()

            member this.ConnectionString = cs

    /// <summary>
    /// Provides factory services for XLS data stores
    /// </summary>
    type internal ExcelDataStoreProvider() =
        inherit DataStoreProvider<ExcelDataStoreQuery>(DataStoreKind.Xls,
            fun cs -> Realization(cs) :> IDataStore<ExcelDataStoreQuery>)
        
        static member GetProvider() =
            ExcelDataStoreProvider() :> IDataStoreProvider
        static member GetStore(cs) =
            ExcelDataStoreProvider.GetProvider().GetDataStore(cs)
    
    let private provider = lazy(ExcelDataStoreProvider() :> IDataStoreProvider)

    [<DataStoreProviderFactory(DataStoreKind.Xls)>]
    let getProvider() = provider.Value

namespace IQ.Core.Data.Contracts

open System.Runtime.CompilerServices


[<Extension>]
module ExcelExtensionsA =
    [<Extension>]
    let SelectWorksheet<'T>(store : IExcelDataStore, name) =
        store.Select<'T>(name |> FindWorksheetByName)

    [<Extension>]
    let SelectWorksheetMatrix(store : IExcelDataStore, name) =
        store.SelectMatrix(name |> FindWorksheetByName)

[<Extension>]
module ExcelExtensionsB =

    [<Extension>]
    let SelectWorksheet<'T>(store : IExcelDataStore, index) =
        store.Select<'T>(index |> FindWorksheetByIndex)

    [<Extension>]
    let SelectWorksheetMatrix(store : IExcelDataStore, index) =
        store.SelectMatrix(index |> FindWorksheetByIndex)
