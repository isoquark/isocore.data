create view Metadata.vView as
	select 
		o.CatalogId,
		o.CatalogName,
		o.SchemaId,
		o.SchemaName,
		x.object_id as ViewId,
		x.name as ViewName,
		o.IsUserDefined,
		d.PropertyValue as Description
	from 
		sys.views x 
		inner join Metadata.vObject o on o.ObjectId = x.object_id
		outer apply Metadata.fDescription(x.object_id, 0) d
	union
		select 
			DB_ID() as CatalogId,
			DB_NAME() as CatalogName,
			x.schema_id as SchemaId,
			s.name as SchemaName,
			x.object_id as ViewId,
			x.name as ViewName,
			convert(bit, 0) as IsUserDefined,
			null as Description
		from
			sys.system_views x
			inner join sys.schemas s on s.schema_id = x.schema_id
