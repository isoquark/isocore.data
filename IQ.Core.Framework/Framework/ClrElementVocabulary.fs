namespace IQ.Core.Framework

open System
open System.Reflection
open System.Diagnostics
open System.Collections.Generic

open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns

/// <summary>
/// Defines a rudimentary vocabulary for representing CLR metadata. By intent, it is not complete.
/// </summary>
[<AutoOpen>]
module ClrElementVocabulary = 

    /// <summary>
    /// Describes a property
    /// </summary>
    type ClrPropertyDescription = {
        /// The name of the property 
        Name : ClrMemberName
        
        /// The position of the property
        Position : int
        
        /// The name of the type that declares the property
        DeclaringType : ClrTypeName       
    
        /// The type of the property value
        ValueType : ClrTypeName

        /// Specifies whether the property is of option<> type
        IsOptional : bool

        /// Specifies whether the property has a get accessor
        CanRead : bool

        /// Specifies the access of the get accessor if applicable
        ReadAccess : ClrAccess option

        /// Specifies whether the property has a set accessor
        CanWrite : bool

        /// Specifies the access of the set accessor if applicable
        WriteAccess : ClrAccess option
    }

    /// <summary>
    /// Describes a field
    /// </summary>
    type ClrFieldDescription = {
        /// The name of the property 
        Name : ClrMemberName
        
        /// The position of the property
        Position : int            
    }

    type ClrMemberDescription =
    | PropertyMemberDescription of ClrPropertyDescription
    | PropertyFieldDescription of ClrFieldDescription

    /// <summary>
    /// Describes a type
    /// </summary>
    type ClrTypeDescription = {
        /// The name of the type
        Name : ClrTypeName

        /// The position of the type
        Position : int

        /// The name of the type that declares the type, if any
        DeclaringType : ClrTypeName option

        DeclaredTypes : ClrTypeName list

        Members : ClrMemberDescription list
    }

    /// <summary>
    /// Describes an assembly
    /// </summary>
    type ClrAssemblyDescription = {
        
        Name : ClrAssemblyName

        Types : ClrTypeDescription list
    }



    
    type IReflectionPrimitive =
        abstract Primitive:obj
    
    /// <summary>
    /// Encapsulates a Type, PropertyInfo other other framework-defined reflection type along
    /// with its sibling-relative position if known
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrReflectionPrimitive<'T> = ClrReflectionPrimitive of primitive : 'T * pos : int option
    with
        member this.Primitive = match this with ClrReflectionPrimitive(primitive=x) -> x
        member this.Position = match this with ClrReflectionPrimitive(pos=x) -> x
        override this.ToString() = this.Primitive.ToString()

        interface IReflectionPrimitive with
            member this.Primitive = this.Primitive :> obj

    /// <summary>
    /// Represents and encapsulates a CLR Type
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrTypeElement  = ClrTypeElement of ClrReflectionPrimitive<Type>
    with 
        override this.ToString() = match this with ClrTypeElement(x) -> x.Primitive.Name
    
    /// <summary>
    /// Represents and encapsulates a CLR Assembly
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrAssemblyElement = ClrAssemblyElement of ClrReflectionPrimitive<Assembly>
    with
        override this.ToString() = match this with ClrAssemblyElement(x)  -> x.Primitive.SimpleName    
    /// <summary>
    /// Represents and encapsulates a CLR (method) parameter 
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrParameterElement = ClrParameterElement of ClrReflectionPrimitive<ParameterInfo>
    with
        override this.ToString() = match this with ClrParameterElement(x)  -> x.Primitive.Name
    
    
    /// <summary>
    /// Represents and encapsulates a CLR property
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrPropertyElement = ClrPropertyElement of ClrReflectionPrimitive<PropertyInfo>
    with
        override this.ToString() = match this with ClrPropertyElement(x)  -> x.Primitive.Name

    /// <summary>
    /// Represents and encapsulates a CLR field
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrFieldElement = ClrFieldElement of ClrReflectionPrimitive<FieldInfo>
    with
        override this.ToString() = match this with ClrFieldElement(x)  -> x.Primitive.Name

    /// <summary>
    /// Represents and encapsulates a union case
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrUnionCaseElement = ClrUnionCaseElement of ClrReflectionPrimitive<UnionCaseInfo>
    with
        override this.ToString() = match this with ClrUnionCaseElement (x)  -> x.Primitive.Name

    /// <summary>
    /// Represents and encapsulates a CLR method
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrMethodElement = ClrMethodElement of ClrReflectionPrimitive<MethodInfo>
    with
        override this.ToString() = match this with ClrMethodElement(x)  -> x.Primitive.Name
    
    /// <summary>
    /// Represents a member that store data of a specific type
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrDataMemberElement = 
        | PropertyMember of ClrPropertyElement
        | FieldMember of ClrFieldElement
    with
        override this.ToString() = match this with PropertyMember(x) -> x.ToString() | FieldMember(x) -> x.ToString()

    /// <summary>
    /// Represents a member of a type (but does not consider nested types as members)
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrMemberElement =
        | DataMemberElement of ClrDataMemberElement
        | MethodElement of ClrMethodElement
    with
        override this.ToString() = match this with DataMemberElement(x) -> x.ToString() | MethodElement(x) -> x.ToString()

    /// <summary>
    /// Represents a CLR metadata primitive
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type ClrElement =
        | MemberElement of element : ClrMemberElement * children : ClrElement list
        | TypeElement of element : ClrTypeElement* children : ClrElement list
        | AssemblyElement of element : ClrAssemblyElement* children : ClrElement list
        | ParameterElement of element : ClrParameterElement
        | UnionCaseElement of element : ClrUnionCaseElement* children : ClrElement list
    with
        override this.ToString() = 
            match this with 
                | MemberElement (element=x) -> x.ToString()
                | TypeElement (element=x) ->  x.ToString()
                | AssemblyElement (element=x) ->  x.ToString()
                | ParameterElement (element=x) ->  x.ToString()
                | UnionCaseElement (element=x) ->  x.ToString()


module ClrElementKind =

    /// <summary>
    /// Determines whether the kind classifies a member
    /// </summary>
    /// <param name="kind">The kind</param>
    let isMemberKind kind =
        match kind with
        | ClrElementKind.Method | ClrElementKind.Property | ClrElementKind.Field | ClrElementKind.Event -> true
        | _ -> false

    /// <summary>
    /// Determines whether the kind classifies a data member
    /// </summary>
    /// <param name="kind">The kind</param>
    let isDataMemberKind kind =
        match kind with
        | ClrElementKind.Property | ClrElementKind.Field -> true
        | _ -> false
                

[<AutoOpen>]
module ClrElementClassification =

    module ClrElement =
        /// <summary>
        /// Classifies the element
        /// </summary>
        /// <param name="element">The element to classify</param>
        let getKind element =
            match element with
            | MemberElement(element=x) -> 
                match x with
                | DataMemberElement(x) ->
                    match x with
                    | PropertyMember(_) ->
                       ClrElementKind.Property
                    | FieldMember(_) -> 
                        ClrElementKind.Field
                | MethodElement(_) ->
                    ClrElementKind.Method
            | TypeElement(element=x) -> 
                ClrElementKind.Type
            | AssemblyElement(_) ->
                ClrElementKind.Assembly
            | ParameterElement(_) ->
                ClrElementKind.Parameter
            | UnionCaseElement(_) ->
                ClrElementKind.UnionCase


        /// <summary>
        /// Determines whether the element is a member 
        /// </summary>
        /// <param name="element"></param>
        let isMember element = element |> getKind |> ClrElementKind.isMemberKind    

        /// <summary>
        /// Inteprets the CLR element as a type element if possible; otherwise, an error is raised
        /// </summary>
        /// <param name="element">The element to interpret</param>
        let asMemberElement element =            
            match element with
            | MemberElement(element=x) -> x
            | _ ->
                argerrord "element" element "Element is not a member"

        /// <summary>
        /// Determines whether the element is a method
        /// </summary>
        /// <param name="element"></param>
        let isMethod element = (element |> getKind) = ClrElementKind.Method

        /// <summary>
        /// Upcasts the element as a method element; otherwise, an error is raised
        /// </summary>
        /// <param name="element">The element to interpret</param>
        let asMethodElement element = 
            let error() = argerrord "element" element "Element is not a method"
            match element with
            | MemberElement(element=x) -> 
                match x with
                | MethodElement(x) -> x
                | _ -> error()
            | _ ->
                error()
            
    
        /// <summary>
        /// Determines whether the element is a type
        /// </summary>
        /// <param name="element">The element to test</param>
        let isType element = (element |> getKind) = ClrElementKind.Type

        /// <summary>
        /// Inteprets the CLR element as a type element if possible; otherwise, an error is raised
        /// </summary>
        /// <param name="element">The element to interpret</param>
        let asTypeElement element =            
            match element with
            | TypeElement(element=x) -> x
            | _ ->
                argerrord "element" element "Element is not a type"

        /// <summary>
        /// Determines whether the element is a data member
        /// </summary>
        /// <param name="element">The element to test</param>
        let isDataMember (element : ClrElement) = element |> getKind |> ClrElementKind.isDataMemberKind

        /// <summary>
        /// Inteprets the CLR element as a data member if possible; otherwise, an error is raised
        /// </summary>
        /// <param name="element">The element to interpret</param>
        let asDataMember element =
            let error() = argerrord "element" element "Element is not a data member"
            match element with
            | MemberElement(element=x) -> 
                match x with
                | DataMemberElement(x) -> x
                | _ -> error()
            | _ -> error()

        /// <summary>
        /// Inteprets the CLR element as a parameter if possible; otherwise, an error is raised
        /// </summary>
        /// <param name="element">The element to interpret</param>
        let asParameterElement element =
            match element with
            | ParameterElement(x) -> x
            | _ -> argerrord "element" element "Element is not a parameter"
                    
    type ClrElement
    with
        member this.Kind = this |> ClrElement.getKind

        

[<AutoOpen>]
module internal ClrElementVocabularyExtensions =
    type ClrElement
    with
    member this.IReflectionPrimitive  =
        match this with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) -> match x with  ClrPropertyElement(x) -> x :> IReflectionPrimitive
                | FieldMember(x) ->  match x with ClrFieldElement(x) -> x :> IReflectionPrimitive
            | MethodElement(x) -> match x with ClrMethodElement(x) -> x :> IReflectionPrimitive
        | TypeElement(element=x) -> match x with ClrTypeElement(x) -> x :> IReflectionPrimitive
        | AssemblyElement(element=x) -> match x with ClrAssemblyElement(x) -> x:>IReflectionPrimitive                    
        | ParameterElement(x) -> match x with ClrParameterElement(x) -> x:>IReflectionPrimitive
        | UnionCaseElement(element=x) -> match x with ClrUnionCaseElement(x) ->x:>IReflectionPrimitive
