create view Metadata.vUserView as
	select * from Metadata.vView x where x.IsUserDefined = 1