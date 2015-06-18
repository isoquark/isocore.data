namespace IQ.Core.Framework

[<AutoOpen>]
module ConnectionStringVocabulary =

    /// <summary>
    /// Responsible for identifying a Data Store, Network Address or other resource
    /// </summary>
    type ConnectionString = ConnectionString of string list
    with
        /// <summary>
        /// The components of the connection string
        /// </summary>
        member this.Components = match this with ConnectionString(components) -> components

module ConnectionString =
    
    /// <summary>
    /// Creates a ConnectionString instance from the supplied text
    /// </summary>
    /// <param name="text">The text to parse</param>
    let parse text =
        text |> Txt.split ";" |> List.ofArray |> ConnectionString    

