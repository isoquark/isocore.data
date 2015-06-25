namespace IQ.Core.Framework

open System
open System.Reflection
open System.Collections.Concurrent
open System.Diagnostics

module ClrDataMemberReference =

    let internal getSubject (mref : ClrDataMemberReference) =        
        match mref with
            | FieldMemberReference(x) -> x.Subject
            | PropertyMemberReference(x) -> x.Subject

    let getName (mref : ClrDataMemberReference)    =
        mref |> getSubject |> fun x -> x.Name
    
    let getMemberType (mref : ClrDataMemberReference) =
        match mref with 
        | FieldMemberReference(x) -> 
            x.FieldType 
        | PropertyMemberReference(x) 
            -> x.PropertyType        

    let getPosition (mref : ClrDataMemberReference) =
        mref |> getSubject |> fun x -> x.Position



module ClrMemberReference =
    let internal getSubject (mref : ClrMemberReference) =        
        match mref with
            | MethodMemberReference(x) -> x.Subject
            | DataMemberReference(x) ->
                x |> ClrDataMemberReference.getSubject
              
    let getName (mref : ClrMemberReference) =
        match mref with
        | MethodMemberReference(x) -> x.Subject.Name
        | DataMemberReference(x) -> x |> ClrDataMemberReference.getName


[<AutoOpen>]
module ClrMemberReferenceExtensions =

    /// <summary>
    /// Defines augmentations for the <see cref="ClrDataMemberReference"/> type
    /// </summary>
    type ClrDataMemberReference
    with
        /// <summary>
        /// The name of the referent
        /// </summary>
        member this.Name = this |> ClrDataMemberReference.getName

        /// <summary>
        /// The CLR type of the data member
        /// </summary>
        member this.MemberType = this |> ClrDataMemberReference.getMemberType

        /// <summary>
        /// The position of the referent
        /// </summary>
        member this.Position = this |> ClrDataMemberReference.getPosition

        /// <summary>
        /// Interprets the reference as a <see cref="ClrElementReference"/>
        /// </summary>
        member this.ElementReference = this |> DataMemberReference |> MemberReference


    /// <summary>
    /// Defines augmentations for the <see cref="ClrMemberReference"/> type
    /// </summary>
    type ClrMemberReference
    with
        member internal this.Subject = this |> ClrMemberReference.getSubject

        /// <summary>
        /// The name of the referent
        /// </summary>
        member this.ReferentName = this.Subject.Name


        /// <summary>
        /// The identified element
        /// </summary>
        member this.Referent = this.Subject.Element
            
        /// <summary>
        /// The position of the referent
        /// </summary>
        member this.ReferentPosition = this.Subject.Position

        /// <summary>
        /// Interprets the reference as a <see cref="ClrElementReference"/>
        /// </summary>
        member this.ElementReference = this |> MemberReference

        /// <summary>
        /// The raw CLR element represented by the referent
        /// </summary>
        member this.MemberInfo = 
            match this.Referent with
            | MemberElement(x) -> 
                match x with
                | MethodElement(x) -> x.MethodInfo :> MemberInfo
                | DataMemberElement(x) ->
                    match x with
                    | PropertyMember(x) -> x.PropertyInfo :> MemberInfo
                    | FieldMember(x) -> x.FieldInfo :> MemberInfo
            | _ -> nosupport()
                

    
    /// <summary>
    /// Defines augmentations for the <see cref="ClrMethodReference"/> type
    /// </summary>
    type ClrMethodReference 
    with
        /// <summary>
        /// The name of the referent
        /// </summary>
        member this.ReferentName = this.Subject.Name

        /// <summary>
        /// The position of the referent
        /// </summary>
        member this.ReferentPosition = this.Subject.Position
        
        /// <summary>
        /// The identified element
        /// </summary>
        member this.Referent = this.Subject.Element
        
        /// <summary>
        /// The raw CLR element represented by the referent
        /// </summary>
        member this.MethodInfo = 
            match this.Referent with 
            | MemberElement(x) -> 
                match x with
                | MethodElement(x) -> x.MethodInfo 
                | _ -> nosupport()
            |_ -> nosupport()
        
        /// <summary>
        /// Interprets the reference as a <see cref="ClrMemberReference"/>
        /// </summary>
        member this.MemberReference = this |> MethodMemberReference
            
        /// <summary>
        /// Interprets the reference as a <see cref="ClrElementReference"/>
        /// </summary>
        member this.ElementReference = this.MemberReference |> MemberReference

    /// <summary>
    /// Defines augmentations for the <see cref="ClrPropertyReference"/> type
    /// </summary>
    type ClrPropertyReference    
    with
        /// <summary>
        /// The name of the referent
        /// </summary>
        member this.ReferentName = this.Subject.Name

        /// <summary>
        /// The relative position of the referent
        /// </summary>
        member this.ReferentPosition = this.Subject.Position

        /// <summary>
        /// The identified element
        /// </summary>
        member this.Referent = this.Subject.Element

        /// <summary>
        /// The raw CLR element represented by the referent
        /// </summary>
        member this.PropertyInfo = 
            match this.Referent with 
            | MemberElement(x) -> 
                match x with
                | DataMemberElement(x) ->
                    match x with
                    | PropertyMember(x) -> x
                    | _ -> nosupport()
                | _ -> nosupport()
            |_ -> nosupport()

        /// <summary>
        /// Interprets the reference as a <see cref="ClrDataMemberReference"/>
        /// </summary>
        member this.DataMemberReference = this |> PropertyMemberReference
        
        /// <summary>
        /// Interprets the reference as a <see cref="ClrMemberReference"/>
        /// </summary>
        member this.MemberReference = this.DataMemberReference |> DataMemberReference

        /// <summary>
        /// Interprets the reference as a <see cref="ClrElementReference"/>
        /// </summary>
        member this.ElementReference = this.MemberReference |> MemberReference
               
    /// <summary>
    /// Defines augmentations for the <see cref="ClrFieldReference"/> type
    /// </summary>
    type ClrFieldReference
    with
        /// <summary>
        /// The name of the referent
        /// </summary>
        member this.ReferentName = this.Subject.Name
        
        /// <summary>
        /// The relative position of the referent
        /// </summary>
        member this.ReferentPosition = this.Subject.Position
        
        /// <summary>
        /// The identified element
        /// </summary>
        member this.Referent = this.Subject.Element

        /// <summary>
        /// Interprets the reference as a <see cref="ClrDataMemberReference"/>
        /// </summary>
        member this.DataMemberReference = this |> FieldMemberReference
        
        /// <summary>
        /// Interprets the reference as a <see cref="ClrMemberReference"/>
        /// </summary>
        member this.MemberReference = this.DataMemberReference |> DataMemberReference

        /// <summary>
        /// Interprets the reference as a <see cref="ClrElementReference"/>
        /// </summary>
        member this.ElementReference =this.MemberReference |> MemberReference

        /// <summary>
        /// The raw CLR element represented by the referent
        /// </summary>
        member this.FieldInfo = 
            match this.Referent with 
            | MemberElement(x) -> 
                match x with
                | DataMemberElement(x) ->
                    match x with
                    | FieldMember(x) -> x
                    | _ -> nosupport()
                | _ -> nosupport()
            |_ -> nosupport()

    /// <summary>
    /// Defines augmentations for the <see cref="ClrMethodParameterReference"/> type
    /// </summary>
    type ClrMethodParameterReference
    with
        /// <summary>
        /// The name of the referent
        /// </summary>
        member this.ReferentName = this.Subject.Name

        /// <summary>
        /// The relative position of the referent
        /// </summary>
        member this.ReferentPosition = this.Subject.Position

        /// <summary>
        /// The identified element
        /// </summary>
        member this.Referent = this.Subject.Element

        /// <summary>
        /// Interprets the reference as a <see cref="ClrElementReference"/>
        /// </summary>
        member this.ElementReference =
            this |> MethodParameterReference

        /// <summary>
        /// The raw CLR element represented by the referent
        /// </summary>
        member this.ParameterInfo = 
            match this.Referent with
            | ParameterElement(x) -> x
            |_ -> nosupport()

    /// <summary>
    /// Defines augmentations for the <see cref="ClrUnionCaseReference"/> type
    /// </summary>
    type ClrUnionCaseReference
    with
        /// <summary>
        /// The name of the union case
        /// </summary>
        member this.ReferentName = this.Subject.Name

        /// <summary>
        /// The relative position of the referent
        /// </summary>
        member this.ReferentPosition = this.Subject.Position

        /// <summary>
        /// The identified element
        /// </summary>
        member this.Referent = this.Subject.Element

        /// <summary>
        /// Interprets the the specific reference as a general element reference
        /// </summary>
        member this.ElementReference =
            this |> UnionCaseReference

        /// <summary>
        /// Indexer that finds a case field by its position
        /// </summary>
        /// <param name="position">The position of the case field</param>
        member this.Item(position) = this.Fields.[position]

        /// <summary>
        /// Indexer that finds a case field by its name
        /// </summary>
        /// <param name="name">The name of the case field</param>
        member this.Item(name) = this.Fields |> List.find(fun f -> f.ReferentName = name)

        /// <summary>
        /// The raw CLR element represented by the referent
        /// </summary>
        member this.UnionCaseInfo = 
            match this.Referent with
            | UnionCaseElement(x) -> x
            |_ -> nosupport()
