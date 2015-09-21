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
    let mdp = {ConnectionString = cs; IgnoreSystemObjects = true} |> SqlMetadataProvider.get
    
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
            let rowValues = command |> SqlCommand.executeQuery                        
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
        command |> SqlCommand.executeQuery

    interface ISqlDataStore with
        member this.GetMatrix q = 
            q |> readMatrix

        member this.InsertMatrix (data : IDataMatrix) =
            data |> SqlTableWriter.bulkInsert cs

        member this.Delete q =
            nosupport()

        member this.ExecuteCommand c =
            c |> SqlStoreCommand.execute cs 

        member this.GetContract() =
            Routine.getContract<'TContract> cs

        member this.ConnectionString = cs
            
        member this.MetadataProvider = mdp

        member this.Get q  = 
            use connection = createConnection()
            use command = connection |> createQueryCommand q
            command |> executeQueryCommand
                    |> fun x -> x.RowValues
                    |> SqlDataReader.toPocos<'T>

        member this.Get()  =
            cs |> SqlDataReader.selectAll<'T>
                
            
        member this.Merge items = 
            items |> SqlProxyWriter.bulkInsert cs
            

        member this.Insert (items : 'T seq) =
            items |> SqlProxyWriter.bulkInsert cs
            

            


/// <summary>
/// Factory that delivers realization of ISqlDataStore
/// </summary>
type SqlDataStore() =                               
    static member Get(config)  =
        SqlDataStoreRealization(config) :> ISqlDataStore
    
    static member Get(cs) =
        SqlDataStoreRealization(SqlDataStoreConfig(cs)) :> ISqlDataStore



