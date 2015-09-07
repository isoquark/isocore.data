namespace IQ.Core.Data.Sql

open System
open System.Linq
open System.Collections.Generic;

open System.Data
open System.Data.SqlClient


open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data
open IQ.Core.Data.Sql.Behavior


type SqlMetadataProviderConfig = {
    ConnectionString : string
    IgnoreSystemObjects : bool
}

open Metadata

module internal MetadataUtil =
    let getMetadataView<'T when 'T :> IMetadataView> (config : SqlMetadataProviderConfig) =
        if config.IgnoreSystemObjects then
            TypedReader.selectSome<'T> config.ConnectionString ("IsUserDefined=1")
        else
            TypedReader.selectAll<'T> config.ConnectionString
            
          
type internal SqlMetadataReader(config : SqlMetadataProviderConfig) =            
    
    let dataTypes = Dictionary<DataObjectName, DataTypeDescription>()        
    let columns = Dictionary<DataObjectName, ColumnDescription ResizeArray>()
    let tables = Dictionary<string, TableDescription ResizeArray>()
    let views = Dictionary<string, ViewDescription ResizeArray>()
    let schemas = Dictionary<string, SchemaDescription>()
   
    member private this.GetMetadataView<'T when 'T :> IMetadataView>() =
        config |> MetadataUtil.getMetadataView<'T>

    member private this.IndexDataTypes() =
        let index = (TypedReader.selectAll<vDataType> config.ConnectionString).ToDictionary(fun x -> x.DataTypeId)
        
        let getName id =
            let item = index.[id]
            DataObjectName(item.SchemaName, item.DataTypeName)

        for id in index.Keys do
            let item = index.[id]
            let description ={
                DataTypeDescription.Name = getName(id)
                MaxLength = item.MaxLength
                Precision = item.Precision
                Scale = item.Scale
                IsNullable = item.IsNullable
                IsTableType = item.IsTableType
                IsCustomObject = item.IsAssemblyType
                IsUserDefined = item.IsUserDefined
                BaseTypeName = if item.BaseTypeId.HasValue then 
                                    getName(item.BaseTypeId.Value |> int) |> Some 
                                else 
                                None  
                DefaultBclTypeName = item.MappedBclType      
                Properties = []
                Documentation = item.Description
            }  
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
            
    member private this.GetColumnDataType(c : vColumn) =
        let dataTypeName = DataObjectName(c.DataTypeSchemaName, c.DataTypeName)
        let reference = 
            this.GetIntrinsicTypeReference(dataTypeName, c.MaxLength, c.Precision, c.Scale)
        match reference with
        | Some x  -> x
        | None ->            
            let dataType = dataTypes.[dataTypeName]
            if dataType.IsUserDefined then
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
            
    member private this.IndexColumns() =
        for column in this.GetMetadataView<vColumn>()  do
            let description = {
                ColumnDescription.Name = column.ColumnName
                ParentName = DataObjectName(column.ParentSchemaName, column.ParentName)
                Position = column.Position
                DataType = this.GetColumnDataType(column)
                Documentation = column.Description
                Nullable = column.IsNullable
                AutoValue = if column.IsComputed then
                                    AutoValueKind.Computed 
                                else if column.IsIdentity then
                                    AutoValueKind.Identity 
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

    member private this.IndexSchemas() =

        for schema in this.GetMetadataView<vSchema>() do
            
            let allObjects = ResizeArray<DataObjectDescription>()
            if tables.ContainsKey(schema.SchemaName) then
                tables.[schema.SchemaName] |> Seq.map(TableDescription) |> allObjects.AddRange
            if views.ContainsKey(schema.SchemaName) then
                views.[schema.SchemaName] |> Seq.map(ViewDescription) |> allObjects.AddRange
            
            schemas.Add(schema.SchemaName,
                {
                   SchemaDescription.Name = schema.SchemaName
                   Objects =  allObjects |> List.ofSeq
                   Documentation = schema.Description
                   Properties = []
                }
            )

    member this.ReadCatalog() =
        this.IndexDataTypes()
        this.IndexColumns()
        this.IndexTables()
        this.IndexViews()
        this.IndexSchemas()
        {
            SqlMetadataCatalog.CatalogName = (SqlConnectionStringBuilder(config.ConnectionString).InitialCatalog)
            Schemas = schemas.Values |> Array.ofSeq :> rolist<_>
        }

    



module SqlMetadataProvider =
    type private Realization(config : SqlMetadataProviderConfig) =
        let tables  = Dictionary<string, ResizeArray<TableDescription>>()        
        let views = Dictionary<string, ResizeArray<ViewDescription>>()
        let procedures = Dictionary<string, ProcedureDescription>()
        let sequences = Dictionary<string, SequenceDescription>()
        let tableFunctions = Dictionary<string, TableFunctionDescription>()
        let dataTypes = Dictionary<string, DataTypeDescription>()
        let allObjects = Dictionary<DataObjectName, IDataObjectDescription>()
        
        let refresh() =
            tables.Clear()
            views.Clear()
            procedures.Clear()
            sequences.Clear()
            tableFunctions.Clear()
            dataTypes.Clear()
            allObjects.Clear()        
            let reader = SqlMetadataReader(config)
            let catalog = reader.ReadCatalog()
            for schema in catalog.Schemas do
                let schemaName = schema.Name
                for o in schema.Objects do
                    allObjects.Add(o.Name, o)
                    match o with
                    | TableDescription(x) -> 
                        if tables.ContainsKey(schemaName) then
                            tables.[schemaName].Add(x)
                        else
                            tables.[schemaName] <- ResizeArray[x]
                    | ViewDescription(x) -> 
                        if views.ContainsKey(schemaName) then
                            views.[schemaName].Add(x)
                        else
                            views.[schemaName] <- ResizeArray[x]
                    | ProcedureDescription(x) -> 
                        procedures.Add(schemaName, x)
                    | DataTypeDescription(x) ->
                         dataTypes.Add(schemaName, x)
                    | SequenceDescription(x) -> 
                        sequences.Add(schemaName, x)
                    | TableFunctionDescription(x) -> 
                        tableFunctions.Add(schemaName, x)

        do
            refresh()
        
        interface ISqlMetadataProvider with
            member x.RefreshCache() =
                refresh()
            member x.DescribeTables(): rolist<TableDescription> = 
                [for t in tables.Values do yield! t] |> RoList.ofSeq
            
            member x.DescribeViews(): rolist<ViewDescription> = 
                [for t in views.Values do yield! t] |> RoList.ofSeq
            
            member x.DescribeTablesInSchema(schemaName: string): rolist<TableDescription> = 
                tables.[schemaName] |> RoList.ofSeq
            
            member x.DescribeViewsInSchema(schemaName: string) = 
                views.[schemaName] |> RoList.ofSeq
            
            member x.DescribeSchemas(): rolist<SchemaDescription> = 
                failwith "Not implemented yet"
            
            member x.DescribeTable(tableName) =
                tables.[tableName.SchemaName]  |> Seq.find(fun x -> x.Name = tableName)

            member x.DescribeView(viewName) =
                views.[viewName.SchemaName] |> Seq.find(fun x -> x.Name = viewName)

            member x.ObjectExists(objectName) =
                allObjects.ContainsKey(objectName)

            member x.GetObjectKind(objectName) =
                if (allObjects.ContainsKey(objectName)) then
                    Nullable<DataElementKind>(allObjects.[objectName].ElementKind)
                else
                    Nullable<DataElementKind>();
                
            member this.Describe q = 
                match q with
                | FindTables(q) ->
                    match q  with
                    | FindAllTables ->
                        [
                            for tableList in tables.Values do
                                for t in tableList do
                                    yield t |> TableDescription
                               
                        ]
                    | FindUserTables ->
                        nosupport()
                    | FindTablesBySchema(schemaName) ->
                        nosupport()
                | FindSchemas(q) -> 
                    match q with
                    | FindAllSchemas ->
                        nosupport() 
                | FindViews(q) -> 
                    match q with
                    | FindAllViews -> 
                        nosupport()
                    | FindUserViews ->
                        nosupport()
                    | FindViewsBySchema(schemaName) ->
                        nosupport()
                | FindProcedures(q) -> 
                    match q with
                    | FindAllProcedures -> 
                        nosupport()
                    | FindProceduresBySchema(schemaName) ->
                        nosupport()
                | FindSequences(q) -> 
                    match q with
                    | FindAllSequences -> 
                        nosupport()
                    | FindSequencesBySchema(schemaName) ->
                        nosupport()

        
    let get(config : SqlMetadataProviderConfig) =
        config |> Realization :> ISqlMetadataProvider

