create view Metadata.vProcedureParameter as
	select 
		x.CatalogId,
		x.CatalogName,
		[x].[SchemaId], 
		[x].[SchemaName], 
		[x].[ProcedureId], 
		[x].[ProcedureName], 
		[x].[IsUserDefined], 
		[x].[Description],
		p.ParameterName, 
		p.Description as ParameterDescription, 
		p.Position,
		p.DataTypeId, 
		p.DataTypeName,
		p.MaxLength,
		p.Precision,
		p.Scale		
	from 
		Metadata.vProcedure x
	cross apply Metadata.fParameters(x.ProcedureId) p