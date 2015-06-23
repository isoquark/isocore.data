namespace IQ.Core.Data.Sql

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection
open System.Diagnostics

open IQ.Core.Data
open IQ.Core.Framework


module internal SqlConnection = 
    let create cs = 
        let connection = new SqlConnection(cs)
        connection.Open() 
        connection


module internal Routine =
        
    
    /// <summary>
    /// Executes a stored procedure
    /// </summary>
    /// <param name="paramValues">The values of the parameters</param>
    /// <param name="proc">The procedure to execute</param>
    /// <param name="cs">The connection string</param>
    let executeProcedure cs (paramValues : RoutineParameterValue list) (proc : ProcedureDescription) =
        use connection = cs |> SqlConnection.create
        use command = new SqlCommand(proc.Name |> SqlFormatter.formatObjectName, connection)
        command.CommandType <- CommandType.StoredProcedure
        
        proc.Parameters |> List.iter(fun x ->
                
                let p = if x.Direction = ParameterDirection.ReturnValue then 
                            SqlParameter("Return", DBNull.Value) 
                        else if x.Direction = ParameterDirection.Input then
                            SqlParameter(x.Name, paramValues |> List.find(fun v -> v.Name = x.Name) |> fun value -> value.Value)
                        else if x.Direction = ParameterDirection.Output then
                            SqlParameter(x.Name, DBNull.Value)
                        else
                           NotSupportedException() |> raise
                p.Direction <- x.Direction
                p.SqlDbType <- x.StorageType |> StorageType.toSqlDbType
                p |> command.Parameters.Add |> ignore             
            )
        command.ExecuteNonQuery() |> ignore
        
        [for parameter in command.Parameters do
            if  parameter.Direction = ParameterDirection.InputOutput || 
                parameter.Direction = ParameterDirection.Output || 
                parameter.Direction = ParameterDirection.ReturnValue then
                yield parameter.ParameterName, parameter.Value
        ]|> ValueIndex.fromNamedItems

    let executeTableFunction cs (paramValues : RoutineParameterValue list) (f : TableFunctionDescription) =
        use connection = cs |> SqlConnection.create
        let sql = f |> SqlFormatter.formatTableFunctionSelect
        use command = new SqlCommand(sql, connection)
        paramValues |> List.iter(fun paramValue ->
            command.Parameters.Add(SqlParameter(paramValue.Name |>  sprintf "@%s" , paramValue.Value)) |> ignore
        )
        //TODO: call execute reader on the command for better efficiency
        use adapter = new SqlDataAdapter(command)
        let table = new DataTable()
        adapter.Fill(table) |> ignore       
        table        

module internal RoutineContract =        
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
                                | MemberElement(x) -> 
                                    match x with
                                    | MethodReference(x) ->
                                        x.Subject.Element = m
                                    | _ -> ArgumentException() |> raise
                                | _ -> ArgumentException() |> raise) 


        match targetMethod |> describeProxy with
        | Some(x) -> x
        | None -> NotImplementedException(sprintf "There is no implementation for the method %s" targetMethod.Name) |> raise  


    let private getProcArgs (m : MethodInfo) (proxy : ProcedureProxyDescription) (methodArgs : obj list)  =
        let methodParameterValues = m.GetParameters() |> Array.mapi (fun i p -> p.Name, methodArgs.[i]) |> ValueIndex.fromNamedItems
        [for p in proxy.Parameters do
            let key = p |> getMethodParameterName  |> NameKey
            match methodParameterValues |> ValueIndex.tryFindValue key  with
            | Some(value) ->
                yield RoutineParameterValue(p.DataElement.Name, p.DataElement.Position, value)
            | None ->
                ()
        ]         
    
    let private getFuncArgs (m : MethodInfo) (proxy : TableFunctionProxyDescription) (methodArgs : obj list) =
         methodArgs |> List.zip proxy.CallProxy.Parameters
                    |> List.map (fun (param, value) -> 
                                    RoutineParameterValue(param.DataElement.Name, param.DataElement.Position, value))
   
       
    /// <summary>
    /// Creates function that will be invoked whenever a contracted method is called to execute a stored procedure
    /// </summary>
    let private createRoutineInvoker<'TContract,'TConfig> =
        let proxies = routineproxies<'TContract> 
        let findProxy = findMethodProxy proxies
                    
        fun (cs : string) (targetMethod : MethodInfo) (args : obj[]) ->

            let proxy = targetMethod |> findProxy 
            let args = args |> List.ofArray
            match proxy with
            | ProcedureProxy proxy ->
                let procArgs = args |> getProcArgs targetMethod proxy
                let results = 
                    proxy.DataElement |> Routine.executeProcedure cs procArgs
                let outputs = 
                    proxy.Parameters |> List.filter(fun x -> x.DataElement.Direction = ParameterDirection.Output)
                
                if outputs.Length = 0 then
                    results |> ValueIndex.tryFindValue (NameKey("Return"))
                else if outputs.Length = 1 then
                    results |> ValueIndex.tryFindValue (NameKey(outputs.Head.DataElement.Name))
                else
                    NotSupportedException("Cannot yet support multiple output parameters") |> raise
            | TableFunctionProxy proxy ->
                let funcArgs = args |> getFuncArgs targetMethod proxy
                let result = proxy.DataElement|> Routine.executeTableFunction cs funcArgs
                result |> DataTable.toProxyValues proxy.ResultProxy.ProxyElement :> obj |> Some
            | _ ->
                NotSupportedException() |> raise

    let get<'TContract when 'TContract : not struct>(cs : string) =        
        createRoutineInvoker<'TContract,string> |> DynamicContract.realize<'TContract,string> cs


