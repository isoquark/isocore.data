namespace IQ.Core.Data.Test

open System
open System.IO
open System.Diagnostics
open System.Data

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data

open IQ.Core.Data.Test.ProxyTestCases

[<TestContainer>]
module ``Proxy Discovery`` =

    [<AutoOpen>]
    module private Proxies =        
        type RecordA = {
            AField1 : int
            AField2 : bool
            AField3 : int64 option        
        }

    [<Test>]
    let ``Read table proxy description - no attribution``() =
        let proxy = tableproxy<RecordA>
        
        let tableExpect = {
            TableDescription.Name = DataObjectName("Proxies", typeof<RecordA>.Name)
            Description = None
            Columns = 
            [
                { 
                  Name = (propname<@ fun (x : RecordA) -> x.AField1 @>).Text
                  Position = 0
                  StorageType = Int32Storage
                  Nullable = false  
                  AutoValue = None              
                }
                { 
                  Name = (propname<@ fun (x : RecordA) -> x.AField2 @>).Text
                  Position = 1
                  StorageType = BitStorage
                  Nullable = false                
                  AutoValue = None              
                }
                { 
                  Name = (propname<@ fun (x : RecordA) -> x.AField3 @>).Text
                  Position = 2
                  StorageType = Int64Storage
                  Nullable = true
                  AutoValue = None              
                }
            ]
        }
        let tableActual = proxy.DataElement
        tableActual |> Claim.equal tableExpect
        
        let recordActual = proxy.ProxyElement
        let recordExpect = recordref<RecordA>
        recordActual |> Claim.equal typeref<RecordA>
        
        let proxyColumnsExpect = 
            [for i in 0..recordExpect.Fields.Length-1 ->
                ColumnProxyDescription(recordExpect.[i], tableExpect.[i])]
        let proxyColumsActual = proxy.Columns
        proxyColumsActual |> Claim.equal proxyColumsActual
        
    
    type private RecordB = {
        [<StorageType(StorageKind.DateTime, 5uy); Column("BField_1")>]
        BField1 : DateTime
        [<StorageType(StorageKind.DateTime, 4uy)>]
        BField2 : DateTime option
        [<StorageType(StorageKind.DateTime); Column("BField_3")>]
        BField3 : DateTime
        BField4 : DateTime
    }

    [<Test>]
    let ``Read DateTimeStorage from table proxy metadata - attribute overrides``() =
        let proxy = tableproxy<RecordB>
        proxy.Columns.Length |> Claim.equal 4

        proxy.[0].DataElement.StorageType |> Claim.equal (DateTimeStorage(5uy))
        proxy.[1].DataElement.StorageType |> Claim.equal (DateTimeStorage(4uy))
        proxy.[2].DataElement.StorageType |> Claim.equal (DateTimeStorage(StorageKind.DateTime.DefaultPrecision))
        proxy.[3].DataElement.StorageType |> Claim.equal (DateTimeStorage(StorageKind.DateTime.DefaultPrecision))


    [<Test>]
    let ``Read Column names from table proxy metadata - attribute overrides``() =
        let proxy = tableproxy<RecordB>
        proxy.Columns.Length |> Claim.equal 4
        
        proxy.[0].DataElement.Name |> Claim.equal "BField_1"
        proxy.[1].DataElement.Name |> Claim.equal "BField2"
        proxy.[2].DataElement.Name |> Claim.equal "BField_3"
        proxy.[3].DataElement.Name |> Claim.equal "BField4"
    
    [<Test>]
    let ``Described [SqlTest].[pTable02Insert] procedure from proxy``() =
            
        let procName = thisMethod() |> ProxyTestCaseMethod.getDbObjectName
        let proxies = procproxies<ISqlTestProcs>
        let proxy = proxies |> List.find(fun x -> x.DataElement.Name = procName)
        let proc = proxy.DataElement

        proc.Name |> Claim.equal procName
        proc.Parameters.Length |> Claim.equal 3

        let param01 = proc.FindParameter "col01"
        param01.Direction |> Claim.equal ParameterDirection.Output
        param01.StorageType |> Claim.equal Int32Storage
        
        let param02 = proc.FindParameter "col02"
        param02.Direction |> Claim.equal ParameterDirection.Input
        param02.StorageType |> Claim.equal (DateTimeStorage(7uy))

        let param03 = proc.FindParameter "col03"
        param03.Direction |> Claim.equal ParameterDirection.Input
        param03.StorageType |> Claim.equal Int64Storage        


    [<Table("MySchema")>]
    type private RecordA = {
        ``Field 01`` : int
        ``Field 02`` : string
        ``Field 03`` : decimal
    }


    module private ModuleA =
        type RecordA = {
            Field01 : int64
            Field02 : int16
            Field03 : int8
        }
    
    [<Schema("MySchema")>]
    module private ModuleB =
        type RecordA = {
            Field01 : int64
            Field02 : int16
            Field03 : int8
        }

    [<Test>]
    let ``Inferred schema name from table proxy``() =
        typeref<RecordA> |> ClrElement.fromTypeRef |> DataProxyMetadata.inferSchemaName |> Claim.equal "MySchema"


    [<Test>]
    let ``Inferred schema name from enclosing module``() =
        ClrElement.fromType<RecordA> |>  DataProxyMetadata.inferSchemaName |> Claim.equal "MySchema"
        ClrElement.fromType<ModuleA.RecordA> |> DataProxyMetadata.inferSchemaName |> Claim.equal "ModuleA"
        ClrElement.fromType<ModuleB.RecordA> |> DataProxyMetadata.inferSchemaName |> Claim.equal "MySchema"

    [<Test>]
    let ``Inferred data object names``() =
        ClrElement.fromType<RecordA> |>  DataProxyMetadata.inferDataObjectName |> Claim.equal (DataObjectName("MySchema", "RecordA"))
        ClrElement.fromType<ModuleA.RecordA> |> DataProxyMetadata.inferDataObjectName |> Claim.equal (DataObjectName("ModuleA", "RecordA"))
        ClrElement.fromType<ModuleB.RecordA> |> DataProxyMetadata.inferDataObjectName |> Claim.equal (DataObjectName("MySchema", "RecordA"))

//    [<Test>]
//    let ``Described [SqlTest].[fTable04Before] table function from proxy``() =
//        let dbFunctionName = thisMethod() |> ProxyTestCaseMethod.getDbObjectName
//        let clrMethodName = BasicElementName("fTable04Before")
//        let clrMethod = interfaceref<ISqlTestFunctions>.Members 
//                      |> List.find(fun m -> m.Name = clrMethodName)
//                      |> fun x ->
//                         match x with
//                         |MethodReference(y) -> y
//                         |_ ->
//                            failwith "Not a method"
//        let dbFunctionProxy = clrMethod |> MethodElement |> DataProxyMetadata.describeTableFunctionProxy
//                              
//
//        ()
