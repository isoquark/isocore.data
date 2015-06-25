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
    /// Represents a type name
    /// </summary>
    type ClrTypeName = ClrTypeName of simpleName : string * fullName : string option * assemblyQualifiedName : string option

    /// <summary>
    /// Represents an assembly name
    /// </summary>
    type ClrAssemblyName = ClrAssemblyName of simpleName : string * fullName : string option

    /// <summary>
    /// Represents the name of a CLR element
    /// </summary>
    type ClrElementName =
        ///Specifies the name of an assembly 
        | AssemblyElementName of ClrAssemblyName
        ///Specifies the name of a type 
        | TypeElementName of ClrTypeName
        ///Specifies the name of a type member
        | MemberElementName of string
        ///Specifies the name of a parameter
        | ParameterElementName of string
    
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
    type ClParameterElement = ClrParameterElement of ClrReflectionPrimitive<ParameterInfo>
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
        | ParameterElement of element : ClParameterElement
        | UnionCaseElement of element : ClrUnionCaseElement* children : ClrElement list
    with
        override this.ToString() = 
            match this with 
                | MemberElement (element=x) -> x.ToString()
                | TypeElement (element=x) ->  x.ToString()
                | AssemblyElement (element=x) ->  x.ToString()
                | ParameterElement (element=x) ->  x.ToString()
                | UnionCaseElement (element=x) ->  x.ToString()


    type ClrAttribution = ClrAttribution of element : ClrElement * attributes : Attribute list

    type ClrAttribution<'T when 'T :> Attribute> = ClrAttribution of ClrElement * attributes : 'T list

[<AutoOpen>]
module internal ClrElementExtensionsA =
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

/// <summary>
/// Defines operations related to the <see cref="ClrTypeName"/> type
/// </summary>
module internal ClrTypeName =
    /// <summary>
    /// Gets the element name
    /// </summary>
    /// <param name="subject"></param>
    let fromType (subject : Type) =

        ClrTypeName(
              subject.Name
            , subject.FullName |> Some
            , subject.AssemblyQualifiedName |> Some)

    /// <summary>
    /// Gets the type name from the <see cref="ClrTypeElement"/>
    /// </summary>
    /// <param name="subject">The type element</param>
    let fromTypeElement (subject : ClrTypeElement) =
        match subject with ClrTypeElement(x) -> x.Primitive |> fromType



module ClrElementProvider =
    

    let private cache = Dictionary<obj, ClrElement>()        


    let private cacheElement (e : ClrElement) =
        cache.Add(e.IReflectionPrimitive.Primitive, e)
        e

    let rec private createElement pos (o : obj) =
        if o |> cache.ContainsKey then
            cache.[o]
        else
            match o with
            | :? Assembly as x->             
                let children = x.GetTypes() |> Array.mapi(fun i t -> t |> createElement i ) |> List.ofArray
                let e = ClrReflectionPrimitive(x, None) |> ClrAssemblyElement
                (e, children) |> AssemblyElement |> cacheElement            

            | :? Type as x-> 
                let children = x |> Type.getPureMembers |> List.mapi(fun i m -> m |> createElement i) 
                let e = ClrReflectionPrimitive(x, pos |> Some) |> ClrTypeElement
                TypeElement(e, children) |> cacheElement
            | :? MethodInfo as x ->
                let children = x.GetParameters() |> Array.mapi(fun i p -> p |> createElement i) |> List.ofArray
                let e = (x, pos |> Some) |> ClrReflectionPrimitive |> ClrMethodElement |> MethodElement
                (e, children) |> MemberElement |> cacheElement
            | :? PropertyInfo as x-> 
                let e = (x, pos |> Some) |> ClrReflectionPrimitive |> ClrPropertyElement |> PropertyMember |> DataMemberElement
                (e, []) |> MemberElement |> cacheElement
            | :? FieldInfo  as x-> 
                let e = (x, pos |> Some) |> ClrReflectionPrimitive |> ClrFieldElement |> FieldMember |> DataMemberElement
                (e, []) |> MemberElement |> cacheElement
            | :? ParameterInfo as x -> 
                ClrReflectionPrimitive(x, pos |> Some) |> ClrParameterElement |> ParameterElement |> cacheElement
            | :? UnionCaseInfo as x ->
                let e = ClrReflectionPrimitive(x, pos |> Some) |> ClrUnionCaseElement
                (e, []) |> UnionCaseElement |> cacheElement
            | :? ConstructorInfo as x -> nosupport()
            | :? EventInfo as x -> nosupport()
            | _ -> nosupport()

    let private addAssembly (a : Assembly) =
        a |> createElement 0 

    let private getElementAssembly (o : obj) =
        match o with
        | :? Assembly as x-> x
        | :? Type as x-> x.Assembly
        | :? MethodInfo as x -> x.DeclaringType.Assembly
        | :? PropertyInfo as x-> x.DeclaringType.Assembly 
        | :? FieldInfo  as x-> x.DeclaringType.Assembly
        | :? ParameterInfo as x -> x.Member.DeclaringType.Assembly
        | :? ConstructorInfo as x -> x.DeclaringType.Assembly            
        | :? EventInfo as x -> x.DeclaringType.Assembly
        | :? UnionCaseInfo as x -> x.DeclaringType.Assembly
        | _ -> nosupport()

    let private addElement (o : obj) =
        o |> getElementAssembly |> addAssembly |> ignore
    
    let getElement (o : obj) =        
        //This is not thread-safe and is temporary
        match  o |> cache.TryGetValue with
        |(false,_) -> 
            o |> addElement |> ignore
            //Note that at this point, element may still not be in cache (for example, it could be a closed generic type
            //that is created in the body of a function)
            //In any case, if it's not there now we add it; in the future, this will probably be removed because this
            //model isn't intended to capture such things
            if o |> cache.ContainsKey |> not then
                o |> createElement 0
            else
                cache.[o]
        |(_,element) -> element
            
        
    
        
        
module internal ClrTypeElement =

    let private getType element =
        match element with ClrTypeElement(x) -> x.Primitive

    let create pos primitive=
        ClrTypeElement(ClrReflectionPrimitive(primitive,pos))        

    
      
/// <summary>
/// Defines operations related to the <see cref="ClrElementName"/> type
/// </summary>
module internal ClrElementName =

    /// <summary>
    /// Gets the name of the element
    /// </summary>
    /// <param name="element"></param>
    let fromElement (element : ClrElement) =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    match x with 
                        ClrPropertyElement(x) -> x.Primitive.Name |> MemberElementName
                | FieldMember(x) -> 
                    match x with
                        ClrFieldElement(x) -> x.Primitive.Name |> MemberElementName
            | MethodElement(x) ->
                  match x with
                    ClrMethodElement(x) -> x.Primitive.Name |> MemberElementName
        | TypeElement(element=x) -> 
            x |> ClrTypeName.fromTypeElement |> TypeElementName
        | AssemblyElement(element=x) ->
            match x with 
                ClrAssemblyElement(x) -> 
                    ClrAssemblyName(x.Primitive.SimpleName, x.Primitive.FullName |> Some) |> AssemblyElementName
        | ParameterElement(x) ->
            match x with
                ClrParameterElement(x) ->
                    x.Primitive.Name |> ParameterElementName
        | UnionCaseElement(element=x) ->
            match x with
                ClrUnionCaseElement(x) ->
                    x.Primitive.Name |> MemberElementName
            
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
    /// Defines augmentations for the <see cref="ClrAssemblyName"/> type
    /// </summary>
    type ClrAssemblyName 
    with
        member this.SimpleName = match this with ClrAssemblyName(simpleName=x) -> x
        member this.FullName = match this with ClrAssemblyName(fullName=x) -> x
        member this.Text =
            match this with ClrAssemblyName(simpleName, fullName) -> match fullName with
                                                                        | Some(x) -> x
                                                                        | None ->
                                                                            simpleName    
    /// <summary>
    /// Represents the name of a CLR element
    /// </summary>
    type ClrElementName
    with
        member this.Text =
            match this with
            | AssemblyElementName x -> x.Text
            | TypeElementName x -> x.Text
            | MemberElementName x -> x
            | ParameterElementName x -> x


        
            
            
                

        
/// <summary>
/// Defines CLR hierarchy upcasts and related augmentations 
/// </summary>
[<AutoOpen>]
module ClrHierarchyExtensions =
    /// <summary>
    /// Defines augmentations for the <see cref="ClrElement"/> type
    /// </summary>
    type ClrElement
    with
        member this.Name = this |> ClrElementName.fromElement

    /// <summary>
    /// Defines augmentations for the <see cref="System.Type"/> type
    /// </summary>
    type Type 
    with
        /// <summary>
        /// Interprets the type as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement

        /// <summary>
        /// Interprets the method as a <see cref="ClrTypeElement"/>
        /// </summary>
        //member this.TypeElement = this |> ClrTypeElement.create None 
        member this.TypeElement = match this.Element with | TypeElement(element=x) -> x | _ -> nosupport()        
        
        /// <summary>
        /// Gets the <see cref="ClrTypeName"/> of the type
        /// </summary>
        member this.ElementTypeName = this |> ClrTypeName.fromType
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the type
        /// </summary>
        member this.ElementName = this.Element.Name
    
    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.Assembly"/> type
    /// </summary>
    type Assembly
    with
        /// <summary>
        /// Interprets the assembly as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement
        /// <summary>
        /// Interprets the assembly as a <see cref="ClrAssemblyElement"/>
        /// </summary>
        member this.AssemblyElement =  match this.Element with | AssemblyElement(element=x) -> x | _ ->nosupport()
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the assembly
        /// </summary>
        member this.ElementName = this.Element.Name

    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.MethodInfo"/> type
    /// </summary>
    type MethodInfo
    with
        /// <summary>
        /// Interprets the method as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement
        /// <summary>
        /// Interprets the method as a <see cref="ClrMethodElement"/>
        /// </summary>
        member this.MethodElement = 
            match this.Element with 
            | MemberElement(element=x) ->  match x with MethodElement(x) ->x | _ -> nosupport()
            | _ -> nosupport()        
        /// <summary>
        /// Interprets the method as a <see cref="ClrMemberElement"/>
        /// </summary>
        member this.MemberElement = 
            match this.Element with 
            | MemberElement(element=x) ->  x
            | _ -> nosupport()
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the method
        /// </summary>
        member this.ElementName = this.Element.Name


    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.ParameterInfo"/> type
    /// </summary>
    type ParameterInfo
    with        
        /// <summary>
        /// Interprets the parameter as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement
        /// <summary>
        /// Interprets the parameter as a <see cref="ClrParameterElement"/>
        /// </summary>
        member this.ParameterElement = match this.Element with | ParameterElement(x) -> x |_ -> nosupport()
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the parameter
        /// </summary>
        member this.ElementName = this.Element.Name


    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.PropertyInfo"/> type
    /// </summary>
    type PropertyInfo
    with        
        /// <summary>
        /// Interprets the property as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement
        /// <summary>
        /// Interprets the property as a <see cref="ClrPropertyElement"/>
        /// </summary>
        member this.PropertyElement = 
            match this.Element with
            |MemberElement(element=x) ->
                match x with
                | DataMemberElement(x) ->
                    match x with PropertyMember(x) -> x | _ -> nosupport()
                | _ -> nosupport()
            |_ -> nosupport()

        /// <summary>
        /// Interprets the property as a <see cref="ClrDataMemberElement"/>
        /// </summary>
        member this.DataMemberElement = 
            match this.Element with
            |MemberElement(element=x) ->
                match x with
                | DataMemberElement(x) -> x                    
                | _ -> nosupport()
            |_ -> nosupport()
        
        /// <summary>
        /// Interprets the property as a <see cref="ClrMemberElement"/>
        /// </summary>
        member this.MemberElement = 
            match this.Element with
            |MemberElement(element=x) -> x
            |_ -> nosupport()
        
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the property
        /// </summary>
        member this.ElementName = this.Element.Name


    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.FieldInfo"/> type
    /// </summary>
    type FieldInfo
    with
        /// <summary>
        /// Interprets the field as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement
        /// <summary>
        /// Interprets the field as a <see cref="ClrFieldElement"/>
        /// </summary>
        member this.FieldElement = 
            match this.Element with
            |MemberElement(element=x) ->
                match x with
                | DataMemberElement(x) ->
                    match x with FieldMember(x) -> x | _ -> nosupport()
                | _ -> nosupport()
            |_ -> nosupport()

        /// <summary>
        /// Interprets the field as a <see cref="ClrDataMemberElement"/>
        /// </summary>
        member this.DataMemberElement = 
            match this.Element with
            |MemberElement(element=x) ->
                match x with
                | DataMemberElement(x) -> x                    
                | _ -> nosupport()
            |_ -> nosupport()
        /// <summary>
        /// Interprets the field as a <see cref="ClrMemberElement"/>
        /// </summary>        
        member this.MemberElement = 
            match this.Element with
            |MemberElement(element=x) -> x
            |_ -> nosupport()
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the field
        /// </summary>
        member this.ElementName = this.Element.Name

    /// <summary>
    /// Defines augmentations for the <see cref="Microsoft.FSharp.Reflection.UnionCaseInfo"/> type
    /// </summary>
    type UnionCaseInfo
    with
        /// <summary>
        /// Interprets the case as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ClrElementProvider.getElement
        /// <summary>
        /// Interprets the case as a <see cref="ClrUnionCaseElement"/>
        /// </summary>
        member this.UnionCaseElement = match this.Element with |UnionCaseElement(element=x) -> x | _ -> nosupport()
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the case
        /// </summary>
        member this.ElementName = this.Element.Name


    /// <summary>
    /// Defines augmentations for the <see cref="ClrPopertyElement"/> type
    /// </summary>
    type ClrPropertyElement
    with
        /// <summary>
        /// Gets the encapluated Property
        /// </summary>
        member this.PropertyInfo = match this with ClrPropertyElement(x) -> x.Primitive
        /// <summary>
        /// Interprets the property as a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.PropertyInfo.Element
        /// <summary>
        /// Upcasts the element to a <see cref="ClrDataMemberElement"/>
        /// </summary>
        member this.PropertyMemberElement = this.PropertyInfo.DataMemberElement
        /// <summary>
        /// Interprets the property as a <see cref="ClrMemberElement"/>
        /// </summary>
        member this.MemberElement = this.PropertyInfo.MemberElement

    /// <summary>
    /// Defines augmentations for the <see cref="ClrMemberElement"/> type
    /// </summary>
    type ClrMemberElement
    with
        member this.MemberInfo = 
            match this with 
                | DataMemberElement(x) ->
                    match x with
                    | PropertyMember(x) -> 
                        match x with ClrPropertyElement(x) -> x.Primitive :> MemberInfo
                    | FieldMember(x) -> 
                        match x with ClrFieldElement(x) -> x.Primitive :> MemberInfo
                | MethodElement(x) ->
                   match x with ClrMethodElement(x) -> x.Primitive :> MemberInfo
                              
        /// <summary>
        /// Upcasts the element to a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.MemberInfo |> ClrElementProvider.getElement
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the member 
        /// </summary>
        member this.ElementName = this.Element.Name


    /// <summary>
    /// Defines augmentations for the <see cref="ClrTypeElement"/> type
    /// </summary>
    type ClrTypeElement
    with
        /// <summary>
        /// Gets the encapluated Type
        /// </summary>
        member this.Type = match this with ClrTypeElement(x) -> x.Primitive
        /// <summary>
        /// Upcasts the element to a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.Type |> ClrElementProvider.getElement
        /// <summary>
        /// Gets the <see cref="ClrElementName"/> of the member 
        /// </summary>
        member this.ElementName = this.Element.Name
        /// <summary>
        /// Gets the <see cref="ClrTypeNameName"/> of the member 
        /// </summary>
        member this.ElementTypeName = this.Type.ElementTypeName

    /// <summary>
    /// Defines augmentations for the <see cref="ClrAssemblyElement"/> type
    /// </summary>
    type ClrAssemblyElement
    with
        /// <summary>
        /// Gets the encapluated Assembly
        /// </summary>
        member this.Assembly = match this with ClrAssemblyElement(x) -> x.Primitive
        /// <summary>
        /// Upcasts the element to a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this.Assembly |> ClrElementProvider.getElement

    /// <summary>
    /// Defines augmentations for the <see cref="ClrParameterElement"/> type
    /// </summary>
    type ClParameterElement
    with
        /// <summary>
        /// Upcasts the element to a <see cref="ClrElement"/>
        /// </summary>
        member this.Element = this |> ParameterElement
        /// <summary>
        /// Gets the encapluated Parameter
        /// </summary>
        member this.ParamerInfo = match this with ClrParameterElement(x) -> x.Primitive

    /// <summary>
    /// Defines augmentations for the <see cref="ClrFieldElement"/> type
    /// </summary>
    type ClrFieldElement
    with
        /// <summary>
        /// Gets the encapluated Field
        /// </summary>
        member this.FieldInfo = match this with ClrFieldElement(x) -> x.Primitive

    /// <summary>
    /// Defines augmentations for the <see cref="ClrMethodElement"/> type
    /// </summary>
    type ClrMethodElement
    with
        /// <summary>
        /// Gets the encapsulated method
        /// </summary>
        member this.MethodInfo = match this with ClrMethodElement(x) -> x.Primitive

    /// <summary>
    /// Defines augmentations for the <see cref="ClrUnionCaseElement"/> type
    /// </summary>
    type ClrUnionCaseElement 
    with
        /// <summary>
        /// Gets the encapsulated case
        /// </summary>
        member this.UnionCaseInfo = match this with ClrUnionCaseElement(x) -> x.Primitive


/// <summary>
/// Implements operations for the <see cref="ClrMethodElement"/> type
/// </summary>
module ClrMethodElement =
    let getParameters (element : ClrMethodElement) =
        element.MethodInfo.GetParameters() |> Array.mapi (fun i x -> ClrParameterElement(ClrReflectionPrimitive(x, i |> Some))) |> List.ofArray


module ClrElement =         
   
    /// <summary>
    /// Retrieves is element's declaring type, if applicable
    /// </summary>
    /// <param name="element"></param>
    let getDeclaringType (element : ClrElement) =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    x.PropertyInfo.DeclaringType |> Some
                | FieldMember(x) -> 
                    x.FieldInfo.DeclaringType |> Some
            | MethodElement(x) ->
                x.MethodInfo.DeclaringType |> Some
        | TypeElement(element=x) -> 
            if x.Type.DeclaringType <> null then
                x.Type.DeclaringType |> Some
            else
                None
        | AssemblyElement(element=x) ->
            None
        | ParameterElement(element=x) ->
            None
        | UnionCaseElement(element=x) ->
            x.UnionCaseInfo.DeclaringType |> Some

    /// <summary>
    /// Gets the assembly in which the element is defined
    /// </summary>
    /// <param name="element">The element</param>
    let getDeclaringAssembly(element : ClrElement) =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    x.PropertyInfo.DeclaringType.Assembly
                | FieldMember(x) -> 
                    x.FieldInfo.DeclaringType.Assembly
            | MethodElement(x) ->
                x.MethodInfo.DeclaringType.Assembly
        | TypeElement(element=x) -> 
                x.Type.Assembly
        | AssemblyElement(element=x) ->
            x.Assembly
        | ParameterElement(x) ->
            x.ParamerInfo.Member.DeclaringType.Assembly
        | UnionCaseElement(element=x) ->
            x.UnionCaseInfo.DeclaringType.Assembly
        

    
    /// <summary>
    /// Gets the name of the element
    /// </summary>
    /// <param name="element"></param>
    let getName (element : ClrElement) = element |> ClrElementName.fromElement

    /// <summary>
    /// Determines whether the element is a member 
    /// </summary>
    /// <param name="element">The element to test</param>
    let isMember (element : ClrElement) =
        match element with
        | MemberElement(element=x) -> true
        |_ -> false

    /// <summary>
    /// Inteprets the CLR element as a type element if possible; otherwise, an error is raised
    /// </summary>
    /// <param name="element">The element to interpret</param>
    let asMemberElement (element : ClrElement)   =            
        match element with
        | MemberElement(element=x) -> x
        | _ ->
            ArgumentException(sprintf"Element %O is not a member"  (element |> getName)) |> raise            
    
    
    /// <summary>
    /// Determines whether the element is a type
    /// </summary>
    /// <param name="element">The element to test</param>
    let isType (element : ClrElement) =
        match element with
        | TypeElement(_) -> true
        | _ -> false

    /// <summary>
    /// Inteprets the CLR element as a type element if possible; otherwise, an error is raised
    /// </summary>
    /// <param name="element">The element to interpret</param>
    let asTypeElement (element : ClrElement)   =            
        match element with
        | TypeElement(element=x) -> x
        | _ ->
            ArgumentException(sprintf"Element %O is not a type"  (element |> getName)) |> raise            

    /// <summary>
    /// Determines whether the element is a data member
    /// </summary>
    /// <param name="element">The element to test</param>
    let isDataMember (element : ClrElement) =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(_) -> true
            | MethodElement(_) -> false
        | _ -> false
        
    /// <summary>
    /// Inteprets the CLR element as a data member if possible; otherwise, an error is raised
    /// </summary>
    /// <param name="element">The element to interpret</param>
    let asDataMember (element : ClrElement) =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) -> x
            | _ -> ArgumentException(sprintf"Element %O is not a data member"  (element |> getName)) |> raise            
        | _ -> ArgumentException(sprintf"Element %O is not a data member"  (element |> getName)) |> raise            

    
    /// <summary>
    /// Retrieves the (direct) children of the element
    /// </summary>
    /// <param name="element">The element to examine</param>
    let getChildren(element : ClrElement) =
        match element with
        | MemberElement(children=x) -> 
            x
        | TypeElement(children=x) -> 
            x
        | AssemblyElement(children=x) -> 
            x
        | ParameterElement(_) ->
            []
        | UnionCaseElement(children=x) ->
            x

    /// <summary>
    /// Gets the acess modifier applied to the element, if applicable
    /// </summary>
    /// <param name="element">The element to examine</param>
    let tryGetAccess (element : ClrElement)  =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    None
                | FieldMember(x) -> 
                    match x with 
                    | ClrFieldElement(x) ->
                        match x with 
                            ClrReflectionPrimitive(primitive=x) ->
                                if x.IsPublic then
                                    PublicAccess |> Some
                                else if x.IsPrivate then
                                    PrivateAccess |> Some 
                                else if x.IsAssembly then
                                    InternalAccess |> Some
                                else if x.IsFamilyOrAssembly then
                                    ProtectedInternalAccess |> Some
                                else
                                    nosupport()
            | MethodElement(x) ->
                match x with 
                | ClrMethodElement(x) ->
                        match x with 
                            ClrReflectionPrimitive(primitive=x) ->
                                if x.IsPublic then
                                    PublicAccess |> Some
                                else if x.IsPrivate then
                                    PrivateAccess |> Some 
                                else if x.IsAssembly then
                                    InternalAccess |> Some
                                else if x.IsFamilyOrAssembly then
                                    ProtectedInternalAccess |> Some
                                else
                                    nosupport()
        | TypeElement(element=x) -> 
            if x.Type.IsPublic  || x.Type.IsNestedPublic then
                PublicAccess |> Some
            else if x.Type.IsNestedPrivate then
                PrivateAccess |> Some
            else if x.Type.IsNotPublic || x.Type.IsNestedAssembly then
                InternalAccess |> Some
            else if x.Type.IsNestedFamORAssem then
                ProtectedInternalAccess |> Some
            else
                nosupport()
        | AssemblyElement(element=x) ->
            None
        | ParameterElement(x) ->
            None
        | UnionCaseElement(element=x) ->
             PublicAccess |> Some
        
    /// <summary>
    /// Gets the acess modifier applied to the element, if applicable; otherwise,
    /// raises an error
    /// </summary>
    /// <param name="element">The element to examine</param>
    let getAccess (element : ClrElement) = 
        element |> tryGetAccess |> Option.get

        

    /// <summary>
    /// Retrieves all attributes applied to the element
    /// </summary>
    /// <param name="element">The element to examine</param>
    let getAllAttributes(element : ClrElement) =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    x.PropertyInfo |> Attribute.GetCustomAttributes
                | FieldMember(x) -> 
                    x.FieldInfo |> Attribute.GetCustomAttributes
            | MethodElement(x) ->
                    x.MethodInfo |> Attribute.GetCustomAttributes
        | TypeElement(element=x) -> 
            x.Type |> Attribute.GetCustomAttributes
        | AssemblyElement(element=x) ->
            x.Assembly |> Attribute.GetCustomAttributes
        | ParameterElement(x) ->
            x.ParamerInfo |> Attribute.GetCustomAttributes
        | UnionCaseElement(element=x) ->
            [|for a in x.UnionCaseInfo.GetCustomAttributes() -> a :?> Attribute|]
        |> List.ofArray

    /// <summary>
    /// Determines whether an attribute of a specified type has been applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let hasAttribute (element : ClrElement) (attribType : Type) =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    Attribute.IsDefined(x.PropertyInfo, attribType) 
                | FieldMember(x) -> 
                    Attribute.IsDefined(x.FieldInfo, attribType) 
            | MethodElement(x) ->
                    Attribute.IsDefined(x.MethodInfo, attribType) 
        | TypeElement(element=x) -> 
            Attribute.IsDefined(x.Type, attribType) 
        | AssemblyElement(element=x) ->
            Attribute.IsDefined(x.Assembly, attribType) 
        | ParameterElement(x) ->
            Attribute.IsDefined(x.ParamerInfo, attribType) 
        | UnionCaseElement(element=x) ->
            x.UnionCaseInfo.GetCustomAttributes() |> Array.filter(fun a -> a.GetType() = attribType) |> Array.isEmpty |> not

    /// <summary>
    /// Retrieves an attribute from the element if it exists and returns None if it odes not
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let tryGetAttribute (element : ClrElement) (attribType : Type) =
        if attribType |> hasAttribute element then
            match element with
            | MemberElement(element=x) -> 
                match x with
                | DataMemberElement(x) ->
                    match x with
                    | PropertyMember(x) ->
                        Attribute.GetCustomAttribute(x.PropertyInfo, attribType)
                    | FieldMember(x) -> 
                        Attribute.GetCustomAttribute(x.FieldInfo, attribType)
                | MethodElement(x) ->
                        Attribute.GetCustomAttribute(x.MethodInfo, attribType)
            | TypeElement(element=x) -> 
                Attribute.GetCustomAttribute(x.Type, attribType)
            | AssemblyElement(element=x) ->
                Attribute.GetCustomAttribute(x.Assembly, attribType)
            | ParameterElement(x) ->
                Attribute.GetCustomAttribute(x.ParamerInfo, attribType)
            | UnionCaseElement(element=x) ->
                x.UnionCaseInfo.GetCustomAttributes() |> Array.find(fun a -> a.GetType() = attribType) :?> Attribute
            |> Some
        else
            None    

    /// <summary>
    /// Retrieves an attribute from the element if it exists and raises an exception otherwise
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getAttribute (element : ClrElement) (attribType : Type) =
        attribType |> tryGetAttribute element |> Option.get

    /// <summary>
    /// Retrieves all attributes of a specified type that have been applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getAttributes (element : ClrElement) (attribType : Type)  =
        match element with
        | MemberElement(element=x) -> 
            match x with
            | DataMemberElement(x) ->
                match x with
                | PropertyMember(x) ->
                    Attribute.GetCustomAttributes(x.PropertyInfo, attribType)
                | FieldMember(x) -> 
                    Attribute.GetCustomAttributes(x.FieldInfo, attribType)
            | MethodElement(x) ->
                    Attribute.GetCustomAttributes(x.MethodInfo, attribType)
        | TypeElement(element=x) -> 
            Attribute.GetCustomAttributes(x.Type, attribType)
        | AssemblyElement(element=x) ->
            Attribute.GetCustomAttributes(x.Assembly, attribType)
        | ParameterElement(x) ->
            Attribute.GetCustomAttributes(x.ParamerInfo, attribType)
        | UnionCaseElement(element=x) ->
            [|for a in x.UnionCaseInfo.GetCustomAttributes() do if a.GetType() = attribType then yield a :?> Attribute|]
        |> List.ofArray

    /// <summary>
    /// Determines whether an attribute is applied to an element
    /// </summary>
    /// <param name="element">The element to examine</param>
    let hasAttributeT<'T when 'T :> Attribute>(element : ClrElement) =
        typeof<'T> |> hasAttribute  element        

    /// <summary>
    /// Retrieves an attribute applied to a member, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let tryGetAttributeT<'T when 'T :> Attribute>(element : ClrElement) =
        match typeof<'T> |> tryGetAttribute element with
        | Some(x) -> x :?> 'T |> Some
        | None -> None
    
    /// <summary>
    /// Retrieves an attribute from the element if it exists and raises an exception otherwise
    /// </summary>
    /// <param name="element">The element to examine</param>
    /// <param name="attribType">The type of attribute to match</param>
    let getAttributeT<'T when 'T :> Attribute>(element : ClrElement) =
        element |> tryGetAttributeT<'T> |> Option.get

    /// <summary>
    /// Retrieves an arbitrary number of attributes of the same type applied to a member
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttributesT<'T when 'T :> Attribute>(subject : MemberInfo) =
        [for a in Attribute.GetCustomAttributes(subject, typeof<'T>) -> a :?> 'T]


//    let rec walk (handler:ClrElement->unit) element =
//        match element with
//        | MemberElement(x) -> 
//            match x with
//            | DataMemberElement(x) -> 
//                element |> handler
//            | MethodElement(x) ->
//                element |> handler
//        | TypeElement(x) -> 
//            ()
//        | AssemblyElement(x) ->
//            ()
//        | ParameterElement(x) ->
//            ()
//        | UnionCaseElement(x) ->
//            ()


    

module ClrDataMemberElement =
    let getValue (instance : obj) (element : ClrDataMemberElement) =
        match element with
        | PropertyMember(x) ->
            instance |> x.PropertyInfo.GetValue
        | FieldMember(x) -> 
            instance |> x.FieldInfo.GetValue

    let getType element =
        match element with
        | PropertyMember(x) ->
            x.PropertyInfo.PropertyType
        | FieldMember(x) -> 
            x.FieldInfo.FieldType

        

[<AutoOpen>]
module ClrElementExtensions = 

    /// <summary>
    /// Defines augmentations for the <see cref="ClrElement"/> type
    /// </summary>
    type ClrElement
    with
        member this.DeclaringType = this |> ClrElement.getDeclaringType
        member this.DeclaringAssembly = this |> ClrElement.getDeclaringAssembly

    /// <summary>
    /// Defines augmentations for the <see cref="ClrElementName"/> type
    /// </summary>
    type ClrElementName
    with
        /// <summary>
        /// Renders the name as text
        /// </summary>
        member this.Text = 
            match this with
            | AssemblyElementName(n) -> n.Text
            | TypeElementName(n) -> n.Text
            | MemberElementName(n) -> n
            | ParameterElementName(n) -> n


    /// <summary>
    /// Defines augmentations for the <see cref="System.Type"/> type
    /// </summary>
    type Type 
    with
        /// <summary>
        /// Gets the applied access modifier
        /// </summary>
        member this.AccessModifier = this.Element |> ClrElement.getAccess
    

    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.MethodInfo"/> type
    /// </summary>
    type MethodInfo
    with
        /// <summary>
        /// Gets the applied access modifier
        /// </summary>
        member this.AccessModifier = this.Element |> ClrElement.getAccess



    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.PropertyInfo"/> type
    /// </summary>
    type PropertyInfo
    with        
        /// <summary>
        /// Gets the applied access modifier
        /// </summary>
        member this.AccessModifier = this.Element |> ClrElement.getAccess


    /// <summary>
    /// Defines augmentations for the <see cref="System.Reflection.FieldInfo"/> type
    /// </summary>
    type FieldInfo
    with
        /// <summary>
        /// Gets the applied access modifier
        /// </summary>
        member this.AccessModifier = this.Element |> ClrElement.getAccess


    /// <summary>
    /// Defines augmentations for the <see cref="ClrMemberElement"/> type
    /// </summary>
    type ClrMemberElement
    with
        /// <summary>
        /// Gets the member's declaring type
        /// </summary>
        member this.DeclaringType = this.Element |> ClrElement.getDeclaringType |> Option.get
     

module ClrAssembly =
    /// <summary>
    /// Gets the type elements defined in the assembly
    /// </summary>
    /// <param name="subject"></param>
    let getTypeElements (subject : ClrAssemblyElement) =
        subject.Assembly |> Assembly.getTypes |> List.mapi(fun i x -> ClrTypeElement(ClrReflectionPrimitive(x,i |> Some)))

    /// <summary>
    /// Retrieves a text resource embedded in the subject assembly if found
    /// </summary>
    /// <param name="shortName">The name of the resource, excluding the namespace path</param>
    /// <param name="subject">The assembly that contains the resource</param>
    let findTextResource shortName (subject : ClrAssemblyElement) =
        subject.Assembly |> Assembly.findTextResource shortName        

    /// <summary>
    /// Writes a text resource contained in an assembly to a file and returns the path
    /// </summary>
    /// <param name="shortName">The name of the resource, excluding the namespace path</param>
    /// <param name="outputDir">The directory into which the resource will be deposited</param>
    /// <param name="subject">The assembly that contains the resource</param>
    let writeTextResource shortName outputDir (subject : ClrAssemblyElement) =
        subject.Assembly |> Assembly.writeTextResource shortName outputDir

    
    

        
//module ClrType =
//    let getMembers (subject : ClrTypeElement) =
//        [
//            yield! subject.Type |> Type.getPureMethods |> List.map(fun x -> x.MemberElement)
//            yield! subject.Type |> Type.getProperties |> List.map(fun x -> x.MemberElement)
//            yield! subject.Type |> Type.getPureFields |> List.map(fun x -> x.MemberElement)
//        ]

