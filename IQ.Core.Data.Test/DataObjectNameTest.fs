namespace IQ.Core.Data.Test

open System
open System.Diagnostics

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data

[<TestContainer>]
module ``DataObjectName Test`` =
    
    [<Test>]
    let ``Parsed semantic representations of DataObjectName``() =
        "(Some Schema,Some Object)" |> DataObjectName.parse |> Claim.equal (DataObjectName("Some Schema", "Some Object"))
        "(,X)" |> DataObjectName.parse |> Claim.equal (DataObjectName("", "X"))

    [<Test; FailureVerification>]
    let ``Correctly failed when attempting to parse semenantic representations of DataObjectName``() =
        (fun () -> "(Some Object)" |> DataObjectName.parse |> ignore ) |> Claim.failWith<ArgumentException>
        (fun () -> "SomeObject," |> DataObjectName.parse |> ignore ) |> Claim.failWith<ArgumentException>
        (fun () -> "(SomeSchema,)" |> DataObjectName.parse |> ignore ) |> Claim.failWith<ArgumentException>
        
        
    

