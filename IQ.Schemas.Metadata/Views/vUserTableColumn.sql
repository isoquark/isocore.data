CREATE VIEW [Metadata].[vUserTableColumn] as
	select 
		* 
	from 
		Metadata.vTableColumn 
	where 
		IsUserDefined = 1
	
