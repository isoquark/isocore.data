namespace IQ.Core.Framework

open System
open System.Reflection

/// <summary>
/// Defines operations for working with CLR types
/// </summary>
module ClrType =
    /// <summary>
    /// Determines whether a supplied type is optional
    /// </summary>
    /// <param name="t">The type to test</param>
    let isOptionType (t : Type) =
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>
      

    /// <summary>
    /// Retrieves an attribute applied to a type, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttribute<'T when 'T :> Attribute>(subject : Type) =
        subject |> ClrMember.getAttribute<'T>


