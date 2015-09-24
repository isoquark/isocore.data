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


              