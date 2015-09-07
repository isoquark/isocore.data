--Note that in some cases, metadata for a procedure can be determined as follows:
-- select * from sys.dm_exec_describe_first_result_set('[schema].[proc]', null, 0);


create view Metadata.vProcedure as
	select 
		x.CatalogId,
		x.CatalogName,
		x.SchemaId,
		x.SchemaName,
		x.ObjectId as ProcedureId,
		x.ObjectName as ProcedureName,
		x.IsUserDefined,
		d.Value as Description
	from 
		Metadata.vObject x 
		left join Metadata.vDescription d on d.MajorId = x.ObjectId and d.MinorId = 0 and d.ClassName = 'OBJECT_OR_COLUMN'
	where
		x.ObjectType = 'SQL_STORED_PROCEDURE'

	