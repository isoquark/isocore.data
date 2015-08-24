﻿// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql

open System
open System.Data
open System.Linq
open System.Data.Linq
open System.Reflection
open System.Text
open System.Data.SqlClient
open System.Diagnostics

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data


module internal SqlCommand =
    let executeQuery (colnames : string rolist) (command : SqlCommand) =
        use reader = command.ExecuteReader()
        if reader.HasRows then
            [|while reader.Read() do
                let buffer = Array.zeroCreate<obj>(colnames.Count)
                let valueCount = buffer |> reader.GetValues
                Debug.Assert((valueCount = colnames.Count), "Column / Value count mismatch")
                yield buffer 
            |] :> rolist<_>
        else
            [||] :> rolist<_>
        
module internal SqlConnection = 
    let create cs = 
        let connection = new SqlConnection(cs)
        connection.Open() 
        connection

