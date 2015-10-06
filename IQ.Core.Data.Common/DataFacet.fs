// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Behavior

open System
open System.Collections.Generic

open FSharp.Data

open IQ.Core.Framework
open IQ.Core.Data.Contracts


module DataFacet = 
    
    let private cast<'T>(x : obj) = x :?> 'T

    let private value<'T>(a : ClrAttribution) =
        a.AttributeInstance |> Option.get |> cast<FacetAttribute<'T>> |> fun x -> x.Value
    
    let private attrib<'A,'T when 'A :> Attribute>(element : ClrElement) =
        match element.TryGetAttribute<'A>() with
            | Some(a) -> a |> value<'T> |> Some
            | None -> None

    let private getRangeMin(r : Range<'T>) =
        match r with |Range(MinValue=x) ->x

    let private getRangeMax(r : Range<'T>) =
        match r with |Range(MaxValue=x) ->x


    /// <summary>
    /// Retrieves the identified facet, if present
    /// </summary>
    /// <param name="facetName">The name of the facet</param>
    /// <param name="element">The element to which the facet may be attached/param>
    let tryGetFacetValue<'T> facetName (element : ClrElement) =
        match facetName with
        | DataFacetNames.Nullable -> 
            element |> attrib<NullableAttribute, 'T>
        
        | DataFacetNames.Position -> 
            element |> attrib<PositionAttribute, 'T>
        
        | DataFacetNames.DataKind -> 
            element |> attrib<DataKindAttribute, 'T>
        
        | DataFacetNames.CustomObjectName -> 
            element |> attrib<CustomDataKindAttribute, 'T>
        
        | DataFacetNames.FixedLength -> 
            element |> attrib<FixedLengthAttribute, 'T>            
        
        | DataFacetNames.MinLength -> 
            match element |> attrib<MinLengthAttribute, 'T> with
            | Some(x) -> Some(x)
            | None -> None
        
        | DataFacetNames.MaxLength -> 
            match element |> attrib<MaxLengthAttribute, 'T> with
            | Some(x) -> Some(x)
            | None -> None

        | DataFacetNames.Precision -> 
            element |> attrib<PrecisionAttribute, 'T>            
        
        | DataFacetNames.Scale -> 
            element |> attrib<ScaleAttribute, 'T>    
        
        | DataFacetNames.MinValue -> 
            match element |> attrib<MinValueAttribute, 'T> with
            | Some(x) -> Some(x)
            | None ->
                match element |> attrib<RangeAttribute, Range<'T>> with
                | Some(x) -> x |> getRangeMin |> Some 
                | None -> None
        
        | DataFacetNames.MaxValue -> 
            match element |> attrib<MaxValueAttribute, 'T> with
            | Some(x) -> Some(x)
            | None ->
                match element |> attrib<RangeAttribute, Range<'T>> with
                | Some(x) -> x |> getRangeMin |> Some 
                | None -> None
        

        | DataFacetNames.XmlSchema ->
            element |> attrib<XmlSchemaAttribute, 'T> 
        
        | DataFacetNames.RepresentationType ->
            element |> attrib<RepresentationTypeAttribute, 'T>   

        | DataFacetNames.DataObjectName ->
            element |> attrib<DataObjectNameAttribute, 'T>

        | _ -> nosupport()    
    

    let getFacetValue<'T> facetName (element : ClrElement) =
        element |> tryGetFacetValue<'T> facetName |> Option.get

    let hasFacet<'T> facetName (element : ClrElement) =
        element |> tryGetFacetValue<'T> facetName |> Option.isSome