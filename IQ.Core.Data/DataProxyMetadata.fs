namespace IQ.Core.Data

open System
//open System.ComponentModel
open System.Reflection
open System.Data

open FSharp.Data

open IQ.Core.Framework

type DescriptionAttribute = System.ComponentModel.DescriptionAttribute

/// <summary>
/// Defines operations that populate a Data Proxy Metamodel
/// </summary>
module DataProxyMetadata =    
    [<Literal>]
    let private DefaultClrTypeMapResource = "Resources/DefaultClrStorageTypeMap.csv"
    
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
            | MemberElement(x) ->
                match x with
                | PropertyReference(x) -> x.PropertyType
                | _ -> NotSupportedException() |> raise                
            | MethodParameterReference(x) -> 
                x.ParameterType
            | _ -> NotSupportedException() |> raise
        valueType |> inferStorageTypeFromClrType
        
    let private inferStorageTypeFromAttribute (attrib : StorageTypeAttribute) =
        attrib |> StorageType.fromAttribute

    /// <summary>
    /// Infers a <see cref"StorageType"/> from a CLR element
    /// </summary>
    /// <param name="element"></param>
    let inferStorageType(proxyref : ClrElementReference) =
        match proxyref |> ClrElement.getAttribute<StorageTypeAttribute> with
        | Some(attrib) -> 
            attrib |> inferStorageTypeFromAttribute
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
                            t.Type.Name
                    | None ->
                        t.Type.Name
                | None ->
                    NotSupportedException() |> raise                    
        
        match proxyref |> ClrElement.getAttribute<SchemaAttribute> with        
        | Some(a) ->
            match a.Name with
            | Some(name) -> 
                name
            | None -> 
                inferFromDeclaringType()

        | None ->
            match proxyref |> ClrElement.getAttribute<DataObjectAttribute> with
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
        
        let getInnerName (simpleName) =
            if simpleName |> Txt.containsCharacter '+' then simpleName |> Txt.rightOfLast "+" else simpleName
            

        let getElementName() =
            match proxyref |> ClrElement.getName with
            | MemberElementName(n) | ParameterElementName(n) -> n            
            | TypeElementName(n) ->
                match n with
                | SimpleTypeName(n) -> 
                    n
                | FullTypeName(n) -> 
                    n |> Txt.rightOfLast "."
                      |> getInnerName
                | AssemblyQualifiedTypeName(n) ->
                        n   |> Txt.matchRegexGroups ["TypeName"]  Txt.StockRegularExpressions.AssemblyQualifiedTypeName
                            |> fun m -> m.["TypeName"]
                            |> Txt.rightOfLast "."
                            |> getInnerName
                    
            | AssemblyElementName(n) ->
                NotSupportedException() |> raise

        match proxyref |> ClrElement.getAttribute<DataObjectAttribute> with
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
        match proxyref.Property |> PropertyInfo.getAttribute<ColumnAttribute> with
        | Some(attrib) ->
            {
                ColumnDescription.Name = 
                    match attrib.Name with 
                    | Some(name) -> name
                    | None -> proxyref.Name.Text
                Position = proxyref.Position |> defaultArg attrib.Position 
                StorageType = proxyref |> PropertyReference |> MemberElement |> inferStorageType
                Nullable = proxyref.Property.PropertyType |> ClrOption.isOptionType
                AutoValue = None
            }

        | None ->
            {
                ColumnDescription.Name = proxyref.Name.Text
                Position = proxyref.Position
                StorageType = proxyref |> PropertyReference |> MemberElement |> inferStorageType
                Nullable = proxyref.Property.PropertyType |> ClrOption.isOptionType
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
                    | PropertyReference(p) -> p
                    | _ -> NotSupportedException() |> raise
                )
            | ClassTypeReference(subject, members) ->
                members |> List.map(fun m -> 
                    match m with
                    | PropertyReference(p) -> p
                    | _ -> NotSupportedException() |> raise
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
            match proxyref.Subject.Element |> ParameterInfo.getAttribute<RoutineParameterAttribute> with
            | Some(attrib) ->
                let position =  match attrib.Position with |Some(p) -> p |None -> proxyref.Position
                match attrib.Name with
                |Some(name) ->
                    name, attrib.Direction, position
                | None ->
                    proxyref.Subject.Name.Text, attrib.Direction, position                
            | None ->
                (proxyref.Subject.Name.Text, ParameterDirection.Input, proxyref.Position)
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
    let describeReturnParameter(proxyref : ClrMethodReturnReference) =
        let storageType = match proxyref.Method |> MethodInfo.getReturnAttribute<StorageTypeAttribute> with
                                | Some(attrib) -> 
                                    attrib |> inferStorageTypeFromAttribute
                                | None -> 
                                    proxyref.ReturnType |> Option.get |> inferStorageTypeFromClrType
        match proxyref.Method |> MethodInfo.getReturnAttribute<RoutineParameterAttribute> with            
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
    let private describeParameterProxy  (proxyref : MethodInputOutputReference) =
        let parameter =
            match proxyref with
            | MethodInputReference(methparam) ->
                methparam |> describeRoutineParameter
            | MethodOutputReference(methreturn) ->
                methreturn |> describeReturnParameter
        ParameterProxyDescription(proxyref, parameter)
                
    let describeProcedureProxy(proxyref : ClrElementReference) =
        let objectName = proxyref |> inferDataObjectName
        match proxyref with
        | MemberElement(m) -> 
            match m with
            | MethodReference(m) ->
                let parameters = 
                    [for p in m.Parameters do
                        yield p |> MethodInputReference
                 
                     if m.Return.ReturnType |> Option.isSome then
                        yield m.Return |> MethodOutputReference                     
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
    let describeTableProxy(proxyref : ClrTypeReference) =
        let objectName = proxyref |> ClrElement.fromTypeRef |> inferDataObjectName
        let columnProxies = proxyref |> describeColumnProxies
        let table = {
            TableDescription.Name = objectName
            Description = proxyref.Type |> getMemberDescription
            Columns = columnProxies |> List.map(fun p -> p.DataElement)
        }
        TableProxyDescription(proxyref, table, columnProxies) |> TableProxy
    
    let describeTableFunctionProxy(proxyref : ClrElementReference) =
        let objectName = proxyref |> inferDataObjectName
        let parameterProxies, columnProxies, clrMethod, returnType =
            match proxyref with
            | MemberElement(m) -> 
                match m with
                | MethodReference(m) ->
                    m.Parameters |> List.map MethodInputReference |> List.map describeParameterProxy, 
                    (match m.Return.ReturnType with
                    | Some(t) ->
                        t |> ClrType.reference |> describeColumnProxies 
                    | None ->
                        NotSupportedException() |> raise),
                    m,
                    m.Return.ReturnType |> Option.get |> ClrType.reference
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
        | MemberElement(x) ->
                match x with
                | MethodReference(m) ->
                    if m.Method |> MethodInfo.hasAttribute<ProcedureAttribute> then
                        proxyref |> describeProcedureProxy
                    else if m.Method |> MethodInfo.hasAttribute<TableFunctionAttribute> then
                        proxyref |> describeTableFunctionProxy
                    else NotSupportedException() |> raise                        
                | _ -> NotSupportedException() |> raise
        | _ -> NotSupportedException() |> raise

    let describeRoutineProxies(proxyref : ClrTypeReference ) =
        match proxyref with
        | InterfaceTypeReference(subject, members) ->
            [for m in members do
                match m with
                | MethodReference(m) ->
                    if m.Method |> MethodInfo.hasAttribute<ProcedureAttribute> then
                        yield m |> MethodReference |> MemberElement |> describeProcedureProxy
                    else if m.Method |> MethodInfo.hasAttribute<TableFunctionAttribute> then
                        yield m |> MethodReference |> MemberElement |> describeTableFunctionProxy
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
    let tableproxy<'T> =
        typeref<'T> |> DataProxyMetadata.describeTableProxy


    let routineproxies<'T> =
        typeref<'T> |> DataProxyMetadata.describeRoutineProxies
            

           

    
               
           
        
                  



        