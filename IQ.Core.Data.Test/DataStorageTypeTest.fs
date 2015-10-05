// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Test

open System
open System.Diagnostics

open IQ.Core.Data
open IQ.Core.Framework.Test


module DataType =
    
    type Tests(ctx, log) =
        inherit ProjectTestContainer(ctx,log)
        [<Fact>]
        let ``Parsed semantic representations of DataStorageType``() =
            "Bit" |> DataType.parse  |> Option.get |> Claim.equal BitDataType
            "UInt8" |> DataType.parse  |> Option.get |> Claim.equal UInt8DataType
            "BinaryVariable(350)" |>  DataType.parse |> Option.get |> Claim.equal (BinaryVariableDataType(350))
            "BinaryFixed(120)" |>  DataType.parse |> Option.get |> Claim.equal (BinaryFixedDataType(120))

        [<Fact>]
        let ``Rendered semantic representations of DataStorageType``() =       
            122 |> AnsiTextFixedDataType |> DataType.toSemanticString |> Claim.equal "AnsiTextFixed(122)"
            122 |> AnsiTextVariableDataType |> DataType.toSemanticString |> Claim.equal "AnsiTextVariable(122)"
            AnsiTextMaxDataType |> DataType.toSemanticString |> Claim.equal "AnsiTextMax"
            120 |> BinaryFixedDataType |> DataType.toSemanticString |> Claim.equal "BinaryFixed(120)"
            350 |> BinaryVariableDataType |> DataType.toSemanticString |> Claim.equal "BinaryVariable(350)"
            BinaryMaxDataType |> DataType.toSemanticString |> Claim.equal "BinaryMax"
    
