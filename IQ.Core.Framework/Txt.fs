// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System
open System.Text
open System.Text.RegularExpressions
open System.Collections.Generic

open FSharp.Text.RegexProvider



    
/// <summary>
/// Defines utility operations for working with text
/// </summary>
module Txt =
            
    /// <summary>
    /// Finds all text to the right of the first occurrence of a specified marker; if no marker is found, returns the empty string
    /// </summary>
    /// <param name="marker">The marker to search for</param>
    /// <param name="text">The text to search</param>
    let rightOfFirst (marker : string) (text : string) =
        let idx = text.IndexOf(marker) 
        if idx <> -1 then
            (idx + marker.Length) |> text.Substring
        else
            String.Empty

    /// <summary>
    /// Finds all text to the right of the last occurrence of a specified marker; if no marker is found, returns the empty string
    /// </summary>
    /// <param name="marker">The marker to search for</param>
    /// <param name="text">The text to search</param>
    let rightOfLast (marker : string) (text : string) =
        let idx = marker |> text.LastIndexOf
        if idx <> -1 then
            (idx + marker.Length) |> text.Substring
        else
            String.Empty

    /// <summary>
    /// Finds text positioned between two character indices
    /// </summary>
    /// <param name="leftidx">The left character index</param>
    /// <param name="rightidx">The right character index</param>
    /// <param name="inclusive">Whether the returned text includes the characters living at the respective indices</param>
    /// <param name="text">The text to search</param>
    let betweenIndices (leftidx : int) (rightidx : int) (inclusive : bool) (text : string) =
        let startidx = if inclusive then leftidx else leftidx + 1
        let endidx = if inclusive then rightidx else rightidx - 1
        text.Substring(startidx, endidx - startidx + 1)

    /// <summary>
    /// Finds text positioned between the first occurrence of a left marker and the last occurrence of a right marker
    /// </summary>
    /// <param name="leftMarker">The left marker</param>
    /// <param name="rightMarker">The right marker</param>
    /// <param name="inclusive">Whether to include the markers in the resulting text</param>
    /// <param name="text"></param>
    let betweenMarkers (leftMarker : string) (rightMarker : string) (inclusive : bool) (text : string) =
        let leftMarkerIdx = leftMarker |> text.IndexOf
        let startidx = if inclusive then leftMarkerIdx else leftMarkerIdx + leftMarker.Length
        let rightMarkerIdx = rightMarker |> text.LastIndexOf
        let endidx = if inclusive then rightMarkerIdx + rightMarker.Length - 1 else rightMarkerIdx - 1
        text |> betweenIndices startidx endidx true        

    /// <summary>
    /// Removes leading and trailing whitespace
    /// </summary>
    /// <param name="text">The text to trim</param>
    let trim (text : string) =
        text.Trim()
    
    /// <summary>
    /// Partitions the string as determined by a supplied delimiter
    /// </summary>
    /// <param name="delimiter">The delimiter </param>
    /// <param name="text">The text to be partitioned</param>
    let split (delimiter : string) (text : string) =
        text.Split([|delimiter|], StringSplitOptions.None)

    /// <summary>
    /// Determines whether text contains a specified character
    /// </summary>
    /// <param name="c">The character for which to search</param>
    /// <param name="text">The text to search</param>
    let containsCharacter (c : char) (text : string) =
        text.IndexOf c <> -1

    /// <summary>
    /// Determines whether the text is contained in a collection of strings
    /// </summary>
    /// <param name="items">The collection of string to search</param>
    /// <param name="text">The text to search for</param>
    let isInSet (items : string seq) (text : string) =
        match items |> Seq.tryFind (fun item -> item = text) with
        | Some(_) -> true
        | None -> false
    
    /// <summary>
    /// Helper that creates the group name-value mapping
    /// </summary>
    /// <param name="groupNames">The group names</param>
    /// <param name="m">The match</param>
    let private attemptGroupMatches (groupNames : string seq) (m : Match) =        
        groupNames |> Seq.map( fun groupName -> 
                        let group = m.Groups.[groupName]
                        if group.Success |> not then
                            ArgumentException() |> raise                
                        groupName, group.Value
                     ) |> Map.ofSeq
    
    /// <summary>
    /// Matches on a list of group names using a supplied regular expression and correlates
    /// a map that correlates group names with their values. If not all groups can be matched,
    /// None is returned
    /// </summary>
    /// <param name="groupNames">The group names</param>
    /// <param name="expression">The regular expression</param>
    /// <param name="text">The text to be searched/matched</param>
    let tryMatchGroups (groupNames : string seq)  (expression : string) text =
        let m = System.Text.RegularExpressions.Regex(expression).Match(text)
        if m.Success |> not then
            None
        else            
            try
                m |> attemptGroupMatches groupNames |> Some
            with
                e ->
                    None
                

    /// <summary>
    /// Matches on a list of group names using a supplied regular expression and correlates
    /// a map that correlates group names with their values. If not all groups can be matched
    /// an exception is raised
    /// </summary>
    /// <param name="groupNames">The group names</param>
    /// <param name="expression">The regular expression</param>
    /// <param name="text">The text to be searched/matched</param>
    let matchRegexGroups (groupNames : string seq)  (expression : string) text =
        //TODO: The regular expression could be compiled/cached to improve performance
        let m = System.Text.RegularExpressions.Regex(expression).Match(text)
        if m.Success |> not then
            ArgumentException() |> raise
            
        m |> attemptGroupMatches groupNames

    /// <summary>
    /// Creates a delimited block of text from supplied components
    /// </summary>
    /// <param name="delimiter">The delimiter use to demarcate the components</param>
    /// <param name="components">The components</param>
    let delimit (delimiter : string) (components : string seq) =
        let components = components |> List.ofSeq
        let sb = new StringBuilder()
        for i in 0..components.Length-1 do
            components.[i] |> sb.Append |> ignore
            if i <> components.Length - 1 then
                delimiter |> sb.Append |> ignore         
        sb.ToString()               

    /// <summary>
    /// Removes all occurrences of a specified string from the subject string
    /// </summary>
    /// <param name="textToRemove">The text that will be removed</param>
    /// <param name="text">The text that will be searched</param>
    let remove textToRemove (text : string) =
        text.Replace(textToRemove, String.Empty)

    /// <summary>
    /// Removes all occurrences of a specified character from the subject string
    /// </summary>
    /// <param name="charToRemove">The character that will be removed</param>
    /// <param name="text">The text that will be searched</param>
    let removeChar (charToRemove : char) (text : string) =
        text |> remove (charToRemove.ToString())

    /// <summary>
    /// Encloses the text between a left and right marker
    /// </summary>
    /// <param name="left">The left marker</param>
    /// <param name="right">The right marker</param>
    /// <param name="text">The text to enclose</param>
    let enclose left right text =
        sprintf "%s%s%s" left text right      

    /// <summary>
    /// Determine whether text starts with a specified substring
    /// </summary>
    /// <param name="start">The text to search form</param>
    /// <param name="text">The text to search</param>
    let startsWith start (text : string) =
        start |> text.StartsWith


/// <summary>
/// Specifies a concrete example of a regular expression
/// </summary>
type RegexExampleAttribute(text) =
    inherit Attribute()
        
    /// <summary>
    /// Gets the example text
    /// </summary>
    member this.Text : string = text    

/// <summary>
/// Defines commonly used regular expressions
/// </summary>
[<AutoOpen>]
module CommonRegex =
    [<Literal>]
    let private AssemblyQualifiedTypeName = @"(?<TypeName>[^,]*),[\s?](?<ShortAssemblyName>[^,]*),[\s?]Version=(?<Version>[^,]*),[\s?]Culture=(?<Culture>[^,]*),[\s?]PublicKeyToken=(?<PublicKeyToken>(.)*)"
    
    [<Literal>]
    let private FullAssemblyName = @"[\s?](?<ShortAssemblyName>[^,]*),[\s?]Version=(?<Version>[^,]*),[\s?]Culture=(?<Culture>[^,]*),[\s?]PublicKeyToken=(?<PublicKeyToken>(.)*)"

    [<Literal>]
    let private QualifiedDataObjectName = @"(((\[?(?<Catalog>[\w]+)\]?)?\.)?(\[?(?<Schema>[\w]+)\]?)?\.)?\[?(?<Name>[\w]+)\]?"

    /// <summary>
    /// Parses an assembly qualified type name
    /// </summary>
    [<RegexExample("System.Xml.NameTable, System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>]
    type AssemblyQualifiedTypeNameRegex = Regex<AssemblyQualifiedTypeName>
    
    /// <summary>
    /// Parses a full assembly name
    /// </summary>
    [<RegexExample("System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>]
    type FullAssemblyNameRegex = Regex<FullAssemblyName>

    /// <summary>
    /// Parses a qualified data object name
    /// </summary>
    [<RegexExample("[CatalogName].[SchemaName].[ObjectName]")>]
    type QualifiedDataObjectNameRegex = Regex<QualifiedDataObjectName>


