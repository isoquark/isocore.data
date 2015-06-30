﻿namespace IQ.Core.Data

open System
//open System.ComponentModel
open System.Reflection
open System.Data
open System.Text.RegularExpressions

open FSharp.Data

open IQ.Core.Framework

type DescriptionAttribute = System.ComponentModel.DescriptionAttribute

/// <summary>
/// Defines operations that populate a Data Proxy Metamodel
/// </summary>
module DataProxyMetadata =    
    [<Literal>]
    let private DefaultClrTypeMapResource = "Data/Resources/DefaultClrStorageTypeMap.csv"
    
    type private DefaultClrTypeMap = CsvProvider<DefaultClrTypeMapResource, Separators="|">
    
    let private getDefaultClrStorageTypeMap() =
        let mapdata = DefaultClrTypeMap.Load(DefaultClrTypeMapResource)
        [for row in mapdata.Rows ->
            try
                Type.GetType(row.ClrTypeName, true), row.StorageType |> StorageType.parse |> Option.get
            with
                e ->
                    reraise()
        ] |> dict
                    
    let private defaultClrStorageTypeMap = getDefaultClrStorageTypeMap()

    let private getMemberDescription(m : MemberInfo) =
        if Attribute.IsDefined(m, typeof<DescriptionAttribute>) then
            (Attribute.GetCustomAttribute(m, typeof<DescriptionAttribute>) :?> DescriptionAttribute).Description |> Some
        else
            None        

    let private inferStorageTypeFromClrType (t : Type) =
        if defaultClrStorageTypeMap.ContainsKey(t.ItemValueType) then
            defaultClrStorageTypeMap.[t.ItemValueType]
        else
            NotSupportedException(sprintf "No default mapping exists for the %O type" t.ItemValueType) |> raise

    let private inferStorageTypeFromClrElement(proxyref : ClrElementReference) =
        let valueType =
            match proxyref with
            | MemberReference(x) ->
                match x with
                | DataMemberReference(x) -> x.MemberType
                | _ -> NotSupportedException() |> raise                
            | MethodParameterReference(x) -> 
                x.ParameterType
            | _ -> NotSupportedException() |> raise
        valueType |> inferStorageTypeFromClrType
        
    /// <summary>
    /// Infers a <see cref"StorageType"/> from a CLR element
    /// </summary>
    /// <param name="element"></param>
    let inferStorageType(proxyref : ClrElementReference) =
        match proxyref |> ClrElementReference.getAttribute<StorageTypeAttribute> with
        | Some(attrib) -> 
            attrib.StorageType
        | None ->            
            proxyref |> inferStorageTypeFromClrElement                                             

    /// <summary>
    /// Infers the name of the schema in which the element lives or represents
    /// </summary>
    /// <param name="clrElement">The element from which to infer the schema name</param>
    let inferSchemaName(proxyref : ClrElementReference) =              
        let inferFromDeclaringType() =
                match proxyref.DeclaringType with
                | Some(t) ->
                    match t |> ClrTypeReference.getAttribute<SchemaAttribute> with
                    | Some(a) ->
                        match a.Name with
                        | Some(n) ->
                            n
                        | None ->
                            t.ReferentTypeName.SimpleName
                    | None ->
                        t.ReferentTypeName.SimpleName
                | None ->
                    NotSupportedException() |> raise                    
        
        match proxyref |> ClrElementReference.getAttribute<SchemaAttribute> with        
        | Some(a) ->
            match a.Name with
            | Some(name) -> 
                name
            | None -> 
                inferFromDeclaringType()

        | None ->
            match proxyref |> ClrElementReference.getAttribute<DataObjectAttribute> with
            | Some(a) ->
                match a.SchemaName with
                | Some(schemaName) -> 
                    schemaName
                | None -> 
                    inferFromDeclaringType()
            | None ->                                
                inferFromDeclaringType()

    /// <summary>
    /// Infers a <see cref="DataObjectName"/> from a CLR element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the object name will be inferred</param>
    let inferDataObjectName(proxyref : ClrElementReference) =        
        
        let getElementName() =
            match proxyref |> ClrElementReference.getReferentName with
            | MemberElementName(n) ->n.Text
            | ParameterElementName(n) -> n.Text           
            | TypeElementName(n) -> n.SimpleName                    
            | AssemblyElementName(n) ->
                NotSupportedException() |> raise

        match proxyref |> ClrElementReference.getAttribute<DataObjectAttribute> with
        | Some(a) -> 
            let schemaName = proxyref |> inferSchemaName
            let localName = 
                match a.Name with
                | Some(x) -> 
                    x
                | None ->
                    getElementName()
            DataObjectName(schemaName, localName)
                    
        | None ->
            let localName = getElementName()
            let schemaName = proxyref |> inferSchemaName
            DataObjectName(schemaName, localName)

    
    /// <summary>
    /// Infers a <see cref"ColumnDescription"/>  from a CLR element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the column description will be inferred</param>
    let describeColumn(proxyref : ClrPropertyReference) =
        let storageType = proxyref.ElementReference |> inferStorageType
        match proxyref.Referent |> ClrElement.tryGetAttributeT<ColumnAttribute> with
        | Some(attrib) ->
            {
                ColumnDescription.Name = 
                    match attrib.Name with 
                    | Some(name) -> name
                    | None -> proxyref.ReferentName.Text
                Position = proxyref.ReferentPosition |> defaultArg attrib.Position 
                StorageType = storageType
                Nullable = proxyref.Referent |> ClrElement.asDataMember |> ClrDataMemberElement.getType |> Option.isOptionType
                AutoValue = None
            }

        | None ->
            {
                ColumnDescription.Name = proxyref.ReferentName.Text
                Position = proxyref.ReferentPosition
                StorageType = storageType
                Nullable = proxyref.Referent |> ClrElement.asDataMember |> ClrDataMemberElement.getType |> Option.isOptionType
                AutoValue = None
            }

    let private describeColumnProxy(clrElement : ClrPropertyReference) =
        let column = clrElement |> describeColumn
        ColumnProxyDescription(clrElement, column)

    /// <summary>
    /// Infers a collection of <see cref="ClrColumnProxyDescription"/> instances from a CLR type element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the column descriptions will be inferred</param>
    let  private describeColumnProxies(proxyref : ClrTypeReference) =
        
        let rec getProperties(tref : ClrTypeReference) =
            match tref with
            | RecordTypeReference(subject,fields) ->
                fields
            | InterfaceTypeReference(subject, members) ->
                members |> List.map(fun m -> 
                    match m with
                    | DataMemberReference(m) -> 
                        match m with
                        | PropertyMemberReference(p) -> p   
                        | _ -> nosupport()                     
                    | _ -> nosupport()
                )
            | ClassTypeReference(subject, members) ->
                members |> List.map(fun m -> 
                    match m with
                    | DataMemberReference(m) -> 
                        match m with
                        | PropertyMemberReference(p) -> p   
                        | _ -> nosupport()                     
                    | _ -> nosupport()
                )
            | UnionTypeReference(subject, cases) ->
                if cases.Length = 1 then
                    //Assume that the columns are defined by the case fields
                    cases.[0].Fields
                else
                    //Assume that the columns are defined by the case labels
                    [for case in cases ->
                        if case.Fields.Length <> 1 then
                            NotSupportedException() |> raise
                        case.Fields.[0]
                    ]
            | CollectionTypeReference(subject,itemType,collectionKind) ->
                itemType |> getProperties
            | StructTypeReference(subject, members) ->
                NotSupportedException() |> raise

        let properties = proxyref |> getProperties
        if properties.Length = 0 then
            NotSupportedException(sprintf "No columns were able to be inferred from the type %O" proxyref) |> raise
        properties |> List.map describeColumnProxy

    /// <summary>
    /// Infers a collection of <see cref="ColumnDescription"/> instances from a CLR type element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the column descriptions will be inferred</param>
    let describeColumns (proxyref : ClrTypeReference) =
        proxyref |> describeColumnProxies |> List.map(fun x -> x.DataElement) 

    /// <summary>
    /// Infers a non-return RoutineParameterDescription from a CLR element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the parameter description will be inferred</param>
    let describeRoutineParameter(proxyref  : ClrMethodParameterReference) =
        let name, direction, position = 
            match proxyref.Subject.Element |> ClrElement.tryGetAttributeT<RoutineParameterAttribute> with
            | Some(attrib) ->
                let position =  match attrib.Position with |Some(p) -> p |None -> proxyref.ReferentPosition
                match attrib.Name with
                |Some(name) ->
                    name, attrib.Direction, position
                | None ->
                    proxyref.Subject.Name.Text, attrib.Direction, position                
            | None ->
                (proxyref.Subject.Name.Text, ParameterDirection.Input, proxyref.ReferentPosition)
        {
            RoutineParameterDescription.Name = name
            Position = position
            Direction = direction
            StorageType = proxyref |> MethodParameterReference |> inferStorageType 
        }
    
    
    /// <summary>
    /// Infers a return RoutineParameterDescription from a CLR element
    /// </summary>
    /// <param name="clrElement">The CLR element from which the parameter description will be inferred</param>
    let describeReturnParameter(proxyref : ClrMethodReference) =
        let storageType = match proxyref.MethodInfo |> MethodInfo.getReturnAttribute<StorageTypeAttribute> with
                                | Some(attrib) -> 
                                    attrib.StorageType
                                | None -> 
                                    proxyref.ReturnType |> Option.get |> inferStorageTypeFromClrType
        match proxyref.MethodInfo|> MethodInfo.getReturnAttribute<RoutineParameterAttribute> with            
        |Some(attrib) ->
            let position =  match attrib.Position with |Some(p) -> p |None -> -1
            {
                RoutineParameterDescription.Name = attrib.Name |> Option.get                   
                Direction = ParameterDirection.Output //Must always be output so value in attribute can be ignored
                StorageType = storageType
                Position = position
            }
        | None ->
            //No attribute, so assume stored procedure return value
            {
                RoutineParameterDescription.Name = "Return"
                Position = -1
                Direction = ParameterDirection.ReturnValue
                StorageType = storageType
            }
    

                                                 
    /// <summary>
    /// Constructs a <see cref="ParameterProxyDescription"/> based on the CLR element that will proxy it
    /// </summary>
    /// <param name="clrElement">The field correlated with the proxy</param>
    let private describeParameterProxy (proxyref : MethodInputOutputReference) =
        let parameter, pos =
            match proxyref with
            | MethodInputReference(methparam) ->
                (methparam |> describeRoutineParameter), methparam.ParameterInfo.ParamerInfo.Position
            | MethodOutputReference(methreturn) ->
                methreturn |> describeReturnParameter, -1
        ParameterProxyDescription(proxyref, pos, parameter)

                
    let describeProcedureProxy(proxyref : ClrElementReference) =
        let objectName = proxyref |> inferDataObjectName
        match proxyref with
        | MemberReference(m) -> 
            match m with
            | MethodMemberReference(m) ->
                let parameters = 
                    [for p in m.Parameters do
                        yield p |> MethodInputReference
                 
                     if m.ReturnType |> Option.isSome then
                        yield m |> MethodOutputReference                     
                    ]  |> List.map describeParameterProxy
            
                let procedure = {
                    ProcedureDescription.Name = objectName
                    Parameters = parameters |> List.map(fun p -> p.DataElement)
                }            
                ProcedureCallProxyDescription(m, procedure, parameters) |> ProcedureProxy
            | _ -> NotSupportedException() |> raise

        | _ -> NotSupportedException() |> raise
                                                                                                        
    /// <summary>
    /// Infers a Table Proxy Description
    /// </summary>
    /// <param name="proxyType">The type of proxy</param>
    let describeTablularProxy(proxyref : ClrTypeReference) =
        let objectName = proxyref |> ClrElementReference.fromTypeRef |> inferDataObjectName
        let columnProxies = proxyref |> describeColumnProxies
        let table = {
            TabularDescription.Name = objectName
            Description = proxyref.ReferentType.Type|> getMemberDescription
            Columns = columnProxies |> List.map(fun p -> p.DataElement)
        }
        TablularProxyDescription(proxyref, table, columnProxies) 
    
    let describeTableFunctionProxy(proxyref : ClrElementReference) =
        let objectName = proxyref |> inferDataObjectName
        let parameterProxies, columnProxies, clrMethod, returnType =
            match proxyref with
            | MemberReference(m) -> 
                match m with
                | MethodMemberReference(m) ->
                    m.Parameters |> List.map MethodInputReference |> List.mapi (fun i x ->  x |> describeParameterProxy ), 
                    (match m.ReturnType with
                    | Some(t) ->
                        t |> ClrTypeReference.reference |> describeColumnProxies 
                    | None ->
                        NotSupportedException() |> raise),
                    m,
                    m.ReturnType |> Option.get |> ClrTypeReference.reference
                | _ ->
                    NotSupportedException() |> raise
            | _ ->
                NotSupportedException() |> raise
        let tableFunction = {
            TableFunctionDescription.Name = objectName   
            Parameters = parameterProxies|> List.map(fun p -> p.DataElement)
            Columns = columnProxies |> List.map(fun c -> c.DataElement)
        }
        let callProxy = TableFunctionCallProxyDescription(clrMethod, tableFunction, parameterProxies)
        let resultProxy = TabularResultProxyDescription(returnType, tableFunction, columnProxies)
        TableFunctionProxyDescription(callProxy, resultProxy) |> TableFunctionProxy

    let describeRoutineProxy (proxyref : ClrElementReference) =
        match proxyref with
        | MemberReference(x) ->
                match x with
                | MethodMemberReference(m) ->
                    if m.Referent |> ClrElement.hasAttributeT<ProcedureAttribute> then
                        proxyref |> describeProcedureProxy
                    else if m.Referent |> ClrElement.hasAttributeT<TableFunctionAttribute> then
                        proxyref |> describeTableFunctionProxy
                    else NotSupportedException() |> raise                        
                | _ -> NotSupportedException() |> raise
        | _ -> NotSupportedException() |> raise

    let describeRoutineProxies(proxyref : ClrTypeReference ) =
        match proxyref with
        | InterfaceTypeReference(subject, members) ->
            [for m in members do
                match m with
                | MethodMemberReference(m) ->
                    if m.Referent |> ClrElement.hasAttributeT<ProcedureAttribute> then
                        yield m |> MethodMemberReference |> MemberReference |> describeProcedureProxy
                    else if m.Referent |>ClrElement.hasAttributeT<TableFunctionAttribute> then
                        yield m |> MethodMemberReference |> MemberReference |> describeTableFunctionProxy
                | _ ->
                    NotSupportedException() |> raise
            ]

                
        | _ ->
             NotSupportedException() |> raise           
        
                    
        
        

/// <summary>
/// Convenience methods/operators intended to minimize syntactic clutter
/// </summary>
[<AutoOpen>]
module DataProxyOperators =    
    let tabularproxy<'T> =
        typeref<'T> |> DataProxyMetadata.describeTablularProxy


    let routineproxies<'T> =
        typeref<'T> |> DataProxyMetadata.describeRoutineProxies
            

           

    
               
           
        
                  



        