create view Metadata.vUserTable as
	select 
*	from 
		Metadata.vTable x where x.IsUserDefined = 1