namespace IQ.Core.Data

open System
open System.IO

open FSharp.Data

open IQ.Core.Framework


[<AutoOpen>]
module CsvReaderVocabulary =
    type CsvFormat = {
        Separators : string
        Quote : char
        HasHeaders : bool
    }



/// <summary>
/// Defines operations for working with delimited text data
/// </summary>
module CsvReader =
    
    let private loadFromFile(format : CsvFormat) (path: string) =
        CsvFile.Load(path, format.Separators, format.Quote, format.HasHeaders, false).Cache()
    
    let getDefaultFormat() = {
        Separators = ","
        Quote = '"'
        HasHeaders = true
    }

    let readText<'T>(format : CsvFormat) (text : string) =
        let record = recordinfo<'T>
        use reader = new StringReader(text)
        use file = CsvFile.Load(reader, format.Separators, format.Quote, format.HasHeaders, false).Cache()

        let converters =
            match file.Headers with
            | Some(headers) ->
                headers |> Array.map(fun header -> 
                    let field = record.FindField(header)
                    field.Name, fun (value : string) -> 
                        Convert.ChangeType(value, field.FieldType)
                ) |> Map.ofArray
            | None ->
                NotSupportedException("CSV file requires headers") |> raise

        let colnames = file.Headers |> Option.get

        let convert fieldName value =
            value |> converters.[fieldName]

        let createValueMap (row : CsvRow) =
            row.Columns |> Array.mapi(fun i col -> (col, row.[i] |> convert col )) |> Map.ofArray

        let createValueMap2 (row : CsvRow) =
            colnames |> Array.map(fun colname -> colname, colname|> row.GetColumn |> convert colname) |> Map.ofArray

        file.Rows |> Seq.map createValueMap2 
                  |> Seq.map (fun valueMap -> record |> ClrRecord.fromValueMap valueMap :?> 'T)
                  |> List.ofSeq
        

                      
                
