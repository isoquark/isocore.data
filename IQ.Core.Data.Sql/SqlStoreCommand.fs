// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Data
open System.Data.SqlClient
open System.Reflection;
open System.Linq;

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data


module internal SqlStoreCommand =
    type private Marker = class end
    
    let private createSqlCommand cs sql=
        let connection = cs |> SqlConnection.create
        new SqlCommand(sql, connection)
    
    let private executeSqlCommand (cmd : SqlCommand) =
        cmd.ExecuteNonQuery()

    
    let private executeSql cs sql =
        use connection = cs |> SqlConnection.create
        use command = new SqlCommand(sql(), connection)
        command.ExecuteNonQuery() |> ignore
         
    [<SqlCommandHandler>]
    let getFileTableRoot cs (spec : GetFileTableRoot) =
        let sql = SqlFormatter.formatGetFileTableRoot()
        use connection = cs |> SqlConnection.create
        use cmd = new SqlCommand(sql, connection)
        cmd.ExecuteScalar() :?> string |> GetFileTableRootResult
    
    [<SqlCommandHandler>]
    let truncateTable cs (spec : TruncateTable)=
        match spec with 
            TruncateTable tableName ->
            let sql = tableName |> SqlFormatter.formatTruncateTable                   
            use connection = cs |> SqlConnection.create
            use sqlcommand = new SqlCommand(sql, connection)
            sqlcommand.ExecuteNonQuery() |> TruncateTableResult
    
    [<SqlCommandHandler>]
    let allocateSequenceRange cs (spec : AllocateSequenceRange) =
        match spec with
            AllocateSequenceRange(seqname,count) ->
            let sql = "sys.sp_sequence_get_range"
            use connection = cs |> SqlConnection.create
            use sqlcommand = new SqlCommand(sql, connection)
            sqlcommand.CommandType <- CommandType.StoredProcedure
            sqlcommand.Parameters.AddWithValue("@sequence_name", seqname |> SqlFormatter.formatObjectName) |> ignore
            sqlcommand.Parameters.AddWithValue("@range_size", count) |> ignore
            let firstValParam = SqlParameter(@"range_first_value", SqlDbType.Variant)
            firstValParam.Direction <- System.Data.ParameterDirection.Output
            sqlcommand.Parameters.Add(firstValParam) |> ignore
            sqlcommand.ExecuteNonQuery() |> ignore
            firstValParam.Value |> AllocateSequenceRangeResult
    
    [<SqlCommandHandler>]
    let createTable cs (spec : CreateTable) =
        match spec with
            CreateTable description ->    
                executeSql cs (fun () -> description |> SqlFormatter.formatTableCreate )

    [<SqlCommandHandler>]
    let dropTable cs (spec : DropTable) =
        match spec with
            |DropTable name ->
                executeSql cs (fun () -> name |> SqlFormatter.formatTableDrop )
                            
    let private commandHandlers = lazy(
        [for m in typeof<Marker>.DeclaringType.GetMethods(BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Static ||| BindingFlags.Instance) do
            if Attribute.IsDefined(m, typeof<SqlCommandHandlerAttribute>) then
                let cmdType = m.GetParameters().Last().ParameterType
                yield (cmdType, m)
        ] |> dict
    )
    let execute cs (spec : 'TCommand) : 'TResult =
        let m = commandHandlers.Value.[spec.GetType()]
        let result = m.Invoke(null, [|cs; spec|]) 
        if typeof<'TResult> = typeof<Unit>  then
            null :> obj :?> 'TResult
        else
            result :?> 'TResult
       
                

               


                
                



