create view Metadata.vUserView as
	select 
		x.* 
	from 
		Metadata.vView x 
	where 
		x.IsUserDefined = 1