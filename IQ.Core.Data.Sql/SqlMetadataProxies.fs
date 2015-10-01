namespace IQ.Core.Data

open System



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

    


