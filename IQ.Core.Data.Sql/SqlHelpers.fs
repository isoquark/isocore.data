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
open System.Collections.Generic

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data


            
type internal SqlCommandFactory(cmdText, connection) =
    let mutable timeout = 1000
    let mutable isProcedure = false

    member this.Build() = 
        let command = new SqlCommand(cmdText, connection) 
        command.CommandType <- if isProcedure then CommandType.StoredProcedure else CommandType.Text
        command.CommandTimeout <- timeout
        command

    member this.WithTimeout(value) = 
        timeout <- value
        this
    member this.Procedure(value) = 
        isProcedure <- value
        this
                


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
        data.Description.Columns |> List.iter(fun col ->
            bcp.ColumnMappings.Add( SqlBulkCopyColumnMapping(col.Name, col.Name)) |> ignore
        )
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
        use command = SqlCommandFactory(sql, connection).Build()
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

module ConnectionString =
    let decode(cs) =
       [for c in cs |> Txt.split ";" do
            let param = c |> Txt.split "="
            if param.Length = 2 then
                yield param.[0], param.[1]
       ] |> dict

    let encode(parameters : IDictionary<string,string>) =
        let sb = StringBuilder();
        parameters |> Seq.iter(fun item ->
            sb.AppendFormat("{0}={1};", item.Key, item.Value) |> ignore
        )
        sb.ToString()
    
    let setParameter name value cs =
        let parameters = cs |> decode
        parameters.[name] <- value
        parameters |> encode

type SqlConnectionString(value) =
    let parameters = value |> ConnectionString.decode
    
    member this.Value = parameters |> ConnectionString.encode

    override this.ToString() =
        this.Value

    member this.SetCommandTimeout(timeout : int) =
        parameters.["CommandTimeout"] <- sprintf "%i" timeout

    member this.CommandTimeout = 
        if parameters.ContainsKey("CommandTimeout") then
            parameters.["CommandTimeout"] |> Int32.Parse
        else
            0