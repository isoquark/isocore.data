﻿// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Data
open System.Reflection
open System.Diagnostics

open IQ.Core.Framework


/// <summary>
/// Defines <see cref="DataObjectProxy"/> helpers
/// </summary>
module DataObjectProxy =
    let getColumns (subject : DataObjectProxy)  =
        match subject with
        | TabularProxy(proxy) -> 
            proxy.Columns
        | ProcedureProxy(proxy) -> 
            []
        | TableFunctionProxy(proxy) ->
            proxy.ResultProxy.Columns

    let getParameters (subject : DataObjectProxy) =
        match subject with
        | TabularProxy(proxy) -> 
            []
        | ProcedureProxy(proxy) -> 
            proxy.Parameters
        | TableFunctionProxy(proxy) ->
            proxy.CallProxy.Parameters
    
    let getProxyElement (subject : DataObjectProxy) =
        match subject with
        | TabularProxy(proxy) -> 
            proxy.ProxyElement |> TypeElement
        | ProcedureProxy(proxy) -> 
            proxy.ProxyElement |> MethodMember |> MemberElement
        | TableFunctionProxy(proxy) ->
            proxy.CallProxy.ProxyElement |> MethodMember |> MemberElement

    let getDataElement (subject : DataObjectProxy) =
        match subject with
        | TabularProxy(proxy) -> 
            proxy.DataElement |> TabularObject
        | ProcedureProxy(proxy) -> 
            proxy.DataElement |> ProcedureObject
        | TableFunctionProxy(proxy) ->
            proxy.CallProxy.DataElement |> TableFunctionObject

    let unwrapTableFunctionProxy (subject : DataObjectProxy) =
        match subject with
        | TableFunctionProxy(proxy) -> proxy
        | _ ->
            ArgumentException() |> raise
        
    let unwrapTableProxy (subject : DataObjectProxy) =
        match subject with
        | TabularProxy(proxy) -> proxy
        | _ ->
            ArgumentException() |> raise
    
    let unwrapProcedureProxy (subject : DataObjectProxy) =
        match subject with
        | ProcedureProxy(proxy) -> proxy
        | _ ->
            ArgumentException() |> raise
                
/// <summary>
/// Defines operators and augmentations for the types in the DataProxyMetamodel module
/// </summary>
[<AutoOpen>]
module DataProxyExtensions =
    type DataObjectProxy
    with
        member this.Columns = this |> DataObjectProxy.getColumns
        member this.Parameters = this |> DataObjectProxy.getParameters
        member this.ProxyElement = this |> DataObjectProxy.getProxyElement
        member this.DataElement = this |> DataObjectProxy.getDataElement
    
    /// <summary>
    /// Defines augmentations for the TableProxyDescription type
    /// </summary>
    type TabularProxyDescription
    with
        /// <summary>
        /// Gets the proxy column description at a supplied ordinal position
        /// </summary>
        /// <param name="i">The column's ordinal position</param>
        member this.Item(i) = this.Columns.[i]

        /// <summary>
        /// Gets the name of the table represented by the proxy
        /// </summary>
        member this.TableName = this.DataElement.Name