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
    let ``Read data proxy metadata - no attribution``() =
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
        

        ()
    

