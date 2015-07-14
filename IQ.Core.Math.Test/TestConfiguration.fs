namespace IQ.Core.Math.Test

open System
open System.Reflection

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data





[<AutoOpen>]
module TestConfiguration =
    
    //This is instantiated/cleaned-up once per collection
    type ProjectTestContext()= 
        inherit TestContext((fun x -> ()) |> CoreRegistration.compose (thisAssembly()))

    [<Literal>]
    let TestCollectionName = "Core Math Tests"

    //This class exists to feed the test infrastructure metadata
    [<AbstractClass; TestCollectionMaker(TestCollectionName)>]
    type ProjectCollectionMarker() = 
        inherit TestCollection<ProjectTestContext>()

    [<AbstractClass; TestContainer(TestCollectionName)>]
    type ProjectTestContainer(context,log) =
        member this.Context : ProjectTestContext = context
        member this.Log : ITestLog = log

