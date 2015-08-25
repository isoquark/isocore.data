create view Metadata.vUserTableType as
	select 
		DB_ID() as CatalogId,
		DB_NAME() as CatalogName,
		o.SchemaId,
		o.SchemaName,
		x.user_type_id as TableTypeId,
		x.name as TableTypeName,
		o.IsUserDefined,
		d.Value as Description
	from 
		sys.table_types x 
		inner join Metadata.vDataType o on o.DataTypeId = x.user_type_id
		left join Metadata.vDescription d on d.MajorId = x.user_type_id and d.MinorId = 0
	where
		o.IsUserDefined = 1
