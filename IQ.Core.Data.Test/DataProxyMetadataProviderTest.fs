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
            TableDescription.Name = DataObjectName("Proxies", "RecordTypeA")
            Description = None
            Columns = 
            [
                { 
                  Name = propname<@ fun (x : RecordA) -> x.AField1 @>
                  Position = 0
                  DataType = None
                  Nullable = false                
                }
                { 
                  Name = propname<@ fun (x : RecordA) -> x.AField2 @>
                  Position = 1
                  DataType = None
                  Nullable = false                
                }
                { 
                  Name = propname<@ fun (x : RecordA) -> x.AField3 @>
                  Position = 2
                  DataType = None
                  Nullable = true
                }
            ]
        }
        proxy.Table |> Claim.equal expectedTable

        ()
    

