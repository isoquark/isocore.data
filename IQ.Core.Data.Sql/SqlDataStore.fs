namespace IQ.Core.Data.Sql

open System
open System.Data
open System.Text
open System.Data.SqlClient

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
/// Defines the domain vocabulary for reasoning about SQL Server data stores
/// </summary>
[<AutoOpen>]
module SqlDataStoreVocabulary =
    
    type SqlQueryParameter = SqlParameter of name : string * value : obj    

    type SqlQuery =
        | TableOrView of tabularName : string * columnNames : string list
        | TableFunction of  functionName : string * parameters : SqlQueryParameter list
        | Procedure of procedureName : string * parameters : SqlQueryParameter list
    
        
    /// <summary>
    /// Defines the contract for a SQL Server Data Store
    /// </summary>
    type ISqlDataStore =
        /// <summary>
        /// Retrieves an identified collection of data entities from the store
        /// </summary>
        abstract Get:SqlQuery -> 'T list

        /// <summary>
        /// Persists a collection of data entities to the store, inserting or updating as appropriate
        /// </summary>
        abstract Put:'T seq -> unit

        /// <summary>
        /// Deletes and identified collection of data entities from the store
        /// </summary>
        abstract Del:SqlQuery -> unit

        abstract Select:unit->'T list

        
        
       
/// <summary>
/// Provides ISqlDataStore realization
/// </summary>
module SqlDataStore =    
    let internal bcp (cs : SqlConnectionString) (data : 'T seq) =
        use bcp = new SqlBulkCopy(cs.Text)
        use dataTable = data |> DataTable.fromProxyValuesT
        dataTable |> bcp.WriteToServer
    
    
    type private SqlDataStore(cs : ConnectionString) =
        let cs = SqlConnectionString(cs.Components)
        interface ISqlDataStore with
            member this.Get q = 
                match q with
                | TableOrView(tabularName,columnNames) ->
                    []
                | TableFunction(functionName, parameters) ->
                    []
                | Procedure(procedureName, parameters) ->
                    []
            member this.Put items = bcp cs items
            member this.Del q = ()

            member this.Select() = 
                //let ptype = tableproxy<'T>
                []
                
                                             
    /// <summary>
    /// Provides access to an identified data store
    /// </summary>
    /// <param name="cs">Connection string that identifies the data store</param>
    let access (cs) =
        SqlDataStore(cs) :> ISqlDataStore

