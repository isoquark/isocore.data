CREATE VIEW [Metadata].[vColumn] as
	select top(50000)
		o.CatalogId,
		o.CatalogName,
		s.SchemaId as ParentSchemaId,
		s.SchemaName as ParentSchemaName,
		o.ObjectId as ParentId,
		o.ObjectName as ParentName,
		o.ObjectType as ParentType,
		x.name as ColumnName,
		d.PropertyValue as Description,
		x.column_id - 1 as Position,
		x.is_computed as IsComputed,
		x.is_identity as IsIdentity,
		x.is_nullable as IsNullable,
		case 
			when t.DataTypeName = 'nvarchar' and x.max_length <> -1 then x.max_length/2
			when t.DataTypeName = 'nchar' then x.max_length/2
			else
				x.max_length
			end as MaxLength,
												
						
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
		outer apply Metadata.fColumnDescription(o.ObjectId, x.column_id) d
	order by
		ParentSchemaName, ParentName, Position
