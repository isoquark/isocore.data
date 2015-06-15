﻿namespace IQ.Core.Data.Test

open System
open System.Diagnostics

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data


[<TestContainer>]
module ``DataStorageType Test`` =

    [<Test>]
    let ``Parsed semantic representations of DataStorageType``() =
        "Bit" |> DataStorageType.parse  |> Option.get |> Claim.equal BitStorage
        "UInt8" |> DataStorageType.parse  |> Option.get |> Claim.equal UInt8Storage
        "BinaryVariable(350)" |>  DataStorageType.parse |> Option.get |> Claim.equal (BinaryVariableStorage(350))
        "BinaryFixed(120)" |>  DataStorageType.parse |> Option.get |> Claim.equal (BinaryFixedStorage(120))

    [<Test>]
    let ``Rendered semantic representations of DataStorageType``() =       
        350 |> BinaryVariableStorage |> fun x -> x.ToSemanticString() |> Claim.equal "BinaryVariable(350)"
        120 |> BinaryFixedStorage |> fun x -> x.ToSemanticString() |> Claim.equal "BinaryFixed(120)"