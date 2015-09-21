// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Behavior


open IQ.Core.Framework

open System
open System.Reflection
open System.Data
open System.Diagnostics
open System.Collections
open System.Collections.Generic
open System.Linq

open IQ.Core.Data.Contracts


type IDataTableConverter =
    abstract FromProxyValues: TableProxyDescription->values : obj seq -> DataTable
    abstract ToProxyValues: Type-> DataTable->IEnumerable

type IDataTableConverter<'T> =
    abstract FromProxyValues: TableProxyDescription->values : 'T seq -> DataTable
    abstract ToProxyValues: DataTable->'T seq


/// <summary>
/// Defines operations for working with Data Tables
/// </summary>
module BclDataTable =            
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
        let table = new DataTable(description.DataElement.Name.Text)
        description.Columns |> List.iter(fun c -> table.Columns.Add(c.DataElement.Name, c.ProxyElement.ReflectedElement.Value.PropertyType) |> ignore)
        table

    /// <summary>
    /// Creates a data table based on a tabular description
    /// </summary>
    /// <param name="d">The tabular description</param>
    let fromMatrixDescription(d : DataMatrixDescription) =
        match d with 
            | DataMatrixDescription(name, columns) ->
                let table = new DataTable(name.Text)
                columns |> Seq.iter(fun c -> table.Columns.Add(c.Name, c.DataType |> DataTypeConverter.toBclTransportType) |> ignore)
                table

    /// <summary>
    /// Creates a <see cref="System.Data.DataTable"/> from a sequence of proxy values
    /// </summary>
    /// <param name="proxyDescription">Description of the proxy</param>
    /// <param name="values">The record values that will be transformed into table rows</param>
    let fromProxyValues (d : TableProxyDescription) (values : obj seq) =
        let excludeDefaults = true
        let excludeAutoIncrement = false
        let columns = [for c in d.Columns do 
                            match c.DataElement.AutoValue with
                                | AutoValueKind.Default ->
                                    if excludeDefaults |> not then
                                        yield c
                                | AutoValueKind.AutoIncrement ->
                                    if excludeAutoIncrement |> not then
                                        yield c
                                | AutoValueKind.None ->
                                    yield c
                                | _ -> ()
                        ]
        
        let table = DataMatrixDescription(d.TableName, d.DataElement.Columns)|> fromMatrixDescription
           
        let pocoConverter =  PocoConverter.getDefault()
        for value in values do
            let valueidx = value |> pocoConverter.ToValueIndex
            [|for column in columns do 
                yield valueidx.[column.ProxyElement.Name.Text] |> DataTypeConverter.toBclTransportValue column.DataElement.DataType
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
         values |> Seq.map(fun x -> x :> obj) |> fromProxyValues (tableproxy<'T> )


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
                values |> Seq.map(fun x -> x :> obj) |> fromProxyValues (tableproxy<'T> )    
        }

    let describe(dataTable : DataTable) =
        let tableName = DataObjectName.fuzzyParse(dataTable.TableName)
        let columns = dataTable.Columns.Cast<DataColumn>() 
                    |> List.ofSeq
                    |> List.map(fun c ->
                        let dataKind = c.DataType |> DataProxyMetadata.inferKindTypefromClrType
                        {
                            ColumnDescription.Name = c.ColumnName
                            ColumnDescription.Position = c.Ordinal
                            ColumnDescription.Nullable = c.AllowDBNull
                            ColumnDescription.AutoValue = if c.AutoIncrement then AutoValueKind.AutoIncrement else AutoValueKind.None
                            ColumnDescription.Documentation = String.Empty
                            ParentName = tableName
                            DataType = dataKind |> DataProxyMetadata.getDefaultTypeFromKind
                            DataKind = dataKind
                            Properties = []
                        }                                        
                    )
        DataMatrixDescription(tableName, columns)

    /// <summary>
    /// Adapts a BCL <see cref="DataTable"/> to <see cref="IDataTable"/>
    /// </summary>
    /// <param name="dataTable">The BCL data table to adapt</param>
    let asDataTable (dataTable : DataTable) =        
                
        let rowValues = [|for row in dataTable.Rows -> row.ItemArray|] :> IReadOnlyList<obj[]>
        let description = dataTable |> describe
        {new IDataMatrix with
            member this.Description = description
            member this.Item(row,col) = dataTable.Rows.[row].[col]
            member this.RowValues = rowValues
        
        }