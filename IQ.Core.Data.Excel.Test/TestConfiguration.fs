
// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Excel.Test

open System
open System.Reflection

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Excel
open IQ.Core.Data.Sql

[<AutoOpen>]
module TestConfiguration =

    let private registerDependencies(registry : ICompositionRegistry) =
            ClrMetadataProvider.getDefault() |> registry.RegisterInstance
            registry.RegisterFactory(fun config -> config |> SqlDataStore.get)
                                
    //This is instantiated/cleaned-up once per collection
    type ProjectTestContext() as this = 
        inherit TestContext(registerDependencies |> CoreRegistration.compose (thisAssembly()))

        let clrMetadataProvider : IClrMetadataProvider = this.AppContext.Resolve()
        let cs = "csSqlDataStore" |> this.ConfigurationManager.GetValue |> ConnectionString.parse
        let store : ISqlDataStore = SqlDataStoreConfig(cs, clrMetadataProvider) |> this.AppContext.Resolve

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

