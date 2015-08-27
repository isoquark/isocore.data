// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql.Test

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Test
open IQ.Core.Data.Sql
open IQ.Core.Framework

open TestProxies

module Routine =
    
            
    type Tests(ctx, log)= 
        inherit ProjectTestContainer(ctx,log)
        
        let store = ctx.Store

        [<FactAttribute>]
        let ``Executed [SqlTest].[pTable02Insert] procedure - Direct``() =
            let procName = thisMethod().Name |> DataObjectName.fuzzyParse
            let procProxy = routineproxies<ISqlTestRoutines> |> List.find(fun x -> x.DataElement.Name = procName)
            let proc = procProxy.DataElement |> DataObjectDescription.unwrapProcedure
            let inputValues =  
                [("col01", 0, DBNull.Value :> obj); ("col02", 1, BclDateTime(2015, 5, 16) :> obj); ("col03", 2, 507L :> obj);]
                |> List.map RoutineParameterValue
            let outputvalues = proc |> Routine.executeProcedure store.ConnectionString inputValues
            let col01Value = outputvalues.["col01"] :?> int
            Claim.greaterOrEqual col01Value 1

        [<FactAttribute>]
        let ``Executed [SqlTest].[pTable02Insert] procedure - Contract``() =
            let procs = store.GetContract<ISqlTestRoutines>()
            let result = procs.pTable02Insert (BclDateTime(2015, 5, 16)) (507L)
            Claim.greater result 1


        [<FactAttribute>]
        let ``Executed [SqlTest].[pTable03Insert] procedure - Direct``() =
            let procName = thisMethod().Name |> DataObjectName.fuzzyParse
            let procProxy = routineproxies<ISqlTestRoutines> |> List.find(fun x -> x.DataElement.Name = procName) 
            let proc = procProxy.DataElement |> DataObjectDescription.unwrapProcedure
            let inputValues =
                [("Col01", 0, 5uy :> obj); ("Col02", 1, 10s :> obj); ("Col03", 2, 15 :> obj); ("Col04", 3, 20L :> obj)]
                |> List.map RoutineParameterValue

            let outputValues = proc |> Routine.executeProcedure store.ConnectionString inputValues
            ()
    
        [<FactAttribute>]
        let ``Executed [SqlTest].[pTable03Insert] procedure - Contract``() =
            let procs = store.GetContract<ISqlTestRoutines>()
            let result = procs.pTable03Insert 5uy 10s 15 20L
            0 |> Claim.greater result
            ()
    
        [<FactAttribute>]
        let ``Executed [SqlTest].[fTable04Before] procedure - List result``() =
            let routines = store.GetContract<ISqlTestRoutines>()
            routines.pTable04Truncate()
        
            let d0 = BclDateTime(2012, 1, 1)
        
        

            let identities =
                [0..2..20] |> List.map(fun i ->                        
                routines.pTable04Insert "ABC" (d0.AddDays(float(i))) (d0.AddDays( float(i) + 1.0))      
            )

            let results = 
                routines.fTable04Before(BclDateTime(2012,1,9))
                |> List.sortBy(fun x -> x.StartDate)

            results.Length |> Claim.equal 5

            results.[0].Code |> Claim.equal "ABC"
            results.[0].StartDate |> Claim.equal (BclDateTime(2012,1,1))
            results.[0].EndDate |> Claim.equal (BclDateTime(2012,1,2))

            results.[1].Code |> Claim.equal "ABC"
            results.[1].StartDate |> Claim.equal (BclDateTime(2012,1,3))
            results.[1].EndDate |> Claim.equal (BclDateTime(2012,1,4))

        [<FactAttribute>]
        let ``Executed [SqlTest].[fTable04Before] procedure - Array result``() =
            let routines = store.GetContract<ISqlTestRoutines>()
            routines.pTable04Truncate()
        
            let d0 = BclDateTime(2012, 1, 1)
        
            let identities =
                [0..2..20] |> List.map(fun i ->                        
                routines.pTable04Insert "ABC" (d0.AddDays(float(i))) (d0.AddDays( float(i) + 1.0))      
            )

            let results = 
                routines.fTable04BeforeArray(BclDateTime(2012,1,9))
                |> Array.sortBy(fun x -> x.StartDate)

            results.Length |> Claim.equal 5

            results.[0].Code |> Claim.equal "ABC"
            results.[0].StartDate |> Claim.equal (BclDateTime(2012,1,1))
            results.[0].EndDate |> Claim.equal (BclDateTime(2012,1,2))

            results.[1].Code |> Claim.equal "ABC"
            results.[1].StartDate |> Claim.equal (BclDateTime(2012,1,3))
            results.[1].EndDate |> Claim.equal (BclDateTime(2012,1,4))


    
        
