namespace IQ.Core.Data.Sql

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data

open System.Data
open System.Data.SqlClient

type internal MetadataReaderConfig = {
    ConnectionString : string
    IgnoreSystemObjects : bool
}

//These proxies align with (a subset of) the columns returned by the views in the Metadata schema
module internal Metadata=
    
    let private toOptionalString s =
        if s |> System.String.IsNullOrEmpty then None else s |> Some
    
    type IMetadataView =
        abstract IsUserDefined : bool
        abstract Documentation : string option
    
    type AdoTypeMap = {
        SqlTypeName : string
        BclTypeName : string
        SqlDbTypeEnum : string        
    }
    
    type vDataType = {
        DataTypeName : string
        SchemaName : string
        MappedBclType : string option
        MappedSqlDbTypeEnum : string option
        MaxLength : int16
        Precision : uint8
        Scale : uint8
        IsNullable : bool
        IsTableType : bool
        IsAssemblyType : bool
        IsUserDefined : bool
        BaseTypeInt : uint8 option
    }
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = None

    type vSchema = {
        SchemaName : string
        Description : string option
        IsUserDefined : bool
    }
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = this.Description 
    
    type vColumn = {
        SchemaName : string
        TableName : string
        ColumnName : string
        Description : string option
        Position : int
        DataTypeName : string
        IsComputed : bool
        IsIdentity : bool
        IsNullable : bool
        IsUserDefined : bool
        MaxLength : int16 option
        Precision : uint8
        Scale : uint8                                
    }
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = this.Description
    
    type vTable = {
        SchemaName : string
        TableName : string
        IsUserDefined : bool
        Description : string option                    
    }           
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = this.Description

    type vView = {
        SchemaName : string
        ViewName : string
        IsUserDefined : bool
        Description : string option                    
    }           
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = this.Description

    let private getMetadataView<'T when 'T :> IMetadataView> (config : MetadataReaderConfig) =
        if config.IgnoreSystemObjects then
            SqlProxyReader.selectSome<'T> config.ConnectionString ("IsUserDefined=1")
        else
            SqlProxyReader.selectAll<'T> config.ConnectionString

//    let private getColumnDataType(c : vColumn) =
//       match c.DataTypeName with
//        | SqlDataTypeNames.bigint ->
//            Int64DataType
//        | SqlDataTypeNames.binary ->
//            c.
          
    let private getColumns(config : MetadataReaderConfig) =
        [for item in config |> getMetadataView<vColumn> ->
            {
                ColumnDescription.Name = item.ColumnName
                Position = item.Position
                StorageType = DataType.AnsiTextFixedDataType(5)
                Documentation = item.Description
                Nullable = item.IsNullable
                AutoValue = if item.IsComputed then
                                    AutoValueKind.Computed |> Some
                                else if item.IsIdentity then
                                    AutoValueKind.Identity |> Some
                                else 
                                    None
                
            
            }
        
        ]
    
    let private getTables(config : MetadataReaderConfig) =
        [for item in config |> getMetadataView<vTable> ->
            {
               TabularDescription.Name = DataObjectName(item.SchemaName, item.TableName)
               Documentation = item.Description
               Columns = []               
            } |> TableDescription
        ]
            

    let getCatalog(config : MetadataReaderConfig)  =         
        {
            CatalogName = SqlConnectionStringBuilder(config.ConnectionString).InitialCatalog
            SqlMetadataCatalog.Schemas = []
        }


