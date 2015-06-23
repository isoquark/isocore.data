namespace IQ.Core.Data

open System
open System.IO

open FSharp.Data

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

        let proxy = tabularproxy<'T>

        let getColumnProxy colName = 
            proxy |> DataObjectProxy.getColumns |> List.find(fun x -> x.DataElement.Name = colName)

        let converters =
            match file.Headers with
            | Some(headers) ->
                headers |> Array.map(fun header -> 
                    let colproxy = header |> getColumnProxy
                    colproxy.DataElement.Name, fun (value : string) -> 
                        value |> Converter.convert colproxy.ProxyElement.PropertyType
                ) |> Map.ofArray
            | None ->
                NotSupportedException("CSV file requires headers") |> raise

        let colnames = file.Headers |> Option.get
      
        //The CSV reader treats whitespace as significant but this is usually not desired
        //so it is eliminated prior to conversion
        let convert colname value =
            value |> Txt.trim |> converters.[colname]
        
        let createValueMap (row : CsvRow) =
            colnames |> Array.map(fun colname -> colname |> getColumnProxy |> fun f -> f.ProxyElement.Name.Text, colname|> row.GetColumn |> convert colname) 
                     |> ValueIndex.fromNamedItems

        file.Rows |> Seq.map createValueMap 
                  |> Seq.map (fun valueMap -> 
                    match proxy.ProxyElement with
                    |TypeElement(e) -> e |> ClrTypeValue.fromValueIndex valueMap :?> 'T
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
        
                      
                
