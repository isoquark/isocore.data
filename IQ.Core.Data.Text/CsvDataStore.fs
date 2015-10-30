// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data




open System
open System.IO
open System.Data
open System.Collections.Generic
open System.Linq

open FSharp.Data

open CsvHelper


open IQ.Core.Data.Behavior
open IQ.Core.Framework
open IQ.Core.Data.Contracts

/// <summary>
/// Defines domain vocabulary for working with CSV data/files
/// </summary>
[<AutoOpen>]
module CsvDataVocabulary =
    /// <summary>
    /// Describes the format of CSV data
    /// </summary>
    type CsvFormat = {
        /// The delimiter
        Separator : string
        
        /// The character to use for text quotations
        Quote : char
        
        /// Specifies whether headers exist
        HasHeaders : bool
    }

    /// <summary>
    /// Describes a CSV file
    /// </summary>
    type CsvFileDescription ={
        
        ///The CSV format
        Format : CsvFormat
        
        ///The filename
        Filename : string
        
        ///The size of the file
        FileSize : int64
        
        ///The number of rows in the file
        RowCount : int
        
        ///The names of the columns listed in ordinal position
        ColNames : string list
    }

/// <summary>
/// Defines operations for reading delimited text
/// </summary>
module CsvReader =       
    
    /// <summary>
    /// Hydrates list of records from file content
    /// </summary>
    /// <param name="file">Representation of the CSV file</param>
    let private read<'T> (file : FSharp.Data.Runtime.CsvFile<CsvRow>) = 

        let proxy = tableproxy<'T> |> TableProxy

        let getColumnProxy colName = 
            proxy |> DataObjectProxy.getColumns |> List.find(fun x -> x.DataElement.Name = colName)

        let converters =
            match file.Headers with
            | Some(headers) ->
                headers |> Array.map(fun header -> 
                    let colproxy = header |> getColumnProxy
                    colproxy.DataElement.Name, fun (value : string) -> 
                        value |> SmartConvert.convert colproxy.ProxyElement.ReflectedElement.Value.PropertyType
                ) |> Map.ofArray
            | None -> nosupport()

        let colnames = file.Headers |> Option.get
      
        //The CSV reader treats whitespace as significant but this is usually not desired
        //so it is eliminated prior to conversion
        let convert colname value =
            value |> Txt.trim |> converters.[colname]
        
        let createValueMap (row : CsvRow) =
            colnames |> Array.map(fun colname -> colname |> getColumnProxy |> fun c -> 
                                    c.ProxyElement.Name.Text, c.ProxyElement.Position, colname|> row.GetColumn |> convert colname) 
                     |> ValueIndex.create
        
        let pocoConverter =  PocoConverter.getDefault()

        file.Rows |> Seq.map createValueMap 
                  |> Seq.map (fun valueIndex -> 
                    match proxy.ProxyElement with
                    |TypeElement(t) -> 
                        pocoConverter.FromValueIndex(valueIndex, t.ReflectedElement.Value) :?> 'T                        
                    | _ ->
                        ArgumentException() |> raise)
                    
                  |> List.ofSeq

    /// <summary>
    /// Gets the default format for CSV files
    /// </summary>
    let getDefaultFormat() = {
        Separator = ","
        Quote = '"'
        HasHeaders = true
    }

    /// <summary>
    /// Describes an identified CSV file
    /// </summary>
    /// <param name="format">The CSV format with which the file is aligned</param>
    /// <param name="path">The path to the file</param>
    let describeFile (format : CsvFormat) (path : string) = 
        let fileinfo = FileInfo(path)
        if fileinfo.Length = 0L then
            ArgumentException("The file is empty") |> raise
        
        let lines = path |> File.ReadLines 
        let enumerator = lines.GetEnumerator()
        enumerator.MoveNext() |> ignore
        let colnames = enumerator.Current |> Txt.split format.Separator |> Array.map Txt.trim |> List.ofArray
        {   CsvFileDescription.Filename = path     
            RowCount = lines |> Seq.count
            ColNames = colnames
            Format = format
            FileSize = FileInfo(path).Length
        }
        
    /// <summary>
    /// Reads CSV-formatted data from a block of text
    /// </summary>
    /// <param name="format">The CSV format</param>
    /// <param name="text">The formatted text</param>
    let readText<'T>(format : CsvFormat) (text : string) =
        use reader = new StringReader(text)
        use file = CsvFile.Load(reader, format.Separator, format.Quote, format.HasHeaders, false).Cache()
        read<'T>  file
        
    /// <summary>
    /// Reads CSV-formatted data from a file
    /// </summary>
    /// <param name="format">The CSV format</param>
    /// <param name="path">The path to the CSV file</param>
//    let readFile<'T>(format : CsvFormat) (path : string) =
//        use file = CsvFile.Load(path, format.Separator, format.Quote, format.HasHeaders, false).Cache()
//        read<'T>  file

    let readFile<'T>(format : CsvFormat) (path : string) : IReadOnlyList<'T> =
        use txtReader = new StreamReader(path)
        use csvReader = new CsvReader(txtReader)
        csvReader.Configuration.Delimiter <- format.Separator
        csvReader.Configuration.HasHeaderRecord <- format.HasHeaders
        csvReader.Configuration.Quote <- format.Quote
        csvReader.GetRecords<'T>().ToList() :> IReadOnlyList<'T> 
        



    let readTable (format : CsvFormat) (path : string) =
        use file = CsvFile.Load(path, format.Separator, format.Quote, format.HasHeaders, false).Cache()
        let getArray(row : CsvRow) =
            let colcount = row.Columns.Length
            let result = Array.zeroCreate<obj>(colcount)
            for i in 0..colcount-1 do
                result.[i] <- row.[i] :> obj
            result

        let colnames = file.Headers |> Option.get
        let table = new DataTable(Path.GetFileNameWithoutExtension(path))
        colnames |> Array.map(table.Columns.Add) |> ignore
        for csvRow in file.Rows do
            table.LoadDataRow( csvRow |> getArray, LoadOption.OverwriteChanges) |> ignore                    
        table


/// <summary>
/// Defines operations for writing delimited text
/// </summary>
module CsvWriter =
    let writeFile<'T> (format : CsvFormat) (path : string) (items : 'T seq) =
        use txtWriter = new StreamWriter(path)
        txtWriter.AutoFlush <- true
        use csvWriter = new CsvWriter(txtWriter)
        csvWriter.Configuration.Delimiter <- format.Separator
        csvWriter.Configuration.HasHeaderRecord <- format.HasHeaders
        csvWriter.Configuration.Quote <- format.Quote
        csvWriter.WriteHeader(typeof<'T>);
        csvWriter.WriteRecords(items);
        
    /// <summary>
    /// Writes a sequence of records to a file in CSV format
    /// </summary>
//    let writeFile<'T> (format : CsvFormat) (path : string) (items : 'T seq) =        
//        let proxy = tableproxy<'T> |> TableProxy
//        let headerRow = 
//            proxy.Columns |> List.map(fun c -> c.DataElement.Name)  |> Txt.delimit format.Separator
//
//        //TODO: This is very crude; needs to escape quotes, for example
//        let formatValue (v : obj) =
//            let value = 
//                if v = null then
//                    null
//                else
//                    if v |> Option.isOptionValue then 
//                        match v |> Option.unwrapValue with
//                        | Some(x) -> x
//                        | None -> null
//                    else
//                        v
//            match value with
//            | :? string as x -> "\"" + x + "\""
//            | _ -> value.ToString()
//
//        use writer = new StreamWriter(path)
//        if format.HasHeaders then
//                proxy.Columns |> List.map(fun c -> c.DataElement.Name)
//                              |> Txt.delimit format.Separator 
//                              |> writer.WriteLine
//        
//        let pocoConverter =  PocoConverter.getDefault()
//        for item in items do
//             item |> pocoConverter.ToValueArray
//                  |> List.ofArray
//                  |> List.map formatValue 
//                  |> Txt.delimit format.Separator 
//                  |> writer.WriteLine
               

module CsvDataStore  =
    let private describeColumn tableName colName position =
        {
            ColumnDescription.Name = colName
            Position = position
            ParentName = tableName
            DataType = VariantDataType
            DataKind = DataKind.Variant
            Documentation = String.Empty
            Nullable = false
            AutoValue = AutoValueKind.None
            Properties = []
        }
        
    let private readMatrix (csvPath) =
        let tableName = DataObjectName(String.Empty, csvPath |> Path.GetFileName)
        use sr = new StreamReader(csvPath)
        use reader = new CsvReader(sr)
        let columns = List<ColumnDescription>()
        let rows = List<obj[]>()        
        let mutable columnsAdded = false        
        while(reader.Read()) do

            if columnsAdded = false then                
                reader.FieldHeaders |> Array.iteri(fun i header -> 
                    columns.Add(describeColumn tableName header i ) |> ignore)                                
                columnsAdded <- true

            [|for col in reader.FieldHeaders ->                        
                            try
                                reader.GetField(col)  :> obj
                            with
                                e ->
                                    Unchecked.defaultof<obj>
            
            |] |> rows.Add
                        
        DataMatrix(DataMatrixDescription(tableName, columns |> List.ofSeq), rows) :> IDataMatrix
            
    let private writeMatrix (csvPath : string) (m : IDataMatrix) =
        use stream = new StreamWriter(csvPath) 
        use writer = new CsvWriter(stream)
        let colCount = m.Description.Columns.Length
        for col in  m.Description.Columns do col.Name |> writer.WriteField
        writer.NextRecord()
        for row in m.Rows do
            for i in 0..colCount-1 do
                writer.WriteField<obj>(row.[i])
            writer.NextRecord()
            
    type internal Realization(cs) =
        do
            if cs |> Directory.Exists |> not then
                Directory.CreateDirectory(cs) |> ignore
                
        interface ICsvDataStore with
            member this.SelectMatrix(q) =  
                match q with
                | FindCsvByFilename(filename) -> 
                    Path.Combine(cs, filename) |> readMatrix 

            member this.MergeMatrix(m : IDataMatrix) = 
                nosupport()
            
            member this.InsertMatrix(m : IDataMatrix)  = 
                    m |> writeMatrix (Path.Combine(cs, m.Description.Name.LocalName))

            member this.Merge (items : seq<_>) : unit =
                nosupport()

            member this.Select(q) =
                let converter = DataMatrix.getTypedConverter<'T>()
                let matrix = (this :> ICsvDataStore).Select(q)
                if matrix.Count() <> 0 then
                    matrix |> Seq.exactlyOne |> converter.ToProxyValues
                else
                    Seq.empty

            member this.SelectAll() = 
                nosupport()
                                            
            member this.Insert (items : seq<'T>) =
                let converter = DataMatrix.getTypedConverter<'T>()                 
                (this :> ICsvDataStore).Insert(items |> converter.FromProxyValues |> Seq.singleton)

            member this.GetCommandContract() =
                nosupport()

            member this.GetQueryContract() =
                nosupport()

            member this.ExecuteCommand(c) =
                nosupport()

            member this.ExecutePureCommand(c) =
                nosupport()

            member this.ConnectionString =
                cs

    type internal CsvDataStoreProvider () =
        inherit DataStoreProvider<CsvDataStoreQuery>(DataStoreKind.Csv,
            fun cs -> Realization(cs) :> IDataStore<CsvDataStoreQuery>)   

        static member GetProvider() =
            CsvDataStoreProvider() :> IDataStoreProvider
        static member GetStore(cs) =
            CsvDataStoreProvider.GetProvider().GetDataStore(cs)

    let private provider = lazy(CsvDataStoreProvider() :> IDataStoreProvider)

    [<DataStoreProviderFactory(DataStoreKind.Csv)>]
    let getProvider() = provider.Value
