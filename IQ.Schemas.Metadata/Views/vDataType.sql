create view Metadata.vDataType as
	select 
		t.user_type_id as DataTypeId,
		t.name as DataTypeName,
		d.PropertyValue as Description,
		t.schema_id as SchemaId,
		s.SchemaName,
		m.BclTypeName as MappedBclType,
		m.SqlDbTypeEnum as MappedSqlDbTypeEnum,
		t.max_length as MaxLength,
		t.precision as Precision,
		t.scale as Scale,		
		isnull(t.is_nullable, 0) as IsNullable,
		t.is_table_type as IsTableType,
		t.is_assembly_type as IsAssemblyType,
		t.is_user_defined as IsUserDefined,
		case t.is_user_defined when
			1 then t.system_type_id 
			else null end BaseTypeId
	from 
		sys.types t 
		inner join Metadata.vSchema s on s.SchemaId = t.schema_id
		left join Metadata.AdoTypeMap m on m.SqlTypeName = t.name
		--left join Metadata.vDescription d on d.MajorId = t.user_type_id and d.MinorId = 0
		outer apply Metadata.fDescription(t.user_type_id, 0) d
