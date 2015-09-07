// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql.Behavior

open System
open System.Data
open System.Text
open System.Data.SqlClient
open System.Collections.Generic

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data

[<AutoOpen>]
module internal Extensisons =
    type DynamicQuery
    with
        member this.ColumnNames = match this with DynamicQuery(ColumnNames = x) -> x
        member this.TabularName = match this with DynamicQuery(TabularName = x) -> x

module internal UntypedReader =
    let select cs (q : SqlDataStoreQuery) =
        match q with
        | DynamicStoreQuery(q) ->
            let sql = q |> SqlFormatter.formatTabularQuery
            use connection = cs |> SqlConnection.create
            use command = new SqlCommand(sql, connection)
            command.CommandType <- CommandType.Text
            let rowValues = command |> SqlCommand.executeQuery (q.ColumnNames) 
            let description = {
                TableDescription.Name = q.TabularName
                Documentation = String.Empty
                Columns = []
                Properties = []
            }
            TabularData(description, rowValues)
        | DirectStoreQuery(sql) ->
            use connection = cs |> SqlConnection.create
            use command = new SqlCommand(sql, connection)
            command.CommandType <- CommandType.Text
            use adapter = new SqlDataAdapter(command)
            use table = new DataTable()
            adapter.Fill(table) |> ignore
            let description = {
                TableDescription.Name = DataObjectName(String.Empty, String.Empty)
                Documentation = String.Empty
                Columns = []
                Properties = []
            }
            let rowValues = List<obj[]>();
            for row in table.Rows do
                rowValues.Add(row.ItemArray)
            TabularData(description, rowValues)

        | TableFunctionQuery(x) ->
            nosupport()
        | ProcedureQuery(x) ->
            nosupport()
        :> IDataTable

            
    let selectFiltered cs (d : ITabularDescription) sql =
        use connection = cs |> SqlConnection.create
        use command = new SqlCommand(sql, connection)
        command.CommandType <- CommandType.Text
        command |> SqlCommand.executeQuery (d.Columns |> RoList.map(fun c -> c.Name) )

    let selectAll cs (d : ITabularDescription) =

        let sql = d |> SqlFormatter.formatTabularSelect
        sql |> selectFiltered cs d 
    
module internal TypedReader =    
    
    let private toPocos<'T>(data : obj[] rolist) =
        let t = typeinfo<'T>
        let itemType = t.ReflectedElement.Value
        let pocoConverter =  PocoConverter.getDefault()
        [for row in data -> 
            pocoConverter.FromValueArray(row, itemType)
        ] 
        |> Collection.create ClrCollectionKind.Array itemType :?> rolist<'T> 
            
    let selectAll<'T> cs  =
        let t = typeinfo<'T>
        let description = t |> DataProxyMetadata.describeTableProxy
        description.DataElement |> UntypedReader.selectAll cs |> toPocos<'T>

    let selectSome<'T> cs (where : string) =
        let t = typeinfo<'T>
        let description = t |> DataProxyMetadata.describeTableProxy
        let sql = sprintf "%s where %s" (SqlFormatter.formatTabularSelectT<'T>()) where
        sql |> UntypedReader.selectFiltered cs description.DataElement  |> toPocos<'T>
              