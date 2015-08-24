create view Metadata.vUserTable as
	select 
		x.*
	from 
		Metadata.vTable x where x.IsUserDefined = 1