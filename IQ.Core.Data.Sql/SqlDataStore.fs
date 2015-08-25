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
                                        
        
type SqlDataStoreConfig = SqlDataStoreConfig of cs : ConnectionString * clrMetadataProvider : IClrMetadataProvider
with
    member this.ConnectionString = match this with SqlDataStoreConfig(cs=x) -> x
    member this.ClrMetadataProvider = match this with SqlDataStoreConfig(clrMetadataProvider=x) ->x
        
       
/// <summary>
/// Provides ISqlDataStore realization
/// </summary>
module SqlDataStore =    
     
        
    type private MetadataProvider(config : SqlDataStoreConfig) =
        let cs = config.ConnectionString.Text
        interface ISqlMetadataProvider with
            member this.Describe q = 
                match q with
                | FindTables(q) ->
                    match q  with
                    | FindAllTables ->
                        let vTables = cs |> SqlProxyReader.selectAll<Metadata.vTable>  
                        [for vTable in vTables ->
                            {TabularDescription.Name = DataObjectName(vTable.SchemaName, vTable.TableName)
                             Documentation = vTable.Description
                             Columns = []
                            } |> TableDescription
                        ]
                    | FindUserTables ->
                        nosupport()
                    | FindTablesBySchema(schemaName) ->
                        nosupport()
                | FindSchemas(q) -> 
                    match q with
                    | FindAllSchemas ->
                        nosupport() 
                | FindViews(q) -> 
                    match q with
                    | FindAllViews -> 
                        nosupport()
                    | FindUserViews ->
                        nosupport()
                    | FindViewsBySchema(schemaName) ->
                        nosupport()
                | FindProcedures(q) -> 
                    match q with
                    | FindAllProcedures -> 
                        nosupport()
                    | FindProceduresBySchema(schemaName) ->
                        nosupport()
                | FindSequences(q) -> 
                    match q with
                    | FindAllSequences -> 
                        nosupport()
                    | FindSequencesBySchema(schemaName) ->
                        nosupport()

                

    type internal Realization(config : SqlDataStoreConfig) =
        let cs = SqlConnectionString(config.ConnectionString.Components) |> fun x -> x.Text
        let mp = config |> MetadataProvider :> ISqlMetadataProvider
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
            
            member this.MetadataProvider = mp    
                                             
    /// <summary>
    /// Provides access to an identified data store
    /// </summary>
    /// <param name="cs">Connection string that identifies the data store</param>
    let get (config) =
        Realization(config) :> ISqlProxyDataStore



