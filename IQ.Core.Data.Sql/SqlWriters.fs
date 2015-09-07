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

type BulkInsertConfig = {
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
        use table = items |> DataTable.fromProxyValuesT 
        bcp.WriteToServer(table)

module internal SqlTableWriter =
    let bulkInsert cs (data : IDataTable) =
        use table = data.Description |> DataTable.fromTabularDescription
        data.RowValues |> Seq.iter(fun x ->table.LoadDataRow(x,true) |> ignore)
        use bcp = new SqlBulkCopy(cs, SqlBulkCopyOptions.CheckConstraints)
        bcp.DestinationTableName <- data.Description.ObjectName |> SqlFormatter.formatObjectName
        bcp.WriteToServer(table)    


