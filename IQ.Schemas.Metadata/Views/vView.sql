create view Metadata.vView as
	select 
		o.SchemaId,
		o.SchemaName,
		x.object_id as ViewId,
		x.name as ViewName,
		o.IsUserDefined,
		d.Value as Description
	from 
		sys.views x 
		inner join Metadata.vObject o on o.ObjectId = x.object_id
		left join Metadata.vDescription d on d.MajorId = x.object_id and d.MinorId = 0
	union
		select 
			x.schema_id as SchemaId,
			s.name as SchemaName,
			x.object_id as ViewId,
			x.name as ViewName,
			convert(bit, 0) as IsUserDefined,
			null as Description
		from
			sys.system_views x
			inner join sys.schemas s on s.schema_id = x.schema_id
