// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Diagnostics

open IQ.Core.Framework

/// <summary>
/// Describes a column proxy
/// </summary>
type ColumnProxyDescription = ColumnProxyDescription of field : ClrProperty * dataElement : ColumnDescription
with
    /// <summary>
    /// Specifies the proxy record field
    /// </summary>
    member this.ProxyElement = 
        match this with ColumnProxyDescription(field=x) -> x
        
    /// <summary>
    /// Specifies the data column
    /// </summary>
    member this.DataElement = 
        match this with ColumnProxyDescription(dataElement=x) -> x

/// <summary>
/// Describes a proxy for a tabular result set
/// </summary>
type TableFunctionResultProxyDescription = TabularResultProxyDescription of proxy : ClrType  * dataElement : TableFunctionDescription * columns : ColumnProxyDescription list
with
    /// <summary>
    /// Specifies the proxy record
    /// </summary>
    member this.ProxyElement = 
        match this with TabularResultProxyDescription(proxy=x) -> x

    /// <summary>
    /// Specifies the data table
    /// </summary>
    member this.DataElement =
        match this with TabularResultProxyDescription(dataElement=x) -> x

    /// <summary>
    /// Specifies the column proxies
    /// </summary>
    member this.Columns = 
        match this with TabularResultProxyDescription(columns=x) -> x

/// <summary>
/// Describes a table proxy
/// </summary>
type TabularProxyDescription = TablularProxyDescription of proxy : ClrType * dataElement : TabularDescription * columns : ColumnProxyDescription list
with
    /// <summary>
    /// Specifies the proxy record
    /// </summary>
    member this.ProxyElement = 
        match this with TablularProxyDescription(proxy=x) -> x

    /// <summary>
    /// Specifies the data table
    /// </summary>
    member this.DataElement =
        match this with TablularProxyDescription(dataElement=x) -> x

    /// <summary>
    /// Specifies the column proxies
    /// </summary>
    member this.Columns = 
        match this with TablularProxyDescription(columns=x) -> x
    

/// <summary>
/// Describes a proxy for a routine parameter
/// </summary>
[<DebuggerDisplay("{DebuggerDisplay,nq}")>]
type ParameterProxyDescription = ParameterProxyDescription of proxy : ClrMethodParameter * proxyParameterPosition : int * dataElement : RoutineParameterDescription 
with   
    /// <summary>
    /// Specifies  the CLR proxy element
    /// </summary>
    member this.ProxyElement = 
        match this  with ParameterProxyDescription(proxy=x) -> x

    /// <summary>
    /// Specifies  the data element that the proxy represents
    /// </summary>
    member this.DataElement = 
        match this with ParameterProxyDescription(dataElement=x) -> x
        
    member this.ProxyParameterPosition =
        match this with ParameterProxyDescription(proxyParameterPosition = x) -> x

    /// <summary>
    /// Formats the element for presentation in the debugger
    /// </summary>
    member private this.DebuggerDisplay = 
        sprintf "@%s %O" this.DataElement.Name this.DataElement.StorageType
                
/// <summary>
/// Describes a proxy for a stored procedure
/// </summary>
type ProcedureCallProxyDescription = ProcedureCallProxyDescription of proxy : ClrMethod * dataElement : ProcedureDescription * parameters : ParameterProxyDescription list
with
    /// <summary>
    /// Specifies  the CLR proxy element
    /// </summary>
    member this.ProxyElement = match this  with ProcedureCallProxyDescription(proxy=x) -> x

    /// <summary>
    /// Specifies  the data element that the proxy represents
    /// </summary>
    member this.DataElement = match this with ProcedureCallProxyDescription(dataElement=x) -> x

    /// <summary>
    /// Specifies the parameter proxies
    /// </summary>
    member this.Parameters = 
        match this with ProcedureCallProxyDescription(parameters=x) -> x

type ProcedureProxyDescription = ProcedureCallProxyDescription

/// <summary>
/// Describes a proxy for calling a table-valued function
/// </summary>
type TableFunctionCallProxyDescription = TableFunctionCallProxyDescription of proxy : ClrMethod * dataElement : TableFunctionDescription * parameters : ParameterProxyDescription list
with
    /// <summary>
    /// Specifies  the CLR proxy element
    /// </summary>
    member this.ProxyElement = match this  with TableFunctionCallProxyDescription(proxy=x) -> x

    /// <summary>
    /// Specifies  the data element that the proxy represents
    /// </summary>
    member this.DataElement = match this with TableFunctionCallProxyDescription(dataElement=x) -> x

    /// <summary>
    /// Specifies the parameter proxies
    /// </summary>
    member this.Parameters = 
        match this with TableFunctionCallProxyDescription(parameters=x) -> x
    
    
type TableFunctionProxyDescription = TableFunctionProxyDescription of call : TableFunctionCallProxyDescription * result : TableFunctionResultProxyDescription
with
    member this.CallProxy = match this with TableFunctionProxyDescription(call=x) -> x
    member this.ResultProxy = match this with TableFunctionProxyDescription(result=x) ->x

    /// <summary>
    /// Specifies  the data element that the proxy represents
    /// </summary>
    member this.DataElement = this.CallProxy.DataElement
    

/// <summary>
/// Unifies proxy description types
/// </summary>
type DataObjectProxy =
| TabularProxy of TabularProxyDescription
| ProcedureProxy of ProcedureProxyDescription
| TableFunctionProxy of TableFunctionProxyDescription

type IDataProxyMetadataProvider =
    abstract DescribeProxies:SqlElementKind->ClrElement->DataObjectProxy list
