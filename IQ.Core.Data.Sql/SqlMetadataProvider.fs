﻿namespace IQ.Core.Data

open System
open System.Linq
open System.Collections.Generic;
open System.Collections.Concurrent;

open System.Data
open System.Data.SqlClient


open IQ.Core.Contracts
open IQ.Core.Framework


type SqlMetadataProviderConfig = {
    ConnectionString : string
    IgnoreSystemObjects : bool
}

open Metadata

module internal MetadataUtil =
    let getMetadataView<'T when 'T :> IMetadataView> (config : SqlMetadataProviderConfig) =
        if config.IgnoreSystemObjects then
            SqlDataReader.selectSome<'T> config.ConnectionString ("IsUserDefined=1")
        else
            SqlDataReader.selectAll<'T> config.ConnectionString
            

[<AutoOpen>]
module internal MetadataExensions =
    type vColumn 
    with
        member this.ParentObjectName = DataObjectName(this.ParentSchemaName, this.ParentName)
    
    type vProcedureParameter
    with
        member this.ParentObjectName = DataObjectName(this.ParentSchemaName, this.ProcedureName)
          

type internal SqlMetadataReader(config : SqlMetadataProviderConfig) as self =            
    
    let dataTypes = Dictionary<DataObjectName, DataTypeDescription>()        
    let columns = Dictionary<DataObjectName, ColumnDescription ResizeArray>()
    let tables = Dictionary<string, TableDescription ResizeArray>()
    let views = Dictionary<string, ViewDescription ResizeArray>()
    let schemas = Dictionary<string, SchemaDescription>()
    let procs = Dictionary<string, RoutineDescription ResizeArray>()
    let sequences = Dictionary<string, SequenceDescription ResizeArray>()
    let allObjects = Dictionary<DataObjectName, DataObjectDescription>()
    let mutable catalog = Unchecked.defaultof<SqlMetadataCatalog>

    let unbracket name =
        name |> Txt.removeChar '[' |> Txt.removeChar ']'
    
    let getObjectKind objectName =
        if (allObjects.ContainsKey(objectName)) then
            Nullable<DataElementKind>(allObjects.[objectName].ElementKind)
        else
            Nullable<DataElementKind>();                    
    
    
    
    do
        self.Refresh()
    
           
   
    member private this.GetMetadataView<'T when 'T :> IMetadataView>() =
        config |> MetadataUtil.getMetadataView<'T>

    member private this.ClearIndex() =
        dataTypes.Clear()
        columns.Clear()
        tables.Clear()
        views.Clear()
        schemas.Clear()
        procs.Clear()
        allObjects.Clear()

    member private this.IndexAllObjects() =
        [for x in tables.Values do yield! x] |> List.iter(fun x -> allObjects.Add(x.Name, x |> TableDescription))
        [for x in views.Values do yield! x] |> List.iter(fun x -> allObjects.Add(x.Name, x |> ViewDescription))
        [for x in procs.Values do yield! x] |> List.iter(fun x -> allObjects.Add(x.Name, x |> RoutineDescription))
        [for x in sequences.Values do yield! x] |> List.iter(fun x -> allObjects.Add(x.Name, x |> SequenceDescription))
        dataTypes.Values |> Seq.iter(fun x -> allObjects.Add(x.Name, x |> DataTypeDescription))

    member private this.CreateTypeDescription(item : vDataType, columns) =
            {
                DataTypeDescription.Name = DataObjectName(item.SchemaName, item.DataTypeName)
                MaxLength = item.MaxLength
                Precision = item.Precision
                Scale = item.Scale
                IsNullable = item.IsNullable
                IsTableType = item.IsTableType
                IsCustomObject = item.IsAssemblyType
                IsUserDefined = item.IsUserDefined
                BaseTypeName = if item.IsUserDefined && not(item.IsTableType) then
                                    DataObjectName(item.BaseSchemaName, item.BaseTypeName) |> Some
                               else
                                    None
                DefaultBclTypeName = item.MappedBclType      
                Properties = []
                Documentation = item.Description
                Columns = columns
           }
        
    
    member private this.IndexSimpleDataTypes() =
        let items = (SqlDataReader.selectAll<vDataType> config.ConnectionString)
                        .Where(fun x -> x.IsTableType = false)
        for dataType in items do
            let description = this.CreateTypeDescription(dataType, [])
            dataTypes.Add(description.Name, description)   

    member private this.IndexComplexDataTypes() =
        let items = (SqlDataReader.selectAll<vDataType> config.ConnectionString)
                        .Where(fun x -> x.IsTableType = true)
                        .ToList()
        
        for dataType in items do
            let description = this.CreateTypeDescription(dataType, columns.[DataObjectName(dataType.SchemaName, dataType.DataTypeName)] |> List.ofSeq)            
            dataTypes.Add(description.Name, description)
               
            
    member private this.GetIntrinsicTypeReference(dataTypeName : DataObjectName, maxlen, precision, scale) =
       match dataTypeName.LocalName with
        | SqlDataTypeNames.bigint ->
            Int64DataType |> Some
        | SqlDataTypeNames.binary ->
            BinaryFixedDataType(maxlen) |> Some
        | SqlDataTypeNames.bit ->
            BitDataType |> Some
        | SqlDataTypeNames.char ->
            AnsiTextFixedDataType(maxlen)  |> Some
        | SqlDataTypeNames.date ->
            DateDataType |> Some
        | SqlDataTypeNames.datetime | SqlDataTypeNames.datetime2 | SqlDataTypeNames.smalldatetime ->
            //See http://blogs.msdn.com/b/cdnsoldevs/archive/2011/06/22/why-you-should-never-use-datetime-again.aspx
            //which notes that the the scale of the classic datetime type is 3; however, metadata correctly reports
            //this as 3 so there is no need to hard-code anything
            DateTimeDataType(precision, scale) |> Some       
        | SqlDataTypeNames.datetimeoffset ->
            DateTimeOffsetDataType |> Some
        | SqlDataTypeNames.decimal ->
            DecimalDataType(precision, scale) |> Some
        | SqlDataTypeNames.float ->
            Float64DataType |> Some
        | SqlDataTypeNames.geography|SqlDataTypeNames.geometry|SqlDataTypeNames.hierarchyid  ->
            ObjectDataType(dataTypeName, dataTypes.[dataTypeName].DefaultBclTypeName) |> Some
        | SqlDataTypeNames.image ->
            BinaryMaxDataType |> Some
        | SqlDataTypeNames.int ->
            Int32DataType |> Some
        | SqlDataTypeNames.money | SqlDataTypeNames.smallmoney->
            MoneyDataType(precision, scale) |> Some
        | SqlDataTypeNames.nchar ->
            UnicodeTextFixedDataType(maxlen) |> Some
        | SqlDataTypeNames.ntext ->
            UnicodeTextMaxDataType |> Some
        | SqlDataTypeNames.numeric ->
            DecimalDataType(precision, scale) |> Some
        | SqlDataTypeNames.nvarchar ->
            match maxlen with
            | -1 -> UnicodeTextMaxDataType 
            | _ -> UnicodeTextVariableDataType(maxlen)
            |> Some
        | SqlDataTypeNames.varchar ->
            match maxlen with
            | -1 -> AnsiTextMaxDataType 
            | _ -> AnsiTextVariableDataType(maxlen)
            |> Some
        | SqlDataTypeNames.real ->
            Float32DataType|> Some
        | SqlDataTypeNames.smallint ->
            Int16DataType|> Some
        | SqlDataTypeNames.sql_variant ->
            VariantDataType|> Some
        | SqlDataTypeNames.sysname ->
            //This should be 128; metadata reports as 256
            UnicodeTextVariableDataType(128) |> Some
        | SqlDataTypeNames.text ->
            AnsiTextMaxDataType |> Some
        | SqlDataTypeNames.time ->
            TimeOfDayDataType(precision, scale) |> Some
        | SqlDataTypeNames.timestamp | SqlDataTypeNames.rowversion ->
            RowversionDataType |> Some
        | SqlDataTypeNames.tinyint ->
            UInt8DataType |> Some
        | SqlDataTypeNames.uniqueidentifier ->
            GuidDataType |> Some
        | SqlDataTypeNames.varbinary ->
            BinaryVariableDataType(maxlen) |> Some
        | SqlDataTypeNames.xml ->
            XmlDataType("TODO") |> Some
        | _ ->
            None
            
    member private this.GetDataTypeReference (dataTypeName, maxLength, precision, scale) =
        let reference = 
            this.GetIntrinsicTypeReference(dataTypeName, maxLength, precision, scale)
        match reference with
        | Some x  -> x
        | None ->            
            let dataType = dataTypes.[dataTypeName]
            if dataType.IsUserDefined then
                if dataType.IsTableType then
                    TableDataType(dataTypeName)
                else
                    let baseTypeName = 
                        if dataType.BaseTypeName |> Option.isNone then
                            nosupport()
                        else
                            dataType.BaseTypeName |> Option.get
                
                    let baseType = dataTypes.[baseTypeName]
                    let reference = 
                        this.GetIntrinsicTypeReference(baseTypeName, dataType.MaxLength |> int, dataType.Precision, dataType.Scale)
                    match reference with
                    | Some x ->
                        CustomPrimitiveDataType(dataTypeName, x)
                    | None ->
                        nosupport()
            else
                nosupport()

    member private this.IndexSequences() =
        ()
//        let items = this.GetMetadataView<vSequence>()
//        for item in items do
//            let dataType = this.GetDataTypeReference(DataObjectName("sys", item.DataTypeName), 0, 0uy, 0uy)
//            let description = {
//                SequenceDescription.CacheSize = item.CacheSize
//                Name = DataObjectName(item.SchemaName, item.SequenceName)
//                Documentation = item.Description
//            }
                
    member private this.IndexColumns() =
                
        let items = this.GetMetadataView<vColumn>()
        for column in items do            
            let dataType = this.GetDataTypeReference(DataObjectName(column.DataTypeSchemaName, column.DataTypeName), column.MaxLength, column.Precision, column.Scale)
            let description = {
                ColumnDescription.Name = column.ColumnName
                ParentName = DataObjectName(column.ParentSchemaName, column.ParentName)
                Position = column.Position
                DataType = dataType
                DataKind = dataType |> DataProxyMetadata.getKind
                Documentation = column.Description
                Nullable = column.IsNullable
                AutoValue = if column.IsComputed then
                                    AutoValueKind.Computed 
                                else if column.IsIdentity then
                                    AutoValueKind.AutoIncrement 
                                else 
                                    AutoValueKind.None
                Properties = []
            
            }
            let parentName = DataObjectName(column.ParentSchemaName, column.ParentName)
            if columns.ContainsKey(parentName) then 
                columns.[parentName].Add(description)
            else
                columns.Add(parentName, ResizeArray([description]))
                                 

    member private this.IndexTables() =
        for table in this.GetMetadataView<vTable>() do
            let tableName  = DataObjectName(table.SchemaName, table.TableName)
            let description = {
                TableDescription.Name = tableName
                Documentation = table.Description
                Columns = columns.[tableName] |> List.ofSeq
                IsFileTable = table.IsFileTable
                Properties = []
            }        
            if tables.ContainsKey(table.SchemaName) then 
                tables.[table.SchemaName].Add(description)
            else
                tables.Add(table.SchemaName, ResizeArray([description]))
            
    member private this.IndexViews() =
        for view in this.GetMetadataView<vView>() do
            let tableName  = DataObjectName(view.SchemaName, view.ViewName)
            let description = {
                ViewDescription.Name = tableName
                Documentation = view.Description
                Columns = columns.[tableName] |> List.ofSeq
                Properties = []
            }        
            if views.ContainsKey(view.SchemaName) then 
                views.[view.SchemaName].Add(description)
            else
                views.Add(view.SchemaName, ResizeArray([description]))

    member private this.IndexProcedures() =
        let parameters = this.GetMetadataView<vProcedureParameter>().GroupBy(fun p -> p.ParentObjectName).ToDictionary(fun x -> x.Key)  
        
        let getParametersByParent(parentName)       =
            if( parameters.ContainsKey(parentName) ) then
                [for parameter in parameters.[parentName] -> 
                    {
                        RoutineParameterDescription.Name = parameter.ParameterName
                        RoutineParameterDescription.DataType = 
                            this.GetDataTypeReference(DataObjectName(parameter.DataTypeSchemaName, parameter.DataTypeName), parameter.MaxLength, parameter.Precision, parameter.Scale)
                        Position = parameter.Position
                        Documentation = parameter.Description
                        Direction = if parameter.IsOutput then RoutineParameterDirection.Output else RoutineParameterDirection.Input
                        Properties = []                    
                    }                                
                ]
            else
                []
        for proc in this.GetMetadataView<vProcedure>() do
            let procName = DataObjectName(proc.SchemaName, proc.ProcedureName)
            let description = 
                {
                    RoutineDescription.Name = procName
                    RoutineDescription.RoutineKind = DataElementKind.Procedure
                    Documentation = proc.Description
                    Parameters = getParametersByParent(procName)
                    RoutineDescription.Properties = []
                    RoutineDescription.Columns = []                            
                }
            if(procs.ContainsKey(proc.SchemaName)) then
                procs.[proc.SchemaName].Add(description)
            else
                procs.Add(proc.SchemaName, ResizeArray([description]))
           
    member private this.IndexSchemas() =

        for schema in this.GetMetadataView<vSchema>() do            
            let allObjects = ResizeArray<DataObjectDescription>()
            if tables.ContainsKey(schema.SchemaName) then
                tables.[schema.SchemaName] |> Seq.map(TableDescription) |> allObjects.AddRange
            if views.ContainsKey(schema.SchemaName) then
                views.[schema.SchemaName] |> Seq.map(ViewDescription) |> allObjects.AddRange
            if procs.ContainsKey(schema.SchemaName) then
                procs.[schema.SchemaName] |> Seq.map(RoutineDescription) |> allObjects.AddRange
                            
            let schemaDataTypes = 
                dataTypes.Values |> Seq.filter(fun x -> x.Name.SchemaName = schema.SchemaName) 
                                 |> Seq.map(DataTypeDescription) 

            schemaDataTypes |> allObjects.AddRange

            schemas.Add(schema.SchemaName,
                {
                   SchemaDescription.Name = schema.SchemaName
                   Objects =  allObjects |> List.ofSeq
                   Documentation = schema.Description
                   Properties = []
                }
            )

    
    member private this.Refresh() =
        this.ClearIndex()

        this.IndexSimpleDataTypes()
        this.IndexSequences()
        this.IndexColumns()
        this.IndexComplexDataTypes()
        this.IndexTables()
        this.IndexViews()
        this.IndexProcedures()                    
        this.IndexSchemas()
        this.IndexAllObjects()
        
        catalog <-         
            {
                SqlMetadataCatalog.CatalogName = (SqlConnectionStringBuilder(config.ConnectionString).InitialCatalog)
                Schemas = schemas.Values |> List.ofSeq
            }


    member private this.ReadCatalog() =
        catalog

    member private this.DescribeTable(tableName : DataObjectName) =
        tables.[tableName.SchemaName] |> Seq.find(fun x -> x.Name = tableName)

    member private this.DescribeView(viewName : DataObjectName) =
        views.[viewName.SchemaName] |> Seq.find(fun x -> x.Name = viewName)

    member private this.DescribeTables() =
        [for t in tables.Values do yield! t] 

    member private this.DescribeViews() = 
        [for t in views.Values do yield! t] 

    member private this.DescribeProcedures() =
        [for p in procs.Values do yield! p]

    member private this.DescribeTablesInSchema(schemaName: string) = 
        tables.[schemaName |> unbracket] |> List.ofSeq

    member private this.DescribeViewsInSchema(schemaName: string) = 
        views.[schemaName |> unbracket] |> List.ofSeq

    member private this.DescribeProceduresInSchema(schemaName: string) = 
        procs.[schemaName |> unbracket] |> List.ofSeq

    member private this.DescribeDataTypes() =
        dataTypes.Values |> List.ofSeq
    
    member private this.DescribeDataType(name : DataObjectName) =
        dataTypes.[name]

    member private this.DescribeDataTypesInSchema(schemaName : string) =
        dataTypes.Values |> Seq.filter(fun x -> x.Name.SchemaName = schemaName) |> List.ofSeq


    interface ISqlMetadataProvider with
        member this.RefreshCache() = this.Refresh()
        member this.DescribeTables() =  this.DescribeTables()                        
        member this.DescribeViews() =  this.DescribeViews()            
        member this.DescribeTablesInSchema(schemaName: string) = schemaName |> this.DescribeTablesInSchema            
        member this.DescribeViewsInSchema(schemaName: string) = schemaName |> this.DescribeViewsInSchema            
        member this.DescribeSchemas() = schemas.Values |> List.ofSeq            
        member this.DescribeTable(tableName) = tableName |> this.DescribeTable
        member this.DescribeView(viewName) = viewName |> this.DescribeView   
        member this.DescribeDataType(name) = name |> this.DescribeDataType
        member this.DescribeDataTypes() = this.DescribeDataTypes()  
        member this.DescribeDataTypesInSchema(schemaName : string) = schemaName |> this.DescribeDataTypesInSchema       

        member this.DescribeDataMatrix(objectName) =
            let kind = objectName |> getObjectKind 

            if kind.HasValue then
                match kind.Value with
                | DataElementKind.Table -> 
                    let dtable = objectName |> this.DescribeTable 
                    DataMatrixDescription(dtable.Name, dtable.Columns)
                | DataElementKind.View -> 
                    let dview = objectName |> this.DescribeView
                    DataMatrixDescription(dview.Name, dview.Columns)
                | _ -> nosupport()
            else
                nosupport()
                    
            
        member this.ObjectExists(objectName) =
            allObjects.ContainsKey(objectName)

        member this.GetObjectKind(objectName) =
            objectName |> getObjectKind
                
        member this.Describe q = 
            match q with
            | FindTables(q) ->
                match q  with
                | FindAllTables ->
                    this.DescribeTables() |> List.map(TableDescription)
                | FindUserTables ->
                    nosupport()
                | FindTablesBySchema(schemaName) ->
                    this.DescribeTablesInSchema(schemaName) |> List.map(TableDescription)
            | FindViews(q) -> 
                match q with
                | FindAllViews -> 
                    this.DescribeViews() |> List.map(ViewDescription)
                | FindUserViews ->
                    nosupport()
                | FindViewsBySchema(schemaName) ->
                    this.DescribeViewsInSchema(schemaName) |> List.map(ViewDescription)
            | FindProcedures(q) -> 
                match q with
                | FindAllProcedures -> 
                    this.DescribeProcedures() |> List.map(RoutineDescription)
                | FindProceduresBySchema(schemaName) ->
                    this.DescribeProceduresInSchema(schemaName) |> List.map(RoutineDescription)
            | FindSequences(q) -> 
                match q with
                | FindAllSequences -> 
                    nosupport()
                | FindSequencesBySchema(schemaName) ->
                    nosupport()
        
    
module SqlMetadataProvider =
    let private providers = ConcurrentDictionary<SqlMetadataProviderConfig, ISqlMetadataProvider>()
        
    let get(config : SqlMetadataProviderConfig) =
        providers.GetOrAdd(config, fun config -> config |> SqlMetadataReader :> ISqlMetadataProvider)

