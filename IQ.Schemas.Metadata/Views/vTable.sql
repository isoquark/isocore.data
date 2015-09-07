create view Metadata.vTable as
	select 
		o.CatalogId,
		o.CatalogName,
		o.SchemaId,
		o.SchemaName,
		x.object_id as TableId,
		x.name as TableName,
		o.IsUserDefined,
		d.Value as Description
	from 
		sys.tables x 
		inner join Metadata.vObject o on o.ObjectId = x.object_id
		left join Metadata.vDescription d on d.MajorId = x.object_id and d.MinorId = 0 and d.ClassName = 'OBJECT_OR_COLUMN'
		