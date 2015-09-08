namespace IQ.Core.Data.Sql

open System

open IQ.Core.Data.Contracts


/// <summary>
/// A builder that provides a fluent API for constructing DynamicQuery values
/// </summary>
type DynamicQueryBuilder(schemaName, localName) =
    
    let columnNames : string ResizeArray = ResizeArray<string>()
    let filters : ColumnFilterCriterion ResizeArray = ResizeArray<ColumnFilterCriterion>()
    let sortCriteria : ColumnSortCriterion ResizeArray = ResizeArray<ColumnSortCriterion>()
    let parameters : QueryParameter ResizeArray = ResizeArray<QueryParameter>()
    let mutable pageNumber : int option = None
    let mutable pageSize : int option = None

    new(tabularName : DataObjectName) =
        DynamicQueryBuilder(tabularName.SchemaName, tabularName.LocalName)
    
    new(tableName : string) =
        let components = tableName.Split('.')
        let objectName =
            DataObjectName( components.[0].Replace("]", String.Empty).Replace("[", String.Empty),
                            components.[1].Replace("]", String.Empty).Replace("[", String.Empty))
        DynamicQueryBuilder(objectName)
    
    static member Build(schemaName, tabularName) =
        DynamicQueryBuilder(schemaName, tabularName)

    member this.Columns([<ParamArray>]names : string[]) =
        columnNames.AddRange(names)
        this

    member this.Columns(names : string seq) =
        columnNames.AddRange(names)
        this        

    member this.Parameter(name, value) =
        parameters.Add(QueryParameter(name,value))
    
    member this.Sort([<ParamArray>]criteria : ColumnSortCriterion[]) =
        sortCriteria.AddRange(criteria)
        this
    
    member this.Sort(criteria : ColumnSortCriterion seq) =
        sortCriteria.AddRange(criteria)
        this

    member this.AscendingSort(colname) =
        sortCriteria.Add(AscendingSort(colname))
        this

    member this.DescendingSort(colname) =
        sortCriteria.Add(AscendingSort(colname))
        this
            
    member this.Page(number : int, size : int) =
        pageNumber <- Some(number)
        pageSize <- Some(size)
        this

    member this.Page(number : Nullable<int>, size : Nullable<int>) =
        pageNumber <- if number.HasValue then number.Value |> Some else None
        pageSize <- if size.HasValue then size.Value |> Some else None

    member this.Filter([<ParamArray>]criteria : ColumnFilterCriterion[]) =
        filters.AddRange(criteria)
        this

    member this.Filter(criteria : ColumnFilterCriterion seq) =
        filters.AddRange(criteria)
        this

    member this.AndEqual(colname, value) = 
        filters.Add(AndFilter(Equal(colname,value)))
        this
    
    member this.OrEqual(colname, value) =
        filters.Add(OrFilter(Equal(colname,value)))
        value
    
    member this.AndNotEqual(colname, value) = 
        filters.Add(AndFilter(NotEqual(colname,value)))
        this
    
    member this.OrNotEqual(colname, value) =
        filters.Add(OrFilter(NotEqual(colname,value)))
        value

    member this.AndGreaterThan(colname, value) = 
        filters.Add(AndFilter(GreaterThan(colname,value)))
        this
    
    member this.OrGreaterThan(colname, value) =
        filters.Add(OrFilter(GreaterThan(colname,value)))
        value

    member this.AndGreaterThanOrEqual(colname, value) = 
        filters.Add(AndFilter(GreaterThanOrEqual(colname,value)))
        this
    
    member this.OrGreaterThanOrEqual(colname, value) =
        filters.Add(OrFilter(GreaterThanOrEqual(colname,value)))
        value

    member this.AndLessThan(colname, value) = 
        filters.Add(AndFilter(LessThan(colname,value)))
        this
    
    member this.OrLessThan(colname, value) =
        filters.Add(OrFilter(LessThan(colname,value)))
        value

    member this.AndLessThanOrEqual(colname, value) = 
        filters.Add(AndFilter(LessThanOrEqual(colname,value)))
        this
    
    member this.OrLessThanOrEqual(colname, value) =
        filters.Add(OrFilter(LessThanOrEqual(colname,value)))
        value

    member this.AndStartsWith(colname, value) = 
        filters.Add(AndFilter(StartsWith(colname,value)))
        this
    
    member this.OrStartsWith(colname, value) =
        filters.Add(OrFilter(StartsWith(colname,value)))
        value

    member this.AndContains(colname, value) = 
        filters.Add(AndFilter(Contains(colname,value)))
        this
    
    member this.OrContains(colname, value) =
        filters.Add(OrFilter(Contains(colname,value)))
        value

    member this.AndEndsWith(colname, value) = 
        filters.Add(AndFilter(EndsWith(colname,value)))
        this
    
    member this.OrEndsWith(colname, value) =
        filters.Add(OrFilter(EndsWith(colname,value)))
        value
    
    member this.Build() =
            DynamicQuery( DataObjectName(schemaName, localName), 
                          columnNames |> List.ofSeq,  
                          filters |> List.ofSeq,
                          sortCriteria |> List.ofSeq,
                          parameters |> List.ofSeq,
                          pageNumber,
                          pageSize
                        ) |> DynamicStoreQuery
    
    static member internal WithDefaults(mdp : ISqlMetadataProvider, q) =
            match q with 
            | DynamicQuery(tableName, columns, filter, sort, parameters, pageNumber, pageSize) ->
                let _columns = 
                  if columns.Length = 0 then
                        let kind = tableName |> mdp.GetObjectKind
                        if kind.HasValue |> not then
                            ArgumentException("Database object doesn't exist") |> raise
                        else
                            match kind.Value with
                            | DataElementKind.Table -> mdp.DescribeTable(tableName).Columns 
                            | DataElementKind.View -> mdp.DescribeView(tableName).Columns 
                            | _ ->
                                nosupport()
                            |> List.map(fun x -> x.Name)
                  else
                     columns
                
                let _sort = if sort.Length = 0 then
                                _columns.[0] |> AscendingSort |> List.singleton
                            else
                                sort

                let _pageNumber, _pageSize = 
                    match pageNumber with
                    | None -> None, None
                    | Some(x) -> x |> Some, (defaultArg pageSize 50) |> Some
                                                    

                let builder = DynamicQueryBuilder(tableName).Columns(_columns).Sort(_sort)
                
                if _pageNumber |> Option.isSome then
                    builder.Page(_pageNumber.Value, _pageSize.Value).Filter(filter).Build()
                else
                    builder.Filter(filter).Build()
                       
                                           
                
                

