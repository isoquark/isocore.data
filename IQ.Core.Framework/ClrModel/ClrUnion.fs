namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent

open Microsoft.FSharp.Reflection

/// <summary>
/// Defines operations for working with union values and metadata
/// </summary>
module ClrUnion =
    
    /// <summary>
    /// Creates a reference to a property field
    /// </summary>
    /// <param name="i">The field's position within the case</param>
    /// <param name="p">The propery that represents the field</param>
    let private referenceField i (p : PropertyInfo) = 
        {
            PropertyReference.Name = p.Name
            Property = p
            Position = i
            PropertyType = p.PropertyType
            ValueType = p.ValueType
        }

    /// <summary>
    /// Creates a reference to a union case
    /// </summary>
    /// <param name="c">The case information</param>
    let private referenceCase(c : UnionCaseInfo) =
        {
            UnionCaseReference.Name = c.Name
            Case = c
            Position = c.Tag
            Fields = c.GetFields() |> List.ofArray |> List.mapi referenceField
        }
    
    /// <summary>
    /// Describes the cases defined by a supplied union type
    /// </summary>
    /// <param name="t">The union type</param>
    let private describeCases(t : Type) =
        FSharpType.GetUnionCases(t, true) |> List.ofArray |> List.map referenceCase

    /// <summary>
    /// Determines whether a supplied type is a union type
    /// </summary>
    /// <param name="t">The candidate type</param>
    let isUnionType (t : Type) =
        FSharpType.IsUnion(t, true)

    /// <summary>
    /// Determines whether a supplied type is a union type
    /// </summary>
    let isUnion<'T>() =
        typeof<'T> |> isUnionType


    /// <summary>
    /// Creates a union description
    /// </summary>
    /// <param name="t">The union type</param>
    let private createReference(t : Type) =      
        {
            UnionReference.Name = t.Name
            Type = t
            Cases =  t |> describeCases
        }

    /// <summary>
    /// Describes the union represented by the type
    /// </summary>
    /// <param name="t"></param>
    let reference(t : Type) =
        if t |> isUnionType |> not then
            ArgumentException(sprintf "The type %O is not a record type" t) |> raise
        
        createReference |> ClrTypeReferenceIndex.getOrAddUnion t


/// <summary>
/// Defines union-related augmentations and operators
/// </summary>
[<AutoOpen>]
module ClrUnionExtensions =    
    /// <summary>
    /// Describes the union identified by a supplied type parameter
    /// </summary>
    let unionref<'T> =
        typeof<'T> |> ClrUnion.reference


    /// <summary>
    /// Defines augmentations for the RecordDescription type
    /// </summary>
    type UnionReference
    with
        /// <summary>
        /// Indexer that finds a union case by its position
        /// </summary>
        /// <param name="position">The position of the case</param>
        member this.Item(position) = this.Cases.[position]
        
        /// <summary>
        /// Indexer that finds a union case by its name
        /// </summary>
        /// <param name="position">The position of the case</param>
        member this.Item(name) = this.Cases |> List.find(fun c -> c.Name = name)

    /// <summary>
    /// Defines augmentations for the UnionCaseDescription type
    /// </summary>
    type UnionCaseReference
    with
        /// <summary>
        /// Indexer that finds a case field by its position
        /// </summary>
        /// <param name="position">The position of the case field</param>
        member this.Item(position) = this.Fields.[position]

        /// <summary>
        /// Indexer that finds a case field by its name
        /// </summary>
        /// <param name="name">The name of the case field</param>
        member this.Item(name) = this.Fields |> List.find(fun f -> f.Name = name)




