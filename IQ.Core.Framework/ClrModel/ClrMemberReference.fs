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
    type ClrDataMemberReference
    with
        member this.Name = this |> ClrDataMemberReference.getName
        member this.MemberType = this |> ClrDataMemberReference.getMemberType
        member this.Position = this |> ClrDataMemberReference.getPosition

    type ClrMemberReference
    with
        member internal this.Subject = this |> ClrMemberReference.getSubject
        member this.Name = this |> ClrMemberReference.getName
            
    
    /// <summary>
    /// Defines augmentations for the <see cref="ClrMethodReference"/> type
    /// </summary>
    type ClrMethodReference 
    with
        /// <summary>
        /// The name of the method
        /// </summary>
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position
        member this.Method = this.Subject.Element
        member this.MethodInfo = 
            match this.Subject.Element with 
            | MemberElement(x) -> 
                match x with
                | MethodElement(x) -> x 
                | _ -> nosupport()
            |_ -> nosupport()

    /// <summary>
    /// Defines augmentations for the <see cref="ClrPropertyReference"/> type
    /// </summary>
    type ClrPropertyReference    
    with
        /// <summary>
        /// The name of the property
        /// </summary>
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position
        member this.Property = this.Subject.Element
        member this.PropertyInfo = 
            match this.Subject.Element with 
            | MemberElement(x) -> 
                match x with
                | DataMemberElement(x) ->
                    match x with
                    | PropertyMember(x) -> x
                    | _ -> nosupport()
                | _ -> nosupport()
            |_ -> nosupport()


    type ClrFieldReference
    with
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position
        member this.Field = this.Subject.Element
        member this.FieldInfo = 
            match this.Subject.Element with 
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
        /// The name of the parameter
        /// </summary>
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position
        member this.Parameter = this.Subject.Element

    /// <summary>
    /// Defines augmentations for the <see cref="ClrUnionCaseReference"/> type
    /// </summary>
    type ClrUnionCaseReference
    with
        /// <summary>
        /// The name of the union case
        /// </summary>
        member this.Name = this.Subject.Name
        member this.Position = this.Subject.Position
        member this.Case = this.Subject.Element
