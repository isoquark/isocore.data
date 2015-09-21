// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Test

open System
open System.IO
open System.Diagnostics
open System.Data
open System.Linq

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
                      Name = propname<@ fun (x : RecordA) -> x.AField1 @>
                      Position = 0
                      Documentation = String.Empty
                      DataType = Int32DataType
                      DataKind = DataKind.Int32
                      Nullable = false  
                      AutoValue = AutoValueKind.None
                      ParentName = tableName
                      Properties = []
                    }
                    { 
                      Name = propname<@ fun (x : RecordA) -> x.AField2 @>
                      Position = 1
                      Documentation = String.Empty
                      DataType = BitDataType
                      DataKind = DataKind.Bit
                      Nullable = false                
                      AutoValue = AutoValueKind.None
                      ParentName = tableName
                      Properties = []
                    }
                    { 
                      Name = propname<@ fun (x : RecordA) -> x.AField3 @>
                      Position = 2
                      Documentation = String.Empty
                      DataType = Int64DataType
                      DataKind = DataKind.Int64
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
            proc.Parameters.Count() |> Claim.equal 3

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

            let resultProxy = proxy.ResultProxy.Value
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
            let description = pmp.DescribeTableProxy<Table0A>()
            

            //Col01
            let col= description.Columns.[0].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col01 @>)
            col.DataType |> Claim.equal Int32DataType
            col.Nullable |> Claim.isFalse
            col.Position |> Claim.equal 0

            //Col02
            let col = description.Columns.[1].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col02 @>)
            col.DataType |> Claim.equal Int64DataType
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal 1

            //Col03
            let col = description.Columns.[2].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col03 @>)
            col.DataType |> Claim.equal (BinaryFixedDataType(50))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal 2

            //Col04
            let col = description.Columns.[3].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col04 @>)
            col.DataType |> Claim.equal BitDataType
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal 3

            //Col05
            let col = description.Columns.[4].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col05 @>)
            col.DataType |> Claim.equal (AnsiTextFixedDataType(50))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal 4

            //Col06
            let idx = 5
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col06 @>)
            col.DataType |> Claim.equal DateDataType
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx
            
            //Col07
            let idx = 6
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col07 @>)
            col.DataType |> Claim.equal (DateTimeDataType(23uy,3uy))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col08
            let idx = 7
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col08 @>)
            col.DataType |> Claim.equal (DateTimeDataType(27uy,7uy))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col09
            let idx = 8
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col09 @>)
            col.DataType |> Claim.equal (DecimalDataType(18uy,12uy))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col10
            let idx = 9
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col10 @>)
            col.DataType |> Claim.equal Float64DataType
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col11
            let idx = 10
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col11 @>)
            col.DataType |> Claim.equal (MoneyDataType(19uy,4uy))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col12
            let idx = 11
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col12 @>)
            col.DataType |> Claim.equal (UnicodeTextFixedDataType(100))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col13
            let idx = 12
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col13 @>)
            col.DataType |> Claim.equal (DecimalDataType(15uy,5uy))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col14
            let idx = 13
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col14 @>)
            col.DataType |> Claim.equal (UnicodeTextVariableDataType(73))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col15
            let idx = 14
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col15 @>)
            col.DataType |> Claim.equal (Float32DataType)
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col16
            let idx = 15
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col16 @>)
            col.DataType |> Claim.equal (DateTimeDataType(16uy, 0uy))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col17
            let idx = 16
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col17 @>)
            col.DataType |> Claim.equal Int16DataType
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col18
            let idx = 17
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col18 @>)
            col.DataType |> Claim.equal (MoneyDataType(10uy, 4uy))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col19
            let idx = 18
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col19 @>)
            col.DataType |> Claim.equal VariantDataType
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col20
            let idx = 19
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col20 @>)
            col.DataType |> Claim.equal UInt8DataType
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col21
            let idx = 20
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col21 @>)
            col.DataType |> Claim.equal GuidDataType
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col22
            let idx = 21
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col22 @>)
            col.DataType |> Claim.equal (BinaryVariableDataType(223))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col23
            let idx = 22
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col23 @>)
            col.DataType |> Claim.equal (UnicodeTextVariableDataType(121))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col24
            let idx = 23
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col24 @>)
            col.DataType |> Claim.equal BinaryMaxDataType
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col25
            let idx = 24
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col25 @>)
            col.DataType |> Claim.equal AnsiTextMaxDataType
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx

            //Col26
            let idx = 25
            let col = description.Columns.[idx].DataElement
            col.Name |> Claim.equal (propname<@ fun (x : Table0A) -> x.Col26 @>)
            col.DataType |> Claim.equal (UnicodeTextVariableDataType(50))
            col.Nullable |> Claim.isTrue
            col.Position |> Claim.equal idx
