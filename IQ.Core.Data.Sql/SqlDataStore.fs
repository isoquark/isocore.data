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
open IQ.Core.Data.Sql.Behavior
                                        
/// <summary>
/// Encapsulates Sql Data Store configuration parameters
/// </summary>
/// <remarks>
/// This should really be defined seperately from the contract because different realizations may
/// require different configuration information
/// </remarks>
type SqlDataStoreConfig = SqlDataStoreConfig of cs : string  
with
    member this.ConnectionString = match this with SqlDataStoreConfig(cs=x) -> x               
            


type internal SqlDataStoreRealization(config : SqlDataStoreConfig) =
    let cs = config.ConnectionString
    let mdp = lazy({ConnectionString = cs; IgnoreSystemObjects = true} |> SqlMetadataProvider.get)
            

    let readTable (q : SqlDataStoreQuery) =
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

    
    interface ITypedSqlDataStore with
        member this.Get q  = 
            match q with
            | DirectStoreQuery(sql) ->
                sql |> TypedReader.selectSome cs 
            | DynamicStoreQuery(x) ->
                cs |> TypedReader.selectAll<'T> 
            | TableFunctionQuery(routine) ->
                nosupport()
            | ProcedureQuery(routine) ->
                nosupport()

        member this.Get q = 
            let q = match q with
                    | DynamicStoreQuery(x) -> DynamicQueryBuilder.WithDefaults(mdp.Value, x)
                    | _ -> q
            q |> readTable
                    
        member this.Get()  =
            cs |> TypedReader.selectAll<'T>
                
            
        member this.Merge items = 
            items |> SqlProxyWriter.bulkInsert cs
            
        member this.Delete q = ()

        member this.Insert (items : 'T seq) =
            items |> SqlProxyWriter.bulkInsert cs
            
        member this.Insert (data : IDataTable) =
            data |> SqlTableWriter.bulkInsert cs

        member this.ExecuteCommand c =
            c |> SqlStoreCommand.execute cs 
            
        member this.GetContract() =
            Routine.getContract<'TContract> cs

        member this.ConnectionString = cs
            
        member this.MetadataProvider = mdp.Value    


/// <summary>
/// Factory that delivers realization of ISqlDataStore
/// </summary>
type TypedSqlDataStore() =                               
    static member Get(config)  =
        SqlDataStoreRealization(config) :> ITypedSqlDataStore
    
    static member Get(cs) =
        SqlDataStoreRealization(SqlDataStoreConfig(cs)) :> ITypedSqlDataStore



