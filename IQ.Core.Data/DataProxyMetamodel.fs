namespace IQ.Core.Data

open System
open System.Data
open System.Reflection

open IQ.Core.Framework

/// <summary>
/// Defines the vocabulary for representing Data Proxy metadata
/// </summary>
[<AutoOpen>]
module DataProxyMetamodel = 

    /// <summary>
    /// Describes a column proxy
    /// </summary>
    type ColumnProxyDescription = ColumnProxyDescription of field : RecordFieldDescription * column : ColumnDescription
    with
        /// <summary>
        /// Specifies the proxy record field
        /// </summary>
        member this.ProxyField = 
            match this with ColumnProxyDescription(field=x) -> x
        /// <summary>
        /// Specifies the data column
        /// </summary>
        member this.Column = 
            match this with ColumnProxyDescription(column=x) -> x
    
    /// <summary>
    /// Describes a table proxy
    /// </summary>
    type TableProxyDescription = TableProxyDescription of record : RecordDescription * table : TableDescription * columns : ColumnProxyDescription list
    with
        /// <summary>
        /// Specifies the proxy record
        /// </summary>
        member this.ProxyRecord = 
            match this with TableProxyDescription(record=x) -> x

        /// <summary>
        /// Specifies the data table
        /// </summary>
        member this.Table =
            match this with TableProxyDescription(table=x) -> x


        member this.ProxyColumns = 
            match this with TableProxyDescription(columns=x) -> x

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
        member this.Item(i) = this.ProxyColumns.[i]

        /// <summary>
        /// Gets the name of the table represented by the proxy
        /// </summary>
        member this.TableName = this.Table.Name