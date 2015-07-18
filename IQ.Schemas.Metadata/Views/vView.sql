create view Metadata.vView as
	select 
		o.SchemaId,
		o.SchemaName,
		x.object_id as TableId,
		x.name as TableName,
		o.IsUserDefined,
		d.Value as Description
	from 
		sys.views x 
		inner join Metadata.vObject o on o.ObjectId = x.object_id
		left join Metadata.vDescription d on d.MajorId = x.object_id and d.MinorId = 0

