CREATE TABLE Metadata.AdoTypeMap
(
	SqlTypeName nvarchar(15) not null,
	BclTypeName nvarchar(25) not null,
	SqlDbTypeEnum nvarchar(25) not null,
	SqlDbTypeDataReader nvarchar(25) not null,
	DbTypeEnum nvarchar(25) not null,
	DbTypeDataReader nvarchar(25) not null

	constraint PK_DataTypeMap primary key(SqlTypeName)
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Maps intrinsic SQL database engine types to intrinsic CLR types; see https://msdn.microsoft.com/en-us/library/cc716729%28v=vs.110%29.aspx',
    @level0type = N'SCHEMA',
    @level0name = N'Metadata',
    @level1type = N'TABLE',
    @level1name = N'AdoTypeMap',
    @level2type = NULL,
    @level2name = NULL