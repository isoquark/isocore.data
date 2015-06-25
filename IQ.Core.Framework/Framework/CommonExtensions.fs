namespace IQ.Core.Framework

open System
open System.Reflection

open Microsoft.FSharp.Quotations.Patterns

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
    let inline thisAssemblyElement() = thisAssembly().AssemblyElement

    /// <summary>
    /// Gets the currently executing method (not to be used for constructors!)
    /// </summary>
    /// <remarks>
    /// Note that since the method is designated inline, the call to get the executing
    /// method is injected at the call-site and so works as expected
    /// </remarks>
    let inline thisMethod() = MethodInfo.GetCurrentMethod() :?> MethodInfo
    let inline thisMethodElement() = thisMethod().MethodElement

    /// <summary>
    /// Gets the currently executing constructor
    /// </summary>
    /// <remarks>
    /// Note that since the method is designated inline, the call to get the executing
    /// method is injected at the call-site and so works as expected
    /// </remarks>
    let inline thisConstructor() = MethodInfo.GetCurrentMethod() :?> ConstructorInfo

    /// <summary>
    /// Gets the properties defined by the type
    /// </summary>
    let props<'T> = typeof<'T> |> Type.getProperties

    /// <summary>
    /// When supplied a property accessor quotation, retrieves the name of the property
    /// </summary>
    /// <param name="q">The property accessor quotation</param>
    /// <remarks>
    /// Inspired heavily by: http://www.contactandcoil.com/software/dotnet/getting-a-property-name-as-a-string-in-f/
    /// </remarks>
    let rec propname q =
       match q with
       | PropertyGet(_,p,_) -> p.ElementName
       | Lambda(_, expr) -> propname expr
       | _ -> nosupport()

    /// <summary>
    /// Gets the type element from the suppied type argument
    /// </summary>
    let clrtype<'T> = typeof<'T>.TypeElement

    type Type
    with
        member this.IsOptionType = this |> Option.isOptionType

        /// <summary>
        /// If optional type, gets the type of the underlying value; otherwise, the type itself
        /// </summary>
        member this.ItemValueType = this |> Type.getItemValueType

    type PropertyInfo
    with
        member this.ValueType = this.PropertyType |> Type.getItemValueType

    type FieldInfo
    with
        member this.ValueType = this.FieldType |> Type.getItemValueType

    /// <summary>
    /// Defines augmentations for the Assembly type
    /// </summary>
    type Assembly
    with
        /// <summary>
        /// Gets the short name of the assembly without version/culture/security information
        /// </summary>
        member this.ShortName = this.GetName().Name    


