namespace IQ.Core.Framework

open System
open System.Text
open System.Text.RegularExpressions

open FSharp.Text.RegexProvider

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


