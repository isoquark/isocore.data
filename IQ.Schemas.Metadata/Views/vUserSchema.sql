CREATE VIEW [Metadata].[vUserSchema] as
	select 
		* 
	from 
		Metadata.vSchema 
	where 
		IsUserDefined = 1
