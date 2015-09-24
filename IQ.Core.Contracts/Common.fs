// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework.Contracts

open System
open System.Reflection
open System.Diagnostics
open System.Collections
open System.Collections.Generic

open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns

/// <summary>
/// Responsible for identifying a value in a ValueMap
/// </summary>
type ValueIndexKey = ValueIndexKey of name : string  * position : int
with
    member this.Name = match this with |ValueIndexKey(name=x) -> x
    member this.Position = match this with |ValueIndexKey(position=x) -> x

/// <summary>
/// Represents a collection of name-indexed or position-indexed values
/// </summary>
type ValueIndex = ValueIndex of (ValueIndexKey*obj) list
    
type BinaryFunc<'T,'TResult> = Func<'T,'T,'TResult>
type BinaryFunc<'T> = BinaryFunc<'T,'T>

type BinaryPredicate<'T0,'T1> = Func<'T0,'T1,bool>
type BinaryPredicate<'T> = BinaryPredicate<'T,'T>

type UnaryFunc<'T,'TResult> = Func<'T,'TResult>
type UnaryFunc<'T> = Func<'T,'T>

/// <summary>
/// Defines the contract used by the application to retrieve and (eventually) specify
/// configuration settings
/// </summary>
type IConfigurationManager =
    /// <summary>
    /// Gets an identified configuration value for a specified environment
    /// </summary>
    /// <param name="environment">The name of the environment</param>
    /// <param name="name">The name of the value</param>
    abstract GetEnvironmentValue:environment:string->name:string->string

    /// <summary>
    /// Gets an identified configuration value for the environment named in the configuration file
    /// </summary>
    /// <param name="environment">The name of the environment</param>
    /// <param name="name">The name of the value</param>
    abstract GetValue:name:string->string

/// <summary>
/// Type alias for delegate that produces configured service realizations
/// </summary>
type ServiceFactory<'TConfig,'I> = 'TConfig->'I
    
/// <summary>
/// Defines contract to allow items/factories to be registered with the container
/// </summary>
type ICompositionRegistry =
    /// <summary>
    /// Registers an instance value
    /// </summary>
    abstract RegisterInstance<'I> : 'I->unit when 'I : not struct
        
    /// <summary>
    /// Registers the interfaces implemented by the provided type
    /// </summary>
    abstract RegisterInterfaces<'T> : unit->unit

    /// <summary>
    /// Registers a service factory method
    /// </summary>
    abstract RegisterFactory<'TConfig, 'I> : ServiceFactory<'TConfig,'I> -> unit when 'I : not struct

    /// <summary>
    /// Registers a service factory method from a delegate (to place nice with C#)
    /// </summary>
    abstract RegisterFactoryDelegate<'TConfig, 'I> : Func<'TConfig,'I> ->unit when 'I : not struct

/// <summary>
/// Defines contract for an application execution context for a given container/root
/// </summary>
type IAppContext =
    inherit IDisposable
        
    /// <summary>
    /// Resoves contract that requires no resolution-time configuration
    /// </summary>
    abstract Resolve<'T> :unit->'T
        
    /// <summary>
    /// Resolves contract by finding a constructor on the type with a parameter name 'key' and passing 
    /// the value to it when instantiating it. This should be used sparingly, if ever
    /// </summary>
    /// <param name="key">The name of the parameter</param>
    /// <param name="value">The value to inject</param>
    abstract Resolve<'T> : key :string * value : obj->'T
        
    /// <summary>
    /// Resoves contract using supplied configuration
    /// </summary>
    abstract Resolve<'C,'I> : config : 'C -> 'I


/// <summary>
/// Defines the contract for the composition root in the DI pattern
/// </summary>
type ICompositionRoot =        
    inherit IDisposable
    inherit ICompositionRegistry
    
    /// <summary>
    /// Call when all dependencies have been specified and the container should
    /// be readied for use
    /// </summary>
    abstract Seal:unit->unit
    
    /// <summary>
    /// Creates a context within which to resolve depencies; resolved dependencies
    /// are disposed when the context is disposed
    /// </summary>
    abstract CreateContext:unit -> IAppContext

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
    /// Classifies a type as an enum
    /// </summary>
    | Enum = 9

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
    | Field = 3
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
type ClrAccessKind =
    /// Indicates that the target is visible everywhere 
    | Public = 1
    /// Indicates that the target is visible only to subclasses
    /// Not supported in F#
    | Protected = 2
    /// Indicates that the target is not visible outside its defining scope
    | Private = 3
    /// Indicates that the target is visible throughout the assembly in which it is defined
    | Internal = 4
    /// Indicates that the target is visible to subclasses and the defining assembly
    /// Not supported in F#; supported in C# using the protected internal modifiers
    | ProtectedOrInternal = 5
    /// Indicates that the target is visible to subclasses in the defining assembl
    /// Not supported in C# or F#
    | ProtectedAndInternal = 6
        
/// <summary>
/// Represents a type name
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrTypeName = ClrTypeName of simpleName : string * fullName : string option * assemblyQualifiedName : string option
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = match this with ClrTypeName(simpleName=x) -> x
                    

/// <summary>
/// Represents an assembly name
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrAssemblyName = ClrAssemblyName of simpleName : string * fullName : string option
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = match this with ClrAssemblyName(simpleName=x) -> x

/// <summary>
/// Represents the name of a member
/// </summary>    
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrMemberName = ClrMemberName of string
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = match this with ClrMemberName(x) -> x
    
/// <summary>
/// Represents the name of a parameter
/// </summary>    
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrParameterName = ClrParameterName of string
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = match this with ClrParameterName(x) -> x

/// <summary>
/// Represents the name of a CLR element
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
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
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() =
        match this with
        | AssemblyElementName(x) -> x.ToString()
        | TypeElementName(x) -> x.ToString()
        | MemberElementName(x) -> x.ToString()
        | ParameterElementName(x) -> x.ToString()

/// <summary>
/// Represents an association between an attribute and the element to which it applies
/// </summary>
type ClrAttribution = {
    /// The element to which the attribute is applied
    Target : ClrElementName
    /// The name of the attribute
    AttributeName : ClrTypeName
    /// The values applied by the attribute
    AppliedValues : ValueIndex
    /// The attribute instance if applicable
    AttributeInstance : Attribute option
}

    
/// <summary>
/// Represents a CLR property
/// </summary>
type ClrProperty = {
    /// The name of the property 
    Name : ClrMemberName        
    /// The position of the property
    Position : int         
    /// The reflected property, if applicable
    ReflectedElement : PropertyInfo option        
    /// The attributes applied directly to the property
    Attributes : ClrAttribution list
    /// The attributes applied directly to the get method, if applicable
    GetMethodAttributes : ClrAttribution list
    /// The attributes applied directly to the set method, if applicable
    SetMethodAttributes : ClrAttribution list
    /// The name of the type that declares the property
    DeclaringType : ClrTypeName           
    /// The type of the property value
    ValueType : ClrTypeName
    /// Specifies whether the property is of option<> type
    IsOptional : bool
    /// Specifies whether the property is of Nullable<> type
    IsNullable : bool
    /// Specifies whether the property has a get accessor
    CanRead : bool
    /// The access specifier of the get accessor if one exists
    ReadAccess : ClrAccessKind option      
    /// Specifies whether the property has a set accessor
    CanWrite : bool
    /// The access specifier of the set accessor if one exists
    WriteAccess : ClrAccessKind option        
    /// Specifies whether the property is static
    IsStatic : bool
}
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        sprintf "%O : %O" this.Name this.ValueType

/// <summary>
/// Represents a field
/// </summary>
type ClrField = {
    /// The name of the property 
    Name : ClrMemberName        
    /// The reflected property, if applicable
    ReflectedElement : FieldInfo option
    /// The position of the property
    Position : int            
    /// The attributes applied to the field
    Attributes : ClrAttribution list
    /// The field's access specifier
    Access : ClrAccessKind
    /// Specifies whether the field is static
    IsStatic : bool
    /// Specifies the name of the field type
    FieldType : ClrTypeName
    /// The name of the type that declares the field
    DeclaringType : ClrTypeName   
    /// Specifies whether the field is a literal value
    IsLiteral : bool
    /// The value of the literal
    LiteralValue : obj option        
}
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        sprintf "%O : %O" this.Name this.FieldType

/// <summary>
/// Represents a method parameter
/// </summary>
type ClrMethodParameter = {
    /// The name of the parameter
    Name : ClrParameterName
    /// The position of the parameter
    Position : int    
    /// The reflected method, if applicable                
    ReflectedElement : ParameterInfo option
    /// The attributes applied to the parameter
    Attributes : ClrAttribution list
    /// Specifies whether the parameter is required
    CanOmit : bool   
    /// Specifies the name of the parameter type
    ParameterType : ClrTypeName    
    /// The name of the method that declares the parameter
    DeclaringMethod : ClrMemberName
    /// Specifies whether the parameter represents a return
    IsReturn : bool
}
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        sprintf "%O : %O" this.Name this.ParameterType


/// <summary>
/// Represents a method
/// </summary>
type ClrMethod = {
    /// The name of the method
    Name : ClrMemberName
    /// The reflected method, if applicable        
    ReflectedElement : MethodInfo option
    /// The position of the method
    Position : int            
    /// The attributes applied to the method
    Attributes : ClrAttribution list
    /// The method's access specifier
    Access : ClrAccessKind
    /// Specifies whether the method is static
    IsStatic : bool
    /// The method parameters
    Parameters : ClrMethodParameter list
    /// The method return type
    ReturnType : ClrTypeName option
    /// The attributes applied to the method return
    /// TODO: calculate this from the return parameter attributes
    ReturnAttributes : ClrAttribution list
    /// The name of the type that declares the method
    DeclaringType : ClrTypeName           
}
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        this.Name.ToString()


/// <summary>
/// Represents a constructor
/// </summary>
type ClrConstructor = {
    /// The name of the constructor    
    Name : ClrMemberName
    /// The reflected constructor, if applicable        
    ReflectedElement : ConstructorInfo option
    /// The position of the property
    Position : int            
    /// The attributes applied to the constructor
    Attributes : ClrAttribution list        
    /// The constructor's access specifier
    Access : ClrAccessKind
    /// Specifies whether the constructor is static
    IsStatic : bool
    /// The constructor parameters
    Parameters : ClrMethodParameter list
    /// The name of the type that declares the constructor
    DeclaringType : ClrTypeName           
}

/// <summary>
/// Represents an event
/// </summary>
type ClrEvent = {
    /// The name of the event
    Name : ClrMemberName
    /// The reflected event, if applicable        
    ReflectedElement : EventInfo option
    /// The position of the event
    Position : int                
    /// The attributes applied to the event
    Attributes : ClrAttribution list
    /// The name of the type that declares the event
    DeclaringType : ClrTypeName           
}

/// <summary>
/// Represents a union case
/// </summary>
type ClrUnionCase = {
    /// The name of the type
    Name : ClrMemberName
    /// The position of the type
    Position : int    
    /// The attributes applied to the case
    Attributes : ClrAttribution list
    /// The name of the type that declares the case
    DeclaringType : ClrTypeName           
    /// The reflected case, if applicable
    ReflectedElement : UnionCaseInfo option
}
    
/// <summary>
/// Represents a member
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrMember =
| PropertyMember of ClrProperty
| FieldMember of ClrField
| MethodMember of ClrMethod
| EventMember of ClrEvent
| ConstructorMember of ClrConstructor
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        match this with
        | PropertyMember(m) -> m.ToString()
        | FieldMember(m) -> m.ToString()
        | MethodMember(m) -> m.ToString()
        | EventMember(m) -> m.ToString()
        | ConstructorMember(m) -> m.ToString()
                                                            

type ClrTypeInfo = {
    /// The name of the type
    Name : ClrTypeName
    /// The position of the type
    Position : int
    /// The reflected type, if applicable        
    ReflectedElement : Type option
    /// The name of the type that declares the type, if any
    DeclaringType : ClrTypeName option
    /// The nested types declared by the type
    DeclaredTypes : ClrTypeName list
    /// The kind of type, if recognized
    Kind : ClrTypeKind
    //Specifies whether the type is of the form option<_>
    IsOptionType : bool
    //The type members
    Members : ClrMember list
    //The access specifier applied to the type
    Access : ClrAccessKind
    /// Specifies whether the type is static
    IsStatic : bool
    /// The attributes applied to the type
    Attributes : ClrAttribution list
    /// Specifies the type of the encapsulated value; will be different from
    /// the Name whenever dealing with options, collections and other
    /// parametrized types
    ItemValueType : ClrTypeName
    /// The namespace in which the type is defined
    Namespace :string

}
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = this.Name.ToString()

/// <summary>
/// Represents a CLR class
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrClass = ClrClass of info : ClrTypeInfo
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        match this with ClrClass(info=x) -> x.Name.ToString()
    
/// <summary>
/// Represents a CLR Enum
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrEnum = ClrEnum of numericType : ClrTypeName * info : ClrTypeInfo
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        match this with ClrEnum(info=x) -> x.Name.ToString()

/// <summary>
/// Represents an F# module
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrModule = ClrModule of info : ClrTypeInfo
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        match this with ClrModule(info=x) -> x.Name.ToString()

/// <summary>
/// Represents a collection type
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrCollection =  ClrCollection of kind : ClrCollectionKind * info : ClrTypeInfo
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        match this with ClrCollection(info=x) -> sprintf "%O seq" x.Name

    member this.ItemType = match this with ClrCollection(kind, info) -> info.ItemValueType

    member this.CollectionKind = match this with ClrCollection(kind, info) -> kind

/// <summary>
/// Represents a struct
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrStruct = ClrStruct of isNullable : bool * info : ClrTypeInfo
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        match this with ClrStruct(info=x) -> x.Name.ToString()

/// <summary>
/// Represents an F# union
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrUnion = ClrUnion of cases : ClrUnionCase list * info : ClrTypeInfo
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        match this with ClrUnion(info=x) -> x.Name.ToString()

/// <summary>
/// Represents an F# record
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrRecord = ClrRecord of info : ClrTypeInfo
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        match this with ClrRecord(info=x) -> x.Name.ToString()

/// <summary>
/// Represents an interface
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrInterface = ClrInterface of info : ClrTypeInfo
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        match this with ClrInterface(info=x) -> x.Name.ToString()

/// <summary>
/// Represents some sort of type
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrType =
    | ClassType of ClrClass
    | EnumType of ClrEnum
    | ModuleType of ClrModule
    | CollectionType of ClrCollection
    | StructType of ClrStruct
    | UnionType of ClrUnion
    | RecordType of ClrRecord
    | InterfaceType of ClrInterface
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = 
        match this with 
        | ClassType(x) -> x.ToString()
        | EnumType(x) -> x.ToString()
        | ModuleType(x) -> x.ToString()
        | CollectionType(x) -> x.ToString()
        | StructType(x) -> x.ToString()
        | UnionType(x) -> x.ToString()
        | RecordType(x) -> x.ToString()
        | InterfaceType(x) -> x.ToString()
          
/// <summary>
/// Represents an assembly
/// </summary>
type ClrAssembly = {
    /// The name of the assembly
    Name : ClrAssemblyName
    /// The reflected assembly, if applicable                
    ReflectedElement : Assembly option
    /// The position of the assembly relative to its specification/reflection context
    Position : int
    /// The types defined in the assembly
    Types : ClrType list
    /// The attributes applied to the assembly
    Attributes : ClrAttribution list
    /// The assemblies referenced by the subject
    References : ClrAssemblyName list
}
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() = this.Name.ToString()
    

/// <summary>
/// Represents any CLR element
/// </summary>
[<DebuggerDisplay("{ToString(),nq}")>]
type ClrElement =
    | MemberElement of description : ClrMember
    | TypeElement of description : ClrType
    | AssemblyElement of description : ClrAssembly
    | ParameterElement of description : ClrMethodParameter
    | UnionCaseElement of description : ClrUnionCase
with
    /// <summary>
    /// Renders a textual representation of the instance that is suitable for diagnostic purposes
    /// </summary>
    override this.ToString() =
        match this with
        | MemberElement(e) -> sprintf "Member: %O" e
        | TypeElement(e) -> sprintf "Type: %O" e
        | AssemblyElement(e) -> sprintf "Assembly: %O" e
        | ParameterElement(e) -> sprintf "Parameter: %O" e
        | UnionCaseElement(e) -> sprintf "Union Case: %O" e
                      
/// <summary>
/// Represents the intent to select a set of ClrType representations
/// </summary>
type ClrTypeQuery =
    /// Find a type by its name
    | FindTypeByName of name : ClrTypeName
    /// Find types of a given kind
    | FindTypesByKind of kind : ClrTypeKind

/// <summary>
/// Represents the intent to select a set of ClrProperty representations
/// </summary>
type ClrPropertyQuery = 
    | FindPropertyByName of name : ClrMemberName * typeQuery : ClrTypeQuery
    | FindPropertiesByType of typeQuery : ClrTypeQuery
         
/// <summary>
/// Represents the intent to select a set of ClrAssembly representations
/// </summary>
type ClrAssemblyQuery =
    | FindAssemblyByName of name : ClrAssemblyName

/// <summary>
/// Represents the intent to select a set of ClrElement representations
/// </summary>
type ClrElementQuery =
    ///Find assembly elements
    | FindAssemblyElement of query : ClrAssemblyQuery
    ///Find property elements
    | FindPropertyElement of query : ClrPropertyQuery
    ///Find type elements
    | FindTypeElement of query : ClrTypeQuery

/// <summary>
/// Defines contract realized by CLR metadata provider
/// </summary>
type IClrMetadataProvider =
    /// <summary>
    /// Find types accessible to the provider
    /// </summary>
    abstract FindTypes:ClrTypeQuery->ClrType list
    /// <summary>
    /// Find assemblies accessible to the provider
    /// </summary>
    abstract FindAssemblies:ClrAssemblyQuery->ClrAssembly list
    /// <summary>
    /// Find properties accessible to the provider
    /// </summary>
    abstract FindProperties:ClrPropertyQuery->ClrProperty list
    /// <summary>
    /// Find elements accessible to the provider
    /// </summary>        
    abstract FindElements:ClrElementQuery->ClrElement list

/// <summary>
/// Indentifies a data conversion operation
/// </summary>
type TransformationIdentifier = TransformationIdentifier of category : string * srcType : ClrTypeName * dstType : ClrTypeName
with
    member this.Category = match this with TransformationIdentifier(category=x) ->x
    member this.SrcType = match this with TransformationIdentifier(srcType=x) -> x
    member this.DstType = match this with TransformationIdentifier(dstType=x) ->x

/// <summary>
/// Defines contract for a transformer that realizes a set of transformations in a given category
/// </summary>
type ITransformer =
    /// <summary>
    /// Converts a supplied value to the destination type
    /// </summary>
    /// <param name="dstType">The destination type</param>
    /// <param name="srcValue">The value to convert</param>
    abstract Transform: dstType : Type -> srcValue : obj -> obj        
        
    /// <summary>
    /// Converts a sequence of supplied values to the destination type
    /// </summary>
    /// <param name="dstType">The destination type</param>
    /// <param name="srcValue">The values to convert</param>
    abstract TransformMany: dstType : Type -> srcValues : 'TSrc seq -> obj seq

    /// <summary>
    /// Gets types into which a source type may be transformed
    /// </summary>
    /// <param name="srcType">The source type</param>
    abstract GetTargetTypes: srcType : Type -> Type list
                
    /// <summary>
    /// Gets the conversions supported by the converter
    /// </summary>
    abstract GetKnownTransformations: unit->TransformationIdentifier list        
        
    /// <summary>
    /// Determines whether the transformer can project an instace of the source type onto the destination type
    /// </summary>
    /// <param name="srcType">The source Type</param>
    /// <param name="dstType">The destination type</param>
    abstract CanTransform : srcType : Type -> dstType : Type -> bool

    /// <summary>
    /// Converts an array of possibly heterogenous source values to an array of possibly heterogenous 
    /// target values
    /// </summary>
    abstract TransformArray: dstTypes : Type[] -> srcValues : obj[] -> obj[]
        
    /// <summary>
    /// Converts to a generic version of itself
    /// </summary>
    abstract AsTyped:unit -> ITypedTransformer
and
    ITypedTransformer =
        /// <summary>
        /// Converts a supplied value to the destination type
        /// </summary>
        /// <param name="srcValue">The value to convert</param>
        abstract Transform<'TSrc, 'TDst> : srcValue :'TSrc ->'TDst
        
        /// <summary>
        /// Converts a sequence of supplied values to the destination type
        /// </summary>
        /// <param name="dstType">The destination type</param>
        /// <param name="srcValue">The values to convert</param>
        abstract TransformMany<'TSrc,'TDst> : srcValues : 'TSrc seq -> 'TDst seq
        
        /// <summary>
        /// Gets types into which a source type may be transformed
        /// </summary>
        /// <param name="srcType">The source type</param>
        abstract GetTargetTypes<'TSrc> : category : string -> Type list
        
        /// <summary>
        /// Gets the conversions supported by the converter
        /// </summary>
        abstract GetKnownTransformations: unit->TransformationIdentifier list        

        /// <summary>
        /// Determines whether the transformer can project an instace of the source type onto the destination type
        /// </summary>
        /// <param name="srcType">The source Type</param>
        /// <param name="dstType">The destination type</param>
        abstract CanTransform<'TSrc,'TDst> :unit -> bool
        
        /// <summary>
        /// Converts to a non-generic version of itself
        /// </summary>
        abstract AsUntyped:unit->ITransformer

/// <summary>
/// Specifies operations for converting POCO values to/from alternate representations
/// </summary>
type IPocoConverter =
    /// <summary>
    /// Creates a ValueIndex from a record value
    /// </summary>
    /// <param name="record">The record whose values will be indexed</param>
    abstract ToValueIndex:record : obj->ValueIndex
    /// <summary>
    /// Creates an array of field values, in declaration order, for a specified record value
    /// </summary>
    /// <param name="record">The record from which a value array will be created</param>
    abstract ToValueArray:obj->obj[]
    /// <summary>
    /// Creates a record from an array of values that are specified in declaration order
    /// </summary>
    /// <param name="valueArray">An array of values in declaration order</param>
    /// <param name="t">The record type</param>
    abstract FromValueArray:valueArray : obj[] * t : Type->obj
    /// <summary>
    /// Creates a record from data supplied in a ValueIndex
    /// </summary>
    /// <param name="idx">The value index</param>
    /// <param name="t">The record type</param>
    abstract FromValueIndex: idx : ValueIndex * t : Type -> obj


/// <summary>
/// Defines the configuration contract for IPocoConverter realizations
/// </summary>
type PocoConverterConfig = PocoConverterConfig of transformer : ITransformer
with
    member this.Transformer = match this with PocoConverterConfig(transformer=x) -> x
    