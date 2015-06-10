namespace IQ.Core.Framework

open System
open System.Reflection
open System.Data
open System.Collections.Concurrent
open System.Linq
open System.IO

open Microsoft.FSharp.Reflection

                  
[<AutoOpen>]
module ClrModelOperators =
    /// <summary>
    /// Describes the record identified by a supplied type parameter
    /// </summary>
    let recordinfo<'T> =
        typeof<'T> |> ClrRecord.describe

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



