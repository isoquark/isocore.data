namespace IQ.Core.Data.Sql.Test

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Test.ProxyTestCases
open IQ.Core.Data.Sql
open IQ.Core.Framework


[<TestContainer; DataStoreTrait>]
module ``Procedure Execution`` =
    let private config = Root.Resolve<IConfigurationManager>()
    let private cs = ConfigSettingNames.SqlTestDb |> config.GetValue 
    let store = Root.Resolve<ISqlDataStore>(ConfigSettingNames.SqlTestDb, cs |> ConnectionString.parse)
        
    [<Test>]
    let ``Executed [SqlTest].[pTable02Insert] procedure - Direct``() =
        let procName = thisMethod() |> ProxyTestCaseMethod.getDbObjectName
        let procProxy = routineproxies<ISqlTestRoutines> |> List.find(fun x -> x.DataElement.Name = procName)
        let proc = procProxy.DataElement |> DataObjectDescription.unwrapProcedure
        let inputValues =  
            [("col01", 0, DBNull.Value :> obj); ("col02", 1, DateTime(2015, 5, 16) :> obj); ("col03", 2, 507L :> obj);]
            |> List.map RoutineParameterValue
        let outputvalues = proc |> Routine.executeProcedure cs inputValues
        let col01Value = outputvalues.["col01"] :?> int
        Claim.greater col01Value 1

    [<Test>]
    let ``Executed [SqlTest].[pTable02Insert] procedure - Contract``() =
        let procs = store.GetContract<ISqlTestRoutines>()
        let result = procs.pTable02Insert (DateTime(2015, 5, 16)) (507L)
        Claim.greater result 1


    [<Test>]
    let ``Executed [SqlTest].[pTable03Insert] procedure - Direct``() =
        let procName = thisMethod() |> ProxyTestCaseMethod.getDbObjectName
        let procProxy = routineproxies<ISqlTestRoutines> |> List.find(fun x -> x.DataElement.Name = procName) 
        let proc = procProxy.DataElement |> DataObjectDescription.unwrapProcedure
        let inputValues =
            [("Col01", 0, 5uy :> obj); ("Col02", 1, 10s :> obj); ("Col03", 2, 15 :> obj); ("Col04", 3, 20L :> obj)]
            |> List.map RoutineParameterValue

        let outputValues = proc |> Routine.executeProcedure cs inputValues
        ()
    
    [<Test>]
    let ``Executed [SqlTest].[pTable03Insert] procedure - Contract``() =
        let procs = store.GetContract<ISqlTestRoutines>()
        let result = procs.pTable03Insert 5uy 10s 15 20L
        0 |> Claim.greater result
        ()
    
    [<Test>]
    let ``Executed [SqlTest].[fTable04Before] procedure - List result``() =
        let routines = store.GetContract<ISqlTestRoutines>()
        routines.pTable04Truncate()
        
        let d0 = DateTime(2012, 1, 1)
        
        let identities =
            [0..2..20] |> List.map(fun i ->                        
            routines.pTable04Insert "ABC" (d0.AddDays(float(i))) (d0.AddDays( float(i) + 1.0))      
        )

        let results = 
            routines.fTable04Before(DateTime(2012,1,9))
            |> List.sortBy(fun x -> x.StartDate)

        results.Length |> Claim.equal 5

        results.[0].Code |> Claim.equal "ABC"
        results.[0].StartDate |> Claim.equal (DateTime(2012,1,1))
        results.[0].EndDate |> Claim.equal (DateTime(2012,1,2))

        results.[1].Code |> Claim.equal "ABC"
        results.[1].StartDate |> Claim.equal (DateTime(2012,1,3))
        results.[1].EndDate |> Claim.equal (DateTime(2012,1,4))

    [<Test>]
    let ``Executed [SqlTest].[fTable04Before] procedure - Array result``() =
        let routines = store.GetContract<ISqlTestRoutines>()
        routines.pTable04Truncate()
        
        let d0 = DateTime(2012, 1, 1)
        
        let identities =
            [0..2..20] |> List.map(fun i ->                        
            routines.pTable04Insert "ABC" (d0.AddDays(float(i))) (d0.AddDays( float(i) + 1.0))      
        )

        let results = 
            routines.fTable04BeforeArray(DateTime(2012,1,9))
            |> Array.sortBy(fun x -> x.StartDate)

        results.Length |> Claim.equal 5

        results.[0].Code |> Claim.equal "ABC"
        results.[0].StartDate |> Claim.equal (DateTime(2012,1,1))
        results.[0].EndDate |> Claim.equal (DateTime(2012,1,2))

        results.[1].Code |> Claim.equal "ABC"
        results.[1].StartDate |> Claim.equal (DateTime(2012,1,3))
        results.[1].EndDate |> Claim.equal (DateTime(2012,1,4))


    
        
