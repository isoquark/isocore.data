namespace IQ.Core.Data.Sql

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection

open IQ.Core.Data
open IQ.Core.Framework

module internal Procedure =
    /// <summary>
    /// Executes a stored procedure
    /// </summary>
    /// <param name="paramValues">The values of the parameters</param>
    /// <param name="proc">The procedure to execute</param>
    /// <param name="cs">The connection string</param>
    let execute cs (paramValues : ValueIndex) (proc : ProcedureDescription) =
        use connection = new SqlConnection(cs)
        connection.Open()
        use command = new SqlCommand(proc.Name |> SqlFormatter.formatObjectName, connection)
        command.CommandType <- CommandType.StoredProcedure
        
        proc.Parameters |> List.iter(fun x ->
                let p = if x.Direction = ParameterDirection.ReturnValue then 
                            SqlParameter("Return", DBNull.Value) 
                        else if x.Direction = ParameterDirection.Input then
                            SqlParameter(x.Name, paramValues.[x.Name])
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


module internal ProcedureContract =        
    let private getMethodParameterName(proxy : ParameterProxyDescription) =
        match proxy with
        | ParameterProxyDescription(proxy=x) ->
            match x with
            | MethodInputReference(m) ->
                m.Subject.Name.Text
            | MethodOutputReference(m) ->
                "Return" 

    let private indexRoutineParameterValues (proxy : ProcedureCallProxyDescription) (methodParameterValues : ValueIndex)  =
        [for p in proxy.Parameters do
            let key = p |> getMethodParameterName  |> NameKey
            match methodParameterValues |> ValueIndex.tryFindValue key  with
            | Some(value) ->
                yield (p.DataElement.Name, value)
            | None ->
                ()
        ] |> ValueIndex.fromNamedItems

    let private indexMethodParameterValues (m : MethodInfo) (values : obj[]) =
        m.GetParameters() |> Array.mapi (fun i p -> p.Name, values.[i]) |> ValueIndex.fromNamedItems
       
    let private invoke<'TContract,'TConfig> =
        let proxies = procproxies<'TContract> 
        
        let tryDescribe(m) =
            proxies |> List.tryFind(fun p -> p.ProxyElement.Subject.Element = m)

        fun (cs : string) (targetMethod : MethodInfo) (args : obj[]) ->
            let methodParamValues = args|> indexMethodParameterValues targetMethod
            match targetMethod |> tryDescribe with
            | Some(description) ->
                let routineParamValues = methodParamValues |> indexRoutineParameterValues description   
                let results = description.DataElement |> Procedure.execute cs routineParamValues
                let outputs = 
                    description.Parameters |> List.filter(fun x -> x.DataElement.Direction = ParameterDirection.Output)
                if outputs.Length = 0 then
                    results |> ValueIndex.tryFindValue (NameKey("Return"))
                else if outputs.Length = 1 then
                    results |> ValueIndex.tryFindValue (NameKey(outputs.Head.DataElement.Name))
                else
                    NotSupportedException("Cannot yet support multiple output parameters") |> raise                    
            | None ->
                NotImplementedException(sprintf "There is no implementation for the method %s" targetMethod.Name) |> raise    

    let get<'TContract when 'TContract : not struct>(cs : string) =        
        invoke<'TContract,string> |> DynamicContract.realize<'TContract,string> cs


