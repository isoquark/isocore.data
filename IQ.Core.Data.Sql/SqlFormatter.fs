namespace IQ.Core.Data.Sql

open System
open System.Data
open System.Linq
open System.Data.Linq
open System.Reflection


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
    /// Formats a select statement for a tabular proxy
    /// </summary>
    let formatTabularSelect<'T>() =
        let ptype = tableproxy<'T>
        let columns = ptype.DataElement.Columns |> List.map(fun c -> c.Name |> formatElementName ) |> Txt.toDelimitedText ","
        let tableName = ptype.TableName |> formatObjectName
        sprintf "select %s from %s" columns tableName
        
        
