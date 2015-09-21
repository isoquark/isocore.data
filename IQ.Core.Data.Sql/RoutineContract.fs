﻿// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql.Behavior

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection
open System.Diagnostics

open IQ.Core.Data
open IQ.Core.Framework
    
/// <summary>
/// Provides capability to execute routines via a strongly-typed contract
/// </summary>
module internal RoutineContract =            
        
    let private addParameter (command : SqlCommand) (param : SqlParameter) =
        param |> command.Parameters.Add |> ignore

    /// <summary>
    /// Executes a stored procedure
    /// </summary>
    /// <param name="cs">The connection string</param>
    /// <param name="paramValues">The values of the parameters</param>
    /// <param name="proc">The procedure to execute</param>
    let private executeProcCommand cs (paramValues : RoutineParameterValue list) (proc : RoutineDescription) =
        use connection = cs |> SqlConnection.create
        use command = new SqlCommand(proc.Name |> SqlFormatter.formatObjectName, connection)
        command.CommandType <- CommandType.StoredProcedure
        
        proc.Parameters |> Seq.iter (fun x ->x |> SqlParameter.create paramValues |> addParameter command)
        command.ExecuteNonQuery() |> ignore
        [for i in [0..command.Parameters.Count-1] do
            let parameter = command.Parameters.[i]  
            if  parameter.Direction = System.Data.ParameterDirection.InputOutput || 
                parameter.Direction = System.Data.ParameterDirection.Output || 
                parameter.Direction = System.Data.ParameterDirection.ReturnValue then
                yield parameter.ParameterName, i, parameter.Value
        ]|>ValueIndex.create                

    /// <summary>
    /// Executes a stored procedure that yields a result set
    /// </summary>
    /// <param name="cs">The connection string</param>
    /// <param name="paramValues">The values of the parameters</param>
    /// <param name="proc">The procedure to execute</param>
    let private executeProcQuery cs (paramValues : RoutineParameterValue list) (proc : RoutineDescription) =
        use connection = cs |> SqlConnection.create
        use command = new SqlCommand(proc.Name |> SqlFormatter.formatObjectName, connection)
        command.CommandType <- CommandType.StoredProcedure
        proc.Parameters |> Seq.iter (fun x ->x |> SqlParameter.create paramValues |> addParameter command)

        use adapter = new SqlDataAdapter(command)
        let table = new DataTable()
        adapter.Fill(table) |> ignore
        table

            
    /// <summary>
    /// Executes a table-valued function and returns a list of object arrays where each array
    /// represents a row of data
    /// </summary>
    /// <param name="cs">The connection string</param>
    /// <param name="paramValues">The values of the parameters</param>
    /// <param name="proc">The function to execute</param>
    let private executeTableFunction cs (paramValues : RoutineParameterValue list) (f : RoutineDescription) =
        use connection = cs |> SqlConnection.create
        let sql = f |> SqlFormatter.formatTableFunctionSelect
        use command = new SqlCommand(sql, connection)
        f.Parameters |> Seq.iter (fun x ->x |> SqlParameter.create paramValues |> addParameter command)
        command |> SqlCommand.executeQuery 
                   

    /// <summary>
    /// Executes a table-valued function and returns a data table
    /// </summary>
    /// <param name="cs">The connection string</param>
    /// <param name="paramValues">The values of the parameters</param>
    /// <param name="proc">The function to execute</param>
    let private executeTableFunctionDataTable cs (paramValues : RoutineParameterValue list) (f : RoutineDescription) =
        use connection = cs |> SqlConnection.create
        let sql = f |> SqlFormatter.formatTableFunctionSelect
        use command = new SqlCommand(sql, connection)
        f.Parameters |> Seq.iter (fun x ->x |> SqlParameter.create paramValues |> addParameter command)
        
        use adapter = new SqlDataAdapter(command)
        let table = new DataTable()
        adapter.Fill(table) |> ignore       
        table        

    let private getMethodParameterName(proxy : ParameterProxyDescription) =
        if proxy.ProxyElement.IsReturn then "Return" else proxy.ProxyElement.Name.Text

    let private findMethodProxy (proxies : DataObjectProxy list) targetMethod =
        let describeProxy m  =
            proxies |> List.tryFind
                (
                    fun p -> match p.ProxyElement with                                
                                | MemberElement(x) -> 
                                    match x with
                                    | MethodMember(x) ->
                                        x.ReflectedElement.Value = m
                                    | _ -> ArgumentException() |> raise
                                | _ -> ArgumentException() |> raise) 


        match targetMethod |> describeProxy with
        | Some(x) -> x
        | None -> NotImplementedException(sprintf "There is no implementation for the method %O" targetMethod.Name) |> raise  


    type private MethodInvocationInfo = {
        ConnectionString : string
        Method : MethodInfo
        MethodArgs : obj list       
    }
   
    /// <summary>
    /// Derives routine arguments from proxy and invocation information
    /// </summary>
    /// <param name="proxy">The routine proxy</param>
    /// <param name="mii">The method invocation information</param>
    let private getRoutineArgs (proxy : DataObjectProxy) (mii : MethodInvocationInfo) =
        match proxy with
        | ProcedureProxy(proxy) -> 
            let methodParameterValues = mii.Method.GetParameters() |> Array.mapi (fun i p -> p.Name, i, mii.MethodArgs.[i]) |> ValueIndex.create
            [for p in proxy.CallProxy.Parameters do
                let key = ValueIndexKey(p |> getMethodParameterName , p.ProxyParameterPosition)
                match methodParameterValues |> ValueIndex.tryFindValue key  with
                | Some(value) ->
                    yield RoutineParameterValue(p.DataElement.Name, p.DataElement.Position, value)
                | None ->
                    ()
            ]         
        | TableFunctionProxy(proxy) -> 
             mii.MethodArgs |> List.zip proxy.CallProxy.Parameters
                            |> List.map (fun (param, value) -> 
                                        RoutineParameterValue(param.DataElement.Name, param.DataElement.Position, value))
        | _ -> []


    let private usedatatable = false
       
    /// <summary>
    /// Creates a function that will be invoked whenever a contracted method is called to execute a stored procedure
    /// </summary>
    let private createInvoker<'TContract,'TConfig>() =
        let proxies = routineproxies<'TContract> 
        let findProxy (mii : MethodInvocationInfo) = 
            mii.Method |>  findMethodProxy proxies
                            
        let invoke(invokeInfo : MethodInvocationInfo) =
            let proxy = invokeInfo |> findProxy 

            let routineArgs = invokeInfo |> getRoutineArgs proxy
                                    
            match proxy with
            | ProcedureProxy proxy ->
                match proxy.ResultProxy with
                | Some(resultProxy) ->
                    proxy.DataElement |> executeProcQuery invokeInfo.ConnectionString routineArgs 
                                      |> BclDataTable.toProxyValues proxy.ResultProxy.Value.ProxyElement :> obj |> Some 
                | None ->                    
                    let results = 
                        proxy.DataElement |> executeProcCommand invokeInfo.ConnectionString routineArgs
                    let outputs = 
                        proxy.CallProxy.Parameters |> List.filter(fun x -> x.DataElement.Direction = RoutineParameterDirection.Output)                
                    if outputs.Length = 0 then
                        results |> ValueIndex.tryFindNamedValue "Return"
                    else if outputs.Length = 1 then
                        results |> ValueIndex.tryFindNamedValue outputs.Head.DataElement.Name
                    else
                        NotSupportedException("Cannot yet support multiple output parameters") |> raise
            
            | TableFunctionProxy proxy ->
                if usedatatable then
                    let result = proxy.DataElement|> executeTableFunctionDataTable invokeInfo.ConnectionString routineArgs
                    result |> BclDataTable.toProxyValues proxy.ResultProxy.Value.ProxyElement :> obj |> Some
                else
                    let result = proxy.DataElement|> executeTableFunction invokeInfo.ConnectionString routineArgs
                    let typedesc = proxy.ResultProxy.Value.ProxyElement
                    match typedesc with
                    | CollectionType(x) ->
                        let provider = ClrMetadataProvider.getDefault()
                        let pocoConverter =  PocoConverter.getDefault()

                        let itemType = typedesc.Name |> provider.FindType |> fun x -> x.ReflectedElement.Value.ItemValueType
                        let items = 
                            [for row in result -> pocoConverter.FromValueArray(row, itemType)]
                        items |> Collection.create x.Kind itemType |> Some                    
                    | _ -> NotSupportedException() |> raise                                            
            | _ -> nosupport()
            
        //Note that this is the method being returned to the caller                
        fun (cs : string) (targetMethod : MethodInfo) (args : obj[]) ->            
            {
                ConnectionString = cs
                Method = targetMethod
                MethodArgs = args |> List.ofArray
            } |> invoke
                        
    /// <summary>
    /// Gets a realization of an identified contract
    /// </summary>
    /// <param name="cs">The connection string to use when executing contract operations</param>
    let get<'TContract when 'TContract : not struct> (cs : string) =        
        createInvoker<'TContract,string>() |> DynamicContract.realize<'TContract,string> cs

