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
        let proxy = tabularproxy<RecordA> 
        
        let tableExpect = {
            TabularDescription.Name = DataObjectName("Proxies", typeof<RecordA>.Name)
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
        let recordExpect = typeref<RecordA>
        recordActual |> Claim.equal typeref<RecordA>
        
        let proxyColumnsExpect = 
            match recordExpect with
            | RecordTypeReference(subject, fields) ->
                [for i in 0..fields.Length-1 ->
                    ColumnProxyDescription(fields.[i], tableExpect.[i])]
            | _ ->
                NotSupportedException() |> raise
        
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
        let proxy = tabularproxy<RecordB> 
        proxy.Columns.Length |> Claim.equal 4

        proxy.[0].DataElement.StorageType |> Claim.equal (DateTimeStorage(5uy))
        proxy.[1].DataElement.StorageType |> Claim.equal (DateTimeStorage(4uy))
        proxy.[2].DataElement.StorageType |> Claim.equal (DateTimeStorage(StorageKind.DateTime.DefaultPrecision))
        proxy.[3].DataElement.StorageType |> Claim.equal (DateTimeStorage(StorageKind.DateTime.DefaultPrecision))


    [<Test>]
    let ``Read Column names from table proxy metadata - attribute overrides``() =
        let proxy = tabularproxy<RecordB> 
        proxy.Columns.Length |> Claim.equal 4
        
        proxy.[0].DataElement.Name |> Claim.equal "BField_1"
        proxy.[1].DataElement.Name |> Claim.equal "BField2"
        proxy.[2].DataElement.Name |> Claim.equal "BField_3"
        proxy.[3].DataElement.Name |> Claim.equal "BField4"
    
    [<Test>]
    let ``Described [SqlTest].[pTable02Insert] procedure from proxy``() =
            
        let procName = thisMethod() |> ProxyTestCaseMethod.getDbObjectName
        let proxies = routineproxies<ISqlTestRoutines>
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
        typeref<RecordA> |> ClrElementReference.fromTypeRef |> DataProxyMetadata.inferSchemaName |> Claim.equal "MySchema"


    [<Test>]
    let ``Inferred schema name from enclosing module``() =
        ClrElementReference.fromType<RecordA> |>  DataProxyMetadata.inferSchemaName |> Claim.equal "MySchema"
        ClrElementReference.fromType<ModuleA.RecordA> |> DataProxyMetadata.inferSchemaName |> Claim.equal "ModuleA"
        ClrElementReference.fromType<ModuleB.RecordA> |> DataProxyMetadata.inferSchemaName |> Claim.equal "MySchema"

    [<Test>]
    let ``Inferred data object names``() =
        ClrElementReference.fromType<RecordA> |>  DataProxyMetadata.inferDataObjectName |> Claim.equal (DataObjectName("MySchema", "RecordA"))
        ClrElementReference.fromType<ModuleA.RecordA> |> DataProxyMetadata.inferDataObjectName |> Claim.equal (DataObjectName("ModuleA", "RecordA"))
        ClrElementReference.fromType<ModuleB.RecordA> |> DataProxyMetadata.inferDataObjectName |> Claim.equal (DataObjectName("MySchema", "RecordA"))

    [<Test>]
    let ``Described [SqlTest].[fTable04Before] table function from proxy``() =
        let dbElementName = thisMethod() |> ProxyTestCaseMethod.getDbObjectName
        dbElementName |> Claim.equal (DataObjectName("SqlTest", "fTable04Before"))
        let methodref = match typeref<ISqlTestRoutines>  with
                          | InterfaceTypeReference(subject, members) ->
                            members |>  List.find(fun m -> m.Name.Text = dbElementName.LocalName)
                            |> fun x ->
                                 match x with
                                 |MethodMemberReference(y) -> y
                                 |_ ->
                                    failwith "Not a method"
                          | _ ->
        
                            NotSupportedException() |> raise
        let proxy = methodref |> MethodMemberReference
                              |> MemberReference
                              |> DataProxyMetadata.describeTableFunctionProxy
                              |> DataObjectProxy.unwrapTableFunctionProxy

        proxy.CallProxy.DataElement.Name |> Claim.equal dbElementName
        proxy.CallProxy.Parameters.Length |> Claim.equal 1
        let param1 = proxy.CallProxy.Parameters.[0]
        param1.DataElement.Name |> Claim.equal "startDate"
        param1.DataElement.Position |> Claim.equal 0
        param1.DataElement.StorageType |> Claim.equal (DateTimeStorage(7uy))

        let resultProxy = proxy.ResultProxy
        resultProxy.Columns.Length |> Claim.equal 4
        resultProxy.Columns.[0].DataElement.Name |> Claim.equal "Id"
        resultProxy.Columns.[1].DataElement.Name |> Claim.equal "Code"
        resultProxy.Columns.[2].DataElement.Name |> Claim.equal "StartDate"
        resultProxy.Columns.[3].DataElement.Name |> Claim.equal "EndDate"
        ()

