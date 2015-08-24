namespace IQ.Core.Data.Sql

open IQ.Core.Contracts
open IQ.Core.Framework
open IQ.Core.Data



//These proxies align with (a subset of) the columns returned by the views in the Metadata schema
module internal Metadata=
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

    type vColumn = {
        SchemaName : string
        TableName : string
        ColumnName : string
        ColumnDescription : string option
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
    
    type vTable = {
        SchemaName : string
        TableName : string
        IsUserDefined : bool
        Description : string option                    
    }           

    type vView = {
        SchemaName : string
        ViewName : string
        IsUserDefined : bool
        Description : string option                    
    }           



