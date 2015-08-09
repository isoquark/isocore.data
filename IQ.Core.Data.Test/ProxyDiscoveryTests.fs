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

open IQ.Core.Data.Test.ProxyTestCases


open IQ.Core.Framework.Test


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


    type private RecordB = {
        [<DataTypeAttribute(DataKind.DateTime, 5uy); Column("BField_1")>]
        BField1 : BclDateTime
        [<DataTypeAttribute(DataKind.DateTime, 4uy)>]
        BField2 : BclDateTime option
        [<DataTypeAttribute(DataKind.DateTime); Column("BField_3")>]
        BField3 : BclDateTime
        BField4 : BclDateTime
    }

    module ModuleC =
        type RecordC = {
            ``Field 01`` : int
            ``Field 02`` : string
            ``Field 03`` : decimal
        }

    type Tests(ctx, log) =
        inherit ProjectTestContainer(ctx,log)

        [<Fact>]
        let ``Read table proxy description - no attribution``() =
            let proxy = tabularproxy<RecordA> 
        
            let tableExpect = {
                TabularDescription.Name = DataObjectName("Proxies", typeof<RecordA>.Name)
                Description = None
                Columns = 
                [
                    { 
                      Name = (propname<@ fun (x : RecordA) -> x.AField1 @>).Text
                      Position = 0
                      StorageType = Int32DataType
                      Nullable = false  
                      AutoValue = None              
                    }
                    { 
                      Name = (propname<@ fun (x : RecordA) -> x.AField2 @>).Text
                      Position = 1
                      StorageType = BitDataType
                      Nullable = false                
                      AutoValue = None              
                    }
                    { 
                      Name = (propname<@ fun (x : RecordA) -> x.AField3 @>).Text
                      Position = 2
                      StorageType = Int64DataType
                      Nullable = true
                      AutoValue = None              
                    }
                ]
            }
            let tableActual = proxy.DataElement
            tableActual |> Claim.equal tableExpect
        
            let recordActual = proxy.ProxyElement
            let recordExpect = typeinfo<RecordA>
            recordActual |> Claim.equal typeinfo<RecordA>

            let proxyColumnsExpect = recordExpect.Properties |> List.mapi(fun pos p -> ColumnProxyDescription(p, tableExpect.[pos]))
                
            let proxyColumsActual = proxy.Columns
            proxyColumsActual |> Claim.equal proxyColumsActual
        
    
        [<Fact>]
        let ``Read DateTimeStorage from table proxy metadata - attribute overrides``() =
            let proxy = tabularproxy<RecordB> 
            proxy.Columns.Length |> Claim.equal 4

            proxy.[0].DataElement.StorageType |> Claim.equal (DateTimeDataType(5uy))
            proxy.[1].DataElement.StorageType |> Claim.equal (DateTimeDataType(4uy))
            proxy.[2].DataElement.StorageType |> Claim.equal (DateTimeDataType(DataKind.DateTime.DefaultPrecision))
            proxy.[3].DataElement.StorageType |> Claim.equal (DateTimeDataType(DataKind.DateTime.DefaultPrecision))


        [<Fact>]
        let ``Read Column names from table proxy metadata - attribute overrides``() =
            let proxy = tabularproxy<RecordB> 
            proxy.Columns.Length |> Claim.equal 4
        
            proxy.[0].DataElement.Name |> Claim.equal "BField_1"
            proxy.[1].DataElement.Name |> Claim.equal "BField2"
            proxy.[2].DataElement.Name |> Claim.equal "BField_3"
            proxy.[3].DataElement.Name |> Claim.equal "BField4"
    
        [<Fact>]
        let ``Described [SqlTest].[pTable02Insert] procedure from proxy``() =
            
            let procName = thisMethod() |> ProxyTestCaseMethod.getDbObjectName
            let proxies = routineproxies<ISqlTestRoutines>
            let proxy = proxies |> List.find(fun x -> x.DataElement.Name = procName) 
            let proc = proxy.DataElement

            proc.Name |> Claim.equal procName
            proc.Parameters.Length |> Claim.equal 3

            let param01 = proc.FindParameter "col01"
            param01.Direction |> Claim.equal ParameterDirection.Output
            param01.StorageType |> Claim.equal Int32DataType
        
            let param02 = proc.FindParameter "col02"
            param02.Direction |> Claim.equal ParameterDirection.Input
            param02.StorageType |> Claim.equal (DateTimeDataType(7uy))

            let param03 = proc.FindParameter "col03"
            param03.Direction |> Claim.equal ParameterDirection.Input
            param03.StorageType |> Claim.equal Int64DataType       



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
            let dbElementName = thisMethod() |> ProxyTestCaseMethod.getDbObjectName
            dbElementName |> Claim.equal (DataObjectName("SqlTest", "fTable04Before"))
            let m = typeinfo<ISqlTestRoutines> |> fun x -> x.Methods |> List.find(fun m -> m.Name = ClrMemberName(dbElementName.LocalName))
        
            let proxy = m |> TableFunctionProxy.describe

            proxy.CallProxy.DataElement.Name |> Claim.equal dbElementName
            proxy.CallProxy.Parameters.Length |> Claim.equal 1
            let param1 = proxy.CallProxy.Parameters.[0]
            param1.DataElement.Name |> Claim.equal "startDate"
            param1.DataElement.Position |> Claim.equal 0
            param1.DataElement.StorageType |> Claim.equal (DateTimeDataType(7uy))

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

