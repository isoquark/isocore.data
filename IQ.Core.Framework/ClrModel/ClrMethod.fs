namespace IQ.Core.Framework

open System
open System.Reflection


/// <summary>
/// Defines utility methods for working with method metadata
/// </summary>
module ClrMethod =
    let private referenceParameter (p : ParameterInfo) =
        {
            ClrMethodParameterReference.Subject = ClrSubjectReference(p.ElementName , p.Position, p)
            ParameterType = p.ParameterType
            ValueType = p.ParameterType.ItemValueType
            IsRequired = (p.IsOptional || p.IsDefined(typeof<OptionalArgumentAttribute>)) |> not   
            Method = p.Member :?> MethodInfo         
           
        }

    /// <summary>
    /// Creates a method reference
    /// </summary>
    /// <param name="m">The method</param>
    let internal reference pos (m : MethodInfo) =
        let returnType = if m.ReturnType  = typeof<Void> then None else m.ReturnType |> Some
        {
            Subject = ClrSubjectReference(m.ElementName, pos, m)
            Return = 
                {
                    ReturnType = returnType
                    Method = m
                    ValueType = match returnType with
                                | Some(x) -> x.ItemValueType |> Some
                                | None -> None
                                
                }
            Parameters = m.GetParameters() |> Array.map referenceParameter |> List.ofArray
        }    

/// <summary>
/// Defines method-related operators and extensions 
/// </summary>
[<AutoOpen>]
module ClrMethodExtensions =

    /// <summary>
    /// Gets the methods defined by a type
    /// </summary>
    let methodrefmap<'T> = 
        typeof<'T> |> Type.getPureMethods |> List.mapi ClrMethod.reference |> List.map(fun m -> m.Subject.Name, m) |> Map.ofList
    

