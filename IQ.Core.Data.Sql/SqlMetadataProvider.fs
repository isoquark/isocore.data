namespace IQ.Core.Data.Sql

open System

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data

open System.Data
open System.Data.SqlClient

type internal SqlMetadataReaderConfig = {
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
        member val BaseTypeInt : Nullable<uint8> = Nullable<uint8>() with get, set
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
        member val SchemaName = String.Empty  with get, set    
        member val TableName = String.Empty  with get, set    
        member val ColumnName = String.Empty  with get, set    
        member val Description = String.Empty  with get, set    
        member val Position = 0  with get, set    
        member val IsComputed = false with get, set
        member val IsIdentity = false with get, set
        member val IsUserDefined = false with get, set
        member val IsNullable = false with get, set
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
        member val TableName = String.Empty  with get, set    
        member val Description = String.Empty  with get, set    
        member val IsUserDefined = false with get, set
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = this.Description

    let getMetadataView<'T when 'T :> IMetadataView> (config : SqlMetadataReaderConfig) =
        if config.IgnoreSystemObjects then
            SqlProxyReader.selectSome<'T> config.ConnectionString ("IsUserDefined=1")
        else
            SqlProxyReader.selectAll<'T> config.ConnectionString


    let private getColumnDataType(c : vColumn) =
       match c.DataTypeName with
        | SqlDataTypeNames.bigint ->
            Int64DataType
        | SqlDataTypeNames.binary ->
            BinaryFixedDataType(c.MaxLength)
        | SqlDataTypeNames.bit ->
            BitDataType
        | SqlDataTypeNames.char ->
            AnsiTextFixedDataType(c.MaxLength)
        | SqlDataTypeNames.date ->
            DateDataType
        | SqlDataTypeNames.datetime ->
            //See http://blogs.msdn.com/b/cdnsoldevs/archive/2011/06/22/why-you-should-never-use-datetime-again.aspx
            //which notes that the the scale of the classic datetime type is 3; however, metadata correctly reports
            //this as 3 so there is no need to hard-code anything
            DateTimeDataType(c.Scale)
        | SqlDataTypeNames.datetime2 ->
            DateTimeDataType(c.Scale)
        | SqlDataTypeNames.datetimeoffset ->
            DateTimeOffsetDataType
        | SqlDataTypeNames.decimal ->
            DecimalDataType(c.Precision, c.Scale)
        | SqlDataTypeNames.float ->
            Float64DataType
        | SqlDataTypeNames.geography ->
            nosupport()

            
          
    let private getColumns(config : SqlMetadataReaderConfig) =
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
    
    let private getTables(config : SqlMetadataReaderConfig) =
        [for item in config |> getMetadataView<vTable> ->
            {
               TabularDescription.Name = DataObjectName(item.SchemaName, item.TableName)
               Documentation = item.Description
               Columns = []               
            } |> TableDescription
        ]
            

    let getCatalog(config : SqlMetadataReaderConfig)  =         
        {
            CatalogName = SqlConnectionStringBuilder(config.ConnectionString).InitialCatalog
            SqlMetadataCatalog.Schemas = []
        }


type internal SqlMetadataReader(config : SqlMetadataReaderConfig) =
    
    let getDataTypes() =
        [for item in config |> Metadata.getMetadataView<Metadata.vDataType> ->
            {
                DataTypeDescription.Name = DataObjectName(item.SchemaName, item.DataTypeName)
                MaxLength = item.MaxLength
                Precision = item.Precision
                Scale = item.Scale
                IsNullable = item.IsNullable
                IsTableType = item.IsTableType
                IsCustomObject = item.IsAssemblyType
                IsUserDefined = item.IsUserDefined
                BaseTypeName = None                
            }                
        ]

    let getDataTypes() =
        ()

