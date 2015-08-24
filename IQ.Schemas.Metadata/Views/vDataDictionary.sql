create view Metadata.vDataDictionary as
select top(25000) 
	c.ParentSchemaId, 
	c.ParentName, 
	t.Description as TableDescription,
	c.ColumnName, 
	c.Description as ColumnDescrption,
	c.Position,
	c.DataTypeName,
	c.IsNullable
from 
	Metadata.vUserColumn c
	inner join Metadata.vTable t on c.ParentId = t.TableId	
where 
	c.ParentSchemaName not in ('Metadata', 'dbo')
order by 
	c.ParentSchemaName, c.ParentName, c.Position
