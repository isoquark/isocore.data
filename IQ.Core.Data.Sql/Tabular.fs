namespace IQ.Core.Data.Sql

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection
open System.Diagnostics

open IQ.Core.Data
open IQ.Core.Framework

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

    let executeProxyQuery cs (tref : ClrTypeReference) =
        let data =
            match tref |> DataProxyMetadata.describeTablularProxy with
            | TabularProxy(proxy) -> proxy.DataElement |> executeQuery cs
            | _ -> nosupport()

        let items = [for row in data -> tref |> ClrTypeValue.fromValueArray row]
        items |> ClrCollection.create ClrCollectionKind.FSharpList tref.Type


