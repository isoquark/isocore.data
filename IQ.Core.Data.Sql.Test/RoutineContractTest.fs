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
open IQ.Core.Data.Contracts

open TestProxies

module Routine =
    
            
    type Tests(ctx, log)= 
        inherit ProjectTestContainer(ctx,log)
        
        let store = ctx.Store


        [<FactAttribute>]
        let ``Executed [SqlTest].[pTable02Insert] procedure``() =
            let procs = store.GetCommandContract<ISqlTestRoutines>()
            let result = procs.pTable02Insert (BclDateTime(2015, 5, 16)) (507L)
            Claim.greater result 1


    
        [<FactAttribute>]
        let ``Executed [SqlTest].[pTable03Insert] procedure``() =
            let procs = store.GetCommandContract<ISqlTestRoutines>()
            let result = procs.pTable03Insert 5uy 10s 15 20L
            0 |> Claim.greater result
    
        [<FactAttribute>]
        let ``Executed [SqlTest].[fTable04Before] procedure - List result``() =
            let routines = store.GetCommandContract<ISqlTestRoutines>()
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
        let ``Executed [SqlTest].[fTable04Before] function - Array result``() =
            let routines = store.GetCommandContract<ISqlTestRoutines>()
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

        [<Fact>]
        let ``Executed [SqlTest].[pTable0CSelect] procedure``() =
            DataObjectName("SqlTest", "Table0C") |> TruncateTable |> store.ExecuteCommand |> ignore            
            let values =
                [for i in 1..100 ->
                    {
                        Table0C.Col01 = i
                        Col02 = i.ToString()
                        Col03 = i |> int16
                    }
                ] 
            values |> store.Insert
            let routines = store.GetCommandContract<ISqlTestRoutines>()
            let actual = routines.pTable0CSelectB(50)
            let expect = values |> List.take 50
            Claim.equal expect actual


        [<Fact>]
        let ``Executed [SqlTest].[pTable0DInsert] procedure``() =
            DataObjectName("SqlTest", "Table0D")  |> store.TrunctateTable
            let records =
                [|for i in 1..100 -> TableType01(Col01 = i, Col02 = i.ToString(), Col03 = (i |> int16))|] 
            store.GetCommandContract<ISqlTestRoutines>().pTable0DInsert(records)
            ()
            
        
