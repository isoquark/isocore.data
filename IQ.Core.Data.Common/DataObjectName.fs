// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Behavior

open IQ.Core.Contracts
open IQ.Core.Framework


open System
open System.Reflection
open System.Data
open System.Diagnostics
open System.Text.RegularExpressions

open IQ.Core.Data.Contracts

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

    let fuzzyParse (text : string) =
        if text.Contains("[") |> not then
            DataObjectName(String.Empty, text)
        else
            let text = text |> Txt.betweenMarkers "[" "]" true 
            let m = QualifiedDataObjectNameRegex().Match(text)
            DataObjectName(m.Schema.Value, m.Name.Value)


[<AutoOpen>]
module DataObjectNameExtensions =
    type DataObjectName with
        /// <summary>
        /// Specifies the name of the schema (or namescope such as a package or namespace)
        /// </summary>
        member this.SchemaName = match this with DataObjectName(SchemaName=x) -> x
        
        /// <summary>
        /// Specifies the name of the object relative to the schema
        /// </summary>
        member this.LocalName = match this with DataObjectName(LocalName=x) -> x
    