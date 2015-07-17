// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open IQ.Core.Contracts
open IQ.Core.Framework


open System
open System.Reflection
open System.Data
open System.Diagnostics
open System.Text.RegularExpressions


module DataObjectName =    
    /// <summary>
    /// Parses a semantic representation of a DataObjectName
    /// </summary>
    /// <param name="text">The semantic representation of a DataObjectName</param>
    [<Parser(typeof<DataObjectName>)>]
    let parse text =
        let groups = text |> Txt.matchRegexGroups ["X";"Y"] @"\((?<X>[^,]*),(?<Y>[^\)]*)\)"
        if groups?Y |> String.IsNullOrWhiteSpace then
            ArgumentException("The LocalName of the DataObject cannot be empty") |> raise
        DataObjectName(groups?X, groups?Y)

