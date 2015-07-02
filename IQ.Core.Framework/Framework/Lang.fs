namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Linq
open System.Collections.Generic
open System.IO

open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns

/// <summary>
/// Defines core global operations and types
/// </summary>
/// <remarks>
/// The content here is more-or-less random at the moment
/// </remarks>
[<AutoOpen>]
module Lang =
    /// <summary>
    /// Raises a <see cref="System.NotSupportedException"/>
    /// </summary>
    let inline nosupport()  = NotSupportedException() |> raise

    /// <summary>
    /// Alias for UInt8 type for consistency
    /// </summary>
    type UInt8 = Byte
                                            
    /// <summary>
    /// Defines custom Seq module operations
    /// </summary>
    module Seq =
        /// <summary>
        /// Counts the number of items in the sequence
        /// </summary>
        /// <param name="items">The items to count</param>
        /// <remarks>
        /// Obviously, this assumes that the sequence is not interminable!
        /// </remarks>
        let count (items : seq<'T>) = items.Count()
    
    /// <summary>
    /// Defines custom Array module operations
    /// </summary>
    module Array =
        /// <summary>
        /// Maps items in an array in parallel
        /// </summary>
        let pmap = Array.Parallel.map


    /// <summary>
    /// Raises a debugging assertion if a supplied predicate fails and emits a diagnostic message
    /// </summary>
    /// <param name="message">The diagnostic to emit if predicate evaluation fails</param>
    /// <param name="predicate">The predicate to evaluate</param>
    let _assert message (predicate: unit -> bool)  = 
        Debug.Assert(predicate(), message)

        
    /// <summary>
    /// Raises a <see cref="System.ArgumentException"/> 
    /// </summary>
    /// <param name="paramName">The name of the parameter</param>
    /// <param name="paramName">The value of the parameter</param>
    let inline argerror paramName paramValue =
        let message = sprintf "The argument value %O for %s is incorrect" paramValue paramName 
        ArgumentException(message) |> raise
            
    /// <summary>
    /// Raises a <see cref="System.ArgumentException"/> with a description
    /// </summary>
    /// <param name="paramName">The name of the parameter</param>
    /// <param name="paramName">The value of the parameter</param>
    /// <param name="description">Explains why the argument is unsatisfactory</param>
    let inline argerrord paramName paramValue description =
        let message = sprintf "The argument value %O for %s is incorrect:%s" paramValue paramName description
        ArgumentException(message) |> raise
    
    /// <summary>
    /// Raises a <see cref="System.NotSupportedException"/>
    /// </summary>
    /// <param name="description"></param>
    let inline nosupportd description = NotSupportedException(description) |> raise

    /// <summary>
    /// Defines augmentations for the TimeSpan type
    /// </summary>
    type TimeSpan
    with
        static member Sum(timespans : TimeSpan seq) =
            timespans |> Seq.map(fun x -> x.Ticks) |> Seq.sum |> TimeSpan.FromTicks

    /// <summary>
    /// The default format string to use when applying the DebuggerDisplay attribute
    /// </summary>
    [<Literal>]
    let DebuggerDisplayDefault = "{ToString(),nq}"

    /// <summary>
    /// Realized by types whose instance that are capable of being faithfully rendered as text.
    /// </summary>
    /// <remarks>
    /// The semantic representation of an instance includes the state necessary to reconstitute the 
    /// instance from that representation
    /// </remarks>
    type ISemanticRepresentation =
        /// <summary>
        /// Faithfully renders an instance as text
        /// </summary>
        abstract ToSemanticString:unit->string

    /// <summary>
    /// Identifies a function that can parse the semantic representation of a type instance
    /// </summary>
    [<AttributeUsage(AttributeTargets.Method)>]
    type ParserAttribute(t) = 
        inherit Attribute()
        
        /// <summary>
        /// The type of element that can be parsed
        /// </summary>
        member this.ElementType : Type = t 

    /// <summary>
    /// Defines augmentations for the <see cref="System.Enum" /> type
    /// </summary>
    type Enum 
    with
        static member Parse<'T when 'T:>Enum >(value) = Enum.Parse(typeof<'T>, value) :?> 'T

    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.AssemblyName" /> type
    /// </summary>
    type AssemblyName
    with
        /// <summary>
        /// Specifies whether the identified assembly is loaded into the current application domain
        /// </summary>
        member this.IsAssemblyLoaded = 
            AppDomain.CurrentDomain.GetAssemblies() 
                |> Array.map(fun a -> a.GetName()) 
                |> Array.exists (fun n -> n = this)
            

    module Assembly =
        /// <summary>
        /// Recursively loads assembly references into the current application domain
        /// </summary>
        /// <param name="subject">The starting assembly</param>
        let rec loadReferences (filter : string option) (subject : Assembly) =
            let references = subject.GetReferencedAssemblies()
            let filtered = match filter with
                            | Some(filter) -> 
                                references |> Array.filter(fun x -> x.Name.StartsWith(filter)) 
                            | None ->
                                references

            filtered |> Array.iter(fun name ->
                if name.IsAssemblyLoaded |>not then
                    name |> AppDomain.CurrentDomain.Load |> loadReferences filter
            )

        
    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.Assembly" /> type
    /// </summary>
    type Assembly
    with
        /// <summary>
        /// Recursively loads assembly references into the current application domain
        /// </summary>
        /// <param name="subject">The starting assembly</param>
        member this.LoadReferences (filter : string option) = this |> Assembly.loadReferences filter

    /// <summary>
    /// Lookup operator to retrieve the value identified by a key in a map
    /// </summary>
    /// <param name="map">The map to search</param>
    /// <param name="key">The value key</param>
    let (?) (map : Map<string,_>) key = map.[key]

    /// <summary>
    /// Specifies the range of allowable values for a given element
    /// </summary>
    type Multiplicity = 
        | ExactlyZero
        | ZeroOrOne
        | ZeroOrMore
        | ExactlyOne
        | OneOrMore
        | BoundedRange of min : uint32 * max : uint32

                



                


    

    



    
