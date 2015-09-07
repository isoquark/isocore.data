namespace IQ.Core.Data.Sql

open System

open IQ.Core.Data.Contracts


/// <summary>
/// A builder that assists in the construction of dynamic SQL
/// </summary>
type DynamicQueryBuilder(schemaName, localName) =
    
    let mutable columnNames : string list = List.empty
    let mutable filters : ColumnFilterCriterion list = List.empty
    let mutable sortCriteria : ColumnSortCriterion list = List.empty
    let mutable pageNumber : int option = None
    let mutable pageSize : int option = None

    new(tabularName : DataObjectName) =
        DynamicQueryBuilder(tabularName.SchemaName, tabularName.LocalName)
    
    
    static member Build(schemaName, tabularName) =
        DynamicQueryBuilder(schemaName, tabularName)

    member this.Columns([<ParamArray>]names : string[]) =
        columnNames <- names |> List.ofSeq
        this

    member this.Columns(names : string seq) =
        columnNames <- names |> List.ofSeq
        this        

    member this.Filter([<ParamArray>]criteria : ColumnFilterCriterion[]) =
        filters <- criteria |> List.ofSeq
        this

    member this.Filter(criteria : ColumnFilterCriterion seq) =
        filters <- criteria |> List.ofSeq
        this
    
    member this.Sort([<ParamArray>]criteria : ColumnSortCriterion[]) =
        sortCriteria  <- criteria |> List.ofSeq
        this

    member this.Sort(criteria : ColumnSortCriterion seq) =
        sortCriteria <- criteria |> List.ofSeq
        this
        

    member this.Page(number : int, size : int) =
        pageNumber <- Some(number)
        pageSize <- Some(size)
        this

    member this.Finish() =
            DynamicQuery(
                            DataObjectName(schemaName, localName), 
                            columnNames,  
                            filters,
                            sortCriteria,
                            pageNumber,
                            pageSize
                        ) |> DynamicStoreQuery

    
    
    static member internal WithDefaults(mdp : ISqlMetadataProvider, q ) =
            match q with 
            | DynamicQuery(tableName, columns, filter, sort, pageNumber, pageSize) ->
                let _columns =   if columns.Length = 0 then
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
                    builder.Page(_pageNumber.Value, _pageSize.Value).Filter(filter).Finish()
                else
                    builder.Filter(filter).Finish()
                       
                                           
                
                

