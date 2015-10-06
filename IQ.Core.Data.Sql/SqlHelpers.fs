﻿// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Data
open System.Linq
open System.Data.Linq
open System.Reflection
open System.Text
open System.Data.SqlClient
open System.Diagnostics

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data


            
        


module internal SqlCommand =
    let executeQuery (command : SqlCommand) =
        use reader = command.ExecuteReader()
        let count = reader.FieldCount
        if reader.HasRows then
            [|while reader.Read() do
                let buffer = Array.zeroCreate<obj>(count)
                let valueCount = buffer |> reader.GetValues
                Debug.Assert((valueCount = count), "Column / Value count mismatch")
                yield buffer 
            |] 
        else
            [||] 
        
module internal SqlConnection = 
    let create cs = 
        let connection = new SqlConnection(cs)
        connection.Open() 
        connection


type internal BulkInsertConfig = {
    OverwriteDefaults : bool
    OverwriteIdentity : bool
}

module internal SqlProxyWriter =
    let bulkInsert<'T> cs (items : 'T seq) =
        let config = {
            OverwriteDefaults = true
            OverwriteIdentity = false
        }
   
        let proxy = tableproxy<'T>
        use bcp = new SqlBulkCopy(cs, SqlBulkCopyOptions.CheckConstraints)
        bcp.DestinationTableName <- proxy.DataElement.Name |> SqlFormatter.formatObjectName
        use table = items |> BclDataTable.fromProxyValuesT 
        bcp.WriteToServer(table)

module internal SqlMatrixWriter =
    let bulkInsert cs (data : IDataMatrix) =
        use table = data.Description |> BclDataTable.fromMatrixDescription
        data.Rows |> Seq.iter(fun x ->table.LoadDataRow(x,true) |> ignore)
        use bcp = new SqlBulkCopy(cs, SqlBulkCopyOptions.CheckConstraints)
        bcp.DestinationTableName <- match data.Description with DataMatrixDescription(Name=x) -> x |> SqlFormatter.formatObjectName
        bcp.WriteToServer(table)    

[<AutoOpen>]
module internal Extensisons =
    type DynamicQuery
    with
        member this.ColumnNames = match this with DynamicQuery(ColumnNames = x) -> x
        member this.TabularName = match this with DynamicQuery(TabularName = x) -> x

module internal SqlDataReader =
    let selectFiltered cs (d : ITabularDescription) sql =
        use connection = cs |> SqlConnection.create
        use command = new SqlCommand(sql, connection)
        command.CommandType <- CommandType.Text
        command |> SqlCommand.executeQuery 

    let selectTable cs (d : ITabularDescription) =

        let sql = d |> SqlFormatter.formatTabularSelect
        sql |> selectFiltered cs d 
    
    let toPocos<'T>(data : obj[] seq) =
        let t = typeinfo<'T>
        let itemType = t.ReflectedElement.Value
        let pocoConverter =  PocoConverter.getDefault()
        [for row in data -> 
            pocoConverter.FromValueArray(row, itemType)
        ] 
        |> CollectionBuilder.create ClrCollectionKind.Array itemType :?> 'T seq

    
    let selectAll<'T> cs  =
        let t = typeinfo<'T>
        let description = t |> DataProxyMetadata.describeTableProxy
        description.DataElement |> selectTable cs |> toPocos<'T>

    let selectSome<'T> cs (where : string) =
        let t = typeinfo<'T>
        let description = t |> DataProxyMetadata.describeTableProxy
        let sql = sprintf "%s where %s" (SqlFormatter.formatTabularSelectT<'T>()) where
        sql |> selectFiltered cs description.DataElement  |> toPocos<'T>
