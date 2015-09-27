// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Contracts

open System.Linq
open System.Runtime.CompilerServices

[<AutoOpen>]
module ContractExtensions =
    type  DataMatrixDescription 
    with
        member this.Name = match this with DataMatrixDescription(Name=x) -> x
        member this.Columns = match this with DataMatrixDescription(Columns=x) ->x
    
