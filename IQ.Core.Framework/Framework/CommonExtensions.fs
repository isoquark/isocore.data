namespace IQ.Core.Framework

open System
open System.Reflection

/// <summary>
/// Defines augmentations and operators for reflection-related capabilities
/// </summary>
[<AutoOpen>]
module ReflectionExtensions =
    /// <summary>
    /// Gets the currently executing assembly
    /// </summary>
    /// <remarks>
    /// Note that since the method is designated inline, the call to get the executing
    /// assembly is injected at the call-site and so works as expected
    /// </remarks>
    let inline thisAssembly() = Assembly.GetExecutingAssembly()

    /// <summary>
    /// Gets the currently executing method
    /// </summary>
    /// <remarks>
    /// Note that since the method is designated inline, the call to get the executing
    /// method is injected at the call-site and so works as expected
    /// </remarks>
    let inline thisMethod() = MethodInfo.GetCurrentMethod()

    /// <summary>
    /// Gets the properties defined by the type
    /// </summary>
    let props<'T> = typeof<'T> |> Type.getProperties

    type Type
    with
        member this.IsOptionType = this |> Option.isOptionType

        /// <summary>
        /// If optional type, gets the type of the underlying value; otherwise, the type itself
        /// </summary>
        member this.ItemValueType = this |> Type.getItemValueType

    type PropertyInfo
    with
        member this.ValueType = this |> PropertyInfo.getValueType

    type FieldInfo
    with
        member this.ValueType = this |> FieldInfo.getValueType        

    /// <summary>
    /// Defines augmentations for the Assembly type
    /// </summary>
    type Assembly
    with
        /// <summary>
        /// Gets the short name of the assembly without version/culture/security information
        /// </summary>
        member this.ShortName = this.GetName().Name    

