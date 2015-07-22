// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.MetaCode.Shell


open IQ.Core.Data
open IQ.Core.Data.Sql

module Main = 
    [<EntryPoint>]
    let main argv = 
        use context = new ShellContext()
        let cs = "csSqlDataStore" |> context.ConfigurationManager.GetValue  |> ConnectionString.parse
        let storeConfig = SqlDataStoreConfig(cs, context.AppContext.Resolve())
        let store : ISqlDataStore = storeConfig |> context.AppContext.Resolve

    
    
        0 
