CREATE VIEW [Metadata].[vColumn] as
	select 
		o.CatalogId,
		o.CatalogName,
		s.SchemaId as ParentSchemaId,
		s.SchemaName as ParentSchemaName,
		o.ObjectId as ParentId,
		o.ObjectName as ParentName,
		o.ObjectType as ParentType,
		x.name as ColumnName,
		d.Value as Description,
		x.column_id - 1 as Position,
		x.is_computed as IsComputed,
		x.is_identity as IsIdentity,
		x.is_nullable as IsNullable,
		x.max_length as MaxLength,
		x.precision as Precision,
		x.scale as Scale,
		x.user_type_id as DataTypeId,
		t.SchemaName as DataTypeSchemaName,
		t.DataTypeName,
		o.IsUserDefined
	from 
		sys.all_columns x
		inner join Metadata.vObject o on o.ObjectId =x.object_id
		inner join Metadata.vSchema s on s.SchemaId = o.SchemaId
		inner join Metadata.vDataType t on t.DataTypeId = x.user_type_id
		--We use the fact that non-column objects, such as contraints, have a minor id of 0 
		left join Metadata.vDescription d on d.MajorId = o.ObjectId and d.MinorId = x.column_id and d.ClassName = 'OBJECT_OR_COLUMN' and d.MinorId <> 0
