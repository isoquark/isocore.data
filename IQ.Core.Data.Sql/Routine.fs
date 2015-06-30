namespace IQ.Core.Data.Sql

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection
open System.Diagnostics

open IQ.Core.Data
open IQ.Core.Framework
    
/// <summary>
/// Provides capability to execute routines
/// </summary>
module internal Routine =            
        
    let private addParameter (command : SqlCommand) (param : SqlParameter) =
        param |> command.Parameters.Add |> ignore

    /// <summary>
    /// Executes a stored procedure
    /// </summary>
    /// <param name="cs">The connection string</param>
    /// <param name="paramValues">The values of the parameters</param>
    /// <param name="proc">The procedure to execute</param>
    let executeProcedure cs (paramValues : DataParameterValue list) (proc : ProcedureDescription) =
        use connection = cs |> SqlConnection.create
        use command = new SqlCommand(proc.Name |> SqlFormatter.formatObjectName, connection)
        command.CommandType <- CommandType.StoredProcedure
        
        proc.Parameters |> List.iter (fun x ->x |> SqlParameter.create paramValues |> addParameter command)
        command.ExecuteNonQuery() |> ignore
        
        // See https://msdn.microsoft.com/en-us/library/haa3afyz(v=vs.110).aspx for how to deal with multiple result sets

        [for i in [0..command.Parameters.Count-1] do
            let parameter = command.Parameters.[i]  
            if  parameter.Direction = ParameterDirection.InputOutput || 
                parameter.Direction = ParameterDirection.Output || 
                parameter.Direction = ParameterDirection.ReturnValue then
                yield parameter.ParameterName, i, parameter.Value
        ]|>ValueIndex.create                
        
    /// <summary>
    /// Executes a table-valued function and returns a list of object arrays where each array
    /// represents a row of data
    /// </summary>
    /// <param name="cs">The connection string</param>
    /// <param name="paramValues">The values of the parameters</param>
    /// <param name="proc">The function to execute</param>
    let executeTableFunction cs (paramValues : DataParameterValue list) (f : TableFunctionDescription) =
        use connection = cs |> SqlConnection.create
        let sql = f |> SqlFormatter.formatTableFunctionSelect
        use command = new SqlCommand(sql, connection)
        f.Parameters |> List.iter (fun x ->x |> SqlParameter.create paramValues |> addParameter command)
        command |> SqlCommand.executeQuery f.Columns                
                   

    /// <summary>
    /// Executes a table-valued function and returns a data table
    /// </summary>
    /// <param name="cs">The connection string</param>
    /// <param name="paramValues">The values of the parameters</param>
    /// <param name="proc">The function to execute</param>
    let executeTableFunctionDataTable cs (paramValues : DataParameterValue list) (f : TableFunctionDescription) =
        use connection = cs |> SqlConnection.create
        let sql = f |> SqlFormatter.formatTableFunctionSelect
        use command = new SqlCommand(sql, connection)
        f.Parameters |> List.iter (fun x ->x |> SqlParameter.create paramValues |> addParameter command)
        
        use adapter = new SqlDataAdapter(command)
        let table = new DataTable()
        adapter.Fill(table) |> ignore       
        table        

    let private getMethodParameterName(proxy : ParameterProxyDescription) =
        match proxy with
        | ParameterProxyDescription(proxy=x) ->
            match x with
            | MethodInputReference(m) ->
                m.Subject.Name.Text
            | MethodOutputReference(m) ->
                "Return" 

    let private findMethodProxy (proxies : DataObjectProxy list) targetMethod =
        let describeProxy m  =
            proxies |> List.tryFind
                (
                    fun p -> match p.ProxyElement with                                
                                | MemberReference(x) -> 
                                    match x with
                                    | MethodMemberReference(x) ->
                                        x.Subject.Element = m
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
            [for p in proxy.Parameters do
                let key = ValueIndexKey(p |> getMethodParameterName , p.ProxyParameterPosition)
                match methodParameterValues |> ValueIndex.tryFindValue key  with
                | Some(value) ->
                    yield DataParameterValue(p.DataElement.Name, p.DataElement.Position, value)
                | None ->
                    ()
            ]         
        | TableFunctionProxy(proxy) -> 
             mii.MethodArgs |> List.zip proxy.CallProxy.Parameters
                            |> List.map (fun (param, value) -> 
                                        DataParameterValue(param.DataElement.Name, param.DataElement.Position, value))
        | _ -> []


    let private usedatatable = false
       
    /// <summary>
    /// Creates function that will be invoked whenever a contracted method is called to execute a stored procedure
    /// </summary>
    let private createInvoker<'TContract,'TConfig> =
        let proxies = routineproxies<'TContract> 
        let findProxy (mii : MethodInvocationInfo) = 
            mii.Method.Element |>  findMethodProxy proxies
                            
        let invoke(mii : MethodInvocationInfo) =
            let proxy = mii |> findProxy 

            let routineArgs = mii |> getRoutineArgs proxy
                                    
            match proxy with
            | ProcedureProxy proxy ->
                let results = 
                    proxy.DataElement |> executeProcedure mii.ConnectionString routineArgs
                let outputs = 
                    proxy.Parameters |> List.filter(fun x -> x.DataElement.Direction = ParameterDirection.Output)
                
                if outputs.Length = 0 then
                    results |> ValueIndex.tryFindNamedValue "Return"
                else if outputs.Length = 1 then
                    results |> ValueIndex.tryFindNamedValue outputs.Head.DataElement.Name
                else
                    NotSupportedException("Cannot yet support multiple output parameters") |> raise
            
            | TableFunctionProxy proxy ->
                if usedatatable then
                    let result = proxy.DataElement|> executeTableFunctionDataTable mii.ConnectionString routineArgs
                    result |> DataTable.toProxyValues proxy.ResultProxy.ProxyElement :> obj |> Some
                else
                    let result = proxy.DataElement|> executeTableFunction mii.ConnectionString routineArgs
                    match proxy.ResultProxy.ProxyElement with
                    | CollectionTypeReference(subject, itemTypeRef, collectionKind) ->            
                        let items = 
                            [for row in result ->
                                itemTypeRef.ReferentType.Type |> RecordValue.fromValueArray row]
                        items |> Collection.create collectionKind itemTypeRef.ReferentType.Type |> Some
                    | _ -> NotSupportedException() |> raise
                        
                    
            | _ -> nosupport()
            
                
        fun (cs : string) (targetMethod : MethodInfo) (args : obj[]) ->            
            {
                ConnectionString = cs
                Method = targetMethod
                MethodArgs = args |> List.ofArray
            } |> invoke
                        
    let getContract<'TContract when 'TContract : not struct>(cs : string) =        
        createInvoker<'TContract,string> |> DynamicContract.realize<'TContract,string> cs


