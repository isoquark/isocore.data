namespace IQ.Core.Framework

open System
open System.Text.RegularExpressions


/// <summary>
/// Defines utility operations for working with text
/// </summary>
module Txt =
    
    /// <summary>
    /// Specifies a concrete example of a regular expression
    /// </summary>
    type internal RegexExampleAttribute(text) =
        inherit Attribute()
        
        /// <summary>
        /// Gets the example text
        /// </summary>
        member this.Text : string = text    

    /// <summary>
    /// Records generally-applicable regular expressions
    /// </summary>
    module StockRegularExpressions =
        /// <summary>
        /// Regular expression for an assembly-qualified type name
        /// </summary>
        [<Literal>]
        [<RegexExample("System.Xml.NameTable, System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>]
        let AssemblyQualifiedTypeName = @"(?<TypeName>[^,]*),[\s?](?<ShortAssemblyName>[^,]*),[\s?]Version=(?<Version>[^,]*),[\s?]Culture=(?<Culture>[^,]*),[\s?]PublicKeyToken=(?<PublicKeyToken>(.)*)"
        /// <summary>
        /// Regular expression for a full assembly name
        /// </summary>
        [<Literal>]
        [<RegexExample("System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>]
        let FullAssemblyName = @"[(?<ShortAssemblyName>[^,]*),[\s?]Version=(?<Version>[^,]*),[\s?]Culture=(?<Culture>[^,]*),[\s?]PublicKeyToken=(?<PublicKeyToken>(.)*)"
    
    /// <summary>
    /// Retrieves all text to the right of a specified marker; if no marker is found, returns the empty string
    /// </summary>
    /// <param name="marker">The marker to search for</param>
    /// <param name="text">The text to search</param>
    let rightOf (marker : string) (text : string) =
        let idx = text.IndexOf(marker) 
        if idx <> -1 then
            (idx + marker.Length) |> text.Substring
        else
            String.Empty

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
    let containsCharacter (c : string) (text : string) =
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
    let private attemptGroupMatches (groupNames : string list) (m : Match) =
        groupNames |> List.map( fun groupName -> 
                        let group = m.Groups.[groupName]
                        if group.Success |> not then
                            ArgumentException() |> raise                
                        groupName, group.Value
                     ) |> Map.ofList
    
    /// <summary>
    /// Matches on a list of group names using a supplied regular expression and correlates
    /// a map that correlates group names with their values. If not all groups can be matched,
    /// None is returned
    /// </summary>
    /// <param name="groupNames">The group names</param>
    /// <param name="expression">The regular expression</param>
    /// <param name="text">The text to be searched/matched</param>
    let tryMatchGroups (groupNames : string list)  (expression : string) text =
        let m = Regex(expression).Match(text)
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
    let matchRegexGroups (groupNames : string list)  (expression : string) text =
        //TODO: The regular expression could be compiled/cached to improve performance
        let m = Regex(expression).Match(text)
        if m.Success |> not then
            ArgumentException() |> raise
            
        m |> attemptGroupMatches groupNames

