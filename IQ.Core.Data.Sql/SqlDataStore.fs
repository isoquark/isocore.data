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


/// <summary>
/// Represents a SQL Server connection string
/// </summary>
type internal SqlConnectionString = | SqlConnectionString of components : string list
with
    /// <summary>
    /// Specifies the connection string components
    /// </summary>
    member this.Components = match this with |SqlConnectionString(components) -> components
    
    /// <summary>
    /// Renders the connection string
    /// </summary>
    member this.Text =
        let sb = StringBuilder()
        for i in [0..this.Components.Length-1] do
            sb.Append(this.Components.[i]) |> ignore
            if i <> this.Components.Length - 1 then
                sb.Append(';') |> ignore
        sb.ToString()
                                        
        
        
       
/// <summary>
/// Provides ISqlDataStore realization
/// </summary>
module SqlDataStore =    
        

    type internal Realization(config : SqlDataStoreConfig) =
        let cs = SqlConnectionString(config.ConnectionString.Components) |> fun x -> x.Text
        let mp = config |> SqlMetadataProvider.get
        interface ISqlDataStore with
            member this.Get q : list<'T> = 
                match q with
                | TabularQuery(tabularName,columnNames) ->
                    typeinfo<'T> |> Tabular.executeProxyQuery cs 
                | TableFunctionQuery(functionName, parameters) ->
                    []
                | ProcedureQuery(procedureName, parameters) ->
                    []
            
            member this.Get() : list<'T> =
                typeinfo<'T> |> Tabular.executeProxyQuery cs 
                
            
            member this.Put items = 
                items |> Tabular.bulkInsert cs
            
            member this.Del q = ()

            member this.Insert (items : 'T seq) =
                items |> Tabular.bulkInsert cs

            member this.ExecuteCommand c =
                c |> SqlStoreCommand.execute cs 
            
            member this.GetContract() =
                Routine.getContract<'TContract> cs

            member this.ConnectionString = cs
            
            member this.MetadataProvider = mp    
                                             
    /// <summary>
    /// Provides access to an identified data store
    /// </summary>
    /// <param name="cs">Connection string that identifies the data store</param>
    let get (config) =
        Realization(config) :> ISqlDataStore



