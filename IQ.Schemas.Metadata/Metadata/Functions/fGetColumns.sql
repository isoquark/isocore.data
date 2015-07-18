create function Metadata.fGetColumns(@ParentId int) returns table as return
	select 
		s.SchemaId as ParentSchemaId,
		s.SchemaName as ParentSchemaName,
		o.ObjectId as ParentId,
		o.ObjectName as ParentName,
		x.name as ColumnName,
		d.Value as Description,
		x.column_id as Position,
		x.is_computed as IsComputed,
		x.is_identity as IsIdentity,
		x.is_nullable as IsNullable,
		x.max_length as MaxLength,
		x.precision as Precision,
		x.scale as Scale,
		x.user_type_id as DataTypeId,
		t.DataTypeName
	from 
		sys.columns x
		inner join Metadata.vObject o on o.ObjectId =x.object_id
		inner join Metadata.vSchema s on s.SchemaId = o.SchemaId
		inner join Metadata.vDataType t on t.DataTypeId = x.user_type_id
		left join Metadata.vDescription d on d.MajorId = o.ObjectId and d.MinorId = x.column_id
	where
		x.object_id = @ParentId
