CREATE VIEW [Metadata].[vParameter] as
	select 
		o.CatalogId,
		o.CatalogName,
		s.SchemaId as ParentSchemaId,
		s.SchemaName as ParentSchemaName,
		o.ObjectId as ParentId,
		o.ObjectName as ParentName,
		x.name as ParameterName,
		d.PropertyValue as Description,
		x.parameter_id as Position,
		x.is_output as IsOutput,
		x.max_length as MaxLength,
		x.precision as Precision,
		x.scale as Scale,
		x.user_type_id as DataTypeId,
		t.SchemaName as DataTypeSchemaName,
		t.DataTypeName,
		o.IsUserDefined
	from 
		sys.all_parameters x
		inner join Metadata.vObject o on o.ObjectId =x.object_id
		inner join Metadata.vSchema s on s.SchemaId = o.SchemaId
		inner join Metadata.vDataType t on t.DataTypeId = x.user_type_id
		outer apply Metadata.fParameterDescription(o.ObjectId, x.parameter_id) d
