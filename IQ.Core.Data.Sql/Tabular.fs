// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection
open System.Diagnostics
open System.Collections.Generic;

open IQ.Core.Data
open IQ.Core.Framework

type BulkInsertConfig = {
    OverwriteDefaults : bool
    OverwriteIdentity : bool
}

/// <summary>
/// Provides capability to execute tabular queries/inserts
/// </summary>
module internal Tabular = 
    let executeQuery cs (tabular : TabularDescription) =
        let sql = tabular |> SqlFormatter.formatTabularSelect
        use connection = cs |> SqlConnection.create
        use command = new SqlCommand(sql, connection)
        command.CommandType <- CommandType.Text
        command |> SqlCommand.executeQuery tabular.Columns

    let executeProxyQuery<'T> cs (tdesc : ClrType)  =
        let proxy = tdesc |> DataProxyMetadata.describeTablularProxy
        let data = proxy.DataElement |> executeQuery cs
        let itemType = tdesc.ReflectedElement.Value
        let pocoConverter =  PocoConverter.getDefault()
        let items = [for row in data -> pocoConverter.FromValueArray(row, itemType)]            
        items |> Collection.create ClrCollectionKind.FSharpList itemType :?> list<'T>

   

    let bulkInsert<'T> cs (items : 'T seq) =
        let config = {
            OverwriteDefaults = true
            OverwriteIdentity = false
        }
   
        let proxy = tabularproxy<'T>
        use bcp = new SqlBulkCopy(cs, SqlBulkCopyOptions.CheckConstraints)
        bcp.DestinationTableName <- proxy.DataElement.Name |> SqlFormatter.formatObjectName
        use table = items |> DataTable.fromProxyValuesT 
        bcp.WriteToServer(table)

