namespace IQ.Core.Data.Sql

open System
open System.Data
open System.Text
open System.Data.SqlClient

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
                                        
        
        
module internal SqlStoreCommand =
    let execute cs command =
        let sql = 
            match command with
            | TruncateTable tableName ->
                tableName |> SqlFormatter.formatTruncateTable   
        use connection = cs |> SqlConnection.create
        use sqlcommand = new SqlCommand(sql, connection)
        sqlcommand.ExecuteNonQuery() |> ignore




 
       
/// <summary>
/// Provides ISqlDataStore realization
/// </summary>
module SqlDataStore =    
        
    type internal Realization(csSqlDataStore : ConnectionString) =
        let cs = SqlConnectionString(csSqlDataStore.Components) |> fun x -> x.Text
        interface ISqlDataStore with
            member this.Get q : list<'T>= 
                match q with
                | TabularQuery(tabularName,columnNames) ->
                    typeinfo<'T> |> Tabular.executeProxyQuery cs :?> list<'T>
                | TableFunctionQuery(functionName, parameters) ->
                    []
                | ProcedureQuery(procedureName, parameters) ->
                    []
            
            member this.Get() : list<'T> =
                typeinfo<'T> |> Tabular.executeProxyQuery cs :?> list<'T>
                
            
            member this.Put items = 
                items |> Tabular.bulkInsert cs
            
            member this.Del q = ()

            member this.BulkInsert (items : 'T seq) =
                items |> Tabular.bulkInsert cs

            member this.ExecuteCommand c =
                c |> SqlStoreCommand.execute cs
            
            member this.GetContract() =
                Routine.getContract<'TContract> cs

            member this.ConnectionString = cs
                
                                             
    /// <summary>
    /// Provides access to an identified data store
    /// </summary>
    /// <param name="cs">Connection string that identifies the data store</param>
    let access (cs) =
        Realization(cs) :> ISqlDataStore



