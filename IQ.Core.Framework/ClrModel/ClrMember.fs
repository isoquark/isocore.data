namespace IQ.Core.Framework

open System
open System.Reflection

/// <summary>
/// Defines operations for working with CLR members
/// </summary>
module ClrMember =
    /// <summary>
    /// Retrieves an attribute applied to a member, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttribute<'T when 'T :> Attribute>(subject : MemberInfo) =
        if Attribute.IsDefined(subject, typeof<'T>) then
            Attribute.GetCustomAttribute(subject, typeof<'T>) :?> 'T |> Some
        else
            None


