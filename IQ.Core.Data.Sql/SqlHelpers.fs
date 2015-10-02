// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
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

