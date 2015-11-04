create function Metadata.fDataMatrixFromTable(@TableSchema sysname, @TableName sysname) returns table as return
select top(1024)
	c.ColumnName as ColumnIdentifier,
	c.ColumnName as SourceColumnName,
	c.ColumnName as TargetColumnName,
	c.Position,
	c.DataTypeSchemaName as DataTypeSchema,
	c.DataTypeName,
	c.IsNullable,
	c.MaxLength,
	c.Precision,
	c.Scale,
	coalesce(convert(varchar(250), c.Description), '') as ColumnDescription
from 
	Metadata.vColumn c 
where 
	c.ParentSchemaName = @TableSchema  and 
	c.ParentName = @TableName and
	c.ColumnName not in ('DbCreateTime', 'DBCreateUser', 'SurrogateKey')
order by 
	c.Position
