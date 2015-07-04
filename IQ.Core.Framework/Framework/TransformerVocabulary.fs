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
        
        /// <summary>
        /// Determines whether the transformer can project an instace of the source type onto the destination type
        /// </summary>
        /// <param name="srcType">The source Type</param>
        /// <param name="dstType">The destination type</param>
        abstract CanTransform : srcType : Type -> dstType : Type -> bool
        
        /// <summary>
        /// Converts to a generic version of itself
        /// </summary>
        abstract AsTyped:unit -> ITypedTransformer
     and
     ITypedTransformer =
        /// <summary>
        /// Converts a supplied value to the destination type
        /// </summary>
        /// <param name="srcValue">The value to convert</param>
        abstract Transform<'TSrc, 'TDst> : srcValue :'TSrc ->'TDst
        
        /// <summary>
        /// Converts a sequence of supplied values to the destination type
        /// </summary>
        /// <param name="dstType">The destination type</param>
        /// <param name="srcValue">The values to convert</param>
        abstract TransformMany<'TSrc,'TDst> : srcValues : 'TSrc seq -> 'TDst seq
        
        /// <summary>
        /// Gets types into which a source type may be transformed
        /// </summary>
        /// <param name="srcType">The source type</param>
        abstract GetTargetTypes<'TSrc> : category : string -> Type list
        
        /// <summary>
        /// Gets the conversions supported by the converter
        /// </summary>
        abstract GetKnownTransformations: unit->TransformationIdentifier list        

        /// <summary>
        /// Determines whether the transformer can project an instace of the source type onto the destination type
        /// </summary>
        /// <param name="srcType">The source Type</param>
        /// <param name="dstType">The destination type</param>
        abstract CanTransform<'TSrc,'TDst> :unit -> bool
        
        /// <summary>
        /// Converts to a non-generic version of itself
        /// </summary>
        abstract AsUntyped:unit->ITransformer


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


