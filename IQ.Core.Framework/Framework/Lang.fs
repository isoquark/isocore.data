﻿namespace IQ.Core.Framework

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
    /// Extracts the MethodInfo of a function used in a lambda expression
    /// </summary>
    /// <remarks>
    /// Useful for quotations of the form <@fun () -> myfunc @>
    /// </remarks>
    let rec funcinfo q =
        match q with
        | Lambda(_, expr) -> funcinfo expr
        | Call(_,m,_) -> m
        | _ -> nosupport()

    /// <summary>
    /// Extracts the name of a function used in a lambda expression
    /// </summary>
    /// <remarks>
    /// Useful for quotations of the form <@fun () -> myfunc @>
    /// </remarks>
    let funcname q = q |> funcinfo |> fun x -> x.Name

            
    let rec propinfo q =
       match q with
       | PropertyGet(_,p,_) -> p
       | Lambda(_, expr) -> propinfo expr
       | _ -> nosupport()
        
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

    type Enum 
    with
        static member Parse<'T when 'T:>Enum >(value) = Enum.Parse(typeof<'T>, value) :?> 'T

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
            
        /// <summary>
    /// Classifies native CLR metadata elements
    /// </summary>
    type ReflectedKind =
        | Assembly = 1
        | Type = 2
        | Method = 3
        | Property = 4
        | Field = 5
        | Constructor = 6
        | Event = 7
        | Parameter = 8
        | UnionCase = 9

    /// <summary>
    /// Classifies CLR collections
    /// </summary>
    type ClrCollectionKind = 
        | Unclassified = 0
        /// Identifies an F# list
        | FSharpList = 1
        /// Identifies an array
        | Array = 2
        /// Identifies a Sytem.Collections.Generic.List<_> collection
        | GenericList = 3


    /// <summary>
    /// Classifies CLR types
    /// </summary>
    type ClrTypeKind =
        | Unclassified = 0
        /// <summary>
        /// Classifies a type as an F# discriminated union
        /// </summary>
        | Union = 1
        /// <summary>
        /// Classifies a type as a record
        /// </summary>
        | Record = 2
        /// <summary>
        /// Classifies a type as an interface
        /// </summary>
        | Interface = 3
        /// <summary>
        /// Classifies a type as a class
        /// </summary>
        | Class = 4 
        /// <summary>
        /// Classifies a type as a collection of some sort
        /// </summary>
        | Collection = 5
        /// <summary>
        /// Classifies a type as a struct (a CLR value type)
        /// </summary>
        | Struct = 6
        /// <summary>
        /// Classifies a type as an F# module
        /// </summary>
        | Module = 7
        /// <summary>
        /// Classifies a type as a nulluable value type, e.g., Nullable<int>
        /// </summary>
        | NullableValue = 8

    /// <summary>
    /// Classifies CLR elements
    /// </summary>
    type ClrElementKind =
        | Unclassified = 0
        /// <summary>
        /// Classifies a CLR element as a propert
        /// </summary>
        | Property = 1
        /// <summary>
        /// Classifies a CLR element as a property
        /// </summary>
        | Method = 2
        /// <summary>
        /// Classifies a CLR element as a field
        /// </summary>
        | StorageField = 3
        /// <summary>
        /// Classifies a CLR element as an event
        /// </summary>
        | Event = 4
        /// <summary>
        /// Classifies a CLR element as a constructor
        /// </summary>
        | Constructor = 5        
        /// <summary>
        /// Classifies a CLR element as a type 
        /// </summary>
        | Type = 6
        /// <summary>
        /// Classifies a CLR element as an assembly
        /// </summary>
        | Assembly = 7
        /// <summary>
        /// Classifies a CLR element as method parameter
        /// </summary>
        | Parameter = 8
        /// <summary>
        /// Classifies a CLR element a union case
        /// </summary>
        | UnionCase = 9

    /// <summary>
    /// Classifies CLR member elements
    /// </summary>
    type ClrMemberKind =
        /// <summary>
        /// Classifies a CLR element as a property
        /// </summary>
        | Property = 1
        /// <summary>
        /// Classifies a CLR element as a property
        /// </summary>
        | Method = 2
        /// <summary>
        /// Classifies a CLR element as a field
        /// </summary>
        | StorageField = 3
        /// <summary>
        /// Classifies a CLR element as an event
        /// </summary>
        | Event = 4
        /// <summary>
        /// Classifies a CLR element as a constructor
        /// </summary>
        | Constructor = 5
        

    /// <summary>
    /// Specifies the visibility of a CLR element
    /// </summary>
    type ClrAccess =
        /// Indicates that the target is visible everywhere 
        | PublicAccess
        /// Indicates that the target is visible only to subclasses
        /// Not supported in F#
        | ProtectedAccess
        /// Indicates that the target is not visible outside its defining scope
        | PrivateAccess
        /// Indicates that the target is visible throughout the assembly in which it is defined
        | InternalAccess
        /// Indicates that the target is visible to subclasses and the defining assembly
        /// Not supported in F#
        | ProtectedOrInternalAccess
        /// Indicates that the target is visible to subclasses in the defining assemlby
        | ProtectedAndInternalAccess
        
    /// <summary>
    /// Represents a type name
    /// </summary>
    [<DebuggerDisplay("{Text, nq}")>]
    type ClrTypeName = ClrTypeName of simpleName : string * fullName : string option * assemblyQualifiedName : string option
    with
        /// <summary>
        /// Gets the simple name of the type
        /// </summary>
        member this.SimpleName = match this with ClrTypeName(simpleName=x) -> x

        member this.Text = 
            match this with 
                ClrTypeName(simpleName, fullName, aqn) ->
                    match aqn with
                    | Some(x) -> x
                    | None ->
                        match fullName with
                        | Some(x) -> x
                        | None ->
                            simpleName
        override this.ToString() = this.Text
                    

    /// <summary>
    /// Represents an assembly name
    /// </summary>
    type ClrAssemblyName = ClrAssemblyName of simpleName : string * fullName : string option
    with
        /// <summary>
        /// Gets the simple name of the assembly
        /// </summary>
        member this.SimpleName = match this with ClrAssemblyName(simpleName=x) -> x
        member this.FullName = match this with ClrAssemblyName(fullName=x) -> x
        member this.Text =
            match this with ClrAssemblyName(simpleName, fullName) -> match fullName with
                                                                        | Some(x) -> x
                                                                        | None ->
                                                                            simpleName    
        override this.ToString() = this.Text

    /// <summary>
    /// Represents the name of a member
    /// </summary>    
    type ClrMemberName = ClrMemberName of string
    with
        member this.Text = match this with ClrMemberName(x) -> x
        override this.ToString() = this.Text
    
    /// <summary>
    /// Represents the name of a parameter
    /// </summary>    
    type ClrParameterName = ClrParameterName of string
    with
        member this.Text = match this with ClrParameterName(x) -> x
        override this.ToString() = this.Text

    /// <summary>
    /// Represents the name of a CLR element
    /// </summary>
    type ClrElementName =
        ///Specifies the name of an assembly 
        | AssemblyElementName of ClrAssemblyName
        ///Specifies the name of a type 
        | TypeElementName of ClrTypeName
        ///Specifies the name of a type member
        | MemberElementName of ClrMemberName
        ///Specifies the name of a parameter
        | ParameterElementName of ClrParameterName
    with
        member this.Text =
            match this with 
                | AssemblyElementName(x) -> x.Text
                | TypeElementName(x) -> x.Text
                | MemberElementName(x) -> x.Text
                | ParameterElementName(x) -> x.Text
        override this.ToString() = this.Text
        member this.SimpleName =
            match this with 
                | AssemblyElementName(x) -> x.SimpleName
                | TypeElementName(x) -> x.SimpleName
                | MemberElementName(x) -> x.Text
                | ParameterElementName(x) -> x.Text
            



       
module ReflectedKind =
    let fromInstance (o : obj) =
        match o with
        | :? Assembly -> ReflectedKind.Assembly
        | :? Type -> ReflectedKind.Type
        | :? MethodInfo -> ReflectedKind.Method
        | :? PropertyInfo -> ReflectedKind.Property
        | :? FieldInfo -> ReflectedKind.Field
        | :? ConstructorInfo -> ReflectedKind.Constructor
        | :? EventInfo -> ReflectedKind.Event
        | :? ParameterInfo -> ReflectedKind.Parameter
        | :? UnionCaseInfo -> ReflectedKind.UnionCase
        | _ -> nosupport()



/// <summary>
/// Defines <see cref="ClrElementName"/>-related augmentations 
/// </summary>
[<AutoOpen>]
module ClrNameExtensions =
    /// <summary>
    /// Defines augmentations for the <see cref="ClrTypeName"/> type
    /// </summary>
    type ClrTypeName 
    with
        /// <summary>
        /// Gets the local name of the type (which does not include enclosing type names or namespace)
        /// </summary>
        member this.SimpleName = match this with ClrTypeName(simpleName=x) -> x
        /// <summary>
        /// Gets namespace and nested type-qualified name of the type
        /// </summary>
        member this.FullName = match this with ClrTypeName(fullName=x) -> x
        /// <summary>
        /// Gets the assembly-qualified type name of the type
        /// </summary>
        member this.AssemblyQualifiedName = match this with ClrTypeName(assemblyQualifiedName=x) -> x
        
        member this.Text = 
            match this with 
                ClrTypeName(simple,full, aqn) -> match aqn with                                    
                                                    | Some(x) -> x
                                                    | None ->
                                                        match full with
                                                        | Some(x) -> x
                                                        | None -> simple 



    /// <summary>
    /// Defines augmentations for the <see cref="ClrMemberElementName"/> type
    /// </summary>
    type ClrMemberName
    with
        member this.Text = match this with ClrMemberName(x) -> x
    

    /// <summary>
    /// Defines augmentations for the <see cref="ClrParameterElementName"/> type
    /// </summary>
    type ClrParameterName
    with
        member this.Text = match this with ClrParameterName(x) -> x

    /// <summary>
    /// Represents the name of a CLR element
    /// </summary>
    type ClrElementName
    with
        member this.Text =
            match this with
            | AssemblyElementName x -> x.Text
            | TypeElementName x -> x.Text
            | MemberElementName x -> x.Text
            | ParameterElementName x -> x.Text
                


    

    



    
