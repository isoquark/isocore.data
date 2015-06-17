namespace IQ.Core.Framework

open System
open System.Reflection

/// <summary>
/// Defines operations for working with CLR properties
/// </summary>
module ClrProperty =
    /// <summary>
    /// Describes the identified property
    /// </summary>
    /// <param name="p">The property</param>
    let describe(p : PropertyInfo) = 
        {
            PropertyDescription.Name = p.Name
            Property = p
            ValueType = p.PropertyType.ValueType
            PropertyType = p.PropertyType
        }


