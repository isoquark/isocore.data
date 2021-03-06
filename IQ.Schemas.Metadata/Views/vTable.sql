﻿create view Metadata.vTable as
	select 
		o.CatalogId,
		o.CatalogName,
		o.SchemaId,
		o.SchemaName,
		x.object_id as TableId,
		x.name as TableName,
		o.IsUserDefined,
		d.PropertyValue as Description,
		x.is_filetable as IsFileTable
	from 
		sys.tables x 
		inner join Metadata.vObject o on o.ObjectId = x.object_id
		outer apply Metadata.fDescription(x.object_id, 0) d
		