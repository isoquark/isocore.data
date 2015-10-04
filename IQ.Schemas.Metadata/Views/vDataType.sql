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
			else null end BaseTypeId,
		sbase.SchemaName as BaseSchemaName,
		tbase.name as BaseTypeName,
		tt.type_table_object_id as ObjectId
		
	from 
		sys.types t 
		inner join Metadata.vSchema s on s.SchemaId = t.schema_id
		left join Metadata.AdoTypeMap m on m.SqlTypeName = t.name
		left join sys.types tbase on t.system_type_id = tbase.system_type_id  and t.is_user_defined = 1 and tbase.is_user_defined = 0
		left join Metadata.vSchema sbase on sbase.SchemaId = tbase.schema_id
		left join sys.table_types tt on tt.user_type_id = t.user_type_id
		outer apply Metadata.fDescription(t.user_type_id, 0) d
