namespace IQ.Core.Data.Sql

open System
open System.Data
open System.Linq
open System.Data.Linq
open System.Reflection
open System.Text
open System.Data.SqlClient
open System.Diagnostics


open IQ.Core.Framework
open IQ.Core.Data

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
            | :? DateTime as x -> sprintf "'%s'" (x.ToString("yyyy-MM-dd HH:mm:ss.fff"))
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
        let parameters = f.Parameters |> List.map (fun x -> x.Name |> formatParameterName)
                       |> Txt.delemit ","
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
        let columns = t.Columns |> List.map(fun c -> c.Name |> formatElementName) |> Txt.delemit ","
        let tableName = t.Name |> formatObjectName
        sprintf "select %s from %s" columns tableName

    /// <summary>
    /// Formats a select statement for a tabular proxy
    /// </summary>
    let formatTabularSelectT<'T>() =
        let ptype = tabularproxy<'T>
        ptype.DataElement |> formatTabularSelect
        

module internal SqlCommand =
    let executeQuery (columns : ColumnDescription list) (command : SqlCommand) =
        use reader = command.ExecuteReader()
        if reader.HasRows then
            [while reader.Read() do
                let buffer = Array.zeroCreate<obj>(columns.Length)
                let valueCount = buffer |> reader.GetValues
                Debug.Assert((valueCount = columns.Length), "Column / Value count mismatch")
                yield buffer 
            ]
        else
            []
        
        
module internal SqlConnection = 
    let create cs = 
        let connection = new SqlConnection(cs)
        connection.Open() 
        connection

module internal SqlParameter =
    let create (paramValues : DataParameterValue list) (d : RoutineParameterDescription) =
        let p = if d.Direction = ParameterDirection.ReturnValue then 
                    SqlParameter("Return", DBNull.Value) 
                else if d.Direction = ParameterDirection.Input then
                    SqlParameter(d.Name |> SqlFormatter.formatParameterName, paramValues |> List.find(fun v -> v.Name = d.Name) |> fun value -> value.Value)
                else if d.Direction = ParameterDirection.Output then
                    SqlParameter(d.Name, DBNull.Value)
                else
                    NotSupportedException() |> raise
        p.Direction <- d.Direction
        p.SqlDbType <- d.StorageType |> StorageType.toSqlDbType   
        p     
