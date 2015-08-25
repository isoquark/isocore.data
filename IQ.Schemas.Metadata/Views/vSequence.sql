create view Metadata.vSequence as
	select 
		o.CatalogId,
		o.CatalogName,
		x.object_id as SequenceId,
		x.name as SequenceName,
		x.schema_id as SchemaId,
		o.SchemaName,
		o.IsUserDefined,
		d.Value as Description,
		x.start_value as StartValue,
		x.increment as Increment,
		x.minimum_value as MinimumValue,
		x.maximum_value as MaximumValue,
		x.is_cycling as IsCycling,
		x.is_cached as IsCached,
		x.cache_size as CacheSize,
		t.DataTypeId,
		t.DataTypeName,
		x.current_value as CurrentValue,
		x.is_exhausted as IsExhausted
	from sys.sequences x
	inner join Metadata.vObject o  on o.ObjectId = x.object_id
	inner join Metadata.vDataType t on t.DataTypeId = x.system_type_id
	left join Metadata.vDescription d on d.MajorId = x.object_id and d.MinorId = 0
