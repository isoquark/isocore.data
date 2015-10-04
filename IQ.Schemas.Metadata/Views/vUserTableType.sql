create view Metadata.vUserTableType as
	select 
		DB_ID() as CatalogId,
		DB_NAME() as CatalogName,
		o.SchemaId,
		o.SchemaName,
		x.user_type_id as TableTypeId,
		x.name as TableTypeName,
		o.IsUserDefined,
		d.PropertyValue as Description,
		x.type_table_object_id as ObjectId
	from 
		sys.table_types x 
		inner join Metadata.vDataType o on o.DataTypeId = x.user_type_id
		outer apply Metadata.fDescription(x.user_type_id, 0) d
	where
		o.IsUserDefined = 1
