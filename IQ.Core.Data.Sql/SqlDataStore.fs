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
    
    type SqlQueryParameter = SqlQueryParameter of name : string * value : obj    

    type SqlQuery =
        | TabularQuery of tabularName : string * columnNames : string list
        | TableFunctionQuery of  functionName : string * parameters : SqlQueryParameter list
        | ProcedureQuery of procedureName : string * parameters : SqlQueryParameter list
    
        
    /// <summary>
    /// Defines the contract for a SQL Server Data Store
    /// </summary>
    type ISqlDataStore =
        /// <summary>
        /// Retrieves an identified collection of data entities from the store
        /// </summary>
        abstract Get:SqlQuery -> 'T list

        abstract Get:unit -> 'T list

        /// <summary>
        /// Persists a collection of data entities to the store, inserting or updating as appropriate
        /// </summary>
        abstract Put:'T seq -> unit

        /// <summary>
        /// Deletes and identified collection of data entities from the store
        /// </summary>
        abstract Del:SqlQuery -> unit

        abstract GetContract: unit -> 'TContract when 'TContract : not struct

        
        
       
/// <summary>
/// Provides ISqlDataStore realization
/// </summary>
module SqlDataStore =    
    let internal bcp (cs : SqlConnectionString) (data : 'T seq) =
        use bcp = new SqlBulkCopy(cs.Text)
        use dataTable = data |> DataTable.fromProxyValuesT
        dataTable |> bcp.WriteToServer
        
    type private SqlDataStore(csSqlDataStore : ConnectionString) =
        let cs = SqlConnectionString(csSqlDataStore.Components)
        interface ISqlDataStore with
            member this.Get q : list<'T>= 
                match q with
                | TabularQuery(tabularName,columnNames) ->
                    typeref<'T> |> Tabular.executeProxyQuery cs.Text :?> list<'T>
                | TableFunctionQuery(functionName, parameters) ->
                    []
                | ProcedureQuery(procedureName, parameters) ->
                    []
            
            member this.Get() : list<'T> =
                typeref<'T> |> Tabular.executeProxyQuery cs.Text :?> list<'T>
                
            
            member this.Put items = bcp cs items
            member this.Del q = ()


            member this.GetContract() =
                Routine.getContract<'TContract>(cs.Text)
                
                                             
    /// <summary>
    /// Provides access to an identified data store
    /// </summary>
    /// <param name="cs">Connection string that identifies the data store</param>
    let access (cs) =
        SqlDataStore(cs) :> ISqlDataStore

