// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Collections
open System.Reflection
open System.Collections.Generic
open System.Data
open System.Runtime.CompilerServices

open IQ.Core.Data.Behavior

module internal DataMatrixInternals =
    let pivot(rows : IReadOnlyList<obj[]>) =
        let specimen = if rows.Count <> 0 then rows.[0] |> Some else None
        let colcount = 
            match specimen with
            | Some(row) ->
                row.Length
            | None ->
                0
        let arrays = [|for i in 1..colcount -> Array.zeroCreate<obj> rows.Count|]
        for colidx in 0..colcount-1 do
            let colarray = arrays.[colidx]
            for rowidx in 0..rows.Count-1 do
                let value  = rows.[rowidx].[colidx]
                colarray.[rowidx] <-value
        arrays :> IReadOnlyList<obj[]>
            

/// <summary>
/// Represents a rectangular array of data
/// </summary>
/// <remarks>
/// The intent is for this type to serve as a lightweight data table
///</remarks>
type DataMatrix =  DataMatrix of Description : DataMatrixDescription * Rows : obj[] IReadOnlyList
with
    interface IDataMatrix with
        member this.Rows = 
            match this with DataMatrix(Rows=x) -> x
        member this.Columns = 
            match this with DataMatrix(Rows = x) -> x |> DataMatrixInternals.pivot
        member this.Description = 
            match this with DataMatrix(Description=x) -> x
        member this.Item(row,col) = 
            match this with DataMatrix(Rows=x) -> x.[row].[col]

type DataMatrixBuilder(name : DataObjectName) =
    let coldefs =  Dictionary<int, ColumnDescription>()
    let rows = List<obj[]>();
    member this.DefineColumn(column) =
        coldefs.Add(column.Position, column)
        this
    member this.AddRow(row) =
        rows.Add(row);
        this
    member this.Build() =
        let coldata = rows |> DataMatrixInternals.pivot
        let description = DataMatrixDescription(name, coldefs.Values |> Seq.sortBy(fun x -> x.Position) |> List.ofSeq)
        {new IDataMatrix with
            member this.Rows = rows :> IReadOnlyList<obj[]>
            member this.Columns = coldata
            member this.Item(row,col) = rows.[row].[col]
            member this.Description = description
        }



module DataMatrixOps =
    
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
        
        let rows = new List<obj[]>()
        let table = DataMatrix(DataMatrixDescription(d.TableName, d.DataElement.Columns), rows)
           
        let pocoConverter =  PocoConverter.getDefault()
        for value in values do
            let valueidx = value |> pocoConverter.ToValueIndex
            [|for column in columns do 
                yield valueidx.[column.ProxyElement.Name.Text] |> DataTypeConverter.toBclTransportValue column.DataElement.DataType
            |] |> rows.Add
        table :> IDataMatrix
    
    /// <summary>
    /// Creates a collection of proxies from rows in a data table
    /// </summary>
    /// <param name="t">The proxy type</param>
    /// <param name="dataTable">The data table</param>
    let toProxyValues (t : ClrType) (dataTable : IDataMatrix) =
        let pocoConverter =  PocoConverter.getDefault()
        match t with

        | CollectionType(x) ->
            let itemType = Type.GetType(x.ItemType.AssemblyQualifiedName |> Option.get)
            let items = 
                [for row in dataTable.Rows ->
                    pocoConverter.FromValueArray(row, itemType)
                    ]
            items |> CollectionBuilder.create x.Kind itemType :?> IEnumerable
        | _ ->
            let items = 
                [for row in dataTable.Rows ->
                    pocoConverter.FromValueArray(row, t.ReflectedElement.Value)] 
            let itemType = t.ReflectedElement.Value 
            items |> CollectionBuilder.create ClrCollectionKind.GenericList itemType :?> IEnumerable
    
    
    /// <summary>
    /// Creates a collection of proxies from rows in a data matrix
    /// </summary>
    /// <param name="t">The proxy type</param>
    /// <param name="dataTable">The data table</param>
    let toProxyValuesT<'T>  (dataTable : IDataMatrix) =
        let t = ClrMetadata().FindType<'T>()
        dataTable |> toProxyValues t :?> IEnumerable<'T>

    let getUntypedConverter() =
        {new IDataMatrixConverter with
            member this.ToProxyValues t matrix =
                let clrType = ClrMetadata().FindType(t) 
                matrix |> toProxyValues clrType
            member this.FromProxyValues d values =
                fromProxyValues d values
        }


    let getTypedConverter<'T>() =
        {new IDataMatrixConverter<'T> with
            member this.ToProxyValues dataTable =
                dataTable |> toProxyValuesT<'T>
            member this.FromProxyValues values =
                values |> Seq.map(fun x -> x :> obj) |> fromProxyValues (tableproxy<'T> )
        }


    /// <summary>
    /// Adapts a BCL <see cref="DataTable"/> to <see cref="IDataTable"/>
    /// </summary>
    /// <param name="dataTable">The BCL data table to adapt</param>
    let fromDataTable (dataTable : DataTable) =
                
        let rowValues = [|for row in dataTable.Rows -> row.ItemArray|] :> IReadOnlyList<obj[]>
        let description = dataTable |> BclDataTable.describe
        {new IDataMatrix with
            member this.Description = description
            member this.Item(row,col) = dataTable.Rows.[row].[col]
            member this.Rows = rowValues
            member this.Columns = rowValues |> DataMatrixInternals.pivot
        
        }

