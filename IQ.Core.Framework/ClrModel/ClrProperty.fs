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
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    /// <param name="p">The property to be referenced</param>
    let internal reference pos (p : PropertyInfo) = 
        {
            Subject = ClrSubjectReference(p.ElementName, pos, p)
            ValueType = p.PropertyType.ValueType
            PropertyType = p.PropertyType
        }

    /// <summary>
    /// Creates a property description
    /// </summary>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    /// <param name="p">The property to be referenced</param>
    let internal describe pos (p : PropertyInfo) =
        {
            ClrPropertyDescription.Name = p.ElementName
            Position = pos
            DeclaringType  = p.DeclaringType.FullName |> FullTypeName
            ValueType = p.PropertyType.ValueType.FullName |> FullTypeName
            IsOptional = p.PropertyType.IsOptionType
            CanRead = p.CanRead
            ReadAccess = if p.CanRead then p.GetMethod |> ClrAccess.getMethodAccess |> Some else None
            CanWrite = p.CanWrite
            WriteAccess =  if p.CanWrite then p.SetMethod |> ClrAccess.getMethodAccess |> Some else None
        }


/// <summary>
/// Defines ClrProperty-related operators and extensions 
/// </summary>
[<AutoOpen>]
module ClrPropertyExtensions =
    /// <summary>
    /// Creates a property reference
    /// </summary>
    /// <param name="p">The property to be referenced</param>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    let internal propref  pos (p : PropertyInfo) =
        p |> ClrProperty.reference pos

    /// <summary>
    /// Creates a property description
    /// </summary>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    /// <param name="p">The property to be referenced</param>
    let internal propinfo pos (p : PropertyInfo) = 
        p |> ClrProperty.describe pos

    /// <summary>
    /// Creates a property description map keyed by name
    /// </summary>
    let propinfomap<'T> = props<'T> |> List.mapi propinfo |> List.map(fun p -> p.Name, p) |> Map.ofList
