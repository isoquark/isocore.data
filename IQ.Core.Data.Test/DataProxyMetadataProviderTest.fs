namespace IQ.Core.Data.Test

open System
open System.IO
open System.Diagnostics

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data

[<TestContainer>]
module ``DataProxyMetadataProvider Test`` =

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
                  Name = propname<@ fun (x : RecordA) -> x.AField1 @>
                  Position = 0
                  StorageType = Int32Storage
                  Nullable = false  
                  AutoValue = None              
                }
                { 
                  Name = propname<@ fun (x : RecordA) -> x.AField2 @>
                  Position = 1
                  StorageType = BitStorage
                  Nullable = false                
                  AutoValue = None              
                }
                { 
                  Name = propname<@ fun (x : RecordA) -> x.AField3 @>
                  Position = 2
                  StorageType = Int64Storage
                  Nullable = true
                  AutoValue = None              
                }
            ]
        }
        let tableActual = proxy.Table
        tableActual |> Claim.equal tableExpect
        
        let recordActual = proxy.ProxyRecord
        let recordExpect = recordinfo<RecordA>
        recordActual |> Claim.equal recordExpect
        
        let proxyColumnsExpect = 
            [for i in 0..recordExpect.Fields.Length-1 ->
                ColumnProxyDescription(recordExpect.[i], tableExpect.[i])]
        let proxyColumsActual = proxy.ProxyColumns
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
        proxy.ProxyColumns.Length |> Claim.equal 4

        proxy.[0].Column.StorageType |> Claim.equal (DateTimeStorage(5uy))
        proxy.[1].Column.StorageType |> Claim.equal (DateTimeStorage(4uy))
        proxy.[2].Column.StorageType |> Claim.equal (DateTimeStorage(StorageKind.DateTime.DefaultPrecision))
        proxy.[3].Column.StorageType |> Claim.equal (DateTimeStorage(StorageKind.DateTime.DefaultPrecision))


    [<Test>]
    let ``Read Column names from table proxy metadata - attribute overrides``() =
        let proxy = tableproxy<RecordB>
        proxy.ProxyColumns.Length |> Claim.equal 4
        
        proxy.[0].Column.Name |> Claim.equal "BField_1"
        proxy.[1].Column.Name |> Claim.equal "BField2"
        proxy.[2].Column.Name |> Claim.equal "BField_3"
        proxy.[3].Column.Name |> Claim.equal "BField4"
    

