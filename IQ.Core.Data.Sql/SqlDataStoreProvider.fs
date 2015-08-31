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
        
/// <summary>
/// Provides ISqlDataStore realization
/// </summary>
module SqlDataStoreProvider =    
                            
    type internal SqlDataStore(config : SqlDataStoreConfig) =
        let cs = config.ConnectionString
        let mp = lazy({ConnectionString = cs; IgnoreSystemObjects = true} |> SqlMetadataProvider.get)
        interface ISqlProxyDataStore with
            member this.Get q  = 
                match q with
                | TabularQuery(x) ->
                    cs |> SqlProxyReader.selectAll<'T> 
                | TableFunctionQuery(functionName, parameters) ->
                    nosupport()
                | ProcedureQuery(procedureName, parameters) ->
                    nosupport()

            member this.GetTabular q =
                match q with
                | TabularQuery(x) ->
                    (x |> SqlTabularReader.selectSome cs) :> ITabularData
                | TableFunctionQuery(functionName, parameters) ->
                    nosupport()
                | ProcedureQuery(procedureName, parameters) ->
                    nosupport()
                    
            member this.Get()  =
                cs |> SqlProxyReader.selectAll<'T>
                
            
            member this.Merge items = 
                items |> SqlProxyWriter.bulkInsert cs
            
            member this.Delete q = ()

            member this.Insert (items : 'T seq) =
                items |> SqlProxyWriter.bulkInsert cs
            
            member this.Insert (data : ITabularData) =
                data |> SqlTableWriter.bulkInsert cs

            member this.ExecuteCommand c =
                c |> SqlStoreCommand.execute cs 
            
            member this.GetContract() =
                Routine.getContract<'TContract> cs

            member this.ConnectionString = cs
            
            member this.MetadataProvider = mp.Value    

    let get(config)  =
        SqlDataStore(config) :> ISqlProxyDataStore



