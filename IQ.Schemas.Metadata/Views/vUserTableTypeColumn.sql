create view Metadata.vUserTableTypeColumn as
	select 
		s.SchemaId,
		s.SchemaName,
		o.ObjectId as TableId,
		tt.name as TableName,
		c.name as ColumnName,
		d.Value as ColumnDescription,
		c.column_id as Position,
		c.user_type_id as DataTypeId,
		t.DataTypeName,
		c.is_computed as IsComputed,
		c.is_identity as IsIdentity,
		c.is_nullable as IsNullable,
		tt.is_user_defined as IsUserDefined,
		c.max_length as MaxLength,
		c.precision as Precision,
		c.scale as Scale
	from 
		sys.table_types tt
		inner join sys.columns c on c.object_id = tt.type_table_object_id
		inner join Metadata.vObject o on o.ObjectId = tt.type_table_object_id
		inner join Metadata.vSchema s on s.SchemaId = tt.schema_id
		inner join Metadata.vDataType t on t.DataTypeId = c.user_type_id
		left join Metadata.vDescription d on d.MajorId = tt.user_type_id and d.MinorId = c.column_id

	