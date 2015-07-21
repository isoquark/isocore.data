namespace IQ.Core.Data.Sql

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data

type SqlDataStoreConfig = SqlDataStoreConfig of cs : ConnectionString * clrMetadataProvider : IClrMetadataProvider
with
    member this.ConnectionString = match this with SqlDataStoreConfig(cs=x) -> x
    member this.ClrMetadataProvider = match this with SqlDataStoreConfig(clrMetadataProvider=x) ->x

module internal SqlMetadataProvider =

    [<AutoOpen>]
    module private Metadata =
        type vDataType = {
            DataTypeId : int
            DataTypeName : string
            SchemaId : int
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

        type vTable = {
            SchemaId : int
            SchemaName : string
            TableId : int
            TableName : string
            IsUserDefined : bool
            Description : string option                    
        }                
    type private MetadataProvider(config : SqlDataStoreConfig) =
        let cs = config.ConnectionString.Text
        let clrmp = config.ClrMetadataProvider
        interface ISqlMetadataProvider with
            member this.Describe q = 
                match q with
                | FindTables(q) ->
                    match q  with
                    | FindAllTables ->
                        let vTables = clrmp.FindType<vTable>() |> Tabular.executeProxyQuery<vTable> cs 
                        [for vTable in vTables ->
                            {TabularDescription.Name = DataObjectName(vTable.SchemaName, vTable.TableName)
                             Description = vTable.Description
                             Columns = []
                            } |> TabularObject
                        ]
                    | _ -> nosupport()
                | _ ->
                    nosupport()
    
    let get(config) =
        MetadataProvider(config) :> ISqlMetadataProvider
    

