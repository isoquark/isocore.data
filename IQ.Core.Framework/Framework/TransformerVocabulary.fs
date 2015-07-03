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
    /// Indentifies a data conversion operation
    /// </summary>
    type TransformationIdentifier = TransformationIdentifier of category : String * srcType : ClrTypeName * dstType : ClrTypeName
    with
        member this.Category = match this with TransformationIdentifier(category=x) ->x
        member this.SrcType = match this with TransformationIdentifier(srcType=x) -> x
        member this.DstType = match this with TransformationIdentifier(dstType=x) ->x
        override this.ToString() =
            sprintf "%s:%s-->%s" this.Category this.DstType.SimpleName this.SrcType.SimpleName
               
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
    /// Defines contract for a transformer that realizes a set of transformations in a given category
    /// </summary>
    type ITransformer =
        /// <summary>
        /// Converts a supplied value to the destination type
        /// </summary>
        /// <param name="dstType">The destination type</param>
        /// <param name="srcValue">The value to convert</param>
        abstract Transform: dstType : Type -> srcValue : obj -> obj        
        
        /// <summary>
        /// Converts a sequence of supplied values to the destination type
        /// </summary>
        /// <param name="dstType">The destination type</param>
        /// <param name="srcValue">The values to convert</param>
        abstract TransformMany: dstType : Type -> srcValues : 'TSrc seq -> obj seq

        /// <summary>
        /// Gets types into which a source type may be transformed
        /// </summary>
        /// <param name="srcType">The source type</param>
        abstract GetTargetTypes: srcType : Type -> Type list
                
        /// <summary>
        /// Gets the conversions supported by the converter
        /// </summary>
        abstract GetKnownTransformations: unit->TransformationIdentifier list


     type ITypedTransformer =
        abstract Transform<'TSrc, 'TDst> : src :'TSrc ->'TDst
        abstract TransformMany<'TSrc,'TDst> : src : 'TSrc seq -> 'TDst seq
        abstract GetTargetTypes<'TSrc> : category : string -> Type list
        abstract GetDefaultTargetType<'TSrc> : unit -> Type


    /// <summary>
    /// Encapsulates data converer configuration parameters
    /// </summary>
    type TransformerConfig = TransformerConfig of searchAssemblies : ClrAssemblyName list * category : string option
    with
        
        /// <summary>
        /// The assemblies that will be searched for converters
        /// </summary>
        member this.SearchAssemblies = match this with TransformerConfig(searchAssemblies=x) -> x
        
        /// <summary>
        /// The name of the conversion category if specified; otherwise the default category is assumed
        /// </summary>
        member this.Category = match this with TransformerConfig(category=x) -> x


 module TransformationIdentifier =
    let create<'TSrc,'TDst>(category) =
        TransformationIdentifier(category, typeof<'TSrc>.TypeName, typeof<'TDst>.TypeName)

    let createDefault<'TSrc,'TDst>() =
        create<'TSrc,'TDst>(DefaultTransformerCategory)


