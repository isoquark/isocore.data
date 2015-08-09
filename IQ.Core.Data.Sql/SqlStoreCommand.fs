// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql

open System
open System.Data
open System.Data.SqlClient


open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data


module internal SqlStoreCommand =
    let execute cs command =
            use connection = cs |> SqlConnection.create
            match command with
            | TruncateTable tableName ->
                let sql = tableName |> SqlFormatter.formatTruncateTable                   
                use sqlcommand = new SqlCommand(sql, connection)
                sqlcommand.ExecuteNonQuery() |> TruncateTableResult
            | AllocateSequenceRange(seqname, count) ->
                let sql = "sys.sp_sequence_get_range"
                use sqlcommand = new SqlCommand(sql, connection)
                sqlcommand.CommandType <- CommandType.StoredProcedure
                sqlcommand.Parameters.AddWithValue("@sequence_name", seqname |> SqlFormatter.formatObjectName) |> ignore
                sqlcommand.Parameters.AddWithValue("@range_size", count) |> ignore
                let firstValParam = SqlParameter(@"range_first_value", SqlDbType.Variant)
                firstValParam.Direction <- System.Data.ParameterDirection.Output
                sqlcommand.Parameters.Add(firstValParam) |> ignore
                sqlcommand.ExecuteNonQuery() |> ignore
                firstValParam.Value |> AllocateSequenceRangeResult
                

               


                
                



