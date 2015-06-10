namespace IQ.Core.Framework

open System
open System.Reflection

/// <summary>
/// Defines operations for working with CLR properties
/// </summary>
module ClrProperty =
    /// <summary>
    /// Retrieves an attribute applied to a property, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttribute<'T when 'T :> Attribute>(subject : PropertyInfo) = 
        subject |> ClrMember.getAttribute<'T>


