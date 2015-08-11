// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework.Contracts

open System

/// <summary>
/// Indentifies a data conversion operation
/// </summary>
type TransformationIdentifier = TransformationIdentifier of category : String * srcType : ClrTypeName * dstType : ClrTypeName
with
    member this.Category = match this with TransformationIdentifier(category=x) ->x
    member this.SrcType = match this with TransformationIdentifier(srcType=x) -> x
    member this.DstType = match this with TransformationIdentifier(dstType=x) ->x
//    override this.ToString() =
//        sprintf "%s:%s-->%s" this.Category this.DstType.SimpleName this.SrcType.SimpleName

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
    /// Converts an array of possibly heterogenous source values to an array of possibly heterogenous 
    /// target values
    /// </summary>
    abstract TransformArray: dstTypes : Type[] -> srcValues : obj[] -> obj[]
        
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


