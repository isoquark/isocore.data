create view Metadata.vUserProcedure as 
	select x.* from Metadata.vProcedure x where x.IsUserDefined = 1