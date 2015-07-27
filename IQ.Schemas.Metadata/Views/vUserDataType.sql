CREATE VIEW [Metadata].[vUserDataType] as
	select 
		[DataTypeId], 
		[DataTypeName], 
		[Description], 
		[SchemaId], 
		[SchemaName], 
		[MappedBclType], 
		[MappedSqlDbTypeEnum], 
		[MaxLength], 
		[Precision], 
		[Scale], 
		[IsNullable], 
		[IsTableType], 
		[IsAssemblyType], 
		[IsUserDefined], 
		[BaseTypeId] 
	from 
		Metadata.vDataType
	 where 
		IsUserDefined = 1

