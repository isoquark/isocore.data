namespace IQ.Core.Framework

open System
open System.Reflection


/// <summary>
/// Defines utility methods for working with method metadata
/// </summary>
module ClrMethod =
    let private referenceParameter (p : ParameterInfo) =
        {
            MethodParameterReference.Name = p.Name
            ParameterType = p.ParameterType
            ValueType = p.ParameterType.ValueType
            Parameter = p
            Position = p.Position 
            IsRequired = (p.IsOptional || p.IsDefined(typeof<OptionalArgumentAttribute>)) |> not   
            Method = p.Member :?> MethodInfo         
           
        }

    /// <summary>
    /// Creates a method reference
    /// </summary>
    /// <param name="m">The method</param>
    let reference(m : MethodInfo) =
        let returnType = if m.ReturnType  = typeof<Void> then None else m.ReturnType |> Some
        {
            MethodReference.Name = m.Name
            Return = 
                {
                    ReturnType = returnType
                    Method = m
                    ValueType = match returnType with
                                | Some(x) -> x.ValueType |> Some
                                | None -> None
                                
                }
            Parameters = m.GetParameters() |> Array.map referenceParameter |> List.ofArray
            Method = m
        }    

/// <summary>
/// Defines method-related operators and extensions 
/// </summary>
[<AutoOpen>]
module ClrMethodExtensions =
    let methodref(m : MethodInfo) = m |> ClrMethod.reference
    

