// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Concurrent
open System.Collections.Generic
open System.Linq.Expressions
open System.Linq
open System.Text.RegularExpressions

open FSharp.Text.RegexProvider



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
                sprintf "Value array (length = %i) and type array (length = %i) must be of the same length" values.Length dstTypes.Length)
        values |> Array.mapi (fun i value -> value |> convert dstTypes.[i])

    
    let private createDelegate1(m : MethodInfo) =
        if not(m.IsStatic) || m.IsGenericMethod then
            nosupportd "This method only allows creating delegates for non-generic static methods"
        let parameterTypes = [| yield! m.GetParameters() |>Array.map(fun x -> x.ParameterType); yield m.ReturnType|]
        let deltype = Expression.GetDelegateType(parameterTypes)
        deltype |> m.CreateDelegate

    let private createDelegate2(m : MethodInfo) =
        if not(m.IsStatic) || m.IsGenericMethod then
            nosupportd "This method only allows creating delegates for non-generic static methods"
        let parameters = m.GetParameters() |> Array.map(fun p -> Expression.Parameter(p.ParameterType, p.Name)) 
        let call = Expression.Call(null, m,parameters |> Array.map(fun p -> p :> Expression))
        Expression.Lambda(call,parameters).Compile()
    
  
    /// <summary>
    /// Creates a lambda expression compiled into a delegate for transformations of the 
    /// form T:A->B where A and B are any types. The resulting signature of the 
    /// delegate is of the form T:obj->obj
    /// </summary>
    /// <param name="m">The (static) method that will be executed when the delegate is invoked</param>
    /// TODO: Make this work for an arbitrary number of arguments and VOID return type
    let private createDelegate(m : MethodInfo) =
        //Define input parameter and 
        let input = Expression.Parameter(typeof<obj>, "input")
        //Cast input parameter to the type required by the method        
        let convert = Expression.Convert(input, m.GetParameters().[0].ParameterType)
        //Call the moethod
        let callresult = Expression.Call(null, m,[|convert :> Expression|])
        //Convert the result of the method to obj
        let result = Expression.Convert(callresult, typeof<obj>)
        //Create lambda function and compile into delegate
        Expression.Lambda<Func<obj,obj>>(result, input).Compile()

    type private TransformationDelegate = Func<obj,obj>           
    
    type private Key = uint64
    type private TransformationIndex = Dictionary<Key, TransformationDelegate>
    let private createDelegateIndex() = TransformationIndex()    

    let inline private createKey (srcType : Type) (dstType : Type)=
        let x = srcType.GetHashCode() |> uint64
        let y = dstType.GetHashCode() |> uint64
        let key = (x <<< 32) ||| y
        key
    
    let inline private getTransformation  srcType dstType (idx : TransformationIndex) =  
        idx.[createKey srcType dstType]
    
    let inline private putTransformation key del (idx : TransformationIndex) = 
        idx.[key] <- del
    
    let inline private hasTransformation key (idx : TransformationIndex) = 
        idx.ContainsKey(key)


    let private discover(config : TransformerConfig) =
        let delegateIndex = createDelegateIndex()
        let category = defaultArg config.Category DefaultTransformerCategory
        let identifiers = ResizeArray<TransformationIdentifier>()
        let handler (e : ClrElement) =
            match e with
            | MemberElement(m) ->
                match m with 
                | MethodMember(m) ->
                    if m.IsStatic then
                        match e |> ClrElement.tryGetAttributeT<TransformationAttribute> with
                        | Some(a) -> 
                            if a.Category = category then                                
                                let parameters = m.Parameters |>List.filter(fun p -> p.IsReturn = false)
                                if parameters.Length <> 1 || m.ReflectedElement.Value.ReturnType = typeof<Void> then
                                    failwith (sprintf "Method %s incorrectly identified as a conversion function" m.Name.Text)
                   
                                let parameter = parameters.Head.ReflectedElement |> Option.get
                                let srcType = parameter.ParameterType
                                let dstType = m.ReturnType |> Option.get |> config.MetadataProvider.FindType |> fun x -> x.ReflectedElement.Value
                    
                                let del = m.ReflectedElement.Value |> createDelegate
                                let key = createKey srcType dstType
                                putTransformation key del delegateIndex
                    
                                TransformationIdentifier(a.Category, srcType.TypeName, dstType.TypeName) |> identifiers.Add
                        | None ->
                            ()                                                               
                | _ -> ()
            | _ -> ()
        
        [for q in config.SearchElements do 
            yield! q |> config.MetadataProvider.FindElements] |> List.iter (fun x -> x |> ClrElement.walk handler)
            
        delegateIndex, identifiers |> List.ofSeq
                   
    type private Realization(config : TransformerConfig) =
        let delegates, identifiers = config |> discover
        let category = match config.Category with | Some(c) -> c | None -> DefaultTransformerCategory
        
        
        let transform dstType srcValue =
            let srcType = srcValue.GetType()
            try
                let transformation = getTransformation srcType dstType delegates
                transformation.Invoke(srcValue)
            with
                | :? KeyNotFoundException as e ->   
                    TransformationUndefinedException(srcType, dstType) |> raise
        
        let canTransform srcType dstType =
            let key = dstType |> createKey srcType            
            delegates |> hasTransformation key 
            
        interface ITransformer with
            member this.Transform dstType srcValue =               
                transform dstType srcValue
            
            member this.TransformMany dstType srcValues =
                if srcValues |> Seq.isEmpty
                    then Seq.empty
                else
                    let srcType = srcValues |> Seq.item 0 |> fun x -> x.GetType()
                    let t = getTransformation srcType dstType delegates
                    srcValues |> Seq.map t.Invoke
            
            member this.GetTargetTypes srcType  = []
            
            member this.GetKnownTransformations() = identifiers
            
            member this.CanTransform srcType dstType =
                dstType |> canTransform srcType 

            member this.AsTyped() = this :> ITypedTransformer
        interface ITypedTransformer with
            member this.Transform value = 
                value |> transform typeof<'TDst> :?> 'TDst
            
            member this.TransformMany values =
                [] |> Seq.ofList
            
            member this.GetTargetTypes<'TSrc> category = nosupport()
            
            member this.GetKnownTransformations() = identifiers
            
            member this.CanTransform<'TSrc,'TDst>() = 
                typeof<'TDst> |> canTransform typeof<'TSrc>
            
            member this.AsUntyped() = this :> ITransformer
           
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
