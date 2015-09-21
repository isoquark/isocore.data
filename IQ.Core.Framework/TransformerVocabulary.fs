// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System

open FSharp.Text.RegexProvider

[<AutoOpen>]
module TransformerVocabulary =
        

    [<Literal>]
    let  DefaultTransformerCategory = "Default"

    [<Literal>]
    let private IdRegexText = @"(?<Category>[^:]*):(?<SrcType>.+?(?=-->))-->(?<DstType>(.)*)"
    [<RegexExample("somecategory:type+1-->type+2")>]
    type private IdRegex = Regex<IdRegexText>
       
               
    /// <summary>
    /// Applied to a function to identify it as a transformation
    /// </summary>
    [<AttributeUsage(AttributeTargets.Method)>]
    type TransformationAttribute(category) =
        inherit Attribute()
        
        new() =
            TransformationAttribute(String.Empty)
        
        member this.Category = if category |> String.IsNullOrWhiteSpace then DefaultTransformerCategory else category



    /// <summary>
    /// Encapsulates Transformer configuration parameters
    /// </summary>
    type TransformerConfig = TransformerConfig of searchElements : ClrElementQuery list * category : string option
    with
        
        /// <summary>
        /// The assemblies that will be searched for converters
        /// </summary>
        member this.SearchElements = match this with TransformerConfig(searchElements=x) -> x
        
        /// <summary>
        /// The name of the conversion category if specified; otherwise the default category is assumed
        /// </summary>
        member this.Category = match this with TransformerConfig(category=x) -> x


    type TransformationUndefinedException(srcType, dstType) =
        inherit Exception()
        member this.SrcTpye : Type = srcType
        member this.DstType : Type = dstType
        override this.ToString() =
            sprintf "A transformation from %s to %s does not exist" srcType.FullName dstType.FullName
        override this.Message = this.ToString()



 module TransformationIdentifier =
    let create<'TSrc,'TDst>(category) =
        TransformationIdentifier(category, typeof<'TSrc>.TypeName, typeof<'TDst>.TypeName)

    let createDefault<'TSrc,'TDst>() =
        create<'TSrc,'TDst>(DefaultTransformerCategory)


