namespace IQ.Core.Data

open IQ.Core.Framework

module ConnectionString =    
    /// <summary>
    /// Creates a ConnectionString instance from the supplied text
    /// </summary>
    /// <param name="text">The text to parse</param>
    let parse text =
        text |> Txt.split ";" |> List.ofArray |> ConnectionString    

    /// <summary>
    /// Renders the instance as semantic text that can subsequently be parsed
    /// </summary>
    /// <param name="cs">The connection string to format</param>
    let format (cs : ConnectionString) =
        cs.Components |> Txt.delemit ";"

[<AutoOpen>]
module ConnectionStringExtensions =
    type ConnectionString
    with
        member this.Text = this |> ConnectionString.format    

