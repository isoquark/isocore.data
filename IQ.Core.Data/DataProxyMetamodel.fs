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
    type TabularResultProxyDescription = TabularResultProxyDescription of proxy : ClrRecordReference  * dataElement : TableFunctionDescription * columns : ColumnProxyDescription list
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
    type TableProxyDescription = TableProxyDescription of proxy : ClrRecordReference * dataElement : TableDescription * columns : ColumnProxyDescription list
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
    /// Describes a proxy for a routine parameter
    /// </summary>
    [<DebuggerDisplay("{DebuggerDisplay,nq}")>]
    type ParameterProxyDescription = ParameterProxyDescription of proxy : MethodInputOutputReference * dataElement : RoutineParameter
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
    type ProcedureProxyDescription = ProcedureProxyDescription of proxy : ClrMethodReference * dataElement : ProcedureDescription * parameters : ParameterProxyDescription list
    with
        /// <summary>
        /// Specifies  the CLR proxy element
        /// </summary>
        member this.ProxyElement = match this  with ProcedureProxyDescription(proxy=x) -> x

        /// <summary>
        /// Specifies  the data element that the proxy represents
        /// </summary>
        member this.DataElement = match this with ProcedureProxyDescription(dataElement=x) -> x

        /// <summary>
        /// Specifies the parameter proxies
        /// </summary>
        member this.Parameters = 
            match this with ProcedureProxyDescription(parameters=x) -> x

    /// <summary>
    /// Describes a proxy for a stored procedure
    /// </summary>
    type TableFunctionProxyDescription = TableFunctionProxyDescription of proxy : ClrMethodReference * dataElement : TableFunctionDescription * parameters : ParameterProxyDescription list
    
    /// <summary>
    /// Unifies all proxy description types
    /// </summary>
    type ProxyDescription =
    | ColumnProxy of ColumnProxyDescription
    | TableProxy of TableProxyDescription
    | ParameterProxy of ParameterProxyDescription
    | ProcedureProxy of ProcedureProxyDescription
    | TableFunctionProxy of TableFunctionProxyDescription
    | TableFunctionResultProxy of TabularResultProxyDescription

/// <summary>
/// Defines operators and augmentations for the types in the DataProxyMetamodel module
/// </summary>
[<AutoOpen>]
module DataProxyMetamodelExtensions =
    /// <summary>
    /// Defines augmentations for the TableProxyDescription type
    /// </summary>
    type TableProxyDescription
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