namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent
open System.Diagnostics


open Microsoft.FSharp.Reflection



module ClrDescription =
    /// <summary>
    /// Creates a property description
    /// </summary>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    /// <param name="p">The property to be referenced</param>
    let internal describeProperty pos (p : PropertyInfo) =
        {
            Name = p.Name |> ClrMemberName 
            Position = pos
            DeclaringType  = p.DeclaringType.ElementTypeName
            ValueType = p.PropertyType |> Type.getItemValueType |> fun x -> x.ElementTypeName
            IsOptional = p.PropertyType |> Option.isOptionType
            CanRead = p.CanRead
            ReadAccess = if p.CanRead then p.GetMethod.AccessModifier |> Some else None
            CanWrite = p.CanWrite
            WriteAccess =  if p.CanWrite then p.SetMethod.AccessModifier |> Some else None
        }

[<AutoOpen>]
module ClrDescriptionExtensions =

    /// <summary>
    /// Creates a property description
    /// </summary>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    /// <param name="p">The property to be referenced</param>
    let internal propinfo pos (p : PropertyInfo) = 
        p |> ClrDescription.describeProperty pos

    /// <summary>
    /// Creates a property description map keyed by name
    /// </summary>
    let propinfomap<'T> = props<'T> |> List.mapi propinfo |> List.map(fun p -> p.Name, p) |> Map.ofList

