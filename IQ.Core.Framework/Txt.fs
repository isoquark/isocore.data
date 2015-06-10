namespace IQ.Core.Framework

open System


/// <summary>
/// Defines utility operations for working with text
/// </summary>
module Txt =
    /// <summary>
    /// Retrieves all text to the right of a specified marker; if no marker is found, returns the empty string
    /// </summary>
    /// <param name="marker">The marker to search for</param>
    /// <param name="text">The text to search</param>
    let rightOf (marker : string) (text : string) =
        let idx = text.IndexOf(marker) 
        if idx <> -1 then
            (idx + marker.Length) |> text.Substring
        else
            String.Empty

    /// <summary>
    /// Removes leading and trailing whitespace
    /// </summary>
    /// <param name="text">The text to trim</param>
    let trim (text : string) =
        text.Trim()
                       

