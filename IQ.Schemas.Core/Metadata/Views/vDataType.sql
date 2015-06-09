create view Metadata.vDataType as
	select 
		t.user_type_id as DataTypeId,
		t.name as DataTypeName,
		t.schema_id as SchemaId,
		s.SchemaName,
		t.max_length as MaxLength,
		t.precision as Precision,
		t.scale as Scale,
		t.is_nullable as Nullable,
		t.is_user_defined as IsUserDefined,
		case t.is_user_defined when
			1 then t.system_type_id 
			else null end BaseTypeId
	from 
		sys.types t 
		inner join Metadata.vSchema s on s.SchemaId = t.schema_id