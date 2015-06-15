namespace IQ.Core.Data

open IQ.Core.Framework

open System
open System.Reflection
open System.Data
open System.Diagnostics


/// <summary>
/// Defines operations for working with Data Tables
/// </summary>
module DataTable =        
    
    /// <summary>
    /// Gets a value identified by a 0-based row/column indices
    /// </summary>
    /// <param name="row">The row index</param>
    /// <param name="col">The column index</param>
    /// <param name="dataTable"></param>
    let getValue<'T> row (col : int) (dataTable : DataTable) =
        dataTable.Rows.[row].[col] :?> 'T

    /// <summary>
    /// Creates a data table based on a record description
    /// </summary>
    /// <param name="description">The record description</param>
    let fromRecordDescription (description : RecordDescription) =
        let table = new DataTable(description.Name)
        description.Fields |> List.iter(fun f -> table.Columns.Add(f.Name, f.FieldType) |> ignore)
        table

    /// <summary>
    /// Creates a data table based on record type
    /// </summary>
    let fromRecordType<'T> =
        recordinfo<'T> |> fromRecordDescription


    /// <summary>
    /// Creates a <see cref="System.Data.DataTable"/> from a sequence of records
    /// </summary>
    /// <param name="values">The record values that will be transformed into table rows</param>
    let fromRecordValues (values : 'T seq) =
        let valueList = values |> List.ofSeq
        if valueList.IsEmpty then
            ArgumentException("Cannot created a DataTable from an empty list of records") |> raise
        let recordType = valueList.Head.GetType()
        let table = recordType |> ClrRecord.describe |> fromRecordDescription
        valueList |> List.iter(fun item ->
            item |> ClrRecord.toValueArray |> table.Rows.Add |> ignore                        
        )
        table
            
    /// <summary>
    /// Creates a list of records from a data table
    /// </summary>
    /// <param name="description">Describes the record</param>
    /// <param name="dataTable">The data table</param>
    let toRecordValues (description : RecordDescription) (dataTable : DataTable) =
        [for row in dataTable.Rows ->
            description |> ClrRecord.fromValueArray row.ItemArray
        ]


 
    