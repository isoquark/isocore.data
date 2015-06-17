namespace IQ.Core.Framework

open System
open System.Reflection


/// <summary>
/// Defines utility methods for working with method metadata
/// </summary>
module ClrMethod =
    let private describeParameter (p : ParameterInfo) =
        {
            MethodParameterDescription.Name = p.Name
            ParameterType = p.ParameterType
            ValueType = p.ParameterType.ValueType
            Parameter = p
            Position = p.Position 
            IsRequired = (p.IsOptional || p.IsDefined(typeof<OptionalArgumentAttribute>)) |> not   
            Method = p.Member :?> MethodInfo         
           
        }

    /// <summary>
    /// Describes a specified method
    /// </summary>
    /// <param name="m">The CLR method information</param>
    let describe(m : MethodInfo) =
        let returnType = if m.ReturnType  = typeof<Void> then None else m.ReturnType |> Some
        {
            MethodDescription.Name = m.Name
            Return = 
                {
                    ReturnType = returnType
                    Method = m
                    ValueType = match returnType with
                                | Some(x) -> x.ValueType |> Some
                                | None -> None
                                
                }
            Parameters = m.GetParameters() |> Array.map describeParameter |> List.ofArray
            Method = m
        }    

    

