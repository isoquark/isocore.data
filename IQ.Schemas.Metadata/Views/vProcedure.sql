create view Metadata.vProcedure as
	select 
		o.SchemaId,
		o.SchemaName,
		x.object_id as ProcedureId,
		x.name as ProcedureName,
		o.IsUserDefined,
		d.Value as Description
	from 
		sys.procedures x 
		inner join Metadata.vObject o on o.ObjectId = x.object_id
		left join Metadata.vDescription d on d.MajorId = x.object_id and d.MinorId = 0
	