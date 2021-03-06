﻿CREATE VIEW [Metadata].[vTableFunction] as
	select 
		x.CatalogId,
		x.CatalogName,
		x.SchemaId,
		x.SchemaName,
		x.ObjectId as FunctionId,
		x.ObjectName as FunctionName,
		x.IsUserDefined,
		d.PropertyValue as Description
	from 
		Metadata.vObject x 
		outer apply Metadata.fDescription(x.ObjectId, 0) d
	--	left join Metadata.vDescription d on d.MajorId = x.ObjectId and d.MinorId = 0
	where
		x.ObjectType in ('SQL_INLINE_TABLE_VALUED_FUNCTION', 'SQL_TABLE_VALUED_FUNCTION')
	
