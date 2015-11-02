// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Collections
open System.Reflection
open System.Collections.Generic
open System.Data
open System.Runtime.CompilerServices

open IQ.Core.Data.Behavior

[<Extension>]
module DataExtensions =
    
    /// <summary>
    /// Converts a <see cref="DataTable"/> to a Data Matrix
    /// </summary>
    /// <param name="t"></param>
    [<Extension>]
    let ToDataMatrix(t : DataTable) =  t |> DataMatrixOps.fromDataTable
