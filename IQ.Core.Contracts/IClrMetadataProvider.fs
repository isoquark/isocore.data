namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics

open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns

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
    /// Not supported in F#
    | ProtectedOrInternal = 5
    /// Indicates that the target is visible to subclasses in the defining assemlby
    | ProtectedAndInternal = 6
        
/// <summary>
/// Represents a type name
/// </summary>
type ClrTypeName = ClrTypeName of simpleName : string * fullName : string option * assemblyQualifiedName : string option
with
    override this.ToString() = match this with ClrTypeName(simpleName=x) -> x
                    

/// <summary>
/// Represents an assembly name
/// </summary>
type ClrAssemblyName = ClrAssemblyName of simpleName : string * fullName : string option
with
    override this.ToString() = match this with ClrAssemblyName(simpleName=x) -> x

/// <summary>
/// Represents the name of a member
/// </summary>    
type ClrMemberName = ClrMemberName of string
with
    override this.ToString() = match this with ClrMemberName(x) -> x
    
/// <summary>
/// Represents the name of a parameter
/// </summary>    
type ClrParameterName = ClrParameterName of string
with
    override this.ToString() = match this with ClrParameterName(x) -> x

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
    AttributeInstance : Attribute option
    /// The attribute instance if applicable
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
    /// The value of the literal encoded as a string, if applicable
    LiteralValue : string option        
}

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
type ClrMember =
| PropertyMember of ClrProperty
| FieldMember of ClrField
| MethodMember of ClrMethod
| EventMember of ClrEvent
| ConstructorMember of ClrConstructor
                                                        

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

}

/// <summary>
/// Represents a CLR class
/// </summary>
type ClrClass = ClrClass of info : ClrTypeInfo

/// <summary>
/// Represents a CLR Enum
/// </summary>
type ClrEnum = ClrEnum of numericType : ClrTypeName * info : ClrTypeInfo

/// <summary>
/// Represents an F# module
/// </summary>
type ClrModule = ClrModule of info : ClrTypeInfo

/// <summary>
/// Represents a collection type
/// </summary>
type ClrCollection =  ClrCollection of kind : ClrCollectionKind * info : ClrTypeInfo

/// <summary>
/// Represents a struct
/// </summary>
type ClrStruct = ClrStruct of isNullable : bool * info : ClrTypeInfo

/// <summary>
/// Represents an F# union
/// </summary>
type ClrUnion = ClrUnion of cases : ClrUnionCase list * info : ClrTypeInfo

/// <summary>
/// Represents an F# record
/// </summary>
type ClrRecord = ClrRecord of info : ClrTypeInfo

/// <summary>
/// Represents an interface
/// </summary>
type ClrInterface = ClrInterface of info : ClrTypeInfo

type ClrType =
    | ClassType of ClrClass
    | EnumType of ClrEnum
    | ModuleType of ClrModule
    | CollectionType of ClrCollection
    | StructType of ClrStruct
    | UnionType of ClrUnion
    | RecordType of ClrRecord
    | InterfaceType of ClrInterface

    
       
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
}

/// <summary>
/// Represents any CLR element
/// </summary>
type ClrElement =
    | MemberElement of description : ClrMember
    | TypeElement of description : ClrType
    | AssemblyElement of description : ClrAssembly
    | ParameterElement of description : ClrMethodParameter
    | UnionCaseElement of description : ClrUnionCase
                      
/// <summary>
/// Represents the intent to select a set of <see cref="ClrType"/> representations
/// </summary>
type ClrTypeQuery =
    /// Find a type by its name
    | FindTypeByName of name : ClrTypeName
    /// Find types of a given kind
    | FindTypesByKind of kind : ClrTypeKind

/// <summary>
/// Represents the intent to select a set of <see cref="ClrProperty"/> representations
/// </summary>
type ClrPropertyQuery = 
    | FindPropertyByName of name : ClrMemberName * typeQuery : ClrTypeQuery
    | FindPropertiesByType of typeQuery : ClrTypeQuery
         
/// <summary>
/// Represents the intent to select a set of <see cref="ClrAssembly"/> representations
/// </summary>
type ClrAssemblyQuery =
    | FindAssemblyByName of name : ClrAssemblyName

/// <summary>
/// Represents the intent to select a set of <see cref="ClrElement"/> representations
/// </summary>
type ClrElementQuery =
    | FindAssemblyElement of query : ClrAssemblyQuery
    | FindPropertyElement of query : ClrPropertyQuery
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

