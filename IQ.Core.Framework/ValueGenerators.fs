namespace IQ.Core.Framework

open System
open System.Reflection

type internal ValueGeneratorKey = ValueGeneratorKey of valueType : Type * configParamTypes : Type[]

/// <summary>
/// Discovers and provides access to value generators
/// </summary>
type ValueGenerators private (search : Assembly[]) =
    static let mutable factory = Option<IValueGeneratorProvider>.None

    let indexGenerators() =
        [for a in search do
            for t in Assembly.GetExecutingAssembly().GetTypes() do
                for i in t.GetInterfaces() do
                    if 
                        i.IsGenericType && 
                        i.ContainsGenericParameters = false && 
                        i.GetGenericTypeDefinition() = typedefof<IValueGenerator<_>> &&
                        i.GetGenericArguments().Length = 1
                    then                            
                        let valueType = i.GetGenericArguments().[0]
                        for c in t.GetConstructors() do
                            yield 
                                ValueGeneratorKey(valueType, c.GetParameters() |> Array.map(fun p -> p.ParameterType)),
                                t                            
        ] |> dict

    let index = indexGenerators()
                
    interface IValueGeneratorProvider with
        member this.GetGenerator<'T>  ([<ParamArray>] parms : obj[]) =
            let key = ValueGeneratorKey(typeof<'T>, parms |> Array.map (fun p -> p.GetType()))        
            let t = index.[key]
            Activator.CreateInstance(t, parms) :?> IValueGenerator<'T>

    static member InitFactory([<ParamArray>] search : Assembly[]) =
        if factory |> Option.isNone then
            factory <- ValueGenerators(search)  :> IValueGeneratorProvider |> Some
        else
            failwith "Alread initialized"

    static member GetGenerator<'T>([<ParamArray>] parms : obj[]) =
        if factory |> Option.isNone then
            failwith "Not Initialized" 
        
        factory.Value.GetGenerator<'T>(parms)


