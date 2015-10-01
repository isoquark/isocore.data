// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Behavior

open System
open System.Collections
open System.Reflection
open System.Collections.Generic

module DataMatrix =
    
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
        let t = ClrMetadata().FindType(typeof<'T>.TypeName)
        dataTable |> toProxyValues t :?> IEnumerable<'T>

    let getUntypedConverter() =
        {new IDataMatrixConverter with
            member this.ToProxyValues t matrix =                
                let clrType = ClrMetadata().FindType(t.TypeName) 
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


