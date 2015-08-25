CREATE VIEW [Metadata].[vUserViewColumn] as
	select 
		* 
	from 
		Metadata.vViewColumn 
	where 
		IsUserDefined = 1
