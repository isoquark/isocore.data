create view Metadata.vProcedureParameter as
	select 
		x.CatalogId,
		x.CatalogName,
		[x].SchemaId as ParentSchemaId, 
		[x].SchemaName ParentSchemaName, 
		[x].ProcedureId, 
		[x].ProcedureName, 
		[x].[IsUserDefined], 
		p.ParameterName, 
		p.Description, 
		p.Position,
		p.IsOutput,
		p.DataTypeId, 
		p.DataTypeSchemaName,
		p.DataTypeName,
		p.MaxLength,
		p.Precision,
		p.Scale		
	from 
		Metadata.vProcedure x
	cross apply Metadata.fParameters(x.ProcedureId) p