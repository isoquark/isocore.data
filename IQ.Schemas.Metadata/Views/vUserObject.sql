CREATE VIEW [Metadata].[vUserObject] as
	select 
		x.*
	from 
		Metadata.vObject x
	where 
		x.IsUserDefined = 1
	
