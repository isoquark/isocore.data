// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Contracts

open System
open System.Diagnostics
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Collections

open IQ.Core.Framework

open IQ.Core.Framework.Contracts


    

/// <summary>
/// Describes a column proxy
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
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
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        sprintf "%O : %O" this.ProxyElement this.DataElement


/// <summary>
/// Describes a proxy for a tabular result set
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type RoutineResultProxyDescription = RoutineResultProxyDescription of proxy : ClrType  * dataElement : RoutineDescription * columns : ColumnProxyDescription list
with
    /// <summary>
    /// Specifies the proxy record
    /// </summary>
    member this.ProxyElement = 
        match this with RoutineResultProxyDescription(proxy=x) -> x

    /// <summary>
    /// Specifies the data table
    /// </summary>
    member this.DataElement =
        match this with RoutineResultProxyDescription(dataElement=x) -> x

    /// <summary>
    /// Specifies the column proxies
    /// </summary>
    member this.Columns = 
        match this with RoutineResultProxyDescription(columns=x) -> x

    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        sprintf "%O : %O" this.ProxyElement this.DataElement

/// <summary>
/// Describes a table proxy
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type TableProxyDescription = TableProxyDescription of proxy : ClrType * dataElement : TableDescription * columns : ColumnProxyDescription list
with
    /// <summary>
    /// Specifies the proxy record
    /// </summary>
    member this.ProxyElement = 
        match this with TableProxyDescription(proxy=x) -> x

    /// <summary>
    /// Specifies the data table
    /// </summary>
    member this.DataElement =
        match this with TableProxyDescription(dataElement=x) -> x

    /// <summary>
    /// Specifies the column proxies
    /// </summary>
    member this.Columns = 
        match this with TableProxyDescription(columns=x) -> x
    
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        sprintf "%O : %O" this.ProxyElement this.DataElement

/// <summary>
/// Describes a table proxy
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ViewProxyDescription = ViewProxyDescription of proxy : ClrType * dataElement : ViewDescription * columns : ColumnProxyDescription list
with
    /// <summary>
    /// Specifies the proxy record
    /// </summary>
    member this.ProxyElement = 
        match this with ViewProxyDescription(proxy=x) -> x

    /// <summary>
    /// Specifies the data table
    /// </summary>
    member this.DataElement =
        match this with ViewProxyDescription(dataElement=x) -> x

    /// <summary>
    /// Specifies the column proxies
    /// </summary>
    member this.Columns = 
        match this with ViewProxyDescription(columns=x) -> x
    
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        sprintf "%O : %O" this.ProxyElement this.DataElement



/// <summary>
/// Describes a proxy for a routine parameter
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
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
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        sprintf "%O : %O" this.ProxyElement this.DataElement
                

/// <summary>
/// Describes a proxy for calling a table-valued function
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type RoutineCallProxyDescription = RoutineCallProxyDescription of proxy : ClrMethod * dataElement : RoutineDescription * parameters : ParameterProxyDescription list
with
    /// <summary>
    /// Specifies  the CLR proxy element
    /// </summary>
    member this.ProxyElement = match this  with RoutineCallProxyDescription(proxy=x) -> x

    /// <summary>
    /// Specifies  the data element that the proxy represents
    /// </summary>
    member this.DataElement = match this with RoutineCallProxyDescription(dataElement=x) -> x

    /// <summary>
    /// Specifies the parameter proxies
    /// </summary>
    member this.Parameters = 
        match this with RoutineCallProxyDescription(parameters=x) -> x
    
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        sprintf "%O : %O" this.ProxyElement this.DataElement
    
[<DebuggerDisplay("{ToString(),nq}")>]
type RoutineProxyDescription = RoutineProxyDescription of call : RoutineCallProxyDescription * result : RoutineResultProxyDescription option
with
    /// <summary>
    /// Specifies the proxy that mediates invocation, e.g., a function/method or a type with fields/properties
    /// that represent parameters
    /// </summary>
    member this.CallProxy = 
        match this with RoutineProxyDescription(call=x) -> x
    
    /// <summary>
    /// Specifies the element that represents an item the function result set
    /// </summary>
    member this.ResultProxy = 
        match this with RoutineProxyDescription(result=x) ->x

    /// <summary>
    /// Specifies  the data element that the proxy represents
    /// </summary>
    member this.DataElement = this.CallProxy.DataElement
    
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        sprintf "%O : %O" this.CallProxy this.DataElement


/// <summary>
/// Unifies proxy description types
/// </summary>
type DataObjectProxy =
| TableProxy of TableProxyDescription
| ViewProxy of ViewProxyDescription
| ProcedureProxy of RoutineProxyDescription
| TableFunctionProxy of RoutineProxyDescription

type IDataProxyMetadataProvider =
    abstract DescribeProxies:SqlElementKind->ClrElement->DataObjectProxy list
    abstract DescribeTableProxy<'T> :unit ->TableProxyDescription


type IDataMatrixConverter = 
    abstract FromProxyValues: TableProxyDescription->values : obj seq -> IDataMatrix
    abstract ToProxyValues: Type-> IDataMatrix->IEnumerable

type IDataMatrixConverter<'T> =
    abstract FromProxyValues: values : 'T seq -> IDataMatrix
    abstract ToProxyValues: IDataMatrix->'T seq
   