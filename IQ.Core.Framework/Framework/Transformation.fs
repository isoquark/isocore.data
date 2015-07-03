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
//        static member Parse text = 
//            let m = IdRegex().Match(text)                                   
//            TransformationIdentifier(m.Category.Value, m.SrcType.Value, m.DstType)
            

     
    
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
    let create<'TSrc,'TDst>(category) =
        TransformationIdentifier(category, typeof<'TSrc>.TypeName, typeof<'TDst>.TypeName)

    let createDefault<'TSrc,'TDst>() =
        create<'TSrc,'TDst>(DefaultTransformerCategory)


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
    let inline private getTransform  srcType dstType (idx : TransformationIndex) = 
        let key = createKey srcType dstType
        idx.[key]
    let inline private putTransform key del (idx : TransformationIndex) = 
        idx.[key] <- del

    let private discover(config : TransformerConfig) =
        use context = CompositionRoot.createContext()
        let provider = context.Resolve<IClrMetadataProvider>()
        let delegateIndex = createDelegateIndex()
        let category = defaultArg config.Category DefaultTransformerCategory
        let identifiers = ResizeArray<TransformationIdentifier>()
        let handler (e : ClrElementDescription) =
            match e with
            | MemberDescription(m) ->
                match m with 
                | MethodDescription(m) ->
                    if m.IsStatic then
                        match e |> ClrElementDescription.tryGetAttributeT<TransformationAttribute> with
                        | Some(a) -> 
                            if a.Category = category then                                
                                let parameters = m.Parameters |>List.filter(fun p -> p.IsReturn = false)
                                if parameters.Length <> 1 || m.ReflectedElement.Value.ReturnType = typeof<Void> then
                                    failwith (sprintf "Method %O incorrectly identified as a conversion function" m)
                   
                                let parameter = parameters.Head.ReflectedElement |> Option.get
                                let srcType = parameter.ParameterType
                                let dstType = m.ReturnType |> Option.get |> provider.DescribeType |> fun x -> x.ReflectedElement.Value
                    
                                let del = m.ReflectedElement.Value |> createDelegate
                                let key = createKey srcType dstType
                                putTransform key del delegateIndex
                    
                                TransformationIdentifier(a.Category, srcType.TypeName, dstType.TypeName) |> identifiers.Add
                        | None ->
                            ()                                                               
                | _ -> ()
            | _ -> ()
        
        
        config.SearchAssemblies |> List.map(fun x -> x |> provider.DescribeAssembly) 
                                |> List.iter (fun x -> x |> AssemblyDescription |> ClrElementDescription.walk handler)


        delegateIndex, identifiers |> List.ofSeq

                   
    type private Realization(config : TransformerConfig) =
        let delegates, identifiers = config |> discover
        let category = match config.Category with | Some(c) -> c | None -> DefaultTransformerCategory
        
        
        let transform dstType srcValue =
            let srcType = srcValue.GetType()
            (getTransform srcType dstType delegates).Invoke(srcValue)
            
        interface ITransformer with
            member this.Transform dstType srcValue =               
                transform dstType srcValue
            member this.TransformMany dstType srcValues =
                if srcValues |> Seq.isEmpty
                    then Seq.empty
                else
                    let srcType = srcValues |> Seq.item 0 |> fun x -> x.GetType()
                    let t = getTransform srcType dstType delegates
                    srcValues |> Seq.map t.Invoke
            member this.GetTargetTypes srcType  = []
            member this.GetKnownTransformations() = identifiers
        interface ITypedTransformer with
            member this.Transform value = 
                value |> transform typeof<'TDst> :?> 'TDst
            member this.TransformMany values =
                [] |> Seq.ofList
           
    let internal get(config : TransformerConfig) =        
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
