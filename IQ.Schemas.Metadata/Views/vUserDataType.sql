CREATE VIEW [Metadata].[vUserDataType] as
	select 
		x.*
	from 
		Metadata.vDataType x
	 where 
		x.IsUserDefined = 1

