namespace IQ.Core.Data

open System
open System.Data
open System.Reflection
open System.Diagnostics

open IQ.Core.Framework

/// <summary>
/// Defines the vocabulary for representing Data Proxy metadata
/// </summary>
[<AutoOpen>]
module DataProxyMetamodel = 

    /// <summary>
    /// References a method parameter or return
    /// </summary>
    type MethodInputOutputReference =
    | MethodInputReference of ClrMethodParameterReference
    | MethodOutputReference of ClrMethodReturnReference
    
    /// <summary>
    /// Describes a column proxy
    /// </summary>
    type ColumnProxyDescription = ColumnProxyDescription of field : ClrPropertyReference * dataElement : ColumnDescription
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
    type TableFunctionResultProxyDescription = TabularResultProxyDescription of proxy : ClrTypeReference  * dataElement : TableFunctionDescription * columns : ColumnProxyDescription list
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
    type TabularProxyDescription = TablularProxyDescription of proxy : ClrTypeReference * dataElement : TabularDescription * columns : ColumnProxyDescription list
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
    type ParameterProxyDescription = ParameterProxyDescription of proxy : MethodInputOutputReference * dataElement : RoutineParameterDescription
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

        /// <summary>
        /// Formats the element for presentation in the debugger
        /// </summary>
        member private this.DebuggerDisplay = 
            sprintf "@%s %O" this.DataElement.Name this.DataElement.StorageType
            

    
    /// <summary>
    /// Describes a proxy for a stored procedure
    /// </summary>
    type ProcedureCallProxyDescription = ProcedureCallProxyDescription of proxy : ClrMethodReference * dataElement : ProcedureDescription * parameters : ParameterProxyDescription list
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
    type TableFunctionCallProxyDescription = TableFunctionCallProxyDescription of proxy : ClrMethodReference * dataElement : TableFunctionDescription * parameters : ParameterProxyDescription list
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
    /// Unifies data object proxy description types
    /// </summary>
    type DataObjectProxy =
    | TabularProxy of TabularProxyDescription
    | ProcedureProxy of ProcedureProxyDescription
    | TableFunctionProxy of TableFunctionProxyDescription


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
            proxy.ProxyElement |> MethodReference |> MemberElement
        | TableFunctionProxy(proxy) ->
            proxy.CallProxy.ProxyElement |> MethodReference |> MemberElement

    let getDataElement (subject : DataObjectProxy) =
        match subject with
        | TabularProxy(proxy) -> 
            proxy.DataElement |> TablularObject
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