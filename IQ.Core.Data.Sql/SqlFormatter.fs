// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Sql

open System
open System.Data
open System.Data.Linq
open System.Data.SqlClient
open System.Diagnostics
open System.Text

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data

/// <summary>
/// Provides rudimentary SQL generation capabilities
/// </summary>
module SqlFormatter =    
    /// <summary>
    /// Formats a given value in a form suitable for inclusion in a SQL script
    /// </summary>
    /// <param name="value">The value to format</param>
    let formatValue (value : obj) =
        if(value <> null) then
            match value with
            | :? DBNull as x -> "null"
            | :? bool as x -> if x = true then "1" else "0"
            | :? BclDateTime as x -> sprintf "'%s'" (x.ToString("yyyy-MM-dd HH:mm:ss.fff"))
            | :? char as x -> if x = ''' then @"''" else x.ToString()
            | :? string as x -> String.Format("'{0}'", x.Replace("'", @"''"))
            | :? Guid as x -> sprintf "'%O'" x
            | _ -> value.ToString()
        else
            "null"

    /// <summary>
    /// Formats an object name in a form suitable for inclusion in a SQL script 
    /// </summary>
    /// <param name="value">The name of the data object</param>
    let formatObjectName (value : DataObjectName) =
        sprintf "[%s].[%s]" value.SchemaName value.LocalName

    /// <summary>
    /// Formats an element name, such as the name of a parameter or column, in a form suitable for inclusion
    /// in a SQL script
    /// </summary>
    /// <param name="name">The name of the element</param>
    let formatElementName name =
        name |> Txt.enclose "[" "]"         

    /// <summary>
    /// Formats a parameter name using the @-convention
    /// </summary>
    /// <param name="paramName">The name of the parameter</param>
    let formatParameterName paramName =
        if paramName |> Txt.startsWith "@" |> not then
            sprintf "@%s" paramName
        else
            paramName
    
    /// <summary>
    /// Creates a SQL SELECT statement of the form "select * from [Schema].[Function](@Param1, ..., @ParamN)
    /// </summary>
    /// <param name="f">The table-valued function</param>
    let formatTableFunctionSelect (f : TableFunctionDescription) =
        let parameters = f.Parameters 
                         |> RoList.map (fun x -> x.Name |> formatParameterName)
                         |> Txt.delimit ","
        sprintf "select * from %s(%s)" (f.Name |> formatObjectName) parameters

    /// <summary>
    /// Create a SQL TRUNCATE TABLE statement
    /// </summary>
    /// <param name="n">The name of the table to be truncated</param>
    let formatTruncateTable (n : DataObjectName) =
        n |> formatObjectName |> sprintf "truncate table %s"

    /// <summary>
    /// Formats a select statement for a tabular element
    /// </summary>
    let formatTabularSelect(t : TabularDescription) =
        let columns = t.Columns 
                    |> RoList.map(fun c -> c.Name |> formatElementName) 
                    |> Txt.delimit ","
        let tableName = t.Name |> formatObjectName
        sprintf "select %s from %s" columns tableName

    /// <summary>
    /// Formats a select statement for a tabular proxy
    /// </summary>
    let formatTabularSelectT<'T>() =
        let ptype = tabularproxy<'T>
        ptype.DataElement |> formatTabularSelect

    let private formatColumnName name =
        sprintf "[%s]" name
    
    let private formatFilter(f : ColumnFilter) =
        match f with
        | Equal(c,v) -> 
            sprintf "[%s] = %s" c <| formatValue(v)
        | NotEqual(c, v) -> 
            sprintf "[%s] <> %s" c <| formatValue(v)
        | GreaterThan(c, v) -> 
            sprintf "[%s] > %s" c <| formatValue(v)
        | NotGreaterThan(c, v) -> 
            sprintf "[%s] !> %s" c <| formatValue(v)
        | LessThan(c, v) -> 
            sprintf "[%s] < %s" c <| formatValue(v)
        | NotLessThan(c, v) -> 
            sprintf "[%s] !< %s" c <| formatValue(v)
        | GreaterThanOrEqual(c, v) -> 
            sprintf "[%s] >= %s" c <| formatValue(v)            
        | LessThanOrEqual(c, v) -> 
            sprintf "[%s] <= %s" c <| formatValue(v)            
        | StartsWith(c, v) -> 
            String.Format("[{0}] like '{1}%'", c, v) 
        | EndsWith(c, v) -> 
            String.Format("[{0}] like '%{1}'", c, v) 
        | Contains(c, v) -> 
            String.Format("[{0}] like '%{1}%'", c, v) 
    
    let private formatFilters(filters : ColumnFilterJoin rolist) =        
        if filters.Count = 0 then
            String.Empty
        else
            let sb = StringBuilder()
            [0..filters.Count-1] |> Seq.iter(fun i ->
                let f = filters.[i]
                let connector = 
                    if i <> 1 then
                        match f with
                        | AndFilter(_) -> " and "
                        | OrFilter(f) ->  " or "
                    else
                        String.Empty
                sb.AppendFormat("{0}{1}", f.Filter |> formatFilter) |> ignore
            )
            sb.ToString()

    let private formatSortCriteria(criteria : ColumnSortCriterion rolist) =
        if criteria.Count = 0 then
            String.Empty
        else
            let sb = StringBuilder()
            let count = criteria.Count
            [0..count-1] |> Seq.iter(fun i ->
                match criteria.[i] with
                | AscendingSort(name) ->
                    sb.AppendFormat("{0} asc", name |> formatColumnName) |> ignore
                | DescendingSort(name) ->
                    sb.AppendFormat("{0} desc", name |> formatColumnName) |> ignore

                if i <> count-1 then
                    sb.Append(", ") |> ignore
            
            )
            sb.ToString()

    let private formatPageInfo(info : TabularPageInfo option) =
        match info with
        | None -> String.Empty
        | Some(x) ->
            match x with 
                TabularPageInfo(pageNumber, pageSize) ->
                    let pageNumber = defaultArg pageNumber 1
                    let pageSize = defaultArg pageSize 100
                    sprintf "offset %i rows fetch next %i rows only" ((pageNumber - 1)*pageSize) pageSize

    let formatTabularQuery(q : TabularDataQuery) =
        match q with
        |TabularDataQuery(tabularName, columnNames, filters, sortCriteria, pageInfo) -> 
            let columns = if columnNames.Count = 0 then "*" else (columnNames |> Txt.delimit ",")
            let from = tabularName |> formatObjectName
            let where = filters |> formatFilters
            let order = sortCriteria |> formatSortCriteria
            let paging = pageInfo |> formatPageInfo
            let sb = StringBuilder()
            sb.AppendFormat("select {0} from {1}\r\n", columns, from) |> ignore
            if where <> String.Empty then
                sb.AppendFormat("where {0}\r\n", where) |> ignore
            if order <> String.Empty then
                sb.AppendFormat("order by {0}\r\n", order) |> ignore
            if pageInfo |> Option.isSome then
                sb.AppendFormat("{0}", paging) |> ignore
            sb.ToString()
                        
        

module internal SqlParameter =
    let create (paramValues : RoutineParameterValue list) (d : RoutineParameterDescription) =
        let p = if d.Direction = RoutineParameterDirection.ReturnValue then 
                    SqlParameter("Return", DBNull.Value) 
                else if d.Direction = RoutineParameterDirection.Input then
                    SqlParameter(d.Name |> SqlFormatter.formatParameterName, paramValues |> List.find(fun v -> v.Name = d.Name) |> fun value -> value.Value)
                else if d.Direction = RoutineParameterDirection.Output then
                    SqlParameter(d.Name, DBNull.Value)
                else
                    NotSupportedException() |> raise
        p.Direction <- enum<System.Data.ParameterDirection>(int d.Direction)
        p.SqlDbType <- d.StorageType |> DataType.toSqlDbType   
        p     
