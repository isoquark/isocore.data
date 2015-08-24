// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.IO
open System.Data
open System.Collections.Generic

open FSharp.Data

open CsvHelper

open IQ.Core.Framework

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

        let proxy = tabularproxy<'T> |> TabularProxy

        let getColumnProxy colName = 
            proxy |> DataObjectProxy.getColumns |> List.find(fun x -> x.DataElement.Name = colName)

        let converters =
            match file.Headers with
            | Some(headers) ->
                headers |> Array.map(fun header -> 
                    let colproxy = header |> getColumnProxy
                    colproxy.DataElement.Name, fun (value : string) -> 
                        value |> Transformer.convert colproxy.ProxyElement.ReflectedElement.Value.PropertyType
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
    let readFile<'T>(format : CsvFormat) (path : string) =
        use file = CsvFile.Load(path, format.Separator, format.Quote, format.HasHeaders, false).Cache()
        read<'T>  file        

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
    /// <summary>
    /// Writes a sequence of records to a file in CSV format
    /// </summary>
    let writeFile<'T> (format : CsvFormat) (path : string) (items : 'T seq) =        
        let proxy = tabularproxy<'T> |> TabularProxy
        let headerRow = 
            proxy.Columns |> List.map(fun c -> c.DataElement.Name) |> List.asReadOnlyList |> Txt.delimit format.Separator

        //TODO: This is very crude; needs to escape quotes, for example
        let formatValue (v : obj) =
            let value = 
                if v = null then
                    null
                else
                    if v |> Option.isOptionValue then 
                        match v |> Option.unwrapValue with
                        | Some(x) -> x
                        | None -> null
                    else
                        v
            match value with
            | :? string as x -> "\"" + x + "\""
            | _ -> value.ToString()

        use writer = new StreamWriter(path)
        if format.HasHeaders then
                proxy.Columns |> List.map(fun c -> c.DataElement.Name)
                              |> List.asReadOnlyList 
                              |> Txt.delimit format.Separator 
                              |> writer.WriteLine
        
        let pocoConverter =  PocoConverter.getDefault()
        for item in items do
             item |> pocoConverter.ToValueArray
                  |> List.ofArray
                  |> List.map formatValue 
                  |> List.asReadOnlyList
                  |> Txt.delimit format.Separator 
                  |> writer.WriteLine
               

type CsvDataStoreQuery =
    | FindCsvByFilename of filename : string

type ICsvDataStore =
    inherit IDataStore<DataTable,CsvDataStoreQuery>

module CsvDataStore  =
    let private rolist(items : seq<_>) = List<_>(items) :> IReadOnlyList<_>

    let private readTable (csvPath)=
        let table = new DataTable( csvPath |> Path.GetFileName)
        use sr = new StreamReader(csvPath)
        use reader = new CsvReader(sr)

        
        let mutable columnsAdded = false        
        while(reader.Read()) do

            if columnsAdded = false then
                reader.FieldHeaders |> Array.iter(fun header -> 
                    table.Columns.Add(header) |> ignore)
                columnsAdded <- true

            let data = [|for col in reader.FieldHeaders ->                        
                            try
                                reader.GetField(col)  :> obj
                            with
                                e ->
                                    Unchecked.defaultof<obj>
            
                       |]
            table.LoadDataRow(data, true) |> ignore
        table
    
    let private writeTable (csvPath : string) (csvTable : DataTable) = 
        use stream = new StreamWriter(csvPath) 
        use writer = new CsvWriter(stream)
        let colCount = csvTable.Columns.Count
        for col in  csvTable.Columns do col.ColumnName |> writer.WriteField
        writer.NextRecord()
        for row in csvTable.Rows do
            for i in 0..colCount-1 do
                writer.WriteField<obj>(row.[i])
            writer.NextRecord()
    
    type Realization(cs) =
        do
            if cs |> Directory.Exists |> not then
                Directory.CreateDirectory(cs) |> ignore
                
        interface ICsvDataStore with
            member this.Select(q) =  
                match q with
                | FindCsvByFilename(filename) -> 
                    Path.Combine(cs, filename) |> readTable |> List.singleton |> rolist
            member this.Delete(q) = 
                nosupport()

            member this.Merge(table) = 
                nosupport()
            
            member this.Insert(tables) = 
                tables |> Seq.iter(fun table ->
                    table |> writeTable (Path.Combine(cs, table.TableName))
                )

            member this.ConnectionString = ConnectionString([cs])
