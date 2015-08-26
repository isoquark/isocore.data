// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Synthetics.Test

open System
open System.Reflection

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data





[<AutoOpen>]
module TestConfiguration =
    
    let private register (registry : ICompositionRegistry) =
        //ClrMetadataProvider.getDefault() |> registry.RegisterInstance
        registry.RegisterFactory(fun config -> config |> Transformer.get)

    //This is instantiated/cleaned-up once per collection
    type ProjectTestContext()= 
        inherit TestContext( register |> CoreRegistration.compose (thisAssembly()))

    [<Literal>]
    let TestCollectionName = "Core Synthetics Tests"

    //This class exists to feed the test infrastructure metadata
    [<AbstractClass; TestCollectionMaker(TestCollectionName)>]
    type ProjectCollectionMarker() = 
        inherit TestCollection<ProjectTestContext>()

    [<AbstractClass; TestContainer(TestCollectionName)>]
    type ProjectTestContainer(context,log) =
        member this.Context : ProjectTestContext = context
        member this.Log : ITestLog = log

