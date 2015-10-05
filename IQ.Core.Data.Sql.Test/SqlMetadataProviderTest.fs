// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql.Test

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Sql
open IQ.Core.Framework
open IQ.Core.Data.Contracts
open IQ.Core.Data.Test


open TestProxies

module SqlMetadataProvider =
    
    type Tests(ctx, log)= 
        inherit ProjectTestContainer(ctx,log)

        let mdp = ctx.Store.GetMetadataProvider()
        let idx =
            let reader = 
                {
                    ConnectionString = ctx.ConnectionString
                    IgnoreSystemObjects = true
                } |> SqlMetadataReader
            let catalog = reader.ReadCatalog()
            SqlMetadataIndex(catalog)
            

        [<Fact>]
        let ``Discovered Tables``() =
            let tables = [for t in idx.GetSchemaTables("SqlTest") -> t.Name, t ] |> dict

            let table = idx.GetTable(objectname<Table01>)          
            table.["Col01"].DataKind |> Claim.equal DataKind.Int32
            table.["Col01"].DataType |> Claim.equal Int32DataType
            
            table.["Col02"].DataKind |> Claim.equal DataKind.Int64
            table.["Col02"].DataType |> Claim.equal Int64DataType

            table.["Col03"].DataKind |> Claim.equal DataKind.UnicodeTextVariable
            table.["Col03"].DataType |> Claim.equal (UnicodeTextVariableDataType(50))

            table.["Col04"].DataKind |> Claim.equal DataKind.UnicodeTextMax
            table.["Col04"].DataType |> Claim.equal UnicodeTextMaxDataType
                        
            table.["Col05"].DataKind |> Claim.equal DataKind.CustomPrimitive            
            table.["Col05"].DataType |> Claim.equal(
                CustomPrimitiveDataType(DataObjectName("SqlTest", "PhoneNumber"), AnsiTextVariableDataType(11)))
            

            

        [<Fact>]
        let ``Discovered Data Types``() =            
            let dataTypes = [for t in idx.GetSchemaDataTypes("SqlTest") -> t.Name, t ] |> dict            

            let name = objectname<TableType01>
            name  |> Claim.containsKey dataTypes            
            let dataType = dataTypes.[name]
            dataType.Documentation |> Claim.equal "Documentation for [SqlTest].[TableType01]"
            dataType.Columns.[0].Name |> Claim.equal "TTCol01"
            dataType.Columns.[1].Name |> Claim.equal "TTCol02"
            dataType.Columns.[2].Name |> Claim.equal "TTCol03"
            
            let name = objectname<TableType02>
            name  |> Claim.containsKey dataTypes
            let dataType = dataTypes.[name]
            dataType.Columns.[0].Name |> Claim.equal (propname<@ fun (x : TableType02) -> x.Col01 @>)

            let name = DataObjectName("SqlTest", "PhoneNumber") 
            name  |> Claim.containsKey dataTypes
            let dataType = dataTypes.[name]
            dataType.BaseTypeName.Value |> Claim.equal (DataObjectName("sys", SqlDataTypeNames.varchar))
           

