﻿// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql.Test

open System
open System.Reflection

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data



[<AutoOpen>]
module TestConfiguration =

    let private registerDependencies(registry : ICompositionRegistry) =
            registry.RegisterInstance<IDataStoreProvider>(SqlDataStore.getProvider())
                                
    //This is instantiated/cleaned-up once per collection
    type ProjectTestContext() as this = 
        inherit TestContext(registerDependencies |> CoreRegistration.compose (thisAssembly()))

        let cs = "csSqlDataStore" |> this.ConfigurationManager.GetValue 
        let dsProvider = this.AppContext.Resolve<IDataStoreProvider>()
        let store = dsProvider.GetDataStore<ISqlDataStore>(cs)

        member this.Store = store
        member this.ConnectionString = cs
        
        
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

