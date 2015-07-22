create view Metadata.vDataDictionary as
select top(25000) 
	c.SchemaName, 
	c.TableName, 
	t.Description as TableDescription,
	c.ColumnName, 
	c.ColumnDescription,
	c.Position,
	c.DataTypeName,
	c.IsNullable
from 
	Metadata.vUserColumn c
	inner join Metadata.vTable t on c.TableId = t.TableId	
where 
	c.SchemaName not in ('Metadata', 'dbo')
order by 
	c.SchemaName, c.TableName, c.Position
