CREATE VIEW [Metadata].[vUserObject] as
	select 
		[x].[CatalogId], 
		[x].[CatalogName], 
		[x].[SchemaId], 
		[x].[SchemaName], 
		[x].[ObjectId], 
		[x].[ObjectName], 
		coalesce([x].[ParentObjectId], 0) as ParentObjectId, 
		[x].[ObjectType], 
		[x].[IsUserDefined]
	from 
		Metadata.vObject x
	where 
		x.IsUserDefined = 1
	
