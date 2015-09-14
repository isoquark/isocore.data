// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Test

open System
open System.IO
open System.Diagnostics
open System.Data

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data



open IQ.Core.Framework.Test

open TestProxies

module DataProxyMetadata =

    [<AutoOpen>]
    module private Proxies =        
        type RecordA = {
            AField1 : int
            AField2 : bool
            AField3 : int64 option        
        }



    module private ModuleA =
        type RecordA = {
            Field01 : int64
            Field02 : int16
            Field03 : int8
        }
    
    [<Schema("MySchema")>]
    module ModuleB =
        type RecordA = {
            Field01 : int64
            Field02 : int16
            Field03 : int8
        }



    module ModuleC =
        type RecordC = {
            ``Field 01`` : int
            ``Field 02`` : string
            ``Field 03`` : decimal
        }

    type Tests(ctx, log) =
        inherit ProjectTestContainer(ctx,log)
        let pmp = DataProxyMetadataProvider.get()

        [<Fact>]
        let ``Read table proxy description - no attribution``() =
            let proxy = tableproxy<RecordA> 
        
            let tableName = DataObjectName("Proxies", typeof<RecordA>.Name)
            let tableExpect = {
                TableDescription.Name = tableName
                Documentation = String.Empty
                Properties = []
                Columns = 
                [
                    { 
                      Name = (propname<@ fun (x : RecordA) -> x.AField1 @>).Text
                      Position = 0
                      Documentation = String.Empty
                      DataType = Int32DataType
                      Nullable = false  
                      AutoValue = AutoValueKind.None
                      ParentName = tableName
                      Properties = []
                    }
                    { 
                      Name = (propname<@ fun (x : RecordA) -> x.AField2 @>).Text
                      Position = 1
                      Documentation = String.Empty
                      DataType = BitDataType
                      Nullable = false                
                      AutoValue = AutoValueKind.None
                      ParentName = tableName
                      Properties = []
                    }
                    { 
                      Name = (propname<@ fun (x : RecordA) -> x.AField3 @>).Text
                      Position = 2
                      Documentation = String.Empty
                      DataType = Int64DataType
                      Nullable = true
                      AutoValue = AutoValueKind.None  
                      ParentName = tableName
                      Properties = []
                    }
                ] 
            }
            let tableActual = proxy.DataElement
            tableActual.Columns.Length |> Claim.equal tableExpect.Columns.Length
            tableActual.Columns.[0] |> Claim.equal  tableExpect.Columns.[0]
            tableActual.Columns.[1] |> Claim.equal  tableExpect.Columns.[1]
            tableActual.Columns.[2] |> Claim.equal  tableExpect.Columns.[2]
        
            let recordActual = proxy.ProxyElement
            let recordExpect = typeinfo<RecordA>
            recordActual |> Claim.equal typeinfo<RecordA>

            let proxyColumnsExpect = recordExpect.Properties |> List.mapi(fun pos p -> ColumnProxyDescription(p, tableExpect.[pos]))
                
            let proxyColumsActual = proxy.Columns
            proxyColumsActual |> Claim.equal proxyColumsActual
        
    
        [<Fact>]
        let ``Described [SqlTest].[pTable02Insert] procedure from proxy``() =
            
            let procName = thisMethod().Name |> DataObjectName.fuzzyParse
            let proxies = routineproxies<ISqlTestRoutines>
            let proxy = proxies |> List.find(fun x -> x.DataElement.Name = procName) 
            let proc = proxy.DataElement

            proc.Name |> Claim.equal procName
            proc.Parameters.Count |> Claim.equal 3

            let param01 = proc.FindParameter "col01"
            param01.Direction |> Claim.equal RoutineParameterDirection.Output
            param01.DataType |> Claim.equal Int32DataType
        
            let param02 = proc.FindParameter "col02"
            param02.Direction |> Claim.equal RoutineParameterDirection.Input
            param02.DataType |> Claim.equal (DateTimeDataType(27uy,7uy))

            let param03 = proc.FindParameter "col03"
            param03.Direction |> Claim.equal RoutineParameterDirection.Input
            param03.DataType |> Claim.equal Int64DataType       



        [<Fact>]
        let ``Inferred schema name from table proxy``() =
            typeinfo<ModuleB.RecordA> |> TypeElement |> DataProxyMetadata.inferSchemaName |> Claim.equal "MySchema"


        [<Fact>]
        let ``Inferred schema name from enclosing module``() =
            typeinfo<ModuleB.RecordA> |> TypeProxy.inferSchemaName |> Claim.equal "MySchema"
            typeinfo<ModuleC.RecordC> |> TypeProxy.inferSchemaName |> Claim.equal "ModuleC"
            

        [<Fact>]
        let ``Inferred data object names``() =
            typeinfo<ModuleB.RecordA> |> TypeProxy.inferDataObjectName |> Claim.equal (DataObjectName("MySchema", "RecordA"))
            typeinfo<ModuleA.RecordA> |> TypeProxy.inferDataObjectName |> Claim.equal (DataObjectName("ModuleA", "RecordA"))

        [<Fact>]
        let ``Described [SqlTest].[fTable04Before] table function from proxy``() =
            let dbElementName = thisMethod().Name |> DataObjectName.fuzzyParse
            dbElementName |> Claim.equal (DataObjectName("SqlTest", "fTable04Before"))
            let m = typeinfo<ISqlTestRoutines> |> fun x -> x.Methods |> List.find(fun m -> m.Name = ClrMemberName(dbElementName.LocalName))
        
            let proxy = m |> TableFunctionProxy.describe

            proxy.CallProxy.DataElement.Name |> Claim.equal dbElementName
            proxy.CallProxy.Parameters.Length |> Claim.equal 1
            let param1 = proxy.CallProxy.Parameters.[0]
            param1.DataElement.Name |> Claim.equal "startDate"
            param1.DataElement.Position |> Claim.equal 0
            param1.DataElement.DataType |> Claim.equal (DateTimeDataType(27uy,7uy))

            let resultProxy = proxy.ResultProxy
            resultProxy.Columns.Length |> Claim.equal 4
            resultProxy.Columns.[0].DataElement.Name |> Claim.equal "Id"
            resultProxy.Columns.[1].DataElement.Name |> Claim.equal "Code"
            resultProxy.Columns.[2].DataElement.Name |> Claim.equal "StartDate"
            resultProxy.Columns.[3].DataElement.Name |> Claim.equal "EndDate"

        [<Fact>]
        let ``Parsed semantic representations of DataObjectName``() =
            "(Some Schema,Some Object)" |> DataObjectName.parse |> Claim.equal (DataObjectName("Some Schema", "Some Object"))
            "(,X)" |> DataObjectName.parse |> Claim.equal (DataObjectName("", "X"))

        [<Fact>]
        let ``Correctly failed when attempting to parse semenantic representations of DataObjectName``() =
            (fun () -> "(Some Object)" |> DataObjectName.parse |> ignore ) |> Claim.failWith<ArgumentException>
            (fun () -> "SomeObject," |> DataObjectName.parse |> ignore ) |> Claim.failWith<ArgumentException>
            (fun () -> "(SomeSchema,)" |> DataObjectName.parse |> ignore ) |> Claim.failWith<ArgumentException>

        [<Fact>]
        let ``Inferred column characteristics from proxy type``() =
            let description = pmp.DescribeTableProxy<Table10>()
            
            //The inferred types should match what is specified in the typeMap value of the DataProxyMetadata module
            let col= description.Columns.[0].DataElement
            col.DataType |> Claim.equal Int32DataType
            col.Nullable |> Claim.isFalse
            col.Position |> Claim.equal 0

            let col = description.Columns.[1].DataElement
            col.DataType |> Claim.equal Int64DataType
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal 1

            let col = description.Columns.[2].DataElement
            col.DataType |> Claim.equal BinaryMaxDataType
            //col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal 2

            let col = description.Columns.[3].DataElement
            col.DataType |> Claim.equal BitDataType
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal 3

            let col = description.Columns.[4].DataElement
            col.DataType |> Claim.equal (UnicodeTextVariableDataType(250))
            //col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal 4

            let col = description.Columns.[5].DataElement
            col.DataType |> Claim.equal (DateTimeDataType(27uy,7uy))
            //col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal 5
            
            let col = description.Columns.[6].DataElement
            col.DataType |> Claim.equal (DateTimeDataType(27uy,7uy))
            //col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal 6

            let col = description.Columns.[7].DataElement
            col.DataType |> Claim.equal (DateTimeDataType(27uy,7uy))
            //col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal 7

            ()
