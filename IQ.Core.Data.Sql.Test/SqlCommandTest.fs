// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql.Test

open System

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Sql
open IQ.Core.Framework

open IQ.Core.Data.Test.TestProxies

module SqlCommandTest =

    type Tests(ctx, log) = 
        inherit ProjectTestContainer(ctx,log)
    
        let store = ctx.Store

        let allocateRange seq count = 
            (seq , count) 
            |> AllocateSequenceRange 
            |> store.ExecuteCommand
            |> fun x -> match x with | AllocateSequenceRangeResult i -> i :?> int |_ -> nosupport()
            

        [<Fact>]
        let ``Allocated sequence range``() =
            let seq = DataObjectName("SqlTest", "Seq01")
            let count = 5
            let startA = allocateRange seq count
            let startB = allocateRange seq count
            Claim.equal count (startB - startA)
            
        [<Fact>]
        let ``Created table``() =
            let storeMetadata = store.MetadataProvider
            let tableName = DataObjectName(SchemaNames.SqlTest, "TableCreateTest")
            if tableName |> storeMetadata.ObjectExists then
                tableName |> DropTable |> store.ExecuteCommand

            let tableExpect = {
                TableDescription.Name = tableName
                Documentation = String.Empty
                Properties = []
                Columns = 
                [
                    {
                        ParentName = tableName
                        Name = "Col01"
                        Position = 0
                        DataType = Int32DataType
                        Documentation = ""        
                        Nullable = false        
                        AutoValue = AutoValueKind .None
                        Properties = []
                     }
                    {
                        ParentName = tableName
                        Name = "Col02"
                        Position = 1
                        DataType = Int64DataType
                        Documentation = ""        
                        Nullable = false        
                        AutoValue = AutoValueKind .None
                        Properties = []
                     }

                ] 
            }            
            
            tableExpect |> CreateTable |> store.ExecuteCommand

            storeMetadata.RefreshCache()

            let tableActual = tableName |> storeMetadata.DescribeTable
            tableActual.Name |> Claim.equal tableExpect.Name
            tableActual.Documentation |> Claim.equal tableExpect.Documentation
            tableActual.Columns.Length |> Claim.equal tableExpect.Columns.Length           
            tableActual.Columns.[0] |> Claim.equal tableExpect.Columns.[0]
            tableActual.Columns.[1] |> Claim.equal tableExpect.Columns.[1]
            ()            
           

