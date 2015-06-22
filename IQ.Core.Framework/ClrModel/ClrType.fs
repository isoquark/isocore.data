namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent
open System.Collections.Generic

open Microsoft.FSharp.Reflection



module ClrType =
    
    /// <summary>
    /// Defines internal reflection cache for efficiency
    /// </summary>
    module internal ClrTypeReferenceIndex =
        let private types =  ConcurrentDictionary<Type, ClrTypeReference>()
        /// <summary>
        /// Retrieves an existing reference if present or constructs a new one and adds it to the index
        /// </summary>
        /// <param name="t">The type to reference</param>
        /// <param name="f">The reference factory</param>
        let getOrAdd(t : Type) (f:Type->ClrTypeReference) =
            types.GetOrAdd(t,f)
    
    /// <summary>
    /// Determines whether a type is a generic enumerable
    /// </summary>
    /// <param name="t">The type to examine</param>
    let internal isNonOptionalCollectionType (t : Type) =
        let isEnumerable = t.GetInterfaces() |> Array.exists(fun x -> x.IsGenericType && x.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>)
        if t.IsArray |> not then
            t.IsGenericType && isEnumerable
        else
            isEnumerable            

    /// <summary>
    /// Determines whether the type is of the form option<IEnumerable<_>>
    /// </summary>
    /// <param name="t">The type to examine</param>
    let internal isOptionalCollectionType (t : Type) =
        t |> ClrOption.isOptionType && t |> ClrOption.getOptionValueType |> Option.get |> (fun x -> x |> isNonOptionalCollectionType)

    /// <summary>
    /// Determines whether a type represents a collection (optional or not)
    /// </summary>
    /// <param name="t">The type to examine</param>
    let isCollectionType (t : Type) =
        t |> isNonOptionalCollectionType || t |> isOptionalCollectionType
                
    let getCollectionValueType (t : Type) =
        //This is far from bullet-proof
        let colltype =
            if t |> isOptionalCollectionType then
                t |> ClrOption.getOptionValueType 
            else if t |> isNonOptionalCollectionType then
                t |> Some
            else
                None
        match colltype with
        | Some(t) ->
            let i = t.GetInterfaces() |> Array.find(fun i -> i.IsGenericType && i.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>)    
            i.GetGenericArguments().[0] |> Some
        | None ->
            None
                
    let getItemValueType (t : Type)  =
        match t |> getCollectionValueType with
        | Some(t) -> t
        | None ->
            match t |> ClrOption.getOptionValueType with
            | Some(t) -> t
            | None ->
                t

    let private referenceMethodParameter(p : ParameterInfo) =
        {
            ClrMethodParameterReference.Subject = ClrSubjectReference(p.ElementName , p.Position, p)
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
            Subject = ClrSubjectReference(m.ElementName, pos, m)
            Return = 
                {
                    ReturnType = returnType
                    Method = m
                    ValueType = match returnType with
                                | Some(x) -> x |> getItemValueType|> Some
                                | None -> None
                                
                }
            Parameters = m.GetParameters() |> Array.map referenceMethodParameter |> List.ofArray
        }    


    /// <summary>
    /// Creates a property description
    /// </summary>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    /// <param name="p">The property to be referenced</param>
    let internal describeProperty pos (p : PropertyInfo) =
        {
            Subject = ClrSubjectDescription(p.ElementName, pos)
            DeclaringType  = p.DeclaringType.FullName |> FullTypeName
            ValueType = p.PropertyType |> getItemValueType |> fun x -> x.FullName |> FullTypeName
            IsOptional = p.PropertyType |> ClrOption.isOptionType
            CanRead = p.CanRead
            ReadAccess = if p.CanRead then p.GetMethod |> ClrAccess.getMethodAccess |> Some else None
            CanWrite = p.CanWrite
            WriteAccess =  if p.CanWrite then p.SetMethod |> ClrAccess.getMethodAccess |> Some else None
        }

    /// <summary>
    /// Creates a property reference
    /// </summary>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    /// <param name="p">The property to be referenced</param>
    let internal referenceProperty pos (p : PropertyInfo) = 
        {
            Subject = ClrSubjectReference(p.ElementName, pos, p)
            ValueType = p.PropertyType |> getItemValueType
            PropertyType = p.PropertyType
        }

    let private describeMember pos (m : MemberInfo) =
        match m with
        | :? MethodInfo as x ->
            x |> referenceMethod pos |> MethodReference
        | :? PropertyInfo as x ->
            x |> referenceProperty pos |> PropertyReference
        | _ ->
            NotSupportedException() |> raise

    /// <summary>
    /// Creates an interface reference
    /// </summary>
    /// <param name="t">The type of the interface to reference</param>
    let private createClassReference(t : Type) =
            ClassTypeReference(
                {Subject = ClrSubjectReference(t.ElementName, -1, t)}, 
                (t |> Type.getPureMethods |> List.mapi describeMember ) 
                |> List.append (t.GetProperties() |> Array.mapi describeMember |> List.ofArray))

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
            {Subject = ClrSubjectReference(t.ElementName, -1, t)},
                (t |> Type.getPureMethods |> List.mapi describeMember ) 
                |> List.append (t.GetProperties() |> Array.mapi describeMember |> List.ofArray))
    
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
            {Subject = ClrSubjectReference(t.ElementName, -1, t)},
                (t |> Type.getPureMethods |> List.mapi describeMember ) 
                |> List.append (t.GetProperties() |> Array.mapi describeMember |> List.ofArray))

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

    /// <summary>
    /// Creates a record reference
    /// </summary>
    /// <param name="t">The CLR type of the record</param>
    let private createRecordReference (t : Type) =
        recordFactory.[t] <- t |> createRecordFactory
        RecordTypeReference(
            {Subject = ClrSubjectReference(t.ElementName, 0, t)},
            FSharpType.GetRecordFields(t,true) 
               |> Array.mapi(fun i p -> 
                     {Subject = ClrSubjectReference(p.ElementName, i, p)
                      PropertyType = p.PropertyType 
                      ValueType =   p.PropertyType |> getItemValueType
                      }) 
               |> List.ofArray
        )

    /// <summary>
    /// Determines whether a supplied type is a record type
    /// </summary>
    /// <param name="t">The candidate type</param>
    let isRecordType(t : Type) =
        FSharpType.IsRecord(t, true)

    /// <summary>
    /// Determines whether a supplied type is a record type
    /// </summary>
    let isRecord<'T>() =
        typeof<'T> |> isRecordType
                       
    /// <summary>
    /// Create a record reference
    /// </summary>
    /// <param name="t">The type</param>
    let referenceRecord(t : Type) =
        if t |> isRecordType |> not then
            ArgumentException(sprintf "The type %O is not a record type" t) |> raise
        
        createRecordReference |> ClrTypeReferenceIndex.getOrAdd t

    /// <summary>
    /// Retrieves record field values indexed by field name
    /// </summary>
    /// <param name="record">The record whose values will be retrieved</param>
    let recordToValueMap (record : obj) =
        match record.GetType() |> referenceRecord with
        | RecordTypeReference(subject, fields) ->
            fields |> List.map(fun field -> field.Name.Text, field.Property.GetValue(record)) |> ValueIndex.fromNamedItems
        | _ -> 
            NotSupportedException() |> raise
    
    /// <summary>
    /// Creates a record from a value map
    /// </summary>
    /// <param name="valueMap">The value map</param>
    /// <param name="tref"></param>
    let recordFromValueMap (valueMap : ValueIndex) (tref : ClrTypeReference) =
        match tref with
        | RecordTypeReference(subject, fields) ->
            fields |> List.map(fun field -> valueMap.[field.Name.Text]) |> Array.ofList |> recordFactory.[subject.Type]
        | _ -> 
            NotSupportedException() |> raise
    
    /// <summary>
    /// Creates an array of field values, in declaration order, for a specified record value
    /// </summary>
    /// <param name="record"></param>
    let recordToValueArray (record : obj) =
        match record.GetType() |> referenceRecord with
        | RecordTypeReference(subject, fields) ->
            [|for i in 0..fields.Length - 1 ->
                record |> fields.[i].Property.GetValue
            |]        
        | _ -> 
            NotSupportedException() |> raise

                
    /// <summary>
    /// Creates a record from an array of values that are specified in declaration order
    /// </summary>
    /// <param name="valueArray">An array of values in declaration order</param>
    /// <param name="tref">Reference to type</param>
    let recordFromValueArray (valueArray : obj[]) (tref : ClrTypeReference) =
        match tref with
        | RecordTypeReference(subject, fields) ->
            valueArray |> recordFactory.[subject.Type]    
        | _ -> 
            NotSupportedException() |> raise
       

    /// <summary>
    /// Creates a reference to a property field
    /// </summary>
    /// <param name="i">The field's position within the case</param>
    /// <param name="p">The property that represents the field</param>
    let private referenceUnionField pos (p : PropertyInfo) = 
        {
            Subject = ClrSubjectReference(p.ElementName, pos, p)
            PropertyType = p.PropertyType
            ValueType = p.PropertyType |> getItemValueType
        }

    /// <summary>
    /// Creates a reference to a union case
    /// </summary>
    /// <param name="c">The case information</param>
    let private referenceUnionCase(c : UnionCaseInfo) =
        {
            ClrUnionCaseReference.Subject = ClrSubjectReference(c.ElementName, c.Tag, c)            
            Fields = c.GetFields() |> List.ofArray |> List.mapi referenceUnionField
        }
    
    /// <summary>
    /// Describes the cases defined by a supplied union type
    /// </summary>
    /// <param name="t">The union type</param>
    let private referenceCases(t : Type) =
        FSharpType.GetUnionCases(t, true) |> List.ofArray |> List.map referenceUnionCase

    /// <summary>
    /// Determines whether a supplied type is a union type
    /// </summary>
    /// <param name="t">The candidate type</param>
    let isUnionType (t : Type) =
        FSharpType.IsUnion(t, true)

    /// <summary>
    /// Determines whether a supplied type is a union type
    /// </summary>
    let isUnion<'T>() =
        typeof<'T> |> isUnionType


    /// <summary>
    /// Creates a union description
    /// </summary>
    /// <param name="t">The union type</param>
    let private createUnionReference(t : Type) =      
        UnionTypeReference(
            {Subject = ClrSubjectReference(t.ElementName, -1, t)},
            t |> referenceCases
        )

    /// <summary>
    /// Describes the union represented by the type
    /// </summary>
    /// <param name="t"></param>
    let referenceUnion(t : Type) =
        if t |> isUnionType |> not then
            ArgumentException(sprintf "The type %O is not a record type" t) |> raise
        
        createUnionReference |> ClrTypeReferenceIndex.getOrAdd t


    /// <summary>
    /// Determines the collection kind
    /// </summary>
    /// <param name="t">The type to examine</param>
    let getCollectionKind t =
        let collType = if t |> ClrOption.isOptionType then t |> ClrOption.getOptionValueType |> Option.get else t
        if collType |> Type.isArray then
            ClrCollectionKind.Array
            //This is not the best way to do this...but probably is the fastest
        else if collType.FullName.StartsWith("Microsoft.FSharp.Collections.FSharpList") then        
            ClrCollectionKind.FSharpList            
        else
            ClrCollectionKind.Unknown

    /// <summary>
    /// Creates a reference to a CLR type
    /// </summary>
    /// <param name="t">The type to reference</param>
    let rec reference (t : Type)  =
        let referenceCollection (t : Type) =
            let itemValueType = t |> getItemValueType |> reference 
            CollectionTypeReference(
                {Subject = ClrSubjectReference(t.ElementName, -1, t)},
                itemValueType,
                t |> getCollectionKind)
        if t |> isCollectionType then
            referenceCollection |> ClrTypeReferenceIndex.getOrAdd t
        else if FSharpType.IsRecord(t, true) then
            t |> referenceRecord 
        else if FSharpType.IsUnion(t, true) then
            t |> referenceUnion 
        else if t.IsInterface then
            t |> referenceInterface 
        else if t.IsClass then
            t |> referenceClass 
        else if t.IsValueType then
            t |> referenceStruct
        else
            NotImplementedException() |> raise

   





module ClrTypeReference =     
    let private getSubject tref =
        match tref with
        | UnionTypeReference(subject,cases) -> subject
        | RecordTypeReference(subject,fields)->subject
        | InterfaceTypeReference(subject,members)->subject
        | ClassTypeReference(subject,members)->subject
        | CollectionTypeReference(subject,itemType,collectionKind)->subject
        | StructTypeReference(subject,fields)->subject            
    
    /// <summary>
    /// Gets the CLR type being referenced
    /// </summary>
    /// <param name="tref">The type reference</param>
    let getType (tref : ClrTypeReference) =
        tref |> getSubject |> fun x -> x.Type                   

    let getName (tref : ClrTypeReference) =
        tref |> getSubject |> fun x -> x.Name

    let getPosition (tref : ClrTypeReference) =
        tref |> getSubject |> fun x -> x.Position

    /// <summary>
    /// Reads an identified attribute from a type, if present
    /// </summary>
    /// <param name="t"></param>
    let getAttribute<'T when 'T :> Attribute> (t : ClrTypeReference) =
        t |> getType |> Type.getAttribute<'T>
                
    let getDeclaringType(tref : ClrTypeReference) =
        tref |> getSubject |> fun s -> s.Type.DeclaringType    
    
    let fromValueMap (valueMap : ValueIndex) (tref : ClrTypeReference) =
        tref |> ClrType.recordFromValueMap valueMap


/// <summary>
/// Defines type-related augmentations and operators
/// </summary>
[<AutoOpen>]
module ClrTypeExtensions =
    /// <summary>
    /// Creates a reference to the type identified by the supplied type parameter
    /// </summary>
    let typeref<'T> = typeof<'T> |> ClrType.reference

    /// <summary>
    /// Gets the properties defined by the type
    /// </summary>
    let props<'T> = typeof<'T> |> Type.getProperties


    /// <summary>
    /// References the union identified by the type parameter
    /// </summary>
    let unionref<'T> =
        typeof<'T> |> ClrType.referenceUnion

    /// <summary>
    /// References the record identified by the type parameter
    /// </summary>
    let recordref<'T> =
        typeof<'T> |> ClrType.referenceRecord

    let interfaceref<'T> =
        typeof<'T> |> ClrType.referenceInterface

    /// <summary>
    /// Creates a property reference
    /// </summary>
    /// <param name="p">The property to be referenced</param>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    let internal propref  pos (p : PropertyInfo) =
        p |> ClrType.referenceProperty pos

    /// <summary>
    /// Creates a property description
    /// </summary>
    /// <param name="pos">The ordinal position of the property relative to its declaration context</param>
    /// <param name="p">The property to be referenced</param>
    let internal propinfo pos (p : PropertyInfo) = 
        p |> ClrType.describeProperty pos

    /// <summary>
    /// Creates a property description map keyed by name
    /// </summary>
    let propinfomap<'T> = props<'T> |> List.mapi propinfo |> List.map(fun p -> p.Name, p) |> Map.ofList

    /// <summary>
    /// Gets the methods defined by a type
    /// </summary>
    let methodrefmap<'T> = 
        typeof<'T> |> Type.getPureMethods |> List.mapi ClrType.referenceMethod |> List.map(fun m -> m.Subject.Name, m) |> Map.ofList
    
    type Type
    with
        member this.IsOptionType = this |> ClrOption.isOptionType

        /// <summary>
        /// Returns true if type realizes IEnumerable<_>
        /// </summary>
        member this.IsGenericEnumerable = this |> ClrType.isNonOptionalCollectionType
        
        /// <summary>
        /// Returns true if type is of the form option<IEnumerable<_>>
        /// </summary>
        member this.IsOptionalEnumerable = this |> ClrType.isOptionalCollectionType

        /// <summary>
        /// If optional type, gets the type of the underlying value; otherwise, the type itself
        /// </summary>
        member this.ItemValueType = this |> ClrType.getItemValueType

    /// <summary>
    /// Defines augmentations for the <see cref="ClrTypeReference"/> type
    /// </summary>
    type ClrTypeReference
    with
        /// <summary>
        /// The name of the record
        /// </summary>
        member this.Name = this |> ClrTypeReference.getName
        member this.Position = this |> ClrTypeReference.getPosition
        member this.Type = this |> ClrTypeReference.getType


    module PropertyInfo =
        /// <summary>
        /// Retrieves an attribute applied to a property, if present
        /// </summary>
        /// <param name="subject">The type to examine</param>
        let getAttribute<'T when 'T :> Attribute>(subject : PropertyInfo) = 
            subject |> MemberInfo.getAttribute<'T>

        /// <summary>
        /// Gets the data type of the property, ignoring whether the property is optional
        /// </summary>
        /// <param name="p">The property</param>
        let getValueType (p : PropertyInfo) =
            p.PropertyType.ItemValueType

    type PropertyInfo
    with
        member this.ValueType = this |> PropertyInfo.getValueType



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

