namespace IQ.Core.Framework

open System
open System.Reflection

/// <summary>
/// Defines operations for working with CLR properties
/// </summary>
module ClrProperty =
    /// <summary>
    /// Creates a property reference
    /// </summary>
    /// <param name="p">The property to be referenced</param>
    let reference i (p : PropertyInfo) = 
        {
            PropertyReference.Name = p.Name
            Property = p
            ValueType = p.PropertyType.ValueType
            PropertyType = p.PropertyType
            Position = i
        }


/// <summary>
/// Defines ClrProperty-related operators and extensions 
/// </summary>
[<AutoOpen>]
module ClrPropertyExtensions =
    let propref  i (p : PropertyInfo) =
        p |> ClrProperty.reference i