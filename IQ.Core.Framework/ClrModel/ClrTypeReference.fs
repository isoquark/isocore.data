namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent
open System.Collections.Generic

open Microsoft.FSharp.Reflection



module ClrTypeReference =
    
    open Type

    /// <summary>
    /// Defines internal reflection cache for efficiency
    /// </summary>
    module private ClrTypeReferenceIndex =
        let private types =  ConcurrentDictionary<Type, ClrTypeReference>()
        /// <summary>
        /// Retrieves an existing reference if present or constructs a new one and adds it to the index
        /// </summary>
        /// <param name="t">The type to reference</param>
        /// <param name="f">The reference factory</param>
        let getOrAdd(t : Type) (f:Type->ClrTypeReference) =
            types.GetOrAdd(t,f)


    let internal getSubject(tref : ClrTypeReference) =
        match tref with 
        |UnionTypeReference(subject=x)
        |RecordTypeReference(subject=x)
        |InterfaceTypeReference(subject=x)
        |ClassTypeReference(subject=x)
        |CollectionTypeReference(subject=x)
        |StructTypeReference(subject=x) -> x        

    let getReferent (tref : ClrTypeReference) =
        let subject = tref |> getSubject
        subject.Element        

    let getTypeReferent (tref : ClrTypeReference) =
        let subject = tref |> getSubject
        match subject.Element with
            | TypeElement(x) -> x
            | _ -> nosupport()   
     
    let getReferentName (tref : ClrTypeReference) =
        let subject = tref |> getSubject
        subject.Element.Name

    let getReferentType (tref : ClrTypeReference) =
        let element = tref |> getTypeReferent
        element.Type

    let getReferentTypeName (tref : ClrTypeReference) =
        let element = tref |> getTypeReferent
        element.Type.ElementTypeName

        
    /// <summary>
    /// Creates a reference to a method parameter
    /// </summary>
    /// <param name="p">The parameter to reference</param>
    let private referenceMethodParameter(p : ParameterInfo) =
        {
            ClrMethodParameterReference.Subject = ClrSubject(p.ElementName , p.Position, p |> ParameterElement)
            ParameterType = p.ParameterType
            ValueType = p.ParameterType |> getItemValueType
            IsRequired = (p.IsOptional || p.IsDefined(typeof<OptionalArgumentAttribute>)) |> not   
            Method = p.Member :?> MethodInfo         
           
        }

    /// <summary>
    /// Creates a method reference
    /// </summary>
    /// <param name="m">The method</param>
    let internal referenceMethod pos (m : MethodInfo) =
        let returnType = if m.ReturnType  = typeof<Void> then None else m.ReturnType |> Some
        {
            Subject = ClrSubject(m.ElementName, pos, m |> MethodElement)
            ReturnType = returnType
            ReturnValueType = match returnType with
                                | Some(x) -> x |> getItemValueType|> Some
                                | None -> None
            Parameters = m.GetParameters() |> Array.map referenceMethodParameter |> List.ofArray
        }    

    /// <summary>
    /// Creates a property reference
    /// </summary>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    /// <param name="p">The property to be referenced</param>
    let internal referenceProperty pos (p : PropertyInfo) = 
        {
            Subject = ClrSubject(p.ElementName, pos, p |> PropertyElement)
            ValueType = p.PropertyType |> getItemValueType
            PropertyType = p.PropertyType
        }

    /// <summary>
    /// Creates a field reference
    /// </summary>
    /// <param name="pos">The ordinal position of the field relative to its declaration context</param>
    /// <param name="f">The field to be referenced</param>
    let internal referenceField pos (f : FieldInfo) =
        {
            Subject = ClrSubject(f.ElementName, pos, f |> FieldElement)
            ValueType = f.FieldType|> getItemValueType
            FieldType = f.FieldType
        }

    /// <summary>
    /// Creates a reference to a member
    /// </summary>
    /// <param name="pos">The position of the member</param>
    /// <param name="m">The member</param>
    let private referenceMember pos (m : MemberInfo) =
        match m with
        | :? MethodInfo as x ->
            x |> referenceMethod pos |> MethodMemberReference
        | :? PropertyInfo as x ->
            x |> referenceProperty pos |> PropertyMemberReference |> DataMemberReference
        | :? FieldInfo as x ->
            x |> referenceField pos |> FieldMemberReference |> DataMemberReference
        | _ ->
            NotSupportedException() |> raise

    /// <summary>
    /// Creates an interface reference
    /// </summary>
    /// <param name="t">The type of the interface to reference</param>
    let private createClassReference(t : Type) =
            ClassTypeReference(
                ClrSubject(t.ElementName, -1, t |> ClrTypeElement |> TypeElement), 
                (t |> Type.getPureMethods |> List.mapi referenceMember ) 
                |> List.append (t.GetProperties() |> Array.mapi referenceMember |> List.ofArray))

    /// <summary>
    /// Gets an interface reference
    /// </summary>
    /// <param name="t">The type that defines the type</param>
    let referenceClass (t : Type) =
        if t.IsClass |> not then
            ArgumentException(sprintf "The type %O is not an interface type" t) |> raise
        
        createClassReference |> ClrTypeReferenceIndex.getOrAdd t

    let private createStructReference(t : Type) =
        StructTypeReference(
            ClrSubject(t.ElementName, -1, t |> ClrTypeElement |> TypeElement ),
                (t |> Type.getPureMethods |> List.mapi referenceMember ) 
                |> List.append (t.GetProperties() |> Array.mapi referenceMember |> List.ofArray))
    
    let private referenceStruct (t : Type) =
        if t.IsValueType |> not then
            ArgumentException(sprintf "The type %O is not a struct" t) |> raise

        createStructReference |> ClrTypeReferenceIndex.getOrAdd t


    /// <summary>
    /// Creates an interface reference
    /// </summary>
    /// <param name="t">The type of the interface to reference</param>
    let private createInterfaceReference(t : Type) =
        InterfaceTypeReference(
            ClrSubject(t.ElementName, -1, t |> ClrTypeElement |> TypeElement),
                (t |> Type.getPureMethods |> List.mapi referenceMember ) 
                |> List.append (t.GetProperties() |> Array.mapi referenceMember |> List.ofArray))

    /// <summary>
    /// Gets an interface reference
    /// </summary>
    /// <param name="t">The type that defines the type</param>
    let referenceInterface (t : Type) =
        if t.IsInterface |> not then
            ArgumentException(sprintf "The type %O is not an interface type" t) |> raise
        
        createInterfaceReference |> ClrTypeReferenceIndex.getOrAdd t

    let private recordFactory = ConcurrentDictionary<Type, obj[]->obj>()

    let private createRecordFactory (t : Type) =
        FSharpValue.PreComputeRecordConstructor(t, true)

    let internal getRecordFactory(tref : ClrTypeReference) =
        let typeElement = tref |> getTypeReferent
        recordFactory.[typeElement.Type]

    /// <summary>
    /// Creates a record reference
    /// </summary>
    /// <param name="t">The CLR type of the record</param>
    let private createRecordReference (t : Type) =
        recordFactory.[t] <- t |> createRecordFactory

        let proprefs = 
            FSharpType.GetRecordFields(t,true) 
               |> Array.mapi(fun i p ->  
                    {
                        ClrPropertyReference.Subject = ClrSubject(p.ElementName, i, p |> PropertyElement)
                        PropertyType = p.PropertyType 
                        ValueType =   p.PropertyType |> getItemValueType
                    }) 
                |> List.ofArray
        RecordTypeReference(ClrSubject(t.ElementName, 0, t |> ClrTypeElement |> TypeElement ), proprefs)

               
       
                       
    /// <summary>
    /// Create a record reference
    /// </summary>
    /// <param name="t">The type</param>
    let referenceRecord(t : Type) =
        if t |> isRecordType |> not then
            ArgumentException(sprintf "The type %O is not a record type" t) |> raise
        
        createRecordReference |> ClrTypeReferenceIndex.getOrAdd t
      
    /// <summary>
    /// Creates a reference to a property field
    /// </summary>
    /// <param name="i">The field's position within the case</param>
    /// <param name="p">The property that represents the field</param>
    let private referenceUnionField pos (p : PropertyInfo) = 
        {
            Subject = ClrSubject(p.ElementName, pos, p |> PropertyElement)
            PropertyType = p.PropertyType
            ValueType = p.PropertyType |> getItemValueType
        }

    /// <summary>
    /// Creates a reference to a union case
    /// </summary>
    /// <param name="c">The case information</param>
    let private referenceUnionCase(c : UnionCaseInfo) =
        {
            ClrUnionCaseReference.Subject = ClrSubject(c.ElementName, c.Tag, c |> UnionCaseElement)            
            Fields = c.GetFields() |> List.ofArray |> List.mapi referenceUnionField
        }
    
    /// <summary>
    /// Describes the cases defined by a supplied union type
    /// </summary>
    /// <param name="t">The union type</param>
    let private referenceCases(t : Type) =
        FSharpType.GetUnionCases(t, true) |> List.ofArray |> List.map referenceUnionCase

    /// <summary>
    /// Creates a union description
    /// </summary>
    /// <param name="t">The union type</param>
    let private createUnionReference(t : Type) =      
        UnionTypeReference(ClrSubject(t.ElementName, -1, t |> ClrTypeElement |> TypeElement ), t |> referenceCases)

    /// <summary>
    /// Describes the union represented by the type
    /// </summary>
    /// <param name="t"></param>
    let referenceUnion(t : Type) =
        if t |> isUnionType |> not then
            ArgumentException(sprintf "The type %O is not a record type" t) |> raise
        
        createUnionReference |> ClrTypeReferenceIndex.getOrAdd t

    /// <summary>
    /// Creates a reference to a CLR type
    /// </summary>
    /// <param name="t">The type to reference</param>
    let rec reference (t : Type)  =
        let referenceCollection (t : Type) =
            let itemValueType = t |> getItemValueType |> reference 
            CollectionTypeReference(
                ClrSubject(t.ElementName, -1, t |> ClrTypeElement |> TypeElement),
                itemValueType,
                t |> getCollectionKind)
        
        match t |> getTypeKind with
            | ClrTypeKind.Collection -> referenceCollection |> ClrTypeReferenceIndex.getOrAdd t
            | ClrTypeKind.Record -> t |> referenceRecord
            | ClrTypeKind.Union  -> t |> referenceUnion 
            | ClrTypeKind.Interface -> t |> referenceInterface
            | ClrTypeKind.Class -> t |> referenceClass
            | ClrTypeKind.Struct -> t |> referenceStruct
            | _ ->
                NotSupportedException() |> raise                
          
    /// <summary>
    /// Reads an identified attribute from a type, if present
    /// </summary>
    /// <param name="t"></param>
    let getAttribute<'T when 'T :> Attribute> (tref : ClrTypeReference) =
         tref |> getReferent |>  AttributeT.tryGetOne<'T>
                               
    let getDeclaringType(tref : ClrTypeReference) =
        let element = tref |> getTypeReferent
        element.DeclaringType 
    
        

/// <summary>
/// Defines type-related augmentations and operators
/// </summary>
[<AutoOpen>]
module ClrTypeReferenceExtensions =
    /// <summary>
    /// Creates a reference to the type identified by the supplied type parameter
    /// </summary>
    let typeref<'T> = typeof<'T> |> ClrTypeReference.reference

    /// <summary>
    /// Creates a property reference
    /// </summary>
    /// <param name="p">The property to be referenced</param>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    let internal propref  pos (p : PropertyInfo) =
        p |> ClrTypeReference.referenceProperty pos


    /// <summary>
    /// Gets the methods defined by a type
    /// </summary>
    let methodrefmap<'T> = 
        typeof<'T> |> Type.getPureMethods |> List.mapi ClrTypeReference.referenceMethod |> List.map(fun m -> m.Subject.Name, m) |> Map.ofList
    

    /// <summary>
    /// Defines augmentations for the UnionCaseDescription type
    /// </summary>
    type ClrUnionCaseReference
    with
        /// <summary>
        /// Indexer that finds a case field by its position
        /// </summary>
        /// <param name="position">The position of the case field</param>
        member this.Item(position) = this.Fields.[position]

        /// <summary>
        /// Indexer that finds a case field by its name
        /// </summary>
        /// <param name="name">The name of the case field</param>
        member this.Item(name) = this.Fields |> List.find(fun f -> f.Name = name)

    /// <summary>
    /// Defines augmentations for the ClrTypeReference type
    /// </summary>
    type ClrTypeReference
    with
        member this.TypeReferent = this |> ClrTypeReference.getTypeReferent
        
        member this.Referent = this |> ClrTypeReference.getReferent
        
        member this.ReferentName = this |> ClrTypeReference.getReferentName
        
        member this.ReferentType = this |> ClrTypeReference.getReferentType

        member this.ReferentTypeName = this |> ClrTypeReference.getReferentTypeName
        

    
