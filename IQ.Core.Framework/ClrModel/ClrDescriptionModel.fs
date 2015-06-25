namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent
open System.Diagnostics


open Microsoft.FSharp.Reflection

[<AutoOpen>]
module ClrDescriptionVocabulary =

    type ClrSubjectDescription = ClrSubjectDescription of name : ClrElementName * position : int
    with
        member this.Name = match this with ClrSubjectDescription(name=x) -> x
        member this.Position = match this with ClrSubjectDescription(position=x) -> x


    /// <summary>
    /// Describes a property
    /// </summary>
    type ClrPropertyDescription = {
        /// The property being referenced
        Subject : ClrSubjectDescription

        /// The name of the type that declares the property
        DeclaringType : ClrTypeName       
    
        /// The type of the property value
        ValueType : ClrTypeName

        /// Specifies whether the property is of option<> type
        IsOptional : bool

        /// Specifies whether the property has a get accessor
        CanRead : bool

        /// Specifies the access of the get accessor if applicable
        ReadAccess : ClrAccess option

        /// Specifies whether the property has a set accessor
        CanWrite : bool

        /// Specifies the access of the set accessor if applicable
        WriteAccess : ClrAccess option
    }


module ClrDescription =
    /// <summary>
    /// Creates a property description
    /// </summary>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    /// <param name="p">The property to be referenced</param>
    let internal describeProperty pos (p : PropertyInfo) =
        {
            ClrPropertyDescription.Subject = ClrSubjectDescription(p.ElementName, pos)
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

    type ClrPropertyDescription
    with
        /// <summary>
        /// The name of the property
        /// </summary>
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position

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
