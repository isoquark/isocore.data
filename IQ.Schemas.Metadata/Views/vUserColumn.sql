create view Metadata.vUserColumn as
	select 
		t.SchemaId, 
		t.SchemaName, 
		t.TableId, 
		t.TableName, 
		c.ColumnName, 
		c.Description as ColumnDescription, 
		c.Position as Position,
		c.DataTypeId, 
		c.DataTypeName,
		c.IsComputed,
		c.IsIdentity,
		c.IsNullable,
		c.MaxLength,
		c.Precision,
		c.Scale
	from Metadata.vUserTable t
		cross apply Metadata.fGetColumns(t.TableId) c

