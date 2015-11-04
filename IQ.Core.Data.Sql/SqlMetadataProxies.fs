namespace IQ.Core.Data

open System



type internal IMetadataView =
    abstract IsUserDefined : bool
    abstract Documentation : string 


//These proxies align with (a subset of) the columns returned by the views in the Metadata schema
module Metadata =
    
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
        member val BaseSchemaName = String.Empty with get, set
        member val BaseTypeName = String.Empty with get, set
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
        member val IsFileTable = false with get, set
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

    type vProcedure() =
        member val CatalogName = String.Empty with get, set
        member val SchemaName = String.Empty with get, set
        member val ProcedureName = String.Empty with get, set
        member val IsUserDefined = false with get, set
        member val Description = String.Empty with get, set
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = this.Description
       
    type vProcedureParameter()=
        member val CatalogName = String.Empty with get, set
        member val ParentSchemaName = String.Empty with get, set
        member val ProcedureName = String.Empty with get, set
        member val IsUserDefined = false with get, set
        member val Description = String.Empty with get, set
        member val ParameterName = String.Empty with get, set
        member val Position = 0  with get, set
        member val DataTypeSchemaName = String.Empty with get,set
        member val DataTypeName = String.Empty with get,set
        member val MaxLength = 0 with get, set
        member val Precision = 0uy with get, set
        member val Scale = 0uy with get, set
        member val IsOutput = false with get, set
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = this.Description
        
    type vSequence() =
        member val CatalogName = String.Empty with get, set
        member val SchemaName = String.Empty with get, set
        member val SequenceName = String.Empty with get, set
        member val IsUserDefined = false with get, set
        member val Description = String.Empty with get, set
        member val StartValue = null with get, set
        member val Increment = null with get, set
        member val MinimumValue = null with get, set
        member val MaximumValue = null with get, set
        member val IsCycling = false with get, set
        member val IsCached = false with get, set
        member val CacheSize = 0 with get, set
        member val DataTypeName = String.Empty with get, set
        member val CurrentValue = null with get, set
        member val IsExhausted = false with get, set
    with
        interface IMetadataView with
            member this.IsUserDefined = this.IsUserDefined
            member this.Documentation = this.Description

    type vDataMatrix() =
        member val DataMatrixIdentifier = String.Empty with get, set
        member val DataMatrixName = String.Empty with get, set
        member val ColumnPosition = 0 with get, set
        member val ColumnIdentifier = String.Empty with get, set
        member val SourceColumnName = String.Empty with get, set
        member val TargetColumnName = String.Empty with get, set
        member val DataTypeSchema = String.Empty with get, set
        member val DataTypeName = String.Empty with get, set
        member val IsNullable = false with get, set
        member val MaxLength = 0 with get, set
        member val Precision = 0uy with get, set
        member val Scale = 0uy with get, set
        member val ColumnDescription = String.Empty with get,set

    type FileFormatType() =
        member val TypeCode = 0 with get, set
        member val Name = String.Empty with get, set
        member val Description = String.Empty with get, set

    type SemanticFileType() =
        member val TypeCode = 0 with get, set
        member val Name = String.Empty with get, set
        member val Description = String.Empty with get, set

    type vTabularFileMatrix() =
        member val SemanticFileTypeCode = 0 with get, set
        member val SemanticFileTypeName = String.Empty with get, set
        member val FileFormatTypeCode = 0 with get, set
        member val FileFormatTypeName = String.Empty with get, set
        member val DataMatrixIdentifier = String.Empty with get, set
        member val DataMatrixName = String.Empty with get, set
        member val ColumnPosition = 0 with get, set
        member val ColumnIdentifier = String.Empty with get, set
        member val SourceColumnName = String.Empty with get, set
        member val TargetColumnName = String.Empty with get, set
        member val DataTypeSchema = String.Empty with get, set
        member val DataTypeName = String.Empty with get, set
        member val IsNullable = false with get, set
        member val MaxLength = 0 with get, set
        member val Precision = 0uy with get, set
        member val Scale = 0uy with get, set
        member val ColumnDescription = String.Empty with get,set

    [<Schema("Metadata")>]
    type IMetadataOps =
        [<TableFunction>]
        abstract member fTabularFileMatrix: semanticFileTypeCode : int -> fileFormatTypeCode : int -> vTabularFileMatrix[]

open Metadata;

type TabularFileColumnMap = {
    SourceColumnPosition : int
    SourceColumnName : string
    TargetColumnPosition : int
    TargetColumnName : string
    MaxDataLength : int
}

type TabularFileTableMap = {
    SemanticTypeCode : int
    FormatTypeCode : int
    TargetTableName : DataObjectName
    ColumnMaps : TabularFileColumnMap list
}

module TabularFile =

    [<Literal>]
    let RecordIdColumnName = "RecordId"
    [<Literal>]
    let InputPathColumnName = "InputPath"
    [<Literal>]
    let ImportTimestampColumnName = "ImportTimestamp"
    [<Literal>]
    let FileSchemaName = "Files"
            
    let private getMgtColumns (tableName : DataObjectName) =
        [
            {
                ColumnDescription.Name = RecordIdColumnName
                ParentName = tableName
                Position = 0
                DataType = Int64DataType
                DataKind = DataKind.Int64
                Documentation = "Uniquely identifies the record"
                Nullable = false
                AutoValue = AutoValueKind.None
                Properties = []
            }
            {
                ColumnDescription.Name = InputPathColumnName
                ParentName = tableName
                Position = 1
                DataType = UnicodeTextVariableDataType(250)
                DataKind = DataKind.UnicodeTextVariable
                Documentation = "The name of the file from which the record was extracted"
                Nullable = false
                AutoValue = AutoValueKind.None
                Properties = []
            }
            {
                ColumnDescription.Name = ImportTimestampColumnName
                ParentName = tableName
                Position = 2
                DataType = DateTimeDataType(27uy, 7uy)
                DataKind = DataKind.DateTime
                Documentation = "The time at which the record was loaded"
                Nullable = false
                AutoValue = AutoValueKind.None
                Properties = []
                        
            }
            
        ]
        
    let describeRecordTable (store : ISqlDataStore) (semanticTypeCode : int) (formatTypeCode : int) =
        let ops = store.GetCommandContract<IMetadataOps>()
        let entries = ops.fTabularFileMatrix semanticTypeCode formatTypeCode
        if entries.Length <> 0 then
            let specimen = entries.[0]
            let tableName = DataObjectName(FileSchemaName, specimen.DataMatrixIdentifier)
            let dataColumns = [ for entry in entries ->
                                {
                                    ColumnDescription.Name = entry.TargetColumnName
                                    ParentName = tableName
                                    Position = entry.ColumnPosition + 3
                                    DataType = if entry.MaxLength = 1 then  AnsiTextFixedDataType(1) else AnsiTextVariableDataType(entry.MaxLength)
                                    DataKind = if entry.MaxLength = 1 then DataKind.AnsiTextFixed else DataKind.AnsiTextMax
                                    Documentation = entry.ColumnDescription
                                    Nullable = entry.IsNullable
                                    AutoValue = AutoValueKind.None
                                    Properties = []
                                }
                           ]
            {
                TableDescription.Name = tableName
                Columns = dataColumns |> List.append (getMgtColumns(tableName))
                Documentation = String.Empty
                Properties = []
                IsFileTable = false
            }
        else
            raise <| ArgumentException(
                sprintf "The tabular file definition with semantic code %i and format code %i does not exist" semanticTypeCode formatTypeCode)

    let describeTableMap (store : ISqlDataStore) (semanticTypeCode : int) (formatTypeCode : int) =
        let ops = store.GetCommandContract<IMetadataOps>()
        let entries = ops.fTabularFileMatrix semanticTypeCode formatTypeCode
        if entries.Length <> 0 then
            let specimen = entries.[0]
            let tableName = DataObjectName(FileSchemaName, specimen.DataMatrixIdentifier)
            {
                SemanticTypeCode = semanticTypeCode
                FormatTypeCode = formatTypeCode
                TargetTableName = tableName
                ColumnMaps = [for entry in entries ->
                                {
                                    TabularFileColumnMap.SourceColumnPosition = entry.ColumnPosition + 3
                                    SourceColumnName = entry.SourceColumnName
                                    TargetColumnPosition = entry.ColumnPosition
                                    TargetColumnName = entry.TargetColumnName
                                    MaxDataLength = entry.MaxLength
                                }
                             ]
           }
        else
            raise <| ArgumentException(
                sprintf "The tabular file definition with semantic code %i and format code %i does not exist" semanticTypeCode formatTypeCode)
        