// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Data
open System.Text
open System.Data.SqlClient
open System.Collections.Generic

open IQ.Core.Contracts
open IQ.Core.Framework
                                        
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
            

module SqlDataStore =
    
    type internal SqlDataStoreRealization(config : SqlDataStoreConfig) =
        let cs = config.ConnectionString
        let mdp = {ConnectionString = cs; IgnoreSystemObjects = true} |> SqlMetadataProvider.get
        let sqlService = SqlService.get(cs) 

        let createConnection() =
            cs |> SqlConnection.create

        let executeQueryCommand(command : SqlCommand) =
            use adapter = new SqlDataAdapter(command)
            use table = new DataTable()
            adapter.Fill(table) |> ignore
            let description = table |> BclDataTable.describe
            let rowValues = List<obj[]>();
            for row in table.Rows do
                rowValues.Add(row.ItemArray)
            DataMatrix(description, rowValues) :> IDataMatrix

        let createQueryCommand (q : SqlDataStoreQuery) connection =
            match q with
            | DynamicStoreQuery(q) ->
                //This guarantees that a well-formed query is provided to the formatter
                let q = match DynamicQueryBuilder.WithDefaults(mdp, q) with 
                        | DynamicStoreQuery(q) -> q | _ -> nosupport()
                let sql = q |> SqlFormatter.formatTabularQuery
                use command = new SqlCommand(sql, connection)
                command.CommandType <- CommandType.Text
                command
            | DirectStoreQuery(sql) ->
                use command = new SqlCommand(sql, connection)
                command.CommandType <- CommandType.Text
                command
            | TableFunctionQuery(x) ->
                nosupport()
            | ProcedureQuery(q) ->
                match q with
                | RoutineQuery(routineName, parameters) ->
                    use command = new SqlCommand(routineName, connection)
                    command.CommandType <- CommandType.StoredProcedure
                    parameters |> List.iter(fun parameter -> 
                        match parameter with 
                            | QueryParameter(name,value) ->
                                command.Parameters.AddWithValue(name, value) |> ignore
                    )
                    command
                                    
        let readMatrix (q : SqlDataStoreQuery) =
            use connection = createConnection()
            use command = connection |> createQueryCommand q
            match q with
            | DynamicStoreQuery(q) ->
                let rowValues = command |> sqlService.ExecuteQueryCommand
                let description = mdp.DescribeDataMatrix(q.TabularName)
                DataMatrix(description, rowValues) :> IDataMatrix
            | DirectStoreQuery(sql) ->
                command |> executeQueryCommand
            | TableFunctionQuery(x) ->
                nosupport()
            | ProcedureQuery(q) ->
                match q with
                | RoutineQuery(routineName, parameters) ->
                    command |> executeQueryCommand                    

        let selectFiltered cs (d : ITabularDescription) sql =
            use connection = createConnection()
            use command = new SqlCommand(sql, connection)
            command.CommandType <- CommandType.Text
            command |> sqlService.ExecuteQueryCommand
    
        interface ISqlDataStore with
            member this.SelectMatrix q  = 
                q |> readMatrix 

            member this.InsertMatrix m  =
                m|> SqlMatrixWriter.bulkInsert cs

            member this.MergeMatrix m =
                nosupport()

            member this.ExecuteCommand c =
                c |> sqlService.ExecuteStoreCommand 

            member this.ExecutePureCommand c =
                c |> sqlService.ExecuteStoreCommand |> ignore

            member this.GetCommandContract() =
                sqlService.GetContract<'TContract>()

            member this.GetQueryContract() =
                sqlService.GetContract<'TContract>()
            
            member this.Select q  = 
                use connection = createConnection()
                use command = connection |> createQueryCommand q
                command |> executeQueryCommand
                        |> fun x -> x.Rows
                        |> SqlDataReader.toPocos<'T>

            member this.SelectAll()  =
                cs |> SqlDataReader.selectAll<'T>
                
            
            member this.Merge items = 
                items |> SqlProxyWriter.bulkInsert cs
            

            member this.Insert (items : 'T seq) =
                items |> SqlProxyWriter.bulkInsert cs

            member this.ConnectionString = cs
            
            
    type internal SqlDataStoreProvider () =
        inherit DataStoreProvider<SqlDataStoreQuery>(DataStoreKind.Sql,
            fun cs -> SqlDataStoreRealization(SqlDataStoreConfig(cs)) :> IDataStore<SqlDataStoreQuery>)   

        static member GetProvider() =
            SqlDataStoreProvider() :> IDataStoreProvider
        static member GetStore(cs) =
            SqlDataStoreProvider.GetProvider().GetDataStore(cs)
                         

    let private provider = lazy(SqlDataStoreProvider() :> IDataStoreProvider)

    [<DataStoreProviderFactory(DataStoreKind.Sql)>]
    let getProvider() = provider.Value


