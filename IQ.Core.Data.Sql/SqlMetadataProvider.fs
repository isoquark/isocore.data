namespace IQ.Core.Data.Sql

open System
open System.Linq
open System.Collections.Generic;

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data

open System.Data
open System.Data.SqlClient

type SqlMetadataProviderConfig = {
    ConnectionString : string
    IgnoreSystemObjects : bool
}

type internal IMetadataView =
    abstract IsUserDefined : bool
    abstract Documentation : string 

//These proxies align with (a subset of) the columns returned by the views in the Metadata schema
module internal Metadata =
    
    let private toOptionalString s =
        if s |> System.String.IsNullOrEmpty then None else s |> Some
    
    
    
    type AdoTypeMap() =
        member val SqlTypeName = String.Empty with get, set
        member val BclTypeName = String.Empty with get, set
        member val SqlDbTypeEnum = String.Empty with get, set
    
    type vDataType() = 
        member val DataTypeId = 0 with get, set
        member val DataTypeName = String.Empty with get, set
        member val SchemaName = String.Empty with get, set
        member val Description = String.Empty  with get, set    
        member val MappedBclType = String.Empty with get, set
        member val MappedSqlDbTypeEnum = String.Empty with get, set
        member val MaxLength = 0s with get, set
        member val Precision = 0uy with get, set
        member val Scale = 0uy with get, set
        member val IsNullable = false with get, set
        member val IsTableType = false with get, set
        member val IsAssemblyType = false with get, set
        member val IsUserDefined = false with get, set
        member val BaseTypeId : Nullable<uint8> = Nullable<uint8>() with get, set
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = String.Empty

    type vSchema() =
        member val SchemaName = String.Empty  with get, set     
        member val Description = String.Empty  with get, set     
        member val IsUserDefined = false with get, set
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = this.Description 

    type vColumn() = 
        member val CatalogName = String.Empty  with get, set     
        member val ParentSchemaName = String.Empty  with get, set    
        member val ParentName = String.Empty  with get, set    
        member val ColumnName = String.Empty  with get, set    
        member val Description = String.Empty  with get, set    
        member val Position = 0  with get, set    
        member val IsComputed = false with get, set
        member val IsIdentity = false with get, set
        member val IsUserDefined = false with get, set
        member val IsNullable = false with get, set
        member val DataTypeSchemaName = String.Empty with get,set
        member val DataTypeName = String.Empty with get,set
        member val MaxLength = 0 with get, set
        member val Precision = 0uy with get, set
        member val Scale = 0uy with get, set
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = this.Description
    
    type vTable() = 
        member val SchemaName = String.Empty  with get, set    
        member val TableName = String.Empty  with get, set    
        member val Description = String.Empty  with get, set    
        member val IsUserDefined = false with get, set
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = this.Description

    type vView() = 
        member val SchemaName = String.Empty  with get, set    
        member val ViewName = String.Empty  with get, set    
        member val Description = String.Empty  with get, set    
        member val IsUserDefined = false with get, set
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = this.Description

    let getMetadataView<'T when 'T :> IMetadataView> (config : SqlMetadataProviderConfig) =
        if config.IgnoreSystemObjects then
            SqlProxyReader.selectSome<'T> config.ConnectionString ("IsUserDefined=1")
        else
            SqlProxyReader.selectAll<'T> config.ConnectionString
            
          
    let private getColumns(config : SqlMetadataProviderConfig) =
        [for item in config |> getMetadataView<vColumn> ->
            {
                ColumnDescription.Name = item.ColumnName
                Position = item.Position
                StorageType = DataType.AnsiTextFixedDataType(5)
                Documentation = item.Description
                Nullable = item.IsNullable
                AutoValue = if item.IsComputed then
                                    AutoValueKind.Computed 
                                else if item.IsIdentity then
                                    AutoValueKind.Identity 
                                else 
                                    AutoValueKind.None
                
            
            }
        
        ]
    

open Metadata

type internal SqlMetadataReader(config : SqlMetadataProviderConfig) =
            
    
    let dataTypes = Dictionary<DataObjectName, DataTypeDescription>()        
    let columns = Dictionary<DataObjectName, ColumnDescription ResizeArray>()
    let tables = Dictionary<string, TabularDescription ResizeArray>()
    let views = Dictionary<string, TabularDescription ResizeArray>()
    let schemas = Dictionary<string, SchemaDescription>()

   
    member private this.GetMetadataView<'T when 'T :> IMetadataView>() =
        config |> getMetadataView<'T>

    member private this.IndexDataTypes() =
        let index = (SqlProxyReader.selectAll<vDataType> config.ConnectionString).ToDictionary(fun x -> x.DataTypeId)
        
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
                    this.GetIntrinsicTypeReference(baseTypeName, baseType.MaxLength |> int, baseType.Precision, baseType.Scale)
                match reference with
                | Some x ->
                    CustomPrimitiveDataType(baseTypeName, x)
                | None ->
                    nosupport()
            else
                nosupport()
            
    member private this.IndexColumns() =
        for column in this.GetMetadataView<vColumn>()  do
            let description = {
                ColumnDescription.Name = column.ColumnName
                Position = column.Position
                StorageType = this.GetColumnDataType(column)
                Documentation = column.Description
                Nullable = column.IsNullable
                AutoValue = if column.IsComputed then
                                    AutoValueKind.Computed 
                                else if column.IsIdentity then
                                    AutoValueKind.Identity 
                                else 
                                    AutoValueKind.None
                
            
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
                TabularDescription.Name = tableName
                Documentation = table.Description
                Columns = columns.[tableName] 
            }        
            if tables.ContainsKey(table.SchemaName) then 
                tables.[table.SchemaName].Add(description)
            else
                tables.Add(table.SchemaName, ResizeArray([description]))
            
    member private this.IndexViews() =
        for view in this.GetMetadataView<vView>() do
            let tableName  = DataObjectName(view.SchemaName, view.ViewName)
            let description = {
                TabularDescription.Name = tableName
                Documentation = view.Description
                Columns = columns.[tableName] 
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
                    
                }
            )

    member this.ReadCatalog() =
        this.IndexDataTypes()
        this.IndexSchemas()
        this.IndexColumns()
        this.IndexTables()
        this.IndexViews()
        {
            SqlMetadataCatalog.CatalogName = (SqlConnectionStringBuilder(config.ConnectionString).InitialCatalog)
            Schemas = schemas.Values |> Array.ofSeq :> rolist<_>
        }

    




module SqlMetadataProvider =
    type private Realization(config : SqlMetadataProviderConfig) =
        let reader = SqlMetadataReader(config)
        let catalog = reader.ReadCatalog()
        let tables  = Dictionary<string, TabularDescription>()        
        let views = Dictionary<string, TabularDescription>()
        let procedures = Dictionary<string, ProcedureDescription>()
        let sequences = Dictionary<string, SequenceDescription>()
        let tableFunctions = Dictionary<string, TableFunctionDescription>()
        let dataTypes = Dictionary<string, DataTypeDescription>()
        do
            for schema in catalog.Schemas do
                let schemaName = schema.Name
                for o in schema.Objects do
                    match o with
                    | TableDescription(x) -> tables.Add(schemaName, x)
                    | ViewDescription(x) -> views.Add(schemaName, x)
                    | ProcedureDescription(x) -> procedures.Add(schemaName, x)
                    | DataTypeDescription(x) -> dataTypes.Add(schemaName, x)
                    | SequenceDescription(x) -> sequences.Add(schemaName, x)
                    | TableFunctionDescription(x) -> tableFunctions.Add(schemaName, x)

        
        interface ISqlMetadataProvider with
            member x.DescribeAllTables(): rolist<TabularDescription> = 
                failwith "Not implemented yet"
            
            member x.DescribeAllViews(): rolist<TabularDescription> = 
                failwith "Not implemented yet"
            
            member x.DescribeSchemaTables(arg1: string): rolist<TabularDescription> = 
                failwith "Not implemented yet"
            
            member x.DescribeSchemaViews(arg1: string): TabularDescription = 
                failwith "Not implemented yet"
            
            member x.DescribeSchemas(): rolist<SchemaDescription> = 
                failwith "Not implemented yet"
            
            member this.Describe q = 
                match q with
                | FindTables(q) ->
                    match q  with
                    | FindAllTables ->
                        [for s in catalog.Schemas do
                            for o in s.Objects do
                                match o with
                                | TableDescription(t) -> yield o
                                |_ ->
                                    ()
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