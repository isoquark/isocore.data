// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection
open System.Diagnostics
open System.Collections

open IQ.Core.Data
open IQ.Core.Framework
    
/// <summary>
/// Provides capability to execute routines via a strongly-typed contract
/// </summary>
module internal RoutineContract =   

         
        
    let private PocoConverter() = PocoConverter.getDefault()

       
    let private addParameter (command : SqlCommand) (paramValues : RoutineParameterValue list) (proxyDescription : ParameterProxyDescription) =
        let elementDescription = proxyDescription.DataElement
        let p = if elementDescription.Direction = RoutineParameterDirection.ReturnValue then 
                    SqlParameter("Return", DBNull.Value) 
                else if elementDescription.Direction = RoutineParameterDirection.Input then
                    let value = paramValues |> List.find(fun v -> v.Name = elementDescription.Name) |> fun value -> value.Value
                    match elementDescription.DataType with
                    | TableDataType(name) ->
                        let values = [for item in (value :?> IEnumerable) -> item] 
                        let itemValueType = proxyDescription.ProxyElement.ReflectedElement.Value.ParameterType.ItemValueType 
                        let itemProxyDesc = itemValueType |> ClrMetadata().FindType |>  DataProxyMetadata.describeTableProxy 
                        let tableParamValue = values |> BclDataTable.fromProxyValues itemProxyDesc
                        SqlParameter(elementDescription.Name, tableParamValue)                                                
                    | _ ->
                        SqlParameter(elementDescription.Name |> SqlFormatter.formatParameterName, value)
                else if elementDescription.Direction = RoutineParameterDirection.Output then
                    SqlParameter(elementDescription.Name, DBNull.Value)
                else
                    NotSupportedException() |> raise
        p.Direction <- enum<System.Data.ParameterDirection>(int elementDescription.Direction)
        match elementDescription.DataType with
            | TableDataType(name) ->
                p.SqlDbType <- SqlDbType.Structured
            | _ ->
                p.SqlDbType <- elementDescription.DataType |> DataType.toSqlDbType   
        command.Parameters.Add(p) |> ignore


    /// <summary>
    /// Executes a stored procedure
    /// </summary>
    /// <param name="cs">The connection string</param>
    /// <param name="paramValues">The values of the parameters</param>
    /// <param name="proc">The procedure to execute</param>
    let private executeProcCommand cs (paramValues : RoutineParameterValue list) (proc : RoutineProxyDescription) =
        use connection = cs |> SqlConnection.create
        use command = new SqlCommand(proc.DataElement.Name |> SqlFormatter.formatObjectName, connection)
        command.CommandType <- CommandType.StoredProcedure
        
        proc.CallProxy.Parameters |> Seq.iter (fun x -> x |> addParameter command paramValues)
        command.ExecuteNonQuery() |> ignore
        [for i in [0..command.Parameters.Count-1] do
            let parameter = command.Parameters.[i]  
            if  parameter.Direction = System.Data.ParameterDirection.InputOutput || 
                parameter.Direction = System.Data.ParameterDirection.Output || 
                parameter.Direction = System.Data.ParameterDirection.ReturnValue then
                yield parameter.ParameterName, i, parameter.Value
        ]|>ValueIndex.create                

    /// <summary>
    /// Executes a stored procedure that returns a result set
    /// </summary>
    /// <param name="cs">The connection string</param>
    /// <param name="paramValues">The values of the parameters</param>
    /// <param name="proc">The proxy of the procedure to execute</param>
    let private executeProcQuery cs (paramValues : RoutineParameterValue list) (routine : RoutineProxyDescription) =
        use connection = cs |> SqlConnection.create
        use command = new SqlCommand(routine.CallProxy.DataElement.Name |> SqlFormatter.formatObjectName, connection)
        command.CommandType <- CommandType.StoredProcedure
        routine.CallProxy.Parameters |> Seq.iter (fun x ->x |> addParameter command paramValues)
        let result = command |> SqlCommand.executeQuery
        let typedesc = routine.ResultProxy.Value.ProxyElement
        match typedesc with
        | CollectionType(x) ->
            let itemType = typedesc.ReflectedElement.Value.ItemValueType
            let items = 
                [for row in result -> PocoConverter().FromValueArray(row, itemType)]
            items |> CollectionBuilder.create x.Kind itemType |> Some                    
        | _ -> NotSupportedException() |> raise                                            
        
            
    /// <summary>
    /// Executes a table-valued function and returns a list of object arrays where each array
    /// represents a row of data
    /// </summary>
    /// <param name="cs">The connection string</param>
    /// <param name="paramValues">The values of the parameters</param>
    /// <param name="proc">The function to execute</param>
    let private executeTableFunction cs (paramValues : RoutineParameterValue list) (f : RoutineProxyDescription) =
        use connection = cs |> SqlConnection.create
        let sql = f.DataElement |> SqlFormatter.formatTableFunctionSelect
        use command = new SqlCommand(sql, connection)
        f.CallProxy.Parameters |> Seq.iter (fun x ->x |> addParameter command paramValues)
        command |> SqlCommand.executeQuery 
                   

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
    let private getRoutineArgs (proxy : DataObjectProxy) (invokeInfo : MethodInvocationInfo) =
        match proxy with
        | ProcedureProxy(proxy) -> 
            let methodParameterValues = 
                invokeInfo.Method.GetParameters() 
                |> Array.mapi (fun i p -> p.Name, i, invokeInfo.MethodArgs.[i]) 
                |> ValueIndex.create
            
            [for p in proxy.CallProxy.Parameters do
                let key = ValueIndexKey(p |> getMethodParameterName , p.ProxyParameterPosition)
                match methodParameterValues |> ValueIndex.tryFindValue key  with
                | Some(value) ->
                    yield RoutineParameterValue(p.DataElement.Name, p.DataElement.Position, value)
                | None ->
                    ()
            ]         
        | TableFunctionProxy(proxy) -> 
             invokeInfo.MethodArgs |> List.zip proxy.CallProxy.Parameters
                            |> List.map (fun (param, value) -> 
                                        RoutineParameterValue(param.DataElement.Name, param.DataElement.Position, value))
        | _ -> []

       
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
                    proxy |> executeProcQuery invokeInfo.ConnectionString routineArgs
                | None ->                    
                    let results = 
                        proxy |> executeProcCommand invokeInfo.ConnectionString routineArgs
                    let outputs = 
                        proxy.CallProxy.Parameters |> List.filter(fun x -> x.DataElement.Direction = RoutineParameterDirection.Output)                
                    if outputs.Length = 0 then
                        results |> ValueIndex.tryFindNamedValue "Return"
                    else if outputs.Length = 1 then
                        results |> ValueIndex.tryFindNamedValue outputs.Head.DataElement.Name
                    else
                        NotSupportedException("Cannot yet support multiple output parameters") |> raise
            
            | TableFunctionProxy proxy ->
                let result = proxy|> executeTableFunction invokeInfo.ConnectionString routineArgs
                let typedesc = proxy.ResultProxy.Value.ProxyElement
                match typedesc with
                | CollectionType(x) ->
                    let itemType = typedesc.ReflectedElement.Value.ItemValueType
                    let items = 
                        [for row in result -> PocoConverter().FromValueArray(row, itemType)]
                    items |> CollectionBuilder.create x.Kind itemType |> Some                    
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


