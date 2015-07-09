namespace IQ.Core.Framework.Test

open System
open System.Reflection

open IQ.Core.Framework
open IQ.Core.Data
open IQ.Core.Data.Sql


[<AutoOpen>]
module TestConfiguration =
                            
    //This is instantiated/cleaned-up once per collection
    type ProjectTestContext()= 
        inherit TestContext(thisAssembly(), 
            fun registry -> registry.RegisterFactory(fun config -> config |> SqlDataStore.access)) 
                                                

    [<Literal>]
    let TestCollectionName = "Core Framework Tests"

    //This class exists to feed the test infrastructure metadata
    [<AbstractClass; TestCollectionMaker(TestCollectionName)>]
    type ProjectCollectionMarker() = 
        inherit TestCollection<ProjectTestContext>()

    [<AbstractClass; TestContainer(TestCollectionName)>]
    type ProjectTestContainer(context,log) =
        member this.Context : ProjectTestContext = context
        member this.Log : ITestLog = log

            