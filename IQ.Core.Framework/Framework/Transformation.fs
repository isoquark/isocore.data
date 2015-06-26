namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Concurrent
open System.Collections.Generic
open System.Linq

[<AutoOpen>]
module TransformerVocabulary =
    
    
    [<Literal>]
    let  DefaultTransformerCategory = "Default"
    
    /// <summary>
    /// Indentifies a data conversion operation
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type TransformationIdentifier = TransformationIdentifier of category : String * srcType : Type * dstType : Type
    with
        member this.Category = match this with TransformationIdentifier(category=x) ->x
        member this.SrcType = match this with TransformationIdentifier(srcType=x) -> x
        member this.DstType = match this with TransformationIdentifier(dstType=x) ->x
        override this.ToString() =
            sprintf "%s:%s -> %s" this.Category this.DstType.FullName this.SrcType.FullName
    
     
    type TransformationFunction<'TSrc,'TDst> = 'TSrc->'TDst
    
    type TransformationFunction = obj -> obj
    
    /// <summary>
    /// Applied to a function to identify it as a converter
    /// </summary>
    [<AttributeUsage(AttributeTargets.Method)>]
    type TransformationAttribute(category) =
        inherit Attribute()
        
        new() =
            TransformationAttribute(String.Empty)
        
        member this.Category = if category |> String.IsNullOrWhiteSpace then DefaultTransformerCategory else category

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
//        abstract GetTargetTypes<'TSrc> : category : string -> Type list
//        abstract GetDefaultTargetType<'TSrc> : unit -> Type


    /// <summary>
    /// Encapsulates data converer configuration parameters
    /// </summary>
    type TransformerConfig = DataConverterConfig of searchAssemblies : ClrAssemblyName list * category : string option
    with
        
        /// <summary>
        /// The assemblies that will be searched for converters
        /// </summary>
        member this.SearchAssemblies = match this with DataConverterConfig(searchAssemblies=x) -> x
        
        /// <summary>
        /// The name of the conversion category if specified; otherwise the default category is assumed
        /// </summary>
        member this.Category = match this with DataConverterConfig(category=x) -> x


module TransformationIdentifier =
    let createDefault(srcType : Type) (dstType : Type) =
        TransformationIdentifier(DefaultTransformerCategory, srcType, dstType)

    let createDefaultT<'TSrc,'TDst>() =
        TransformationIdentifier(DefaultTransformerCategory, typeof<'TSrc>, typeof<'TDst>)
    
    let createT<'TSrc,'TDst>(category) =
        TransformationIdentifier(category, typeof<'TSrc>, typeof<'TDst>)

    let create category srcType dstType =
        TransformationIdentifier(category, srcType, dstType)


/// <summary>
/// Defines generally-applicable conversion utilities
/// </summary>
module Transformer =
        
    /// <summary>
    /// Converts a value to specified type
    /// </summary>
    /// <param name="value">The value to convert</param>
    let convert (dstType : Type) (value : obj) =
        if value = null then
            null                   
        else if value.GetType() = dstType then
            value
        else
            let valueType = dstType.ItemValueType
            if dstType |> Option.isOptionType then
                if value |> Option.isOptionValue then
                    //Convert an option value to an option type
                    Convert.ChangeType(value |> Option.unwrapValue |> Option.get, valueType) |> Option.makeSome
                else
                    //Convert an non-option value to an option type; note though, special
                    //handling is required for DBNull
                    if value.GetType() = typeof<DBNull> then
                        dstType |> Option.makeNone
                    else
                        Convert.ChangeType(value, valueType) |> Option.makeSome
            else
                if value |> Option.isOptionValue then
                    //Convert an option value to a non-option type
                    Convert.ChangeType(value |> Option.unwrapValue |> Option.get, valueType)
                else
                   //Convert a non-option value to a non-option type
                    Convert.ChangeType(value, valueType)

    /// <summary>
    /// Converts a value to generically-specified type
    /// </summary>
    /// <param name="value">The value to convert</param>
    let convertT<'T> (value : obj) =
        value |> convert typeof<'T> :?> 'T

    /// <summary>
    /// Converts an array of values
    /// </summary>
    /// <param name="dstTypes">The destination types</param>
    /// <param name="values">The source values</param>
    let convertArray (dstTypes : Type[]) (values : obj[])  =
        if values.Length <> dstTypes.Length then
            raise <| ArgumentException(
                sprintf "Value array (length = %i) and type array (length = %i must be of the same length" values.Length dstTypes.Length)
        values |> Array.mapi (fun i value -> value |> convert dstTypes.[i])



    let private discover(config : TransformerConfig) =
        let functions = ConcurrentDictionary<TransformationIdentifier, obj->obj>()
        let handler (e : ClrElement) =
            if e.Kind = ClrElementKind.Method && e.IsStatic then
                match e |> ClrElement.tryGetAttributeT<TransformationAttribute> with
                | Some(a) ->
                    let m = e |> ClrElement.asMethodElement
                    let parameters = m.MethodInfo |> ClrElementProvider.getParameters
                    if parameters.Length <> 1 || m.MethodInfo.ReturnType = typeof<Void> then
                        failwith (sprintf "Method %O incorrectly identified as a conversion function" m)
                    let cid = TransformationIdentifier(a.Category, parameters.Head.ParamerInfo.ParameterType, m.MethodInfo.ReturnType)
                    //TODO: this can be done much faster by creating/caching a delegate
                    let f  srcValue  =
                        m.MethodInfo.Invoke(null, [|srcValue|])                    
                    if functions.TryAdd(cid,f) |> not then
                        failwith (sprintf "Key for %O already exists" cid)
                | _ -> ()
        
        config.SearchAssemblies |> List.map ClrElementProvider.getAssemblyElement 
                                |> List.iter (fun x -> x |> ClrElement.walk handler)
        functions
                   
    type private Realization(config : TransformerConfig) =
        let transformations = config |> discover
        let category = match config.Category with | Some(c) -> c | None -> DefaultTransformerCategory
        //let targetTypes = lazy(transformations.Keys.Select(fun x -> x.DstType).Distinct() |> List.ofSeq)
        
        let transform dstType srcValue =
            let id = TransformationIdentifier(category, srcValue.GetType(), dstType)
            let transform = transformations.[id]
            srcValue |> transform
            
        interface ITransformer with
            member this.Transform dstType srcValue =               
                transform dstType srcValue
            member this.TransformMany dstType srcValues =
                if srcValues |> Seq.isEmpty
                    then Seq.empty
                else
                    let srcType = srcValues |> Seq.nth 0 |> fun x -> x.GetType()
                    //let srcType = srcValues.GetType().GetGenericArguments() |> Seq.exactlyOne
                    let id = TransformationIdentifier(category, srcType, dstType)
                    let transform = transformations.[id]
                    srcValues |> Seq.map transform
            member this.GetTargetTypes srcType  = []
            member this.GetKnownTransformations() = transformations.Keys |> List.ofSeq
        interface ITypedTransformer with
            member this.Transform value = 
                value |> transform typeof<'TDst> :?> 'TDst
            member this.TransformMany values =
                [] |> Seq.ofList

            


    let get(config : TransformerConfig) =        
        Realization(config) :> ITransformer

        

[<AutoOpen>]
module ConvertExtensions =
    
    type Convert
    with
        /// <summary>
        /// Converts the supplied value to an optional UInt8
        /// </summary>
        /// <param name="value">The value to convert</param>
        static member ToUInt8Option(value : int option) =
            match value with
            | Some(v) -> Convert.ToByte(v) |> Some
            | None -> None
