﻿// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Excel.Test

open System
open System.Reflection

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Excel

[<AutoOpen>]
module TestConfiguration =

    let private registerDependencies(registry : ICompositionRegistry) =
            let provider = DataStoreProvider.Create(SqlDataStore.getProvider(), ExcelDataStore.getProvider())
            registry.RegisterInstance<IDataStoreProvider>(provider)
                                
    //This is instantiated/cleaned-up once per collection
    type ProjectTestContext() as this = 
        inherit TestContext(registerDependencies |> CoreRegistration.compose (thisAssembly()))

        let cs = "csSqlDataStore" |> this.ConfigurationManager.GetValue 
        let store = this.AppContext.Resolve<IDataStoreProvider>().GetDataStore<ISqlDataStore>(cs)
        member this.Store = store

        
        
    [<Literal>]
    let TestCollectionName = "Core Sql Tests"

    //This class exists to feed the test infrastructure metadata
    [<AbstractClass; TestCollectionMaker(TestCollectionName)>]
    type ProjectCollectionMarker() = 
        inherit TestCollection<ProjectTestContext>()

    [<AbstractClass; TestContainer(TestCollectionName)>]
    type ProjectTestContainer(ctx : ProjectTestContext,log) =
        member this.Context = ctx
        member this.Log : ITestLog = log

