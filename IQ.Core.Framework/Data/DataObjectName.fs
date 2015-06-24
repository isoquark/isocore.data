namespace IQ.Core.Data

open IQ.Core.Framework

open System
open System.Reflection
open System.Data
open System.Diagnostics
open System.Text.RegularExpressions

[<AutoOpen>]
module DataObjectNameVocabulary =     
    /// <summary>
    /// Responsible for identifying a data object within the scope of some catalog
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type DataObjectName = DataObjectName of schemaName : string * localName : string
    with
        /// <summary>
        /// Specifies the name of the schema (or namescope such as a package or namespace)
        /// </summary>
        member this.SchemaName = match this with DataObjectName(schemaName=x) -> x
        
        /// <summary>
        /// Specifies the name of the object relative to the schema
        /// </summary>
        member this.LocalName = match this with DataObjectName(localName=x) -> x
            
        /// <summary>
        /// Renders a faithful representation of an instance as text
        /// </summary>
        member this.ToSemanticString() =
            match this with DataObjectName(s,l) -> sprintf "(%s,%s)" s l

        /// <summary>
        /// Renders a representation of an instance as text
        /// </summary>
        override this.ToString() =
            this.ToSemanticString()


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

