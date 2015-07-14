﻿namespace IQ.Core.Data.Sql.Test

open System
open System.Reflection

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Sql


[<AutoOpen>]
module TestConfiguration =
                        
    //This is instantiated/cleaned-up once per collection
    type ProjectTestContext() as this = 
        inherit TestContext(ProjectTestContext.RegisterDependencies |> CoreRegistration.compose (thisAssembly()))
                
        let store : ISqlDataStore = 
            "csSqlDataStore" |> this.ConfigurationManager.GetValue |> ConnectionString.parse |> this.AppContext.Resolve

        member this.Store = store
        
        static member private RegisterDependencies(registry : ICompositionRegistry) =
            registry.RegisterFactory(fun config -> config |> SqlDataStore.access)

        
        

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

