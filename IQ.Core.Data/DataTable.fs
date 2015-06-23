namespace IQ.Core.Data

open IQ.Core.Framework

open System
open System.Reflection
open System.Data
open System.Diagnostics
open System.Collections
open System.Collections.Generic


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
    /// Creates a data table based on a proxy description
    /// </summary>
    /// <param name="description">The proxy description</param>
    let fromProxyDescription (description : DataObjectProxy) =        
        let table = new DataTable(description.DataElement.Name.ToSemanticString())
        description.Columns |> List.iter(fun f -> table.Columns.Add(f.DataElement.Name, f.ProxyElement.PropertyType) |> ignore)
        table

    let fromProxyType<'T> =
        tabularproxy<'T> |> fromProxyDescription

    /// <summary>
    /// Creates a <see cref="System.Data.DataTable"/> from a sequence of proxy records
    /// </summary>
    /// <param name="proxyDescription">Description of the proxy</param>
    /// <param name="values">The record values that will be transformed into table rows</param>
    let fromProxyValues proxyDescription  (values : obj seq) =
        let valueList = values |> List.ofSeq
        if valueList.IsEmpty then
            ArgumentException("Cannot create a DataTable from an empty list of records") |> raise
        let table = proxyDescription |> fromProxyDescription
        valueList |> List.iter(fun item ->
            item |> ClrTypeValue.toValueArray |> table.Rows.Add |> ignore                        
        )
        table


    /// <summary>
    /// Creates a <see cref="System.Data.DataTable"/> from a sequence of records
    /// </summary>
    /// <param name="values">The record values that will be transformed into table rows</param>
    let fromProxyValuesT (values : 'T seq) =
         values |> Seq.map(fun x -> x :> obj) |> fromProxyValues tabularproxy<'T>
        
                
    /// <summary>
    /// Creates a list of records from a data table
    /// </summary>
    /// <param name="description">Describes the record</param>
    /// <param name="dataTable">The data table</param>
    let toProxyValues (typeref : ClrTypeReference) (dataTable : DataTable) =
        match typeref with
        | CollectionTypeReference(subject, itemType, collectionKind) ->            
            let items = 
                [for row in dataTable.Rows ->
                    itemType |> ClrTypeValue.fromValueArray row.ItemArray]
            items |> ClrCollection.create collectionKind itemType.Type :?> IEnumerable
        | _ ->
            [for row in dataTable.Rows ->
                typeref |> ClrTypeValue.fromValueArray row.ItemArray] :> IEnumerable

    let toProxyValuesT<'T> (typeref : ClrTypeReference) (dataTable : DataTable) =
        dataTable |> toProxyValues typeref :?> IEnumerable<'T>

 
    