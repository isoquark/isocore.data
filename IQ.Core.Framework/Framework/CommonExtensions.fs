namespace IQ.Core.Framework

open System
open System.Reflection

open Microsoft.FSharp.Quotations.Patterns

/// <summary>
/// Defines augmentations and operators for reflection-related capabilities
/// </summary>
[<AutoOpen>]
module ReflectionExtensions =
    let inline thisAssemblyElement() = thisAssembly().AssemblyElement

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



