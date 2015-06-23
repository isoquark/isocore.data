namespace IQ.Core.Data

open IQ.Core.Framework

open System
open System.Reflection
open System.Data
open System.Diagnostics
open System.Collections
open System.Collections.Generic



module DataTypeConverter =
    let private stripOption (o : obj) =
        if o = null then
            o
        else if o |> ClrOption.isOptionValue then
            match o |> ClrOption.unwrapValue with
            | Some(x) -> x 
            | None -> DBNull.Value :> obj
        else
             o
    


    let toClrStorageValue storageType  (value : obj) =
        let value = value |> stripOption
        if value = null then
            DBNull.Value :> obj
        else
            let clrType = storageType |> StorageType.toClrType
            value |> Converter.convert clrType
    
    let toClrStorageType storageType =
        storageType |> StorageType.toClrType
            
            
    
                  
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
        description.Columns |> List.iter(fun c -> table.Columns.Add(c.DataElement.Name, c.ProxyElement.PropertyType) |> ignore)
        table

    /// <summary>
    /// Creates a data table based on a tabular description
    /// </summary>
    /// <param name="d">The tabular description</param>
    let fromTabularDescription(d : TabularDescription) =
        let table = new DataTable(d.Name.ToSemanticString())
        d.Columns |> List.iter(fun c -> table.Columns.Add(c.Name, c.StorageType |> DataTypeConverter.toClrStorageType) |> ignore)
        table

    let fromProxyType<'T> =
        tabularproxy<'T> |> TabularProxy

    /// <summary>
    /// Creates a <see cref="System.Data.DataTable"/> from a sequence of proxy values
    /// </summary>
    /// <param name="proxyDescription">Description of the proxy</param>
    /// <param name="values">The record values that will be transformed into table rows</param>
    let fromProxyValues (d : TabularProxyDescription) (values : obj seq) =
        let excludeDefaults = true
        let excludeIdentity = false
        let columns = [for c in d.Columns do 
                            match c.DataElement.AutoValue with
                            | Some(autoval) ->
                                match autoval with
                                | AutoValueKind.Default ->
                                    if excludeDefaults |> not then
                                        yield c
                                | AutoValueKind.Identity ->
                                    if excludeIdentity |> not then
                                        yield c
                                | AutoValueKind.None ->
                                    yield c
                                | AutoValueKind.Sequence ->
                                    yield c
                                | _ -> ()
                            | None ->
                                    yield c
                        ]
        
        let table = d.DataElement |> fromTabularDescription
           
        for value in values do
            let valueidx = value |>ClrTypeValue.toValueIndex
            [|for column in columns do 
                yield valueidx.[column.ProxyElement.Name.Text] |> DataTypeConverter.toClrStorageValue column.DataElement.StorageType
            |] |> table.Rows.Add |> ignore
        table                

    /// <summary>
    /// Creates a <see cref="System.Data.DataTable"/> from a sequence of records
    /// </summary>
    /// <param name="values">The record values that will be transformed into table rows</param>
    let fromProxyValuesT (values : 'T seq) =
         values |> Seq.map(fun x -> x :> obj) |> fromProxyValues (tabularproxy<'T> )
                
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

 
    