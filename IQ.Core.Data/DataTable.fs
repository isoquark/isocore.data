// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data


open IQ.Core.Framework

open System
open System.Reflection
open System.Data
open System.Diagnostics
open System.Collections
open System.Collections.Generic



type IDataTableConverter =
    abstract FromProxyValues: TabularProxyDescription->values : obj seq -> DataTable
    abstract ToProxyValues: Type-> DataTable->IEnumerable

type IDataTableConverter<'T> =
    abstract FromProxyValues: TabularProxyDescription->values : 'T seq -> DataTable
    abstract ToProxyValues: DataTable->'T seq

type IDataTableStore =
    abstract Merge:DataTable->unit
    abstract Delete:'Q->unit
    abstract Select: 'Q->DataTable
    abstract Insert:DataTable->unit

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
        description.Columns |> List.iter(fun c -> table.Columns.Add(c.DataElement.Name, c.ProxyElement.ReflectedElement.Value.PropertyType) |> ignore)
        table

    /// <summary>
    /// Creates a data table based on a tabular description
    /// </summary>
    /// <param name="d">The tabular description</param>
    let fromTabularDescription(d : TabularDescription) =
        let table = new DataTable(d.Name.ToSemanticString())
        d.Columns |> List.iter(fun c -> table.Columns.Add(c.Name, c.StorageType |> DataTypeConverter.toBclTransportType) |> ignore)
        table

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
           
        let pocoConverter =  PocoConverter.getDefault()
        for value in values do
            let valueidx = value |> pocoConverter.ToValueIndex
            [|for column in columns do 
                yield valueidx.[column.ProxyElement.Name.Text] |> DataTypeConverter.toBclTransportValue column.DataElement.StorageType
            |] |> table.Rows.Add |> ignore
        table                
                

    /// <summary>
    /// Creates a collection of proxies from rows in a data table
    /// </summary>
    /// <param name="t">The proxy type</param>
    /// <param name="dataTable">The data table</param>
    let toProxyValues (t : ClrType) (dataTable : DataTable) =
        let pocoConverter =  PocoConverter.getDefault()
        match t with
        | CollectionType(x) ->
            let items = 
                [for row in dataTable.Rows ->
                    pocoConverter.FromValueArray(row.ItemArray, t.ReflectedElement.Value)
                    ]
            let itemType = t.ReflectedElement.Value 
            items |> Collection.create x.Kind itemType :?> IEnumerable
        | _ ->
            let items = 
                [for row in dataTable.Rows ->                
                    pocoConverter.FromValueArray(row.ItemArray, t.ReflectedElement.Value)] 
            let itemType = t.ReflectedElement.Value 
            items |> Collection.create ClrCollectionKind.GenericList itemType :?> IEnumerable


    /// <summary>
    /// Creates a collection of proxies from rows in a data table
    /// </summary>
    /// <param name="t">The proxy type</param>
    /// <param name="dataTable">The data table</param>
    let toProxyValuesT<'T>  (dataTable : DataTable) =        
        let t = ClrMetadataProvider.getDefault().FindType(typeof<'T>.TypeName)
        dataTable |> toProxyValues t :?> IEnumerable<'T>

 
    /// <summary>
    /// Creates a <see cref="System.Data.DataTable"/> from a sequence of records
    /// </summary>
    /// <param name="values">The record values that will be transformed into table rows</param>
    let fromProxyValuesT (values : 'T seq) =
         values |> Seq.map(fun x -> x :> obj) |> fromProxyValues (tabularproxy<'T> )


    let getUntypedConverter() =
        {new IDataTableConverter with
            member this.ToProxyValues t dataTable =
                let clrType = ClrMetadataProvider.getDefault().FindType(t.TypeName) 
                dataTable |> toProxyValues clrType
            member this.FromProxyValues d values =
                fromProxyValues d values        
        }


    let getTypedConverter<'T>() =
        {new IDataTableConverter<'T> with
            member this.ToProxyValues dataTable =
                dataTable |> toProxyValuesT<'T>
            member this.FromProxyValues d values =
                values |> Seq.map(fun x -> x :> obj) |> fromProxyValues (tabularproxy<'T> )    
        }
            