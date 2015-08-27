// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql

open System
open System.Data
open System.Text
open System.Data.SqlClient
open System.Collections.Generic

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data

module internal SqlTabularReader =
    let selectSome cs (q : TabularDataQuery) =
        let sql = q |> SqlFormatter.formatTabularQuery
        use connection = cs |> SqlConnection.create
        use command = new SqlCommand(sql, connection)
        command.CommandType <- CommandType.Text
        let rowValues = command |> SqlCommand.executeQuery (q.ColumnNames) 
        let description = {
            TabularDescription.Name = q.TabularName
            Documentation = String.Empty
            Columns = []                       
        }
        TabularData(description, rowValues)

    let selectFromSql cs (d : TabularDescription) sql =
        use connection = cs |> SqlConnection.create
        use command = new SqlCommand(sql, connection)
        command.CommandType <- CommandType.Text
        command |> SqlCommand.executeQuery (d.Columns |> List.map(fun c -> c.Name) |> List.asReadOnlyList)
            
    let selectAll cs (d : TabularDescription) =
        let sql = d |> SqlFormatter.formatTabularSelect
        sql |> selectFromSql cs d 
    
module internal SqlProxyReader =    
    
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
        let description = t |> DataProxyMetadata.describeTablularProxy
        description.DataElement |> SqlTabularReader.selectAll cs |> toPocos<'T>

    let selectSome<'T> cs (where : string) =
        let t = typeinfo<'T>
        let description = t |> DataProxyMetadata.describeTablularProxy
        let sql = sprintf "%s where %s" (SqlFormatter.formatTabularSelectT<'T>()) where
        sql |> SqlTabularReader.selectFromSql cs description.DataElement  |> toPocos<'T>
              