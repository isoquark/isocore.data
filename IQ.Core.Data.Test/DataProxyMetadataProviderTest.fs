namespace IQ.Core.Data.Test

open System
open System.IO
open System.Diagnostics

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data

[<TestContainer>]
module ``DataAttributeReader Test`` =

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
        
        let expectedTable = {
            TableDescription.Name = DataObjectName("Proxies", typeof<RecordA>.Name)
            Description = None
            Columns = 
            [
                { 
                  Name = propname<@ fun (x : RecordA) -> x.AField1 @>
                  Position = 0
                  StorageType = None
                  Nullable = false  
                  AutoValue = None              
                }
                { 
                  Name = propname<@ fun (x : RecordA) -> x.AField2 @>
                  Position = 1
                  StorageType = None
                  Nullable = false                
                  AutoValue = None              
                }
                { 
                  Name = propname<@ fun (x : RecordA) -> x.AField3 @>
                  Position = 2
                  StorageType = None
                  Nullable = true
                  AutoValue = None              
                }
            ]
        }
        proxy.Table |> Claim.equal expectedTable

        ()
    

